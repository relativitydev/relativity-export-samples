using System.Collections.Concurrent;
using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1.Builders.ExportSettings;
using Relativity.Export.V1.Model;
using Relativity.Export.V1.Model.ExportJobSettings;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public record class RunnableJobResult(string ResultMessage, ExportStatus? ResultStatus);

public partial class BaseExportService
{
	[SampleMetadata(18, nameof(Job_StartAllRunnableJobs), "Starts all runnable export jobs")]
	public async Task Job_StartAllRunnableJobs()
	{
		// Your workspace ID.
		// This is where we point to the workspace where we want to export from.
		int workspaceID = 1020245;

		// Switch to false if you don't want to create jobs for this sample
		bool createJobs = true;

		// Create job manager from service factory
		using Relativity.Export.V1.IExportJobManager jobManager = this._serviceFactory.CreateProxy<Relativity.Export.V1.IExportJobManager>();

		if (createJobs)
		{
			// Create example jobs
			Task[] jobsCreationTasks = new Task[4];
			for (int i = 0; i < jobsCreationTasks.Length; i++)
			{
				OutputHelper.UpdateStatus("Creating new jobs");

				int localScope = i;
				jobsCreationTasks[localScope] = Task.Run(async () =>
				{
					await StartSample_CreateJobAsync(jobManager, workspaceID, localScope);
				});
			}

			// Await for all jobs to be created but not started
			await Task.WhenAll(jobsCreationTasks);
		}

		// Get list of the existing export jobs
		OutputHelper.UpdateStatus("Fetching export jobs list");
		var result = await jobManager.ListAsync(workspaceID, 0, 1000);
		var runnableJobs = result.Value.Jobs.Where(job => job.JobStatus == ExportStatus.New).ToList();

		OutputHelper.PrintLog("Runnable jobs:");
		foreach (var job in runnableJobs)
		{
			string jobDataString = $"Job ID: {job.ID}\n"
				+ $"Application Name: {job.ApplicationName}\n"
				+ $"Job Status: [aquamarine1]{job.JobStatus}[/]";

			OutputHelper.PrintLog(jobDataString);
		}

		// Run all jobs
		var jobResults = await StartSample_StartMultipleJobsAsync(jobManager, runnableJobs, workspaceID);

		var finalMessage = string.Join("\n\n", jobResults.Select(jobResult => jobResult.ResultMessage));
		var jobsStatuses = jobResults.Select(jobResult => jobResult.ResultStatus).ToList();

		OutputHelper.PrintBulkExportJobResult(finalMessage, jobsStatuses);
	}

	private async Task StartSample_CreateJobAsync(Relativity.Export.V1.IExportJobManager jobManager, int workspaceID, int iteration)
	{
		// Your View ID.
		// View will provide us with available data to export, requires folder to be visible there.
		int viewID = 1042326;

		// Your Folder ID.
		// Our targetted folder. If you want to export from the workspace root, 
		// the ID is different from the workspace ID.
		int folderID = 1003697;

		// Job related data
		Guid jobID = Guid.NewGuid();
		string? applicationName = "Export-Service-Sample-App";
		string? correlationID = "Sample-Job-0001";

		OutputHelper.PrintSampleData(new Dictionary<string, string>
		{
			{"Workspace ID", workspaceID.ToString() },
			{"View ID", viewID.ToString() },
			{"Folder ID", folderID.ToString() },
			{"Artifact Type ID", "10" },
			{"Job ID", jobID.ToString() },
			{"Application Name", applicationName },
			{"Correlation ID", correlationID }
		});

		// Export source settings
		var sourceSettings = ExportSourceSettingsBuilder.Create()
			.FromFolder(exportSourceArtifactID: folderID, viewID: viewID)
			.WithSubfolders() // include subfolders
			.WithCustomStartAtDocumentNumber(1)
			.Build();

		// Select and assign an order to long text fields that Relativity checks for text when performing an export.
		// You must provide at least one long text field to use this functionality
		// If there won't be a value for a field, the next field in the list will be used
		// Example:
		// 1003668 - Extracted Text
		// 1003677 - Folder Name
		// If there won't be any value for Extracted Text, Folder Name will be used
		List<int> fulltextPrecedenceFieldsArtifactIds = new() { 1003668, 1003677 };

		// Artifact settings
		var artifactSettings = ExportArtifactSettingsBuilder.Create()
			.WithDefaultFileNamePattern()
			.WithoutApplyingFileNamePatternToImages()
			.ExportImages(settings => settings
				.WithImagePrecedenceArtifactIDs(new List<int> { -1 }) // Exports images
				.WithTypeOfImage(ImageType.Pdf))
			.ExportFullText(settings => settings
				.ExportFullTextAsFile()
				.WithTextFileEncoding("UTF-8")
				.WithPrecedenceFieldsArtifactIDs(fulltextPrecedenceFieldsArtifactIds))
			.ExportNative(settings => settings
				.WithNativePrecedenceArtifactIDs(new List<int> { -1 })) // Exports native files
			.ExportPdf() // Export PDF files
			.WithFieldArtifactIDs(new List<int> { 1003676, 1003667 }) // Fields to export
			.WithoutExportingMultiChoicesAsNested()
			.Build();

		// Subdirectory settings
		var subdirectorySettings = SubdirectorySettingsBuilder.Create()
			.WithSubdirectoryStartNumber(1)
			.WithMaxNumberOfFilesInDirectory(100)
			.WithDefaultPrefixes()
			.OverridePrefixDefaults(prefixes =>
			{
				// Optional overrides
				prefixes.FullTextSubdirectoryPrefix = "FULLTEXT_";
				prefixes.NativeSubdirectoryPrefix = "NATIVE_";
				prefixes.ImageSubdirectoryPrefix = "IMAGE_";
				prefixes.PdfSubdirectoryPrefix = "PDF_";
			})
			.WithSubdirectoryDigitPadding(5)
			.Build();

		// Volume settings
		var volumeSettings = VolumeSettingsBuilder.Create()
			.WithVolumePrefix("VOL_FOLDER_")
			.WithVolumeStartNumber(1)
			.WithVolumeMaxSizeInMegabytes(100)
			.WithVolumeDigitPadding(5)
			.Build();

		// Loadfile settings
		var loadfileSettings = LoadFileSettingsBuilder.Create()
			.WithoutExportingMsAccess()
			.WithoutCustomCultureInfo()
			.WithLoadFileFormat(LoadFileFormat.CSV)
			.WithEncoding("UTF-8")
			.WithImageLoadFileFormat(ImageLoadFileFormat.IPRO)
			.WithPdfFileFormat(PdfLoadFileFormat.IPRO_FullText)
			.WithDelimiterSettings(delimiters =>
				delimiters.WithDefaultDelimiters())
			.Build();

		// Output settings
		var outputSettings = ExportOutputSettingsBuilder.Create()
			.WithoutArchiveCreation()
			.WithDefaultFolderStructure()
			.WithDefaultDestinationPath()
			.WithSubdirectorySettings(subdirectorySettings)
			.WithVolumeSettings(volumeSettings)
			.WithLoadFileSettings(loadfileSettings)
			.Build();

		// Connect all settings in the Job builder
		var jobSettings = ExportJobSettingsBuilder.Create()
			.WithExportSourceSettings(sourceSettings)
			.WithExportArtifactSettings(artifactSettings)
			.WithExportOutputSettings(outputSettings)
			.Build();

		// Create export job
		OutputHelper.PrintLog("Creating job");
		var validationResult = await jobManager.CreateAsync(
			workspaceID,
			jobID,
			jobSettings,
			applicationName,
			correlationID);

		// check validation result
		if (!validationResult.IsSuccess)
		{
			OutputHelper.PrintError($"<{validationResult.ErrorCode}> {validationResult.ErrorMessage}");

			// iterate errors and print them
			foreach (var validationError in validationResult.Value.ValidationErrors)
			{
				OutputHelper.PrintError($"{validationError.Key} - {validationError.Value}");
			}

			return;
		}

		OutputHelper.PrintLog("Job created successfully");
	}

