using System.Text.Json;
using Relativity.Export.V1.Model;
using Relativity.Export.V1.Model.ExportJobSettings;
using Spectre.Console;
using Spectre.Console.Json;

namespace Relativity.Export.Samples.RelConsole.Helpers;

public record class SampleLog(LogLevel LogLevel, string Message, bool HideTimeStamp = false);
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

	public void LogWarning(string message, bool hideTimeStamp = false)
	{
		Log(new SampleLog(LogLevel.Warning, $"[orange1]{message}[/]", hideTimeStamp));
	}

	public void LogInformation(string message, bool hideTimeStamp = false)
	{
		Log(new SampleLog(LogLevel.Information, message, hideTimeStamp));
	}

	public void LogError(string message, bool hideTimeStamp = false)
	{
		Log(new SampleLog(LogLevel.Error, $"[red]{message}[/]", hideTimeStamp));
	}

	public void Log(SampleLog log)
	{
		var table = new Table()
			.NoBorder()
			.HideHeaders();

		table.AddColumns(new TableColumn("Level").Width(8),
			new TableColumn("Date"),
			new TableColumn("Message"));

		if (log.HideTimeStamp)
		{
			table.AddColumns(new TableColumn("Level").Width(8),
				new TableColumn("Message"));

			table.AddRow(new Markup(LevelToMessage(log.LogLevel)),
				new Markup(log.Message));
		}
		else
		{
			table.AddColumns(new TableColumn("Level").Width(8),
				new TableColumn("Date"),
				new TableColumn("Message"));

			table.AddRow(new Markup(LevelToMessage(log.LogLevel)),
				new Markup(DateTime.Now.ToString("HH:mm:ss"), new Style(Color.Orange1)),
				new Markup(log.Message));
		}

		AnsiConsole.Write(table);
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

	public void PrintDictionaryData<K, V>(Dictionary<K, V> data, string header) where K : notnull
	{
		var dataGrid = new Grid()
			.AddColumn(new GridColumn().NoWrap())
			.AddColumn(new GridColumn().NoWrap());

		foreach (var record in data)
		{
			dataGrid.AddRow(
			[
				new Markup($"[orange1]{record.Key}[/]"),
				new Markup(record.Value?.ToString() ?? "#null")
			]);
		}

		var sampleData = new Panel(dataGrid)
			.RoundedBorder()
			.BorderColor(Color.Orange1)
			.Header($"[aquamarine1]{header}[/]", Justify.Center);

		AnsiConsole.Write(sampleData);
	}

	private string LevelToMessage(LogLevel logLevel) => logLevel switch
	{
		LogLevel.Information => "[bold][aquamarine1]<INFO>[/][/]",
		LogLevel.Warning => "[bold][orange1]<WARN>[/][/]",
		LogLevel.Error => "[bold][red]<ERR>[/][/]",
		_ => throw new ArgumentOutOfRangeException(nameof(logLevel), $"Not expected direction value: {logLevel}"),
	};
}