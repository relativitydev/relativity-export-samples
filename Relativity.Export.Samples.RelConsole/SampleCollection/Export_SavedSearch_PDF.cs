using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1.Builders.ExportSettings;
using Relativity.Export.V1.Model.ExportJobSettings;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public partial class BaseExportService
{
	[SampleMetadata(8, nameof(Export_FromSavedSearch_PDF), "Exports PDF files from saved search")]
	public async Task Export_FromSavedSearch_PDF()
	{
		// Your workspace ID.
		// This is where we point to the workspace where we want to export from.
		int workspaceID = 1020245;

		// Your saved search ID.
		int savedSearchID = 1042325;

		// Job related data
		Guid jobID = Guid.NewGuid();
		string? applicationName = "Export-Service-Sample-App";
		string? correlationID = $"Sample-Job-{nameof(Export_FromSavedSearch_PDF)}";

		_logger.PrintSampleData(new Dictionary<string, string>
		{
			{"Workspace ID", workspaceID.ToString() },
			{"Saved Search ID", savedSearchID.ToString() },
			{"Artifact Type ID", "10" },
			{"Job ID", jobID.ToString() },
			{"Application Name", applicationName },
			{"Correlation ID", correlationID }
		});

		// This represents alternative approach of using export SDK builders
		var settingsBuilder = ExportJobSettingsBuilder.Create()
			.WithExportSourceSettings(exportSourceSettings => // Export Source Settings
				exportSourceSettings.FromSavedSearch(exportSourceArtifactID: savedSearchID)
					.WithDefaultStartAtDocumentNumber())
			.WithExportArtifactSettings(artifactSettings => // Artifact Settings
				artifactSettings.WithCustomPatternBuilder('_')
						.AppendIdentifier()
						.AppendCustomText("CustomPatternText")
						.AppendOriginalFileName()
						.BuildPattern() // Ends building the file pattern and applies it to the resulting object
					.WithoutApplyingFileNamePatternToImages()
					.WithoutExportingImages()
					.WithoutExportingFullText()
					.WithoutExportingNative()
					.ExportPdf()
					.WithFieldArtifactIDs(new List<int> { 1003676, 1003667 }) // Fields to export
					.WithoutFieldAliases()
					.ExportMultiChoicesAsNested())
			.WithExportOutputSettings(settings => // Export output settings
				settings.WithoutArchiveCreation()
					.WithDefaultFolderStructure()
					.WithDefaultDestinationPath()
					.WithSubdirectorySettings(subSettings => // Subdirectory settings
						subSettings.WithSubdirectoryStartNumber(1)
							.WithMaxNumberOfFilesInDirectory(10)
							.WithDefaultPrefixes()
							.OverridePrefixDefaults(prefixes =>
							{
								prefixes.ImageSubdirectoryPrefix = "PDF_FILES_";
							})
							.WithSubdirectoryDigitPadding(5))
					.WithVolumeSettings(volumeSettings => // Volume settings
						volumeSettings.WithVolumePrefix("VOL_SEARCH_")
						.WithVolumeStartNumber(1)
						.WithVolumeMaxSizeInMegabytes(100)
						.WithVolumeDigitPadding(5))
					.WithLoadFileSettings(loadFileSettings => // Loadfile settings
						loadFileSettings.WithoutExportingMsAccess()
							.WithoutCustomCultureInfo()
							.WithDefaultDateTimeFormat()
							.WithLoadFileFormat(LoadFileFormat.CSV)
							.WithEncoding("UTF-8")
							.WithImageLoadFileFormat(ImageLoadFileFormat.IPRO)
							.WithPdfFileFormat(PdfLoadFileFormat.IPRO)
							.WithDelimiterSettings(delimiterSettings => // Delimiter settings
								delimiterSettings.WithDefaultDelimiters())));

		// Build settings from builder
		var jobSettings = settingsBuilder.Build();

		// Create proxy to use IExportJobManager
		using Relativity.Export.V1.IExportJobManager jobManager = this._serviceFactory.CreateProxy<Relativity.Export.V1.IExportJobManager>();

		_logger.PrintJobJson(jobSettings);

		// Create export job
		_logger.LogInformation("Creating job");
		var validationResult = await jobManager.CreateAsync(
			workspaceID,
			jobID,
			jobSettings,
			applicationName,
			correlationID);

		if (validationResult is null)
		{
			_logger.LogError("Something went wrong with fetching response");
			return;
		}

		// check validation result
		if (!validationResult.IsSuccess)
		{
			_logger.LogError($"<{validationResult.ErrorCode}> {validationResult.ErrorMessage}");

			// iterate errors and print them
			foreach (var validationError in validationResult.Value.ValidationErrors)
			{
				_logger.LogError($"{validationError.Key} - {validationError.Value}");
			}

			return;
		}

		_logger.LogInformation("Job created successfully");

		// Start export job
		_logger.LogInformation($"Stating job with <{jobID}> ID");
		var startResponse = await jobManager.StartAsync(workspaceID, jobID);

		// Check for errors that occured during job start
		if (!string.IsNullOrEmpty(startResponse.ErrorMessage))
		{
			_logger.LogError($"<{startResponse.ErrorCode}> {startResponse.ErrorMessage}");

			return;
		}

		// Get status of the job and await for the completed state
		_logger.LogInformation("Awaiting job status updates");
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
				+ $"Exported files count: {jobResult.Value.ExportedFilesCount}\n"
				+ $"Total size of exported files: {jobResult.Value.TotalSizeOfExportedFiles}\n"
				+ $"Records with warnings: {jobResult.Value.RecordsWithErrors}\n"
				+ $"Records with errors: {jobResult.Value.RecordsWithErrors}\n"
				+ $"Output URL: [orange1]{jobResult.Value.ExportJobOutput.OutputUrl}[/]";

		_logger.LogInformation("Job Completed");
		_logger.PrintExportJobResult(resultData, jobResult.Value);
	}
}
