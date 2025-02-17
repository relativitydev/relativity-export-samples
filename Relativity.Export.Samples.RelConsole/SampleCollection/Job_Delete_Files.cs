using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public partial class BaseExportService
{
	[SampleMetadata(nameof(Job_Delete_Files), "Deletes exported files of specified export job")]
	public async Task Job_Delete_Files()
	{
		// Your workspace ID.
		int workspaceID = 1020245;

		// Job GUID
		Guid jobId = Guid.Parse("00000000-0000-0000-0000-000000000000");

		// Create job manager from service factory
		using IExportJobManager jobManager = _serviceFactory.CreateProxy<IExportJobManager>();

		_logger.LogInformation("Job details before deleting files:");
		await PrintJobDetailsAsync(jobManager, workspaceID, jobId);

		// Delete files of the export job
		OutputHelper.UpdateStatus("Deleting files of the export job");
		var result = await jobManager.DeleteAsync(workspaceID, jobId);

		_logger.LogInformation("Job details after deleting files:");
		await PrintJobDetailsAsync(jobManager, workspaceID, jobId);

		if (result.IsSuccess)
		{
			_logger.LogInformation("Files deleted successfully");
		}
		else
		{
			string errorMessage = $"<{result.ErrorCode}> {result.ErrorMessage}";
			_logger.LogError($"Failed to delete files\n{errorMessage}");
		}
	}

	private async Task PrintJobDetailsAsync(IExportJobManager jobManager, int workspaceID, Guid jobId)
	{
		OutputHelper.UpdateStatus("Fetching job details");
		var job = (await jobManager.GetAsync(workspaceID, jobId)).Value;

		string jobDataString = $"Job ID: {job.ID}\n"
			+ $"Application Name: {job.ApplicationName}\n"
			+ $"Job Status: [aquamarine1]{job.JobStatus}[/]\n"
			+ $"Output URL: [orange1]{job.ExportJobOutput.OutputUrl ?? ""}[/]\n"
			+ $"Is output deleted: [orange1]{job.IsOutputDeleted}[/]\n";

		_logger.PrintExportJobResult(jobDataString, job);

		OutputHelper.ClearStatus();
	}
}