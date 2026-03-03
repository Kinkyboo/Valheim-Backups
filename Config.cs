using System.Text.Json.Serialization;

public class Config
{
	[JsonPropertyName("count")]
	public int BackupCount { get; set; } = 30;

	[JsonPropertyName("backup_directory")]
	public string BackupDirectory { get; set; } = "";
}