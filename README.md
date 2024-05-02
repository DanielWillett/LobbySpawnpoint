# Commands
## /lobbyset
**Permission**: `lobby.set`

Sets the spawn for new players to your position and rotation.
## /lobbytp
**Permission**: `lobby.tp`

Teleport to the set lobby position.
## /lobbyreset
**Permission**: `lobby.reset`

Reset which players are considered 'new'.


# Translations
## spawnpoint_set
**Formatting**: {0} = x-position, {1} = y-position, {2} = z-position, {3} = yaw rotation</h4><p>Called after setting the spawnpoint.

## spawnpoint_teleported
**Formatting**: {0} = x-position, {1} = y-position, {2} = z-position, {3} = yaw rotation</h4><p>Called after teleporting to the spawnpoint.

## spawnpoint_reset
Called after resetting the player save file.

# Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<LobbySpawnpointConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  
  <!-- X coordinate of spawnpoint position -->
  <X>decimal</X>

  <!-- Y coordinate of spawnpoint position -->
  <Y>decimal</Y>

  <!-- Z coordinate of spawnpoint position -->
  <Z>decimal</Z>

  <!-- If the coordinates have been configured, which enables the plugin's functionality. -->
  <SetUp>true|false</SetUp>

  <!-- Should the plugin's data be saved in plain-text instead of raw binary (larger file size). Changing this will auto-convert the old file -->
  <PlainTextSaveFile>true|false</PlainTextSaveFile>
</SkillSetsConfiguration>
```

# Storage
New players are saved in a binary file, which can optionally be made into plain-text from config. It's not recommended to edit the binary file at all, and edit the plain-text file with caution. It must be encoded in ASCII with no BOM (byte order mark).

Changing the type in config will auto-convert the old file to the new type if it exists.