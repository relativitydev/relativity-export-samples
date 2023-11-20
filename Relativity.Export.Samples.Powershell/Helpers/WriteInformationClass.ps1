class WriteInformation {

    WriteInformation() {
    }

    getSettingValidationErrors($response) {
        Write-Host "Settings validation errors:" -ForegroundColor DarkRed
        if ($response) {
            $validationErrors = $response.Value.ValidationErrors  | ConvertTo-Json -Depth 10
            Write-Host $validationErrors -ForegroundColor Red
        }
    }

    getStartJobErrors($response) {
        if ($response) {
            $errorCode = $response.ErrorCode
            $errorMessage = $response.ErrorMessage
            Write-Host "Start job error: $errorCode : $errorMessage" -ForegroundColor Red
        }
    }

    getCancelJobErrors($response) {
        if ($response) {
            $errorCode = $response.ErrorCode
            $errorMessage = $response.ErrorMessage
            Write-Host "Cancel job error: $errorCode : $errorMessage" -ForegroundColor Red
        }
    }

    getJobSummary($response) {
        if ($global:WebRequest.checkIfSuccess($response)) {
            $responseValue = $response.Value | ConvertTo-Json -Depth 10
            Write-Host $responseValue
        }
    }

    [string]getJobState($response) {
        if ($global:WebRequest.checkIfSuccess($response)) {
            if (![string]::IsNullOrEmpty($response.ErrorCode) -or ![string]::IsNullOrEmpty($response.ErrorMessage)) {
                $errorCode = $response.ErrorCode
                $errorMessage = $response.ErrorMessage
                Write-Host "Retrieving job state failed" -ForegroundColor Red
                Write-Host "$errorCode : $errorMessage" -ForegroundColor Red
                return ""
            }
            return $response.Value.jobStatus;
        }
        return ""
    }

    [bool]isJobCompleted($jobStatus) {
        $completedStatuses = 'Completed', 'CompletedWithErrors', 'Cancelled', 'Failed'
        return $completedStatuses -contains $jobStatus
    }

    getJobErrors($response) {
        Write-Host "Get errors:" -ForegroundColor DarkRed
        if ($global:WebRequest.checkIfSuccess($response)) {
            [int]$totalNumberOfErros = $response.Value.TotalNumberOfErrors
            Write-Host "Item level errors count: $totalNumberOfErros" -ForegroundColor DarkMagenta

            if ($totalNumberOfErros -gt 0) {
                $errors = $response.Value.Errors | ConvertTo-Json -Depth 10
                Write-Host $errors
            }
        }
    }

    [bool]hasRecordsWithErrors($response) {
        if ($global:WebRequest.checkIfSuccess($response)) {
            return ($response.Value.RecordsWithErrors -gt 0)
        }
        return $false
    }

    [string]getSampleExportJobSettings([string]$folderID, [string]$viewId, [string]$fulltextPrecedenceFieldsArtifactIds, [string]$fieldArtifactIds, [string]$applicationName, [string]$applicationId) {
        return '{
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
    }
}