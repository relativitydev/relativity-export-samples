namespace Relativity.Export.Samples.RelConsole.Helpers;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class SampleMetadataAttribute : System.Attribute
{
	public string Name { get; set; } = default!;
	public string? Description { get; set; } = default!;

	public SampleMetadataAttribute(string name, string description)
	{
		Name = name;
		Description = description;
	}
}
