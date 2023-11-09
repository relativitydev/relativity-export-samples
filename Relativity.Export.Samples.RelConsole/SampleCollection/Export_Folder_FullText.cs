using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1.Builders.ExportSettings;
using Relativity.Export.V1.Model.ExportJobSettings;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public partial class BaseExportService
{
	[SampleMetadata(4, nameof(Export_FromFolder_FullText), "Exports Fulltext from folder")]
	public async Task Export_FromFolder_FullText()
	{
		// Your workspace ID.
		// This is where we point to the workspace where we want to export from.
		int workspaceID = 1020245;

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
		string? correlationID = $"Sample-Job-{nameof(Export_FromFolder_FullText)}";

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
			.WithCustomStartAtDocumentNumber(1)
			.Build();

		// Select and assign an order to long text fields that Relativity checks for text when performing an export.
		// You must provide at least one long text field to use this functionality
		// If there won't be a value for a field, the next field in the list will be used
		// Example:
		// 1003668 - Extracted Text
		// 1003677 - Folder Name
		// If there won't be any value for Extracted Text, Folder Name will be used
		List<int> precedenceFieldsArtifactIds = new() { 1003668, 1003677 };

		// Artifact settings
		var artifactSettings = ExportArtifactSettingsBuilder.Create()
			.WithDefaultFileNamePattern()
			.WithoutApplyingFileNamePatternToImages()
			.WithoutExportingImages()
			.ExportFullText(settings => settings.ExportFullTextAsFile()
				.WithTextFileEncoding("UTF-8")
				.WithPrecedenceFieldsArtifactIDs(precedenceFieldsArtifactIds))
			.WithoutExportingNative()
			.WithoutExportingPdf()
			.WithFieldArtifactIDs(new List<int> { 1003676, 1003667 })
			.WithoutExportingMultiChoicesAsNested()
			.Build();

		// Subdirectory settings
		var subdirectorySettings = SubdirectorySettingsBuilder.Create()
			.WithSubdirectoryStartNumber(1)
			.WithMaxNumberOfFilesInDirectory(100)
			.WithDefaultPrefixes()
			.OverridePrefixDefaults(prefixes =>
			{
				prefixes.FullTextSubdirectoryPrefix = "FULLTEXT_";
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

		// Create proxy to use IExportJobManager
		using Relativity.Export.V1.IExportJobManager jobManager = this._serviceFactory.CreateProxy<Relativity.Export.V1.IExportJobManager>();

		OutputHelper.PrintJobJson(jobSettings);

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

		// Start export job
		OutputHelper.PrintLog($"Stating job with <{jobID}> ID");
		var startResponse = await jobManager.StartAsync(workspaceID, jobID);

		// Check for errors that occured during job start
		if (!string.IsNullOrEmpty(startResponse.ErrorMessage))
		{
			OutputHelper.PrintError($"<{startResponse.ErrorCode}> {startResponse.ErrorMessage}");

			return;
		}

		// Get status of the job and await for the completed state
		OutputHelper.PrintLog("Awaiting job status updates");
		var jobResult = await this.WaitForJobToBeCompletedAsync(async () =>
		{
			return await jobManager.GetAsync(workspaceID, jobID);
		});

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
		OutputHelper.PrintExportJobResult(resultData, jobResult);
	}
}