	private async Task<List<RunnableJobResult>> StartSample_StartMultipleJobsAsync(Relativity.Export.V1.IExportJobManager jobManager, List<ExportJob> runnableExportJobs, int workspaceID)
	{
		OutputHelper.UpdateStatus("Starting all runnable jobs");

		// Start all runnable jobs
		ConcurrentBag<RunnableJobResult> jobResults = new();
		Task[] runningJobsTasks = new Task[runnableExportJobs.Count()];

		for (int i = 0; i < runningJobsTasks.Length; i++)
		{
			int localScope = i;
			runningJobsTasks[localScope] = Task.Run(async () =>
			{
				var jobResult = await StartSample_RunJobAsync(jobManager, workspaceID, runnableExportJobs[localScope].ID);
				jobResults.Add(jobResult);
			});
		}

		// Await for all jobs to be completed
		OutputHelper.UpdateStatus("Awaiting for all jobs to be completed");
		await Task.WhenAll(runningJobsTasks);

		return jobResults.ToList();
	}

	private async Task<RunnableJobResult> StartSample_RunJobAsync(Relativity.Export.V1.IExportJobManager jobManager, int workspaceID, Guid jobID)
	{
		// Start export job via job manager
		OutputHelper.PrintLog($"Stating job with <{jobID}> ID");
		var startResponse = await jobManager.StartAsync(workspaceID, jobID);

		// Check for errors that occured during job start
		if (!string.IsNullOrEmpty(startResponse.ErrorMessage))
		{
			OutputHelper.PrintError($"<{startResponse.ErrorCode}> {startResponse.ErrorMessage}");

			return new RunnableJobResult($"<{startResponse.ErrorCode}> {startResponse.ErrorMessage}", null);
		}

		// Get status of the job and await for the completed state
		OutputHelper.PrintLog("Awaiting job status updates");
		var jobResult = await this.WaitForJobToBeCompletedAsync(async () =>
		{
			return await jobManager.GetAsync(workspaceID, jobID);
		}, frequency: 4000);

		string resultData =
				$"Export job ID: {jobResult.ExportJobID}\n"
				+ $"Correlation ID: {jobResult.Value.CorrelationID}\n"
				+ $"Job status: {jobResult.Value.JobStatus}\n"
				+ $"Job error count: {jobResult.Value.JobErrorsCount}\n"
				+ $"Total records: {jobResult.Value.TotalRecords}\n"
				+ $"Processed records: {jobResult.Value.ProcessedRecords}\n"
				+ $"Records with warnings: {jobResult.Value.RecordsWithErrors}\n"
				+ $"Records with errors: {jobResult.Value.RecordsWithErrors}\n"
				+ $"Output URL: [orange1]{jobResult.Value.OutputUrl}[/]";

		OutputHelper.PrintLog("Job Completed");

		return new RunnableJobResult(resultData, jobResult.Value.JobStatus);
	}
}