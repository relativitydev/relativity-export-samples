namespace Relativity.Export.Samples.RelConsole.Helpers;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class SampleMetadataAttribute : System.Attribute
{
	public int ID { get; set; }
	public string Name { get; set; } = default!;
	public string? Description { get; set; } = default!;

	public SampleMetadataAttribute(int id, string name, string description)
	{
		ID = id;
		Name = name;
		Description = description;
	}
}
