using System.Diagnostics;
using System.Text.Json;

internal class Program
{
	private static string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	private static string gameDir = $"{userFolder}\\AppData\\LocalLow\\IronGate\\Valheim\\";
	private static DirectoryInfo valheimWorlds = new DirectoryInfo($"{gameDir}/worlds_local");
	private static DirectoryInfo valheimCharacters = new DirectoryInfo($"{gameDir}/characters_local");
	private static DirectoryInfo backupDir = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/valheim_backups");
	private static Config config;

	private static Dictionary<string, DateTime> WorldFilesInventory = new Dictionary<string, DateTime>();
	private static Dictionary<string, DateTime> CharacterFilesInventory = new Dictionary<string, DateTime>();

	private static void Main(string[] args)
	{
		if (backupDir.Exists == false)
		{
			backupDir.Create();
		}

		var shortcut = new FileInfo($"{backupDir}/Valheim.lnk");
		if(shortcut.Exists == false)
		{
			ShortcutCreator.CreateFolderShortcut(gameDir, $"{backupDir}/Valheim.lnk", "Valheim");
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

		//Perform backup on startup
		Backup();
		InventorySaveFiles();

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
				running = false;
			}

			if(InventorySaveFiles())
			{
				System.Console.WriteLine("File change detected");
				System.Threading.Thread.Sleep(10000); //Give the system time to save changes
				Backup();
				//Do one more inventory after copy to avoid double backups
				InventorySaveFiles();
			}

			System.Threading.Thread.Sleep(5000);
		}
	}

	private static bool InventorySaveFiles()
	{
		bool changeDetected = false;

		List<FileInfo> worldFiles = valheimWorlds.GetFiles()
			.Where(f => f.Name.Contains("_backup_auto-") == false).ToList();

		List<FileInfo> characterFiles = valheimCharacters.GetFiles()
			.Where(f => f.Name.Contains("_backup_auto-") == false).ToList();

		bool check1 = CompareInventory(worldFiles, WorldFilesInventory);
		bool check2 = CompareInventory(characterFiles, CharacterFilesInventory);

		return (check1 || check2);
	}

	private static bool CompareInventory(List<FileInfo> newFiles, Dictionary<string, DateTime> inventory)
	{
		bool changeDetected = false;

		foreach(var file in newFiles)
		{
			if(inventory.ContainsKey(file.Name))
			{
				var oldFile = inventory[file.Name];
				if(oldFile < file.LastWriteTime)
				{
					changeDetected = true;
				}
			}
			else
			{
				changeDetected = true;
				inventory.Add(file.Name, file.LastWriteTime);
			}
		}

		//Reset inventory
		inventory.Clear();
		foreach(var file in newFiles)
		{
			inventory.Add(file.Name, file.LastWriteTime);
		}

		return changeDetected;
	}

	private static void Backup()
	{
		System.Console.WriteLine("Backing up");

		var now = DateTime.Now;
		var newDir = backupDir.CreateSubdirectory(now.ToString("yyyy-MM-dd_HH_mm_ss"));
		var characters = newDir.CreateSubdirectory("characters_local");
		var worlds = newDir.CreateSubdirectory("worlds_local");

		BackupDirectory(valheimCharacters, characters);
		BackupDirectory(valheimWorlds, worlds);

		System.Console.WriteLine($"Backup {newDir.Name} created @ {now.ToString("yyyy-MM-dd HH:mm:ss")}");

		//Remove oldest
		var backups = backupDir.GetDirectories();
		if (backups.Length > config.BackupCount)
		{
			var oldest = backups.OrderBy(d => d.CreationTime).First();

			oldest.Delete(true);
			System.Console.WriteLine($"Deleted oldest backup {oldest.Name}");
		}
	}

	private static void BackupDirectory(DirectoryInfo sourceDir, DirectoryInfo targetDir)
	{
		foreach (var item in sourceDir.GetFiles())
		{
			while(true)
			{
				try
				{
					item.CopyTo($"{targetDir.FullName}/{item.Name}");
					break; // File could b copied
				}
				catch(Exception e)
				{
					
				}
			}
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