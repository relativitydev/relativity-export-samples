. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"
. "$global:rootDir\Helpers\BaseExportService.ps1"

# Your workspace ID: this is where we point to the workspace where we want to export from
[int]$workspaceId = 1022188

# Set false if you don't want to create new jobs for this sample
[bool]$createNewJobs = $true
[int]$numberOfJobs = 4

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

# Create export jobs, get list of export jobs and start all runnable jobs
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()
$global:BaseExportService = [BaseExportService]::new()

Context "Starts all runnable export jobs" {
    $jobList = $null

    if ($createNewJobs) {
        Describe "Create export jobs" {
            for ([int]$i = 0; $i -lt $numberOfJobs; $i++) {
                $jobId = New-Guid
                $global:BaseExportService.createExportJob($jobId, $exportJobSettings)
            }
        }
    }

    Describe "Get list of the existing export jobs with New status and start these jobs" {
        $uri = $global:Endpoints.exportJobsList(0, 1000)
        $response = $global:WebRequest.callGet($uri)
        
        if (!$global:WebRequest.checkIfSuccess($response)) {
            exit 1
        }

        Write-Host "All jobs count: " $response.Value.TotalNumberOfJobs

        $jobList = $response.Value.Jobs | Where-Object JobStatus -eq 'New'
        Write-Host "Runnable jobs count:" $jobList.Count

        $jobList | ForEach-Object {
            $jobId = $_.ID
            Write-Host "Start job: Job ID: $jobId, JobStatus:" $_.JobStatus
            $global:BaseExportService.startExportJobEx($jobId, $false)
            $global:BaseExportService.waitForExportJobToBeCompletedEx($jobId, $false)
        }
    }
}
