. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"
. "$global:rootDir\Helpers\BaseExportService.ps1"

# Your workspace ID: this is where we point to the workspace where we want to export from
[int]$workspaceId = 1022188

# Set false if you don't want to create a new job for this sample. 
[bool]$createNewJob = $true

#  Guid of the job - if creating a new job
$jobId = New-Guid

# Export settings parameters
# Your view ID: view will provide us with available data to export, requires folder to be visible there.
# Your folder ID: our targetted folder. If you want to export from the workspace root, the ID is different from the workspace ID.
[int]$viewId = 1003684
[int]$folderID = 1003697

# ArtifactIds: Example: 1003668 - Extracted Text, 1003677 - Folder Name, 1003676 - Artifact ID, 1003667 - Control Number
$fulltextPrecedenceFieldsArtifactIds = '[1003668,1003677]'
$fieldArtifactIds = '[1003676,1003667]'

# Job related data
[string]$applicationName = "Export-Service-Sample-Powershell"
[string]$applicationId = "Sample-Job-" + $MyInvocation.MyCommand.Name.Replace(".ps1", "")

# Export job settings
$exportJobSettings = $global:WriteInformation.getSampleExportJobSettings($folderID, $viewId, $fulltextPrecedenceFieldsArtifactIds, $fieldArtifactIds, $applicationName, $applicationId)

# List export jobs
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()
$global:BaseExportService = [BaseExportService]::new()

Context "List export jobs" {
    if ($createNewJob) {
        Describe "Create export job" {
            $global:BaseExportService.createExportJob($jobId, $exportJobSettings)
        }
    }

    Describe "List export jobs" {
        $global:BaseExportService.getListOfExportJobs()
    }
}
