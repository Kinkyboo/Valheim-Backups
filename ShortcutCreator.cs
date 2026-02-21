internal class ShortcutCreator
{
	public static void CreateFolderShortcut(string targetFolder, string shortcutPath, string? description = null)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Shortcut creation is Windows-only.");

        if (!Directory.Exists(targetFolder))
            throw new DirectoryNotFoundException($"Folder not found: {targetFolder}");

        // Ensure the shortcutPath is a .lnk file (shortcut file)
        if (!shortcutPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            shortcutPath += ".lnk";

        Type shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("WScript.Shell is unavailable (WSH may be disabled by policy).");

        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Failed to create WScript.Shell.");

        dynamic shortcut = shell.CreateShortcut(shortcutPath);

        // Folder target
        shortcut.TargetPath = targetFolder;
        shortcut.WorkingDirectory = targetFolder;

        // Optional
        if (!string.IsNullOrWhiteSpace(description))
            shortcut.Description = description;

        // Optional: use a folder icon
        shortcut.IconLocation = "shell32.dll,3";

        shortcut.Save();
    }
}