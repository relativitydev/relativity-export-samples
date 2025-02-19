using System.Diagnostics;
using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1.Model;
using Relativity.Services.ServiceProxy;
using Spectre.Console;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public partial class BaseExportService
{
	private readonly Logger _logger;
	private readonly IServiceFactory _serviceFactory;
	private readonly string _host;
	private readonly string _username;
	private readonly string _password;

	public BaseExportService(string host, string username, string password, string[] args = default!)
	{
		_host = host;
		_username = username;
		_password = password;

		_serviceFactory = GetServiceFactory();

		_logger = new Logger(args);
	}

	protected IServiceFactory GetServiceFactory()
	{
		Uri relativityRestUri = new Uri($"{_host}relativity.rest/api");
		Credentials credentials = new UsernamePasswordCredentials(_username, _password);

		ServiceFactorySettings settings = new ServiceFactorySettings(relativityRestUri, credentials);

		return new ServiceFactory(settings);
	}

	private async Task<ValueResponse<ExportJob>> WaitForJobToBeCompletedAsync(Func<Task<ValueResponse<ExportJob>>> job, bool updateStatus = true, int frequency = 5_000)
	{
		ValueResponse<ExportJob>? jobStatus = null;
		CancellationTokenSource tokenSource = new();
		Stopwatch watch = Stopwatch.StartNew();
		int retries = 3;

		if (updateStatus)
		{
			// request time measurement
			Task watchUpdater = Task.Run(async () =>
			{
				while (!tokenSource.IsCancellationRequested)
				{
					OutputHelper.UpdateStatus($"Fetching updates ({watch.ElapsedMilliseconds} ms)");
					await Task.Delay(100);
				}
			}, tokenSource.Token);
		}

		do
		{
			try
			{
				jobStatus = await job();

				if (jobStatus is null)
					throw new Exception("The response was incorrect");

				string logData =
					$"Export job ID: {jobStatus.ExportJobID}\n"
					+ $"Job status: [aquamarine1]{jobStatus.Value.JobStatus}[/]";

				_logger.LogInformation(logData);

				await Task.Delay(frequency);
				retries = 3;
			}
			catch (Exception) when (retries > 0)
			{
				retries--;
				_logger.LogWarning($"Retrying job status fetching ({retries} retries left)");
				await Task.Delay(3000);

			}
			catch (Exception ex)
			{
				AnsiConsole.WriteException(ex);
				throw;
			}
			finally
			{
				watch.Restart();
			}
		} while (jobStatus?.Value.JobStatus is not ExportStatus.Completed
			and not ExportStatus.CompletedWithErrors
			and not ExportStatus.Failed
			and not ExportStatus.Cancelled);

		tokenSource.Cancel();

		if (jobStatus.Value.JobStatus == ExportStatus.Failed)
		{
			_logger.LogError($"{jobStatus.Value.ErrorCode} - {jobStatus.Value.ErrorMessage}");
		}

		return jobStatus;
	}
}
