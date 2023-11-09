using System.Reflection;
using System.Text.Json;
using Relativity.Export.Samples.RelConsole.SampleCollection;
using Relativity.Export.V1.Model;
using Relativity.Export.V1.Model.ExportJobSettings;
using Spectre.Console;
using Spectre.Console.Json;
using Spectre.Console.Rendering;

namespace Relativity.Export.Samples.RelConsole.Helpers;

public record class SampleMetadata(int ID, string Name, string Description = default!, bool IsSelected = false);
public record class SampleLog(Level LogLevel, string Message);
public enum Level
{
	Information,
	Warning,
	Error
}

public static class OutputHelper
{
	private static string[] _args = default!;
	private static Dictionary<int, MethodInfo> _sampleRunner = new();
	private static StatusContext _statusContext = default!;

	public static async Task StartAsync(string[] args, string relativityUrl, string relativityUsername, string relativityPassword)
	{
		try
		{
			_args = args;
			List<SampleMetadata> samples;
			int selectedSampleId = -1;
			bool isSampleValid = args.Length > 0 && Int32.TryParse(args[0], out selectedSampleId);

			samples = GetSamples(isSampleValid ? selectedSampleId : -1);

			if (!args.Contains("-noui"))
			{
				var iconCanvas = new CanvasImage(Path.Join(AppContext.BaseDirectory, "Helpers", "logo.png"));
				iconCanvas.MaxWidth(20);

				var iconPanel = new Panel(iconCanvas).Expand().NoBorder();

				var samplesPanel = GetSamplesPanel(samples);
				var metadataPanel = GetSampleMetadataPanel(samples.FirstOrDefault(s => s.ID == selectedSampleId));

				Columns[] dataColumns = new Columns[]
				{
					new Columns(samplesPanel),
					new Columns(metadataPanel)
				};

				AnsiConsole.Write(iconPanel);
				AnsiConsole.Write(new Columns(dataColumns));
			}

			if (isSampleValid)
			{
				BaseExportService instance = new(relativityUrl, relativityUsername, relativityPassword);

				var runnableSample = _sampleRunner[selectedSampleId];

				await AnsiConsole.Status()
					.StartAsync("Thinking...", async ctx =>
					{
						_statusContext = ctx;
						_statusContext.Status($"[bold][orange1]Running Sample...[/][/]");
						_statusContext.Spinner(Spinner.Known.Dots8Bit);
						_statusContext.SpinnerStyle(Style.Parse("orange1"));

						var sampleTask = (Task)runnableSample.Invoke(instance, null)!;
						await sampleTask;
					});

				AnsiConsole.MarkupLine("[bold][aquamarine1]Sample finished![/][/]");
			}
		}
		catch (Exception ex)
		{
			AnsiConsole.WriteException(ex);
		}
	}

	public static void UpdateStatus(string message)
	{
		_statusContext.Status($"[bold][orange1]{message}[/][/]");
		_statusContext.Refresh();
	}

	public static void PrintExportJobResult(string finalMessage, ValueResponse<ExportJob> exportJob)
	{
		int processed = exportJob.Value.ProcessedRecords - exportJob.Value.RecordsWithErrors - exportJob.Value.RecordsWithWarnings ?? 0;

		PrintLog(finalMessage);
		AnsiConsole.WriteLine();
		AnsiConsole.Write(new BreakdownChart()
			.Width(60)
			.AddItem("Processed", processed, Color.Green)
			.AddItem("Records with errors", exportJob.Value.RecordsWithErrors ?? 0, Color.Red)
			.AddItem("Records with warnings", exportJob.Value.RecordsWithWarnings ?? 0, Color.Yellow));
	}

	public static void PrintBulkExportJobResult(string finalMessage, List<ExportStatus?> exportStatuses)
	{
		int successJobs = exportStatuses.Count(s => s == ExportStatus.Completed);
		int failedJobs = exportStatuses.Count(s => s == ExportStatus.Failed);
		int cancelledJobs = exportStatuses.Count(s => s == ExportStatus.Cancelled);
		int completedWithErrorsJobs = exportStatuses.Count(s => s == ExportStatus.CompletedWithErrors);

		PrintLog(string.IsNullOrEmpty(finalMessage) ? "Bulk export completed" : finalMessage);
		AnsiConsole.WriteLine();
		AnsiConsole.Write(new BreakdownChart()
			.Width(60)
			.AddItem("Success", successJobs, Color.Green)
			.AddItem("Completed With Errors", completedWithErrorsJobs, Color.OrangeRed1)
			.AddItem("Failed", failedJobs, Color.Red)
			.AddItem("Cancelled", cancelledJobs, Color.Yellow));
	}

