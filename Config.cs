using System.Text.Json.Serialization;

public class Config
{
	[JsonPropertyName("count")]
	public int BackupCount { get; set; } = 30;
	[JsonPropertyName("interval")]
	public int BackupInterval { get; set; } = 5;

	[JsonPropertyName("backup_directory")]
	public string BackupDirectory { get; set; } = "";
}