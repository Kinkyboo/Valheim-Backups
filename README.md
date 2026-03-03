# Valheim backups

Backups Valheim local saves from `\AppData\LocalLow\IronGate\Valheim\characters_local` and `\AppData\LocalLow\IronGate\Valheim\worlds_local`

Default Backup directory

`C:\Users\<user>\Documents\valheim_backups`

# Usage
Let the program run in the background.

Every time a change to the save files are detected a backup is created.

To manually create a backup simply press save in game menu.

## Build instructions

dotnet SDK 8.0+

`dotnet publish -c release`