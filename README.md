<p align="center">
    <img align="center" src="./Relativity.Export.Samples.RelConsole//Helpers/logo.png" />
<p>

# Relativity Export SDK
## Table of contents
### [Introduction](#introduction)
- [Prerequisites](#prerequisites)
- [Gloassary](#glossary)
  
### [Getting Started](#getting-started)
- [Http Clients](#http-clients)
- [Kepler .NET Client](#kepler-net-client)
- [NuGet](#relativityexportsdk-nuget)
- [Authorization](#authorization)
- [Permissions](#permissions)
- [Builders](#builders)
- [General flow description](#general-flow-description)
- [Example of simple export job flow](#example-of-simple-export-job-flow)
- [Export Job States](#export-job-states)
- [Error Codes](#error-codes)
- [Export Job States](#export-job-states)
  
### Code Samples
- [List of samples](#list-of-samples)
- [Kepler samples](#kepler-samples)
- [Powershell samples](#powershell-samples)

# Introduction
The *Relativity Export Service API* is a service designed to facilitate the efficient export of documents, images, PDFs, native files, and Relativity Dynamic Objects (RDOs) from within Relativity workspaces. 

This service leverages the RESTful API architecture to streamline the creation, configuration, and execution of export jobs, allowing for a seamless data transfer experience.

With its advanced job configuration options, the API offers a high degree of flexibility, enabling users to tailor the export process to specific requirements. 

Moreover, the service incorporates error handling mechanisms to swiftly pinpoint and address any issues that may arise during the export process, ensuring a smooth and reliable operation.

## Prerequisites
1. The following Relativity applications must be installed:
   | Application | GUID | Location |
   | --- | --- | --- |
   | **Export** | 4abc11b0-b3c7-4508-87f8-308185423caf | workspace |
   | **DataTransfer.Legacy** | 9f9d45ff-5dcd-462d-996d-b9033ea8cfce | instance |

2. [Appropriate user permissions need to be set.](https://github.com/relativitydev/relativity-export-samples/tree/main#permissions)
3. For .NET Kepler Client install these nugets:
   - [Relativity.Export.SDK](https://www.nuget.org/packages/Relativity.Export.SDK)
   - [Relativity.Kepler.Client.SDK](https://www.nuget.org/packages/Relativity.Kepler.Client.SDK/) - for .NET Kepler Client

## Glossary
**Data exporting** - Functionality that extracts data from selected workspace.

**ExportJobSettings** - Configuration which decides about export behavior e.g. export source, items to export, structure of exported files, etc.

**ExportJob** - Contains data about the job current state and progress. It is created during export job creation and is updated during export process.

**Kepler service** - API service created but using the Relativity Kepler framework. This framework provides you with the ability to build custom REST Endpoints via a .NET interface. Additionally, the Kepler framework includes a client proxy that you can use when interacting with the services through .NET. <br>
[See more information](https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Kepler_framework.htm#Client-s)

**Item Error** - An error that may occur during the export process and concerns only one exported record from job.

# Getting started
Export Service is built as a standard Relativity Kepler Service. It provides sets of endpoints that must be called sequentially in order to execute export. The following sections outline how to make calls to export service.

## HTTP Clients
You can make calls to a export service using any standard REST or HTTP client, because all APIs (Keplers APIs) are exposed over the HTTP protocol. You need to set the required X-CSRF-Header.<br>
[More details](https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Kepler_framework.htm#Client-s)

```cs
    string usernamePassword = string.Format("{0}:{1}", username, password);
    string base64usernamePassword = Convert.ToBase64String(Encoding.ASCII.GetBytes(usernamePassword));

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("X-CSRF-Header", "-");

    // Basic authentication
    client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64usernamePassword);

    var createExportUri = $"{relativityUrl}/export/v1/workspaces/{workspaceId}/jobs/{jobId}";
    
    var response = await httpClient.PostAsJsonAsync(createExportUri, payload);
```

## Kepler .NET Client

You can access Kepler service from any .NET language using the client library provided as part of the Kepler framework. It exposes a factory class that you can use to create the client proxy by passing URIs to export services and credentials. Then use .NET proxy to interact with a export service as a set of .NET objects. When you call a member method, the proxy makes a corresponding HTTP request to the respective service endpoint.<br>
[More details](https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Kepler_framework.htm#Client-s)

> Example of factory creation:<br>
(Using `Relativity.Kepler.Client.SDK` package)

```cs
public IServiceFactory GetServiceFactory()
{
    Uri relativityRestUri = new Uri($"{this._host}relativity.rest/api");
    Credentials credentials = new UsernamePasswordCredentials(this._username, this._password);

    ServiceFactorySettings settings = new ServiceFactorySettings(relativityRestUri, credentials);

    // Create proxy factory.
    return new ServiceFactory(settings);
}
```

> Example of proxy creation:

```cs
// Get the instance of the service factory.
IServicefactory factory = GetServiceFactory();

// Use the factory to create an instance of the proxy.
using Relativity.Export.V1.IExportJobManager jobManager = factory.CreateProxy<Relativity.Export.V1.IExportJobManager>();

// Use the proxy to call the service.
var result = await jobManager.CreateAsync(
    workspaceID,
    jobID,
    jobSettings,
    applicationName,
    correlationID);
```

Kepler contracts for export service are exposed in `Relativity.Export.SDK` package.

You can also find factory implementations in the `Relativity.Kepler.Client.SDK` package

## Relativity.Export.SDK NuGet
`Relativity.Export.SDK` is a .NET library that contains kepler interfaces for export service. <br>
It provides and simplifies executing export in client application. `Relativity.Export.SDK` targets `.NET Framework 4.6.2` and `.NET standard 2.0`.

![Version](https://img.shields.io/nuget/v/Relativity.Export.SDK?link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRelativity.Export.SDK)
![Nuget](https://img.shields.io/nuget/dt/Relativity.Export.SDK?link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRelativity.Export.SDK)
### Installing via NuGet
```
Install-Package Relativity.Export.SDK
```

### Installing via .NET CLI
```
dotnet add package Relativity.Export.SDK
```

## Relativity.Kepler.Client.SDK NuGet
Public Relativity Kepler Client SDK. <br>
Contains implementation of factories for kepler services and proxies.

![Version](https://img.shields.io/nuget/v/Relativity.Kepler.Client.SDK?link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FRelativity.Kepler.Client.SDK)
![Downloads](https://img.shields.io/nuget/dt/Relativity.Kepler.Client.SDK)

### Installing via NuGet
```
Install-Package Relativity.Kepler.Client.SDK
```

### Installing via .NET CLI
```
dotnet add package Relativity.Kepler.Client.SDK
```

## Authorization
> HTTP clients

Export Service API conforms to the same authentication rules as other Relativity REST APIs.<br>
The more details can be found under the following link: [REST API Authentication](https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm)

> Kepler .NET client

The Kepler framework uses a proxy to handle client requests. The more details can be found under the following link: [Proxies And Authentication](https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Proxies_and_authentication.htm#Service)

## Permissions
The following Relativity permissions are required to use export features provided in Export Service API.

| Object Security Section | Permissions |
| --- | --- |
| Production | `View` |
| Relativity Export Service Job | `Add` `Edit` `View` |

| Tab Visibility |
| --- |
| - |

| Admin Operation |
| --- |
| Allow Export |

## Builders
Builders provided in `Relativity.Export.SDK` package help to create settings for export job in correct and consistent way. It is highly recommended to prepare these objects in such a way in .NET application. They are implemented in fluent api pattern so it is very easy to use them. Moreover, using them in client application will avoid the risk of incorrect and inconsistent configuration which may lead to errors during export process.

Each builder final step results in `IFinalStep<>` generic interface which is used to build final object via `Build()` method. 

Main components of export job settings are:
- `ExportSourceSettings` - contains information about source of data to export
- `ExportArtifactSettings` - contains information about artifacts to export
- `ExportOutputSettings` - contains information about output format and structure of exported files
  
which are all later combined into `ExportJobSettings`.

The following example shows how to create export job settings using builders that exports native, fulltext, images and PDF files from folder.

### Example of ExportSourceSettings
```cs
// Export source settings
var sourceSettings = ExportSourceSettingsBuilder.Create()
    .FromFolder(exportSourceArtifactID: folderID, viewID: viewID)
    .WithSubfolders() // include subfolders
    .WithCustomStartAtDocumentNumber(1)
    .Build();
```

### Example of ExportArtifactSettings
```cs
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
```

### Example of ExportOutputSettings
```cs
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
    .WithDefaultDestinationPath()
    .WithSubdirectorySettings(subdirectorySettings)
    .WithVolumeSettings(volumeSettings)
    .WithLoadFileSettings(loadfileSettings)
    .Build();
```

### Combined settings
```cs
// Export job settings
var jobSettings = ExportJobSettingsBuilder.Create()
    .WithExportSourceSettings(sourceSettings)
    .WithExportArtifactSettings(artifactSettings)
    .WithExportOutputSettings(outputSettings)
    .Build();
```

It's possible to use builders in a method chaining manner to create settings, but it's less readable and more error prone.

> Example
```cs
var jobSettings = ExportJobSettingsBuilder.Create()
    .WithExportSourceSettings(exportSourceSettings => // Export Source Settings
        exportSourceSettings.FromSavedSearch(exportSourceArtifactID: savedSearchID)
            .WithDefaultStartAtDocumentNumber())
    .WithExportArtifactSettings(artifactSettings => 
    // ...
    .Build();
```
	

## General flow description
1. **Create Export Job** <br>
Creates export job entity in particular workspace. Job is defined by its unique Id generated by user and provided in the request which is used in the next steps.

2. **Start Export Job** <br>
Starts Export Job which enables the process that schedules export data from workspace based on the configuration assigned in previous step.

3. **(Optional) Cancel Export Job** <br>
If the job is running, you can cancel it.

4. **Check Export Job status** <br>
You can check the status of the export job, it will indicate whenever it failed, was completed or is still running.

## Example of simple export job flow
Exporting native files from folder.
1. Create configuration
> C# Builders
```cs
// Workspace ID.
int workspaceID = 1020245;

// View ID.
int viewID = 1042326;

// Folder ID.
int folderID = 1003697;

// Job related data
Guid jobID = Guid.NewGuid();
string? applicationName = "Export-Service-Sample-App";
string? correlationID = "Sample-Job";

// Export source settings
var sourceSettings = ExportSourceSettingsBuilder.Create()
    .FromFolder(exportSourceArtifactID: folderID, viewID: viewID)
    .WithCustomStartAtDocumentNumber(1)
    .Build();

// Artifact settings
var artifactSettings = ExportArtifactSettingsBuilder.Create()
    .WithDefaultFileNamePattern()
    .WithoutApplyingFileNamePatternToImages()
    .WithoutExportingImages()
    .WithoutExportingFullText()
    .ExportNative(settings => settings.WithNativePrecedenceArtifactIDs(new List<int> { -1 })) // Exports only native files
    .WithoutExportingPdf()
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
        prefixes.NativeSubdirectoryPrefix = "Native_";
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
```

> C#
```cs
// Workspace ID.
int workspaceID = 1020245;

// View ID.
int viewID = 1042326;

// Folder ID.
int folderID = 1003697;

// Job related data
Guid jobID = Guid.NewGuid();
string? applicationName = "Export-Service-Sample-App";
string? correlationID = "Sample-Job";

var sourceSettings = new ExportSourceSettings()
{
    ArtifactTypeID = 10,
    ExportSourceArtifactID = folderID,
    ExportSourceType = ExportSourceType.Folder,
    ViewID = viewID,
    StartAtDocumentNumber = 1
};

var artifactSettings = new ExportArtifactSettings()
{
    FileNamePattern = "{identifier}",
    ApplyFileNamePatternToImages = false,
    ExportFullText = false,
    ExportImages = false,
    ExportNative = true,
    ExportPdf = false,
    FieldArtifactIDs = new List<int>() { 1003676, 1003667 },
    NativeFilesExportSettings = new NativeFilesExportSettings()
    {
        NativePrecedenceArtifactIDs = new List<int>() { -1 }
    },
    ExportMultiChoicesAsNested = false
};

var subdirectorySettings = new SubdirectorySettings()
{
    SubdirectoryStartNumber = 1,
    MaxNumberOfFilesInDirectory = 100,
    FullTextSubdirectoryPrefix = String.Empty,
    ImageSubdirectoryPrefix = String.Empty,
    NativeSubdirectoryPrefix = "Native_",
    PdfSubdirectoryPrefix = String.Empty,
    SubdirectoryDigitPadding = 5
};

var volumeSettings = new VolumeSettings()
{
    VolumePrefix = "VOL_FOLDER_",
    VolumeStartNumber = 1,
    VolumeMaxSizeInMegabytes = 100,
    VolumeDigitPadding = 5
};

var loadfileSettings = new LoadFileSettings()
{
    ExportMsAccess = false,
    CultureInfo = "en-US",
    LoadFileFormat = LoadFileFormat.CSV,
    Encoding = "UTF-8",
    ImageLoadFileFormat = ImageLoadFileFormat.IPRO,
    PdfLoadFileFormat = PdfLoadFileFormat.IPRO_FullText,
    DelimitersSettings = new DelimitersSettings()
    {
        MultiRecordDelimiter = (char)059,
        NestedValueDelimiter = (char)092,
        NewlineDelimiter = (char)174,
        QuoteDelimiter = (char)254,
        RecordDelimiter = (char)020
    }
};

var outputSettings = new ExportOutputSettings()
{
    CreateArchive = false,
    SubdirectorySettings = subdirectorySettings,
    LoadFileSettings = loadfileSettings,
    VolumeSettings = volumeSettings
};

var jobSettings = new ExportJobSettings()
{
    ExportArtifactSettings = artifactSettings,
    ExportOutputSettings = outputSettings,
    ExportSourceSettings = sourceSettings
};
```

> JSON
```json
{                                               
   "ExportSourceSettings": {                    
      "ArtifactTypeID": 10,                     
      "ExportSourceType": 2,                    
      "ExportSourceArtifactID": 1003697,        
      "ViewID": 1042326,                        
      "StartAtDocumentNumber": 1                
   },                                           
   "ExportArtifactSettings": {                  
      "FileNamePattern": "{identifier}",        
      "ApplyFileNamePatternToImages": false,    
      "ExportNative": true,                     
      "ExportPdf": false,                       
      "ExportImages": false,                    
      "ExportFullText": false,                  
      "ExportMultiChoicesAsNested": false,      
      "ImageExportSettings": null,              
      "FullTextExportSettings": null,           
      "NativeFilesExportSettings": {            
         "NativePrecedenceArtifactIDs": [       
            -1                                  
         ]                                      
      },                                        
      "FieldArtifactIDs": [                     
         1003676,                               
         1003667                                
      ]                                         
   },                                           
   "ExportOutputSettings": {                    
      "LoadFileSettings": {                     
         "LoadFileFormat": 1,                   
         "ImageLoadFileFormat": 1,              
         "PdfLoadFileFormat": 2,                
         "Encoding": "UTF-8",                   
         "DelimitersSettings": {                
            "NestedValueDelimiter": "\\",       
            "RecordDelimiter": "\u0014",        
            "QuoteDelimiter": "\u00FE",         
            "NewlineDelimiter": "\u00AE",       
            "MultiRecordDelimiter": ";"         
         },                                     
         "ExportMsAccess": false,               
         "CultureInfo": null                    
      },                                        
      "VolumeSettings": {                       
         "VolumePrefix": "VOL_FOLDER_",         
         "VolumeStartNumber": 1,                
         "VolumeMaxSizeInMegabytes": 100,       
         "VolumeDigitPadding": 5                
      },                                        
      "SubdirectorySettings": {                 
         "SubdirectoryStartNumber": 1,          
         "MaxNumberOfFilesInDirectory": 100,    
         "ImageSubdirectoryPrefix": "",         
         "NativeSubdirectoryPrefix": "Native_", 
         "FullTextSubdirectoryPrefix": "",      
         "PdfSubdirectoryPrefix": "",           
         "SubdirectoryDigitPadding": 5          
      },                                        
      "CreateArchive": false,                   
      "FolderStructure": 0,                     
      "DestinationPath": null                   
   }                                            
}                                               
```

2. Create export job
> .NET Kepler
```cs
using Relativity.Export.V1.IExportJobManager jobManager = serviceFactory.CreateProxy<Relativity.Export.V1.IExportJobManager>();

var validationResult = await jobManager.CreateAsync(
    workspaceID,
    jobID,
    jobSettings,
    applicationName,
    correlationID);
```
3. Start export job
> .NET Kepler
```cs
// Start export job
var startResponse = await jobManager.StartAsync(workspaceID, jobID);

// Check for errors that occured during job start
if (!string.IsNullOrEmpty(startResponse.ErrorMessage))
{
    _logger.LogError($"<{startResponse.ErrorCode}> {startResponse.ErrorMessage}");
    // ...
}
```

4. Check export job status
> .NET Kepler
```cs
do
{
    var status = await jobManager.GetAsync(workspaceID, jobID);
    Console.WriteLine($"Job status: {status.Value.JobStatus}");
} while (jobStatus?.Value.JobStatus is not ExportStatus.Completed
    and not ExportStatus.CompletedWithErrors
    and not ExportStatus.Failed
    and not ExportStatus.Cancelled);
```



## Export Job States
| Value | State | Description |
| --- | --- | --- |
| 0 | New | Export job created but not started yet. |
| 1 | Scheduled | Export job scheduled and waiting for an agent. |
| 2 | Running | Job executing, export of data is currently in progress. |
| 3 | Completed | Export job completed. All records processed without errors. |
| 4 | CompletedWithErrors | Export job completed with some errors. All records processed but one or more item level errors occurred. |
| 5 | Failed | Export job failed with a fatal error. Not all records were processed. |
| 6 | Cancelled | Job cancelled by user. |
| 7 | Transferring | Export from Relativity to Transfer Service location completed. The transfer job is in progress, and export results are syncing to the destination location. |

## Error Codes
Error handling in Export Service returns Error Codes and Error Messages:

- in every response for failed HTTP request
- when requested by user for all item errors that occurred during export of particular data source
  
### Error code structure
Error code structure
Error code returned from the Export Service API endpoint has the following structure:

**`[Source].[ErrorType].[Operation].[ErrorNumber]`**

Examples:

| Error code | Description |
| --- | --- |
| JOB.01 | Incorrect total items processed number |

> Sources

| Source | Description |
| --- | --- |
| JOB | Jobs |
| CONF | Configuration |
| EXT | External |
| ITEM | Item |
| MES | Message |
| IO | Input/Output |

> Error Types

| Type | Description |
| --- | --- |
| GET | Receiving data |
| CRE | Creatiing |
| IMG | Image file |
| PDF | PDF file |
| MSA | MsAccess |
| PRO | Production |
| LFL | Main load file |
| SCH | Scheduling |
| TS | Transfer service |
| OM | Object manager |
| SQL | Structured Query Language |
| FS | Folder service |
| DG | Data grid |
| DATA | Corrupted data |
| VLD | Validation |

> Operations

| Operation | Description |
| --- | --- |
| GET | Get |
| COPY | Copy |
| SAVE | Save | 
| DELETE | Delete |
| CHECK | Check |

# Samples
There are two types of sample application that demonstrate the use of Export Service API features.

- `RelConsole` - .NET console application (.NET 6, C#, Kepler Client).
- `Powershell` - Powershell scripts.

## List of samples

| Sample Name | .NET & Kepler | Powershell |
| --- | --- | --- |
| Export_FromFolder_NativeFiles | [Sample 1](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Folder_NativeFiles.cs) | [Sample](placeholder) |
| Export_FromFolder_Images | [Sample 2](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Folder_Images.cs) | [Sample](placeholder) |
| Export_FromFolder_PDF | [Sample 3](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Folder_PDF.cs) | [Sample](placeholder) |
| Export_FromFolder_FullText | [Sample 4](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Folder_FullText.cs) | [Sample](placeholder) |
| Export_FromFolder_All | [Sample 5](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Folder_All.cs) | [Sample](placeholder) |
| Export_FromSavedSearch_NativeFiles | [Sample 6](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_SavedSearch_NativeFiles.cs) | [Sample](placeholder) |
| Export_FromSavedSearch_Images | [Sample 7](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_SavedSearch_Images.cs) | [Sample](placeholder) |
| Export_FromSavedSearch_PDF | [Sample 8](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_SavedSearch_PDF.cs) | [Sample]() |
| Export_FromSavedSearch_FullText | [Sample 9](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_SavedSearch_FullText.cs) | [Sample](placeholder) |
| Export_FromSavedSearch_All | [Sample 10](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_SavedSearch_All.cs) | [Sample](placeholder) |
| Export_FromProduction_NativeFiles | [Sample 11](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Production_NativeFiles.cs) | [Sample](placeholder) |
| Export_FromProduction_Images | [Sample 12](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Production_Images.cs) | [Sample](placeholder) |
| Export_FromProduction_PDF | [Sample 13](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Production_PDF.cs) | [Sample](placeholder) |
| Export_FromProduction_Fulltext | [Sample 14](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Production_FullText.cs) | [Sample](placeholder) |
| Export_FromProduction_All | [Sample 15](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Production_All.cs) | [Sample](placeholder) |
| Export_RDO | [Sample 16](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_RDO.cs) | [Sample](placeholder) |
| Job_ListExportJobs | [Sample 17](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Job_List.cs) | [Sample](placeholder) |
| Job_StartAllRunnableJobs | [Sample 18](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Job_StartAllRunnableJobs.cs) | [Sample](placeholder) |
| Job_GetSettings | [Sample 19](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Job_GetSettings.cs) | [Sample](placeholder) |
| Job_Cancel | [Sample 20](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Job_Cancel.cs) | [Sample](placeholder) |

## Kepler Samples
To run the sample code:
1. Replace the variables with your url and credentials in `Program.cs`.
```cs
using System.Text;
using Relativity.Export.Samples.RelConsole.Helpers;

System.Console.OutputEncoding = Encoding.UTF8;
System.Console.InputEncoding = Encoding.UTF8;

// Replace with your Relativity instance url
string relativityUrl = "http://host/";

// Replace with your Relativity credentials
string username = "username";
string password = "password";

await OutputHelper.StartAsync(args, relativityUrl, username, password);
```
2. Replace the required variables within the samples<br>
   For example in [Export_FromFolder_NativeFiles](https://github.com/relativitydev/relativity-export-samples/blob/main/Relativity.Export.Samples.RelConsole/SampleCollection/Export_Folder_NativeFiles.cs) sample replace 
```cs
// ...
// with your workspace ID
int workspaceID = 1020245;
// with your view ID
int viewID = 1042326;
// with your folder ID
int folderID = 1003697;
// with your field artifact IDs
    .WithFieldArtifactIDs(new List<int> { 1003676, 1003667 }) 
// ...
```

3. Run the sample via `dotnet run {selectedSampleID}` command. <br>
Arguments:
    - `{selectedSampleID}` - ID of the sample to run. 
    - `-json` - appends additional json details to the sample output.
    - `-noui` - disables some UI elements on the initial screen

Example:
> Runnnig sample with ID 1 and appending json details to the output
```bash
dotnet run 1 -json
```

Example: 
> Showing the sample list
```bash
dotent run
```

## Powershell Samples
...
