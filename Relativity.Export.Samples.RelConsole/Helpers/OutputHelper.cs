using System.Reflection;
using Relativity.Export.Samples.RelConsole.SampleCollection;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Relativity.Export.Samples.RelConsole.Helpers;

public record class SampleMetadata(int ID, string Name, string Description = default!, bool IsSelected = false);

public static class OutputHelper
{
	private static string[] _args = default!;
	private static Dictionary<int, MethodInfo> _sampleRunner = new();
	private static StatusContext _statusContext = default!;
	private static object _statusLock = new();

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

				var samplesPanel = GetSamplesPanel(samples);
				var metadataPanel = GetSampleMetadataPanel(samples.FirstOrDefault(s => s.ID == selectedSampleId));

				Columns[] dataColumns =
				[
					new Columns(samplesPanel),
					new Columns(metadataPanel)
				];

				PrintRelativityLogo();
				AnsiConsole.Write(new Columns(dataColumns));
			}

			if (isSampleValid)
			{
				BaseExportService instance = new(relativityUrl, relativityUsername, relativityPassword, args);

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
			else if (args.Length == 0 || args.Contains("-help"))
			{
				var containerGrid = new Grid()
					.AddColumn();

				var options = new Grid()
					.AddColumn(new GridColumn().NoWrap())
					.AddColumn(new GridColumn().NoWrap());

				options.AddRow("[aquamarine1][bold]{number}[/][/]", "Sample ID from the sample list");
				options.AddRow("[aquamarine1][bold]-noui[/][/]", "disables some UI elements on the initial screen");
				options.AddRow("[aquamarine1][bold]-json[/][/]", "adds additional JSON output to the console");

				var example = new Panel("dotnet run 1 -json")
					.RoundedBorder()
					.BorderColor(Color.Orange1)
					.Expand()
					.Header("[aquamarine1]Example[/]", Justify.Center);

				containerGrid.AddRow(options);
				containerGrid.AddRow(example);

				var argsPanel = new Panel(containerGrid)
					.BorderColor(Color.Aquamarine1)
					.Header("[orange1]Console Arguments[/]", Justify.Center);

				AnsiConsole.Write(argsPanel);
			}
		}
		catch (Exception ex)
		{
			AnsiConsole.WriteException(ex);
		}
	}

	public static void PrintRelativityLogo()
	{
		var iconCanvas = new CanvasImage(Path.Join(AppContext.BaseDirectory, "Helpers", "logo.png"));
		iconCanvas.MaxWidth(20);

		var iconPanel = new Panel(iconCanvas)
			.Expand()
			.NoBorder();

		AnsiConsole.Write(iconPanel);
	}

	public static void UpdateStatus(string message)
	{
		lock (_statusLock)
		{
			_statusContext.Status($"[bold][orange1]{message}[/][/]");
			_statusContext.Refresh();
		}
	}

	public static void ClearStatus()
	{
		lock (_statusLock)
		{
			_statusContext.Status("...");
			_statusContext.Refresh();
		}
	}

	private static List<SampleMetadata> GetSamples(int selectedSampleId)
	{
		List<SampleMetadata> samples = new();
		var sampleMethods = Assembly.GetExecutingAssembly()
			 .GetTypes()
			 .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
			 .Where(method => method.GetCustomAttributes(typeof(SampleMetadataAttribute), false).Length > 0)
			 .ToArray();

		var samplesMetadataAttribute = sampleMethods.Select(m => m.GetCustomAttribute<SampleMetadataAttribute>())
			.OrderBy(x => x?.Name)
			.ToArray();

		for (int i = 0; i < samplesMetadataAttribute.Length; i++)
		{
			var data = samplesMetadataAttribute[i];

			// Sample ID is 1-based
			// Automatically assigned to the sample based on alphabetical order of samples names
			var sampleID = i + 1;

			if (data is null)
				continue;

			var sampleMetadata = new SampleMetadata(ID: sampleID,
				data.Name,
				!string.IsNullOrEmpty(data.Description) ? data.Description : "No description",
				sampleID == selectedSampleId); // is sample currently selected

			samples.Add(sampleMetadata);
			_sampleRunner.Add(i + 1, sampleMethods[i]);
		}

		return samples;
	}

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