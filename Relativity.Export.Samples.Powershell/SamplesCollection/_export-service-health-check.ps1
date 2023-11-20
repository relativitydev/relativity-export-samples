# export
. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"

$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()

Context "Exports health check" {
    Describe "health check" {
        Write-Host "Check if Export.Service application is alive" -ForegroundColor Cyan
        $uri = $global:Endpoints.exportHealthCheck()
        $response = $global:WebRequest.callGet($uri)
        $global:WebRequest.checkIfSuccess($response)
        Write-Host "Export.Service is healthy" -ForegroundColor Green
    }
}