. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"
. "$global:rootDir\Helpers\BaseExportService.ps1"

# Your workspace ID: this is where we point to the workspace where we want to export from
[int]$workspaceId = 1022188

# Export settings parameters
# Your view ID: view will provide us with available data to export, requires folder to be visible there.
# Your folder ID: our targetted folder. If you want to export from the workspace root, the ID is different from the workspace ID.
# ExportSourceType: ExportFolder or ExportFolderWithSubfolders
[int]$viewId = 1003684
[int]$folderID = 1003697
[bool]$withSubfolders = $true

# ArtifactIds: Example: 1003668 - Extracted Text, 1003677 - Folder Name, 1003676 - Artifact ID, 1003667 - Control Number
$fulltextPrecedenceFieldsArtifactIds = '[1003668,1003677]'
$fieldArtifactIds = '[1003676,1003667]'

# Job related data
$jobId = New-Guid
[string]$applicationName = "Export-Service-Sample-Powershell"
[string]$applicationId = "Sample-Job-" + $MyInvocation.MyCommand.Name.Replace(".ps1", "")
[int]$exportSourceType = if ($withSubfolders) { 3 } else { 2 }

# Export job settings
[string]$exportJobSettings = 
'{
    "settings": {
        "ExportSourceSettings": {
            "ArtifactTypeID":10,
            "ExportSourceType":' + $exportSourceType + ',
            "ExportSourceArtifactID":' + $folderID + ',
            "ViewID":' + $viewId + ',
            "StartAtDocumentNumber":1
        },
        "ExportArtifactSettings": {
            "FileNamePattern":"{identifier}",
            "ExportNative":true,
            "ExportPdf":true,
            "ExportImages":true,
            "ExportFullText":true,
            "ExportMultiChoicesAsNested":false,
            "ImageExportSettings": {
                "ImagePrecedenceArtifactIDs":[-1],
                "TypeOfImage":2,
            },         
            "FullTextExportSettings": {
                "ExportFullTextAsFile":true,
                "TextFileEncoding":"utf-8",
                "PrecedenceFieldsArtifactIDs":' + $fulltextPrecedenceFieldsArtifactIds + '
            },
            "NativeFilesExportSettings": {
                "NativePrecedenceArtifactIDs":[-1]
            },
            "FieldArtifactIDs":' + $fieldArtifactIds + ',
            "ApplyFileNamePatternToImages":false
        },
        "ExportOutputSettings": {
            "LoadFileSettings": {
                "LoadFileFormat":"CSV",
				"LoadfileNameFormat":"WithDataSourceNameAndDate",
                "ImageLoadFileFormat":"IPRO",
                "PdfLoadFileFormat":"IPRO",
                "Encoding":"utf-8",
                "DelimitersSettings": {
                    "NestedValueDelimiter":"B",
                    "RecordDelimiter":"E",
                    "QuoteDelimiter":"D",
                    "NewlineDelimiter":"C",
                    "MultiRecordDelimiter":"A"
                },
                "ExportMsAccess": false
            },
            "VolumeSettings": {
                "VolumePrefix":"VOL_FOLDER",
                "VolumeStartNumber":"1",
                "VolumeMaxSizeInMegabytes":100,
                "VolumeDigitPadding":5
            },
            "SubdirectorySettings": {
                "SubdirectoryStartNumber":1,
                "MaxNumberOfFilesInDirectory":100,
                "ImageSubdirectoryPrefix":"IMAGE_",
                "NativeSubdirectoryPrefix":"NATIVE_",
                "FullTextSubdirectoryPrefix":"FULLTEXT_",
                "PdfSubdirectoryPrefix":"PDFS_",
                "SubdirectoryDigitPadding":5
            },
            "CreateArchive":false,
            "FolderStructure":0
        }
    },
    "applicationName":"' + $applicationName + '",
    "correlationID":"' + $applicationId + '"
}'

# Create, run export job and display export job result
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()
$global:BaseExportService = [BaseExportService]::new()

Context "Exports native, fulltext, images and PDF files from folder" {
    Describe "Create export job" {
        $global:BaseExportService.createExportJob($jobId, $exportJobSettings)
    }

    Describe "Start export job" {
        $global:BaseExportService.startExportJob($jobId)
    }

    Describe "Wait for export job to be completed" {
        $global:BaseExportService.waitForExportJobToBeCompleted($jobId)
    }

    Describe "Export job summary" {
        $global:BaseExportService.exportJobResult($jobId)
    }
}

