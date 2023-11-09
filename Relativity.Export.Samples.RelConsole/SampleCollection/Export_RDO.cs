using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1.Builders.ExportSettings;
using Relativity.Export.V1.Model.ExportJobSettings;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public partial class BaseExportService
{
	[SampleMetadata(16, nameof(Export_RDO), "Exports RDO objects")]
	public async Task Export_RDO()
	{
		// Your workspace ID.
		// This is where we point to the workspace where we want to export from.
		// int workspaceID = 1020245;
		int workspaceID = 6648897;

		// Your Folder ID.
		// Our targetted folder. If you want to export from the workspace root, 
		// the ID is different from the workspace ID.
		// int viewID = 1042316;
		int viewID = 1036791; // Dev view

		// Artifact type ID of entities to export.
		// In this case we export import/export job so we set the ID to 1001057.
		// int artifactTypeID = 1001057;
		int artifactTypeID = 1000022;

		// Job related data
		Guid jobID = Guid.NewGuid();
		string? applicationName = "Export-Service-Sample-App";
		string? correlationID = $"Sample-Job-{nameof(Export_RDO)}";

		OutputHelper.PrintSampleData(new Dictionary<string, string>
		{
			{"Workspace ID", workspaceID.ToString() },
			{"View ID", viewID.ToString() },
			{"Artifact Type ID", artifactTypeID.ToString() },
			{"Job ID", jobID.ToString() },
			{"Application Name", applicationName },
			{"Correlation ID", correlationID }
		});

		// This represents alternative approach of using export SDK builders
		// Export source settings
		Action<IExportSourceSettingsBuilder> sourceSettingsBuilder = (settingsBuilder) =>
		{
			// Set object type ID, Folder ID and View ID
			settingsBuilder.FromObjects(artifactTypeID: artifactTypeID, viewID)
				.WithCustomStartAtDocumentNumber(1);
		};

		// Select and assign an order to long text fields that Relativity checks for text when performing an export.
		// You must provide at least one long text field to use this functionality
		// If there won't be a value for a field, the next field in the list will be used
		// Example:
		// 1003668 - Extracted Text
		// 1003677 - Folder Name
		// If there won't be any value for Extracted Text, Folder Name will be used
		List<int> fulltextPrecedenceFieldsArtifactIds = new() { 1003668, 1003677 };

		// Artifact settings
		Action<IExportArtifactSettingsBuilder> artifactSettingsBuilder = (settingsBuilder) =>
		{
			settingsBuilder.WithDefaultFileNamePattern()
				.WithoutApplyingFileNamePatternToImages()
				.WithoutExportingImages()
				.WithoutExportingFullText()
				.WithoutExportingNative()
				.WithoutExportingPdf() // Export PDF files
				.WithFieldArtifactIDs(new List<int>() { 1036665 }) // Fields to export
				.ExportMultiChoicesAsNested();
		};

		// Subdirectory settings
		Action<ISubdirectorySettingsBuilder> subdirectorySettingsBuilder = (settingsBuilder) =>
		{
			settingsBuilder.WithSubdirectoryStartNumber(1)
				.WithMaxNumberOfFilesInDirectory(100)
				.WithDefaultPrefixes()
				.WithSubdirectoryDigitPadding(5);
		};

		// Volume settings
		Action<IVolumeSettingsBuilder> volumeSettingsBuilder = (settingsBuilder) =>
		{
			settingsBuilder.WithVolumePrefix("VOL_FOLDER_")
				.WithVolumeStartNumber(1)
				.WithVolumeMaxSizeInMegabytes(100)
				.WithVolumeDigitPadding(5);
		};

		// Loadfile settings
		Action<ILoadFileSettingsBuilder> loadFileSettingsBuilder = (settingsBuilder) =>
		{
			settingsBuilder.WithoutExportingMsAccess()
				.WithCustomCultureInfo("en-US")
				.WithLoadFileFormat(LoadFileFormat.CSV)
				.WithEncoding("UTF-8")
				.WithImageLoadFileFormat(ImageLoadFileFormat.IPRO)
				.WithPdfFileFormat(PdfLoadFileFormat.Opticon)
				.WithDelimiterSettings(delimiters =>
					delimiters.WithDefaultDelimiters());
		};

		// Output settings
		Action<IExportOutputSettingsBuilder> outputSettingsBuilder = (settingsBuilder) =>
		{
			settingsBuilder.WithArchiveCreation()
				.WithCustomFolderStructure(FolderStructure.Classic)
				.WithDefaultDestinationPath()
				.WithSubdirectorySettings(subdirectorySettingsBuilder)
				.WithVolumeSettings(volumeSettingsBuilder)
				.WithLoadFileSettings(loadFileSettingsBuilder);
		};

		// Connect all settings in the Job builder
		var settingsBuilder = ExportJobSettingsBuilder.Create()
			.WithExportSourceSettings(sourceSettingsBuilder)
			.WithExportArtifactSettings(artifactSettingsBuilder)
			.WithExportOutputSettings(outputSettingsBuilder);

		// Build settings from builder
		var jobSettings = settingsBuilder.Build();

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