class BaseExportService {
    BaseExportService() {

    }

    createExportJob($jobId, $exportJobSettings) {
        Write-Host "Create export job '$jobId'" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportJobCreateUri($jobId)
        $body = $exportJobSettings
        $response = $global:WebRequest.callPost($uri, $body)
        if (!$global:WebRequest.checkIfSuccess($response)) {
            $global:WriteInformation.getSettingValidationErrors($response)
            exit 1
        }

        Write-Host "Export job '$jobId' created" -ForegroundColor Green
    }

    startExportJob($jobId) {
        $global:BaseExportService.startExportJobEx($jobId, $true)
    }

    startExportJobEx($jobId, [bool]$throwFailure = $true) {
        Write-Host "Start export job '$jobId'" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportJobStart($jobId)
        $body = ''
        $response = $global:WebRequest.callPost($uri, $body)
        
        if ($response.IsSuccess) {
            Write-Host "Export job '$jobId' started" -ForegroundColor Green
        }
        else {
            $global:WriteInformation.getStartJobErrors($response)
            if ($throwFailure ) { exit 1 }
        }
    }

    cancelExportJob($jobId) {
        Write-Host "Cancel export job '$jobId'" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportJobCancel($jobId)
        $body = ''
        $response = $global:WebRequest.callPost($uri, $body)
        
        if (!$global:WebRequest.checkIfSuccess($response)) {
            $global:WriteInformation.getCancelJobErrors($response)
        }

        Write-Host "Export job '$jobId' cancelled" -ForegroundColor Green
    }

    getExportJobSettings($jobId) {
        Write-Host "Get export job '$jobId' settings" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportJobSettings($jobId)
        $response = $global:WebRequest.callGet($uri)
        
        if ($response.IsSuccess) {
            $settings = $response.Value | ConvertTo-Json -Depth 10
            Write-Host $settings
        }
        else {
            $errorCode = $response.ErrorCode
            $errorMessage = $response.ErrorMessage
            Write-Host "Get export job '$jobId' settings failed: $errorCode - $errorMessage" -ForegroundColor Red
        }
    }

    getListOfExportJobs() {
        Write-Host "Get export jobs list" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportJobsList(0, 10)
        $response = $global:WebRequest.callGet($uri)
        
        if ($response.IsSuccess) {
            $jobsList = $response.Value | ConvertTo-Json -Depth 10
            Write-Host $jobsList
        }
        else {
            $errorCode = $response.ErrorCode
            $errorMessage = $response.ErrorMessage
            Write-Host "Get export jobs list failed: $errorCode - $errorMessage" -ForegroundColor Red
        }
    }

    waitForExportJobToBeCompleted($jobId) {
        $global:BaseExportService.waitForExportJobToBeCompletedEx($jobId, $true)
    }

    waitForExportJobToBeCompletedEx($jobId, [bool]$throwFailure = $true) {
        Write-Host "Wait for export job '$jobId' to be completed" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportJobGet($jobId)
        $response = $global:WebRequest.callGet($uri)
        $jobState = $global:WriteInformation.getJobState($response)
        $isjobCompleted = $global:WriteInformation.isJobCompleted($jobState)
        
        [int]$sleepTime = 5
        [int]$counter = 0;
        
        while (!$isjobCompleted) {
            Start-Sleep -Seconds $sleepTime
            $response = $global:WebRequest.callGet($uri)
            $jobState = $global:WriteInformation.getJobState($response)
            $isjobCompleted = $global:WriteInformation.isJobCompleted($jobState)
            $time = Get-Date -Format "HH:mm:ss"
            Write-Host "$time : Current job status: $jobState"

            $counter++
            [bool]$jobExecutionNotStarted = ($jobState -eq 'New' -or $jobState -eq 'Scheduled')
            if ($jobExecutionNotStarted -and $counter -ge 60) {
                Write-Host "After more than 5 minutes job is still not being executed" -ForegroundColor DarkRed
                Write-Host "Waiting for job '$jobId' to complete aborted" -ForegroundColor DarkRed
                if ($throwFailure) { 
                    exit 1 
                }
                else {
                    break
                }
            }
        }
        
        if ($isjobCompleted) { Write-Host "Export job '$jobId' in completed state '$jobState'" -ForegroundColor Green }
    }

    exportJobResult($jobId) {
        Write-Host "Export job '$jobId' summary" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportJobGet($jobId)
        $response = $global:WebRequest.callGet($uri)
        $global:WriteInformation.getJobSummary($response)

        if ($global:WriteInformation.hasRecordsWithErrors($response)) {
            $uri = $global:Endpoints.exportJobErrors($jobId)
            $response = $global:WebRequest.callGet($uri)
            $global:WriteInformation.getJobErrors($response)
        }
    }
}