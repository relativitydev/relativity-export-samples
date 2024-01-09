using System.Collections.Concurrent;
using Relativity.Export.Samples.RelConsole.Helpers;
using Relativity.Export.V1.Builders.ExportSettings;
using Relativity.Export.V1.Model;
using Relativity.Export.V1.Model.ExportJobSettings;

namespace Relativity.Export.Samples.RelConsole.SampleCollection;

public partial class BaseExportService
{
	[SampleMetadata(19, nameof(Job_GetSettings), "Gets settings of a selected job")]
	public async Task Job_GetSettings()
	{
		// Your workspace ID.
		// This is where we point to the workspace where we want to export from.
		int workspaceID = 1020245;

		// Guid of the job you want to fetch settings of
		Guid jobID = Guid.NewGuid();

		OutputHelper.UpdateStatus("Preparing job...");

		// Create job manager from service factory
		using Relativity.Export.V1.IExportJobManager jobManager = this._serviceFactory.CreateProxy<Relativity.Export.V1.IExportJobManager>();

		// Create a sample job to fetch settings from
		await GetSettingsSample_CreateJobAsync(jobManager, workspaceID, jobID);

		// Get settings of the job
		OutputHelper.UpdateStatus("Fetching job settings...");
		var settings = await jobManager.GetSettingsAsync(workspaceID, jobID);

		_logger.PrintJobJson(settings.Value, true);
	}

	private async Task GetSettingsSample_CreateJobAsync(Relativity.Export.V1.IExportJobManager jobManager, int workspaceID, Guid jobID)
	{
		// Your View ID.
		// View will provide us with available data to export, requires folder to be visible there.
		int viewID = 1042326;

		// Your Folder ID.
		// Our targetted folder. If you want to export from the workspace root, 
		// the ID is different from the workspace ID.
		int folderID = 1003697;

		// Job related data
		string? applicationName = "Export-Service-Sample-App";
		string? correlationID = "Sample-Job-0001";

		_logger.PrintSampleData(new Dictionary<string, string>
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
			.WithDefaultDateTimeFormat()
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
			.WithoutTransferJobID()
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
	}
}