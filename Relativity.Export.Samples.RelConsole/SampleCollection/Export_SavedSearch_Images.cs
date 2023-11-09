using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1.Builders.ExportSettings;
using Relativity.Export.V1.Model.ExportJobSettings;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public partial class BaseExportService
{
	[SampleMetadata(7, nameof(Export_FromSavedSearch_Images), "Exports images from saved search")]
	public async Task Export_FromSavedSearch_Images()
	{
		// Your workspace ID.
		// This is where we point to the workspace where we want to export from.
		int workspaceID = 1020245;

		// Your saved search ID.
		int savedSearchID = 1042325;

		// Job related data
		Guid jobID = Guid.NewGuid();
		string? applicationName = "Export-Service-Sample-App";
		string? correlationID = $"Sample-Job-{nameof(Export_FromSavedSearch_Images)}";

		OutputHelper.PrintSampleData(new Dictionary<string, string>
		{
			{"Workspace ID", workspaceID.ToString() },
			{"Saved Search ID", savedSearchID.ToString() },
			{"Artifact Type ID", "10" },
			{"Job ID", jobID.ToString() },
			{"Application Name", applicationName },
			{"Correlation ID", correlationID }
		});

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
					.ExportImages(settings => settings.WithImagePrecedenceArtifactIDs(new List<int> { -1 }) // exports only images
						.WithTypeOfImage(ImageType.Pdf))
					.WithoutExportingFullText()
					.WithoutExportingNative()
					.WithoutExportingPdf()
					.WithFieldArtifactIDs(new List<int> { 1003676, 1003667 }) // Fields to export
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
								prefixes.ImageSubdirectoryPrefix = "IMG_";
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
