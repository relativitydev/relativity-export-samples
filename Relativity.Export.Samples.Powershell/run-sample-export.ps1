$global:rootDir = "$PSScriptRoot"
. "$global:rootDir\Helpers\AuthClass.ps1"
. "$global:rootDir\Helpers\WebRequestClass.ps1"

$hostAddress = "https://sample-host/"
$userName = "sample@username"
$password = "password!"

$global:Auth = [Auth]::new($hostAddress, $userName, $password)
$global:WebRequest = [WebRequest]::new($global:Auth)

# Uncomment the samples you wish to run:
Describe "Sample export" {
    . "$global:rootDir\SamplesCollection\_export-service-health-check.ps1"
    # . "$global:rootDir\SamplesCollection\export-folder-all.ps1"
    # . "$global:rootDir\SamplesCollection\export-folder-full-text.ps1"
    # . "$global:rootDir\SamplesCollection\export-folder-images.ps1"
    # . "$global:rootDir\SamplesCollection\export-folder-native-files.ps1"
    # . "$global:rootDir\SamplesCollection\export-folder-pdf.ps1"
    # . "$global:rootDir\SamplesCollection\export-production-all.ps1"
    # . "$global:rootDir\SamplesCollection\export-production-full-text.ps1"
    # . "$global:rootDir\SamplesCollection\export-production-images.ps1"
    # . "$global:rootDir\SamplesCollection\export-production-native-files.ps1"
    # . "$global:rootDir\SamplesCollection\export-production-pdf.ps1"
    # . "$global:rootDir\SamplesCollection\export-rdo.ps1"
    # . "$global:rootDir\SamplesCollection\export-saved-search-all.ps1"
    # . "$global:rootDir\SamplesCollection\export-saved-search-full-text.ps1"
    # . "$global:rootDir\SamplesCollection\export-saved-search-images.ps1"
    # . "$global:rootDir\SamplesCollection\export-saved-search-native-files.ps1"
    # . "$global:rootDir\SamplesCollection\export-saved-search-pdf.ps1"
    # . "$global:rootDir\SamplesCollection\job-cancel.ps1"
    # . "$global:rootDir\SamplesCollection\job-get.ps1"
    # . "$global:rootDir\SamplesCollection\job-get-settings.ps1"
    # . "$global:rootDir\SamplesCollection\job-list.ps1"
    # . "$global:rootDir\SamplesCollection\job-start-all-runnable-jobs.ps1"
}