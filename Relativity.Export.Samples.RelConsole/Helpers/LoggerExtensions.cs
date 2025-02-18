using Relativity.Export.V1.Model;
using Spectre.Console;

namespace Relativity.Export.Samples.RelConsole.Helpers;

public static class LoggerExtensions
{
    public static void PrintAliases(this Logger logger, Dictionary<int, string> data)
    {
        logger.PrintDictionaryData(data, "Field Aliases");
    }

    public static void PrintSampleData(this Logger logger, Dictionary<string, string> data)
    {
        logger.PrintDictionaryData(data, "Sample Data");
    }

    public static void PrintExportJobResult(this Logger logger, string finalMessage, ExportJob exportJob)
    {
        int processed = exportJob.ProcessedRecords - exportJob.RecordsWithErrors - exportJob.RecordsWithWarnings ?? 0;

        logger.LogInformation(finalMessage, hideTimeStamp: true);
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new BreakdownChart()
            .Width(60)
            .AddItem("Processed", processed, Color.Green)
            .AddItem("Records with errors", exportJob.RecordsWithErrors ?? 0, Color.Red)
            .AddItem("Records with warnings", exportJob.RecordsWithWarnings ?? 0, Color.Yellow));
        AnsiConsole.WriteLine();
    }

    public static void PrintBulkExportJobResult(this Logger logger, string finalMessage, List<ExportStatus?> exportStatuses)
    {
        int successJobs = exportStatuses.Count(s => s == ExportStatus.Completed);
        int failedJobs = exportStatuses.Count(s => s == ExportStatus.Failed);
        int cancelledJobs = exportStatuses.Count(s => s == ExportStatus.Cancelled);
        int completedWithErrorsJobs = exportStatuses.Count(s => s == ExportStatus.CompletedWithErrors);

        logger.LogInformation(string.IsNullOrEmpty(finalMessage) ? "Bulk export completed" : finalMessage, hideTimeStamp: true);
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new BreakdownChart()
            .Width(60)
            .AddItem("Success", successJobs, Color.Green)
            .AddItem("Completed With Errors", completedWithErrorsJobs, Color.OrangeRed1)
            .AddItem("Failed", failedJobs, Color.Red)
            .AddItem("Cancelled", cancelledJobs, Color.Yellow));
        AnsiConsole.WriteLine();
    }
}