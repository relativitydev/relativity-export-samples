. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"
. "$global:rootDir\Helpers\BaseExportService.ps1"

# Your workspace ID: this is where we point to the workspace where we want to export from
[int]$workspaceId = 1022188

# Set false if you want to cancel already existing job and then set $jobId. 
# Example $jobId = "06210047-c819-424c-8bd4-4807cf574cf5"
[bool]$createNewJob = $true

#  Guid of the job you want to cancel
$jobId = New-Guid

# Export settings parameters
# Your view ID: view will provide us with available data to export, requires folder to be visible there.
# Your folder ID: our targetted folder. If you want to export from the workspace root, the ID is different from the workspace ID.
[int]$viewId = 1003684
[int]$folderID = 1003697

# ArtifactIds: Example: 1003676 - Artifact ID, 1003667 - Control Number
$fieldArtifactIds = '[1003676,1003667]'

# Job related data
[string]$applicationName = "Export-Service-Sample-Powershell"
[string]$applicationId = "Sample-Job-" + $MyInvocation.MyCommand.Name.Replace(".ps1", "")

# Export job settings
$exportJobSettings = '{
    "settings": {
        "ExportSourceSettings": {
            "ArtifactTypeID":10,
            "ExportSourceType":3,
            "ExportSourceArtifactID":' + $folderID + ',
            "ViewID":' + $viewId + ',
            "StartAtDocumentNumber":1
        },
        "ExportArtifactSettings": {
            "FileNamePattern":"{identifier}",
            "ExportNative":false,
            "ExportPdf":true,
            "ExportImages":false,
            "ExportFullText":false,
            "ExportMultiChoicesAsNested":false,        
            "FieldArtifactIDs":' + $fieldArtifactIds + ',
            "ApplyFileNamePatternToImages":false
        },
        "ExportOutputSettings": {
            "LoadFileSettings": {
                "LoadFileFormat":"CSV",
                "ImageLoadFileFormat":"IPRO",
                "PdfLoadFileFormat":"IPRO_FullText",
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

# Create and run export job to cancel it
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()
$global:BaseExportService = [BaseExportService]::new()

Context "Cancel running export job" {
    if ($createNewJob) {
        Describe "Create export job" {
            $global:BaseExportService.createExportJob($jobId, $exportJobSettings)
        }
    
        Describe "Start export job" {
            $global:BaseExportService.startExportJob($jobId)
        }
    }
    
    Describe "Cancel export job" {
        # Job must be in running state to be canceled
        $global:BaseExportService.cancelExportJob($jobId)
    }
}
