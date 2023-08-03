# ZModLauncher

![image](https://user-images.githubusercontent.com/98064221/210580022-cab6eeec-c17b-4334-868d-e70852062a8d.png)

# Introduction

Welcome to the official repository page for ZModLauncher! This mod launcher is designed with all games and mods in mind, providing various features and toolsets that can assist other mod developers in tailoring their mods to their intended audience in a very organized, streamlined, yet simplistic and efficient manner. For example, macro scripts can be used to implement game-specific functionality within the launcher directly, without intruding on the user experience and, therefore, the underlying program code. Mods can be configured to launch from their own executable, or to toggle on and off at will. A robust versioning system allows base mods to be updated by including patch files. Mods automatically perform cleanup operations when deleted. A mod can even include a link to a web resource to provide more information to the user.

Credits: [Epic Games](https://store.epicgames.com/en-US/download) (UI Design Inspiration), [JD](https://github.com/jdscodelab/EpicGamesLauncherUI) (Base UI Design)

# Backend Developer Guide

ZModLauncher is essentially one source project; however, multiple editions of the launcher intended for different purposes can be compiled and distributed with ease using a launcherconfig.json file. The JSON file's content matches the following structure:

```json
{
	"DropboxRefreshToken": "",
	"DropboxClientId": "",
	"DropboxClientSecret": "",
	"PatreonClientId": "",
	"PatreonClientSecret": "",
	"PatreonCreatorUrl": "",
	"PatreonRedirectUri": "",
	"YouTubeResourceLink": "",
	"TwitterResourceLink": "",
	"RoadmapResourceLink": "https://trello.com/b/CUk6PqKu/zmodlauncher",
	"FAQResourceLink": "",
	"PrepareLauncherMessage": "PREPARING THE TEMPLATE MOD LAUNCHER",
	"RejectTierId": "",
	"IsLauncherOfflineForMaintenance": true
}
```

The launcher utilizes Dropbox as a file hosting platform, although it is not conventional for it to serve as a proper content-delivery network. Games, mods, and other media and configuration files are uploaded to Dropbox following predefined folder and file structures that the launcher can dynamically read and render in the UI. To specify a new launcher edition, you need to create a developer app in Dropbox and then enter the appropriate information into the aforementioned configuration file.

Patreon integration is also implemented directly into the launcher, which means you can create an app client for a desired launcher edition on the Patreon creator account of choice, and then enter the necessary information into the configuration file, allowing users to subscribe to your campaign and authorize themselves within the launcher to use it. The `RejectTierId` field in the configuration file can be set to prevent patrons of a specific tier from accessing the launcher. The `IsLauncherOfflineForMaintenance` field can be set to force the launcher into offline maintenance mode for any major and necessary backend changes.

External resource links for YouTube, Twitter, and even the official launcher roadmap can be specified in the configuration file, which will dynamically update in the Settings menu of the launcher:

![image](https://user-images.githubusercontent.com/98064221/210592098-6bb94185-30e1-459c-ab2a-61da7517cb30.png)

The `PrepareLauncherMessage` field in the configuration file changes the message displayed in the user interface when the launcher is preparing content:

![image](https://user-images.githubusercontent.com/98064221/210592475-0bfcc8c7-324a-4e37-a040-1ad49aabf5d4.png)

#

Each launcher app client registered through Dropbox follows a specific folder structure to maintain consistency and ease of maintenance across launcher editions. The root folder for every launcher app folder follows this naming scheme: `[PublisherName]...Mods`. The root folder is required for the launcher to work properly.

The launcher uses two specific database files constructed in the JSON file format, similar to the internal launcher configuration file. These files, named `games.json` and `mods.json`, are placed in the launcher's root folder. The two databases can be considered as one singular database in a canonical sense; however, they are intentionally split apart to establish a separation of concerns, preventing user-made errors in one database from affecting the other.

The games database file follows the following JSON structure:

```json
{
    "GAME NAME": {
        "LocalPath": "Game",
        "ExecutablePath": "game.exe"
    }
}
```

Each game entry has a case-insensitive name key, which must match the game's true name to enable the launcher to automatically determine the intended game's location if it is locally installed through Steam/Epic Games on the user's machine. Otherwise, the game's install folder must be manually specified in the launcher. The name of the game's associated folder in Dropbox must also match the name key in the JSON.

It is important to note that the `LocalPath` property is relative to the target game's base installation folder location. For example, if a game is installed through Steam and is located in `C:\Program Files (x86)\Steam\steamapps\common\GAME NAME\`, and there is an additional folder named `Game` inside the base folder where the main files are located, the `LocalPath` should be set to `Game`. The launcher will then read the full path as `C:\Program Files (x86)\Steam\steamapps\common\GAME NAME\Game`, enabling it to properly detect the game's executable path, `game.exe`, within the folder.

The mods database file follows the following JSON structure, where all properties are entirely optional:

```json
{
    "MOD NAME": {
        "ExecutablePath": "",
        "IsUsingSharedToggleMacro": true,
        "NativeToggleMacroPath": "",
        "ModInfoUri": ""
    }
}
```

Each mod entry has a case-insensitive name key, which must match the name of the mod's associated folder in Dropbox. The `ExecutablePath` property allows you to specify the path to an executable located in the mod's extracted folder location, which then prompts the launcher to recognize the mod as a launchable mod. Launchable mods can be launched directly by clicking their associated mod card in the launcher:

![image](https://user-images.githubusercontent.com/98064221/210600528-084758ff-4145-4ef4-b16e-bdb97fde8be1.png)

The `IsUsingSharedToggleMacro` property determines whether the mod should execute a shared toggle macro script placed in the mod's associated game folder in Dropbox when the mod is being toggled on and off. This property is ignored if the mod is a launchable mod.

The `NativeToggleMacroPath` property specifies the file path to a toggle macro script native to the mod's local install path. When the mod is toggled on and off, a native toggle macro can be executed alongside a shared toggle macro, providing additional flexibility and versatility.

The `ModInfoUri` property defines a web link to an external resource, such as a YouTube video or Patreon post. This link will appear as a clickable option in the options dropdown of a mod card in the launcher:

![image](https://user-images.githubusercontent.com/98064221/210601644-10a9b5ed-2582-4625-8bd5-c77f5303b415.png)

#

Every mod folder is placed in its associated game folder in the root folder of the launcher client in Dropbox. To display an image for a game's item card in the launcher, simply place an image file (supported formats are JPG, JPEG, BMP, GIF, PNG, and WEBP) in its associated game folder. The same applies to mod folders; to display an image for a mod, place an image file in its respective mod folder.

# Integrity Checks

Sometimes, it is necessary to adjust a game's file/folder state before installing mods for that game. An integrity check is performed by a separate executable file, called an integrity checker, placed in a game's folder in Dropbox. The integrity checker's filename must contain "integritychecker" for it to be recognized by the launcher; otherwise, it is recognized as a shared toggle macro.

When the game's item card is clicked in the launcher for the first time, the launcher downloads the game's associated integrity checker to the game's installation folder and executes it to perform the necessary integrity check. After running, the launcher automatically deletes the integrity checker, as it is no longer needed unless the native.manifest file for the launcher is deleted or the value for the `HasRunIntegrityCheck` property for the associated game in the manifest file is set to false. In these cases, the integrity check runs again when the item card is clicked.

# Toggle Macros

Toggle macros are small, unmanaged executable files that run independently of the launcher. They handle game-specific requirements, which is a common theme across many games. For example, God of War requires the modification of a file named boot_options.json, which controls mod load order and other configuration settings. Instead of hard-coding this functionality, the launcher uses toggle macros to provide the necessary tools and flexibility for game compatibility.

There are two types of toggle macros implemented in the launcher: shared toggle macros and native toggle macros. A shared toggle macro is a single executable placed in a game's folder and automatically used by all toggleable mods for that game when they are toggled in the launcher. The `IsUsingSharedToggleMacro` property in the mods database JSON can be manually set to prevent a toggleable mod from using the shared toggle macro if it would interfere with the mod's functionality. On the other hand, a native toggle macro is an executable placed within a mod's folder and is only run for that specific mod when it is toggled on and off. Native toggle macros are particularly useful for mods that require special setup to work properly with a game.

# Mod Versioning & Updating

Any type of mod can be automatically updated within the launcher using patch files packaged in the ZIP archive file format. To create a patch file, simply ZIP the file(s) or folder(s) you want to replace or add into the original base mod's files. The patch file's filename must follow this structure: `[GAME-NAME]_[TARGET-GAME-VERSION]_[MOD-NAME]_[NEW-MOD-VERSION].zip`. The target game version refers to the file version of the game's executable that the mod update targets. The new mod version is assigned as the new version when the base mod is updated or when a previously updated mod is updated again. The new mod version must be greater than the version of the previously updated mod or the base mod.

# Launcher Updating

The launcher has the ability to automatically update when a new launcher version is released. To update the launcher, modify the value of the variable `LauncherVersion` in the file `SignInPage.xaml.cs` of the launcher's source project to a version greater than the current launcher version. Compile the executable with the necessary changes. It is recommended, though not required, to also update the version number in the Assembly Information for the executable to maintain consistency. Finally, place the updated launcher executable in the root folder of the launcher app client in Dropbox with the filename format: `[LAUNCHER-NAME]_[NEW-LAUNCHER-VERSION].exe`. Anyone with an older version compared to the new launcher version will have their launcher automatically updated when the launcher is initialized before the sign-in page.