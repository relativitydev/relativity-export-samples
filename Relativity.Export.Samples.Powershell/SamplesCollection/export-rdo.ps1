. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"
. "$global:rootDir\Helpers\BaseExportService.ps1"

# Your workspace ID: this is where we point to the workspace where we want to export from
[int]$workspaceId = 1022188

# Export settings parameters
# Your artifact type ID: Artifact Type ID of entities to export (example: Artifact Type Id of 'Relativity Time Zone' object type)
# Your view ID: view will provide us with available data to export. (example: ArtifactID of view 'All Time Zones' - a view for object type 'Relativity Time Zone')
[int]$artifactTypeId = 1000025
[int]$viewId = 1037860

# ArtifactIds: ArtifactIds of fields for exported Object Type 'Relativity Time Zone', fields must be in exported view. Example: 1037734 - Name
$fieldArtifactIds = '[1037734]'

# Job related data
$jobId = New-Guid
[string]$applicationName = "Export-Service-Sample-Powershell"
[string]$applicationId = "Sample-Job-" + $MyInvocation.MyCommand.Name.Replace(".ps1", "")

# Export job settings
[string]$exportJobSettings = 
'{
    "settings": {
        "ExportSourceSettings": {
            "ArtifactTypeID":' + $artifactTypeId + ',
            "ExportSourceType":4,
            "ExportSourceArtifactID":1,
            "ViewID":' + $viewId + ',
            "StartAtDocumentNumber":1
        },
        "ExportArtifactSettings": {
            "FileNamePattern":"{identifier}",
            "ExportNative":false,
            "ExportPdf":false,
            "ExportImages":false,
            "ExportFullText":false,
            "ExportMultiChoicesAsNested":true,        
            "FieldArtifactIDs":' + $fieldArtifactIds + ',
            "ApplyFileNamePatternToImages":false
        },
        "ExportOutputSettings": {
            "LoadFileSettings": {
                "LoadFileFormat":"CSV",
                "ImageLoadFileFormat":"IPRO",
                "PdfLoadFileFormat":"Opticon",
                "Encoding":"utf-8",
                "DelimitersSettings": {
                    "NestedValueDelimiter":"B",
                    "RecordDelimiter":"E",
                    "QuoteDelimiter":"D",
                    "NewlineDelimiter":"C",
                    "MultiRecordDelimiter":"A"
                },
                "ExportMsAccess": false,
                "CultureInfo": "en-US"
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

Context "Exports RDO objects" {
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

