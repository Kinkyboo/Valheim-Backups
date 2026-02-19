using System.Diagnostics;
using System.Text.Json;

internal class Program
{
	private static DateTime lastBackup = DateTime.Now;
	private static string gameDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/../LocalLow/IronGate/Valheim/";
	private static DirectoryInfo valheimWorlds = new DirectoryInfo($"{gameDir}/worlds_local");
	private static DirectoryInfo valheimCharacters = new DirectoryInfo($"{gameDir}/characters_local");
	private static DirectoryInfo backupDir = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/valheim_backups");
	private static Config config;

	private static void Main(string[] args)
	{
		if (backupDir.Exists == false)
		{
			backupDir.Create();
		}

		config = GetConfig();
		backupDir = new DirectoryInfo(config.BackupDirectory);
		if (backupDir.Exists == false)
		{
			System.Console.WriteLine($"Configured backup directory \"{backupDir.FullName}\" does not exist");
		}

		System.Console.WriteLine($"Backup location: {backupDir.FullName}");

		if (valheimWorlds.Exists == false || valheimCharacters.Exists == false)
		{
			System.Console.WriteLine("Valheim worlds or characters directory does not exist");
			System.Console.WriteLine("Play the game bruh");
			System.Console.Write("Press any key to continue ...");
			Console.ReadKey();
			return;
		}

		System.Console.WriteLine("Press ctrl+c to terminate");
		bool running = false;

		Backup();

		while (true)
		{
			if (IsProcessRunning("valheim"))
			{
				if (running == false)
					System.Console.WriteLine("Valheim is running");
				running = true;
			}
			else
			{
				lastBackup = DateTime.Now;
				running = false;
			}

			var delta = DateTime.Now - lastBackup;
			if (delta.Minutes >= config.BackupInterval)
			{
				Backup();
			}
			System.Threading.Thread.Sleep(10000);
		}
	}

	private static void Backup()
	{
		System.Console.WriteLine("Backing up");
		lastBackup = DateTime.Now;

		var newDir = backupDir.CreateSubdirectory($"{lastBackup.Ticks}");
		var characters = newDir.CreateSubdirectory("characters_local");
		var worlds = newDir.CreateSubdirectory("worlds_local");

		foreach (var item in valheimCharacters.GetFiles())
		{
			item.CopyTo($"{characters.FullName}/{item.Name}");
		}

		foreach (var item in valheimCharacters.GetFiles())
		{
			item.CopyTo($"{worlds.FullName}/{item.Name}");
		}

		System.Console.WriteLine($"Backup {newDir.Name} created");

		//Remove oldest
		var backups = backupDir.GetDirectories();
		if (backups.Length > config.BackupCount)
		{
			var ticks = new List<long>();
			foreach (var dir in backups)
			{
				var asLong = long.Parse(dir.Name);
				ticks.Add(asLong);
			}

			var oldest = ticks.Order().First();
			var oldDir = backups.First(d => d.Name == oldest.ToString());

			oldDir.Delete(true);
			System.Console.WriteLine($"Deleted oldest backup {oldDir.Name}");
		}
	}


	private static bool IsProcessRunning(string processName)
	{
		return Process.GetProcessesByName(processName).Length > 0;
	}

	private static Config GetConfig()
	{
		var config = new Config();
		var file = new FileInfo($"{backupDir.FullName}/config.json");
		if (file.Exists)
		{
			string raw = File.ReadAllText(file.FullName);
			config = JsonSerializer.Deserialize<Config>(raw);
		}
		else
		{
			System.Console.WriteLine("Config.json not found, creating default config");
			var options = new JsonSerializerOptions() { WriteIndented = true };
			config.BackupDirectory = backupDir.FullName;
			string json = JsonSerializer.Serialize(config, options);

			File.WriteAllText(file.FullName, json);
			System.Console.WriteLine($"Config created in {file.FullName}");
		}
		return config;
	}
}