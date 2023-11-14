using System.Text.Json;
using Relativity.Export.V1.Model;
using Relativity.Export.V1.Model.ExportJobSettings;
using Spectre.Console;
using Spectre.Console.Json;

namespace Relativity.Export.Samples.RelConsole.Helpers;

public record class SampleLog(LogLevel LogLevel, string Message);
public enum LogLevel
{
	Information,
	Warning,
	Error
}

public class Logger
{
	private readonly string[] _args = default!;

	public Logger(string[] args)
	{
		_args = args;
	}

	public void PrintJobJson(ExportJobSettings settings, bool print = false)
	{
		if (!_args.Contains("-json"))
		{
			if (!print)
				return;
		}

		// create JSON for preview
		var serializerOptions = new JsonSerializerOptions()
		{
			WriteIndented = true,
		};

		var json = JsonSerializer.Serialize(settings, serializerOptions);

		var panel = new Panel(new JsonText(json))
			.RoundedBorder()
			.BorderColor(Color.Orange1)
			.Header("[aquamarine1]Job JSON[/]", Justify.Center);

		AnsiConsole.Write(panel);
	}


	public void LogWarning(string message)
	{
		Log(new SampleLog(LogLevel.Warning, $"[orange1]{message}[/]"));
	}

	public void LogInformation(string message)
	{
		Log(new SampleLog(LogLevel.Information, message));
	}

	public void LogError(string message)
	{
		Log(new SampleLog(LogLevel.Error, $"[red]{message}[/]"));
	}

	public void Log(SampleLog log)
	{
		var table = new Table()
			.NoBorder()
			.HideHeaders();

		table.AddColumns(new TableColumn("Level").Width(8),
			new TableColumn("Date"),
			new TableColumn("Message"));

		table.AddRow(new Markup(LevelToMessage(log.LogLevel)),
			new Markup(DateTime.Now.ToString("HH:mm:ss"), new Style(Color.Orange1)),
			new Markup(log.Message));

		AnsiConsole.Write(table);
	}

	public void PrintSampleData(Dictionary<string, string> data)
	{
		var dataGrid = new Grid()
			.AddColumn(new GridColumn().NoWrap())
			.AddColumn(new GridColumn().NoWrap());

		foreach (var record in data)
		{
			dataGrid.AddRow(new Markup[]
			{
				new Markup($"[orange1]{record.Key}[/]"),
				new Markup(record.Value)
			});
		}

		var sampleData = new Panel(dataGrid)
			.RoundedBorder()
			.BorderColor(Color.Orange1)
			.Header("[aquamarine1]Sample Data[/]", Justify.Center);

		AnsiConsole.Write(sampleData);
	}

	public void PrintExportJobResult(string finalMessage, ValueResponse<ExportJob> exportJob)
	{
		int processed = exportJob.Value.ProcessedRecords - exportJob.Value.RecordsWithErrors - exportJob.Value.RecordsWithWarnings ?? 0;

		LogInformation(finalMessage);
		AnsiConsole.WriteLine();
		AnsiConsole.Write(new BreakdownChart()
			.Width(60)
			.AddItem("Processed", processed, Color.Green)
			.AddItem("Records with errors", exportJob.Value.RecordsWithErrors ?? 0, Color.Red)
			.AddItem("Records with warnings", exportJob.Value.RecordsWithWarnings ?? 0, Color.Yellow));
	}

	public void PrintBulkExportJobResult(string finalMessage, List<ExportStatus?> exportStatuses)
	{
		int successJobs = exportStatuses.Count(s => s == ExportStatus.Completed);
		int failedJobs = exportStatuses.Count(s => s == ExportStatus.Failed);
		int cancelledJobs = exportStatuses.Count(s => s == ExportStatus.Cancelled);
		int completedWithErrorsJobs = exportStatuses.Count(s => s == ExportStatus.CompletedWithErrors);

		LogInformation(string.IsNullOrEmpty(finalMessage) ? "Bulk export completed" : finalMessage);
		AnsiConsole.WriteLine();
		AnsiConsole.Write(new BreakdownChart()
			.Width(60)
			.AddItem("Success", successJobs, Color.Green)
			.AddItem("Completed With Errors", completedWithErrorsJobs, Color.OrangeRed1)
			.AddItem("Failed", failedJobs, Color.Red)
			.AddItem("Cancelled", cancelledJobs, Color.Yellow));
	}

	private string LevelToMessage(LogLevel logLevel) => logLevel switch
	{
		LogLevel.Information => "[bold][aquamarine1]<INFO>[/][/]",
		LogLevel.Warning => "[bold][orange1]<WARN>[/][/]",
		LogLevel.Error => "[bold][red]<ERR>[/][/]",
		_ => throw new ArgumentOutOfRangeException(nameof(logLevel), $"Not expected direction value: {logLevel}"),
	};
}