	public static void PrintJobJson(ExportJobSettings settings, bool print = false)
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

	public static void PrintWarning(string message)
	{
		PrintLog(new SampleLog(Level.Warning, $"[orange1]{message}[/]"));
	}

	public static void PrintLog(string message)
	{
		PrintLog(new SampleLog(Level.Information, message));
	}

	public static void PrintError(string message)
	{
		PrintLog(new SampleLog(Level.Error, $"[red]{message}[/]"));
	}

	public static void PrintLog(SampleLog log)
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

	public static void PrintSampleData(Dictionary<string, string> data)
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

	private static List<SampleMetadata> GetSamples(int selectedSampleId)
	{
		List<SampleMetadata> samples = new();
		var sampleMethods = Assembly.GetExecutingAssembly()
			 .GetTypes()
			 .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
			 .Where(method => method.GetCustomAttributes(typeof(SampleMetadataAttribute), false).Length > 0)
			 .ToArray();

		foreach (var method in sampleMethods)
		{
			var typeMeta = method.GetCustomAttribute<SampleMetadataAttribute>();

			if (typeMeta is null)
				continue;

			samples.Add(new SampleMetadata(typeMeta.ID, typeMeta.Name, typeMeta.Description ?? "No description", typeMeta.ID == selectedSampleId));

			_sampleRunner.Add(typeMeta.ID, method);
		}

		return samples;
	}

	private static string LevelToMessage(Level logLevel) => logLevel switch
	{
		Level.Information => "[bold][aquamarine1]<INFO>[/][/]",
		Level.Warning => "[bold][orange1]<WARN>[/][/]",
		Level.Error => "[bold][red]<ERR>[/][/]",
		_ => throw new ArgumentOutOfRangeException(nameof(logLevel), $"Not expected direction value: {logLevel}"),
	};

	private static Panel GetSampleMetadataPanel(SampleMetadata? sample)
	{
		if (sample is null)
		{
			return new Panel("Select Sample from the list")
				.RoundedBorder()
				.BorderColor(Color.Orange1)
				.Header("[aquamarine1]Metadata[/]", Justify.Center);
		}

		var metaGrid = new Grid()
			.Expand()
			.AddColumn(new GridColumn().NoWrap())
			.AddColumn(new GridColumn());

		metaGrid.AddRow(new Markup[]
		{
			new Markup($"[orange1]ID[/]"),
			new Markup(sample.ID.ToString())
		})
		.AddRow(new Markup[]
		{
			new Markup($"[orange1]Name[/]"),
			new Markup(sample.Name)
		})
		.AddRow(new Markup[]
		{
			new Markup($"[orange1]Description[/]"),
			new Markup(sample.Description)
		});

		var metadataPanel = new Panel(metaGrid)
			.RoundedBorder()
			.BorderColor(Color.Orange1)
			.Header("[aquamarine1]Metadata[/]", Justify.Center);

		return metadataPanel;
	}

	private static Panel GetSamplesPanel(List<SampleMetadata> samplesMetadata)
	{
		var samplesTable = new Table()
			.Expand();

		samplesTable.AddColumns("ID", "Name");

		foreach (var sample in samplesMetadata.OrderBy(s => s.ID))
		{
			IRenderable[] rows = new IRenderable[]
			{
				new Markup(sample.ID.ToString(), new Style(sample.IsSelected ? Color.Aquamarine1 : Color.Orange1)),
				new Markup(sample.Name, new Style(sample.IsSelected ? Color.Aquamarine1 : Color.Orange1))
			};

			samplesTable.AddRow(rows);
		}

		var samplesPanel = new Panel(samplesTable)
			.RoundedBorder()
			.BorderColor(Color.Orange1)
			.Header("[aquamarine1]Export Samples[/]", Justify.Center);

		return samplesPanel;
	}
}