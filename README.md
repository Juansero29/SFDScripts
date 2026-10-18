# SFDScripts

[![Project Build, Test & Deploy](https://github.com/Juansero29/SFDScripts/actions/workflows/build-test-and-deploy.yml/badge.svg)](https://github.com/Juansero29/SFDScripts/actions/workflows/build-test-and-deploy.yml)

## My Superfighters Deluxe Scripts

This is a collection of the scripts I use when hosting, most of the time I take existing maps along with their scripts, then I modify them to "enhace" them: upgrade the performances, refactor them, make them more readable, remove useless code, change the naming of classes and variables, etc...

As for scripts I have made "myself":

- "MarioPanadero.cs" which is largely inspired on the Battle Of Teams script used by Nets in most of his maps.
- Hardcore script modifications and updates, which are here thanks to @Antonikon

This repo is here solely as a collection of modified scripts. I, in no case, declare the author of all of the code that this repo contains.

## REPO STRUCTURE

```text
•
├── Backup (a backup of a collection of maps used on the server I hosted)
│
├── SFDScripts (the script project, net8.0)
│   │
│   ├── External (scripts that can run on any map)
│   │
│   ├── Internal (scripts that belong inside a specific map)
│   │
│   └── Tests
│
├── tools (SFD.ScriptTools: link the game DLL and weld a script into a .txt)
│
├── lib (local symlink to SFD.GameScriptInterface.dll; not committed)
│
├── docs (script API backup, plus the 10 October 2026 migration note)
│
├── LICENSE (the LGPL-2.1 License for this repo)
│
└── SFDScripts.sln
```

The 10 October 2026 migration onto the current Superfighters Deluxe script compiler is written up in [docs/2026-10-10-modern-sfd-migration.md](docs/2026-10-10-modern-sfd-migration.md).

## PROJECTS

I'm currently working on two main projects on SFD:

1. Making all battle of teams map bot-enabled (available to be played with bots)
2. Restoring Antonikon's Hardcore server using his open-sourced script and remastering his maps.

You can follow these projects in here: <https://github.com/Juansero29/SFDScripts/projects>

## CONTRIBUTING

You're free to make comments, create issues and PRs! I'm open to suggestions.

Check [CONTRIBUTING.MD](https://github.com/Juansero29/SFDScripts/blob/master/CONTRIBUTING.md) for very detailed information on how to start contributing.



## CLONING, BUILDING, AND DEPLOYING

### Prerequisites

- Git, and Git LFS. Map files (`*.sfdm`) are stored with Git LFS.
- The [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Superfighters Deluxe, installed through Steam. The script API DLL ships with the game and is not in this repository.
- `SFDScriptInjector` on `PATH` when you want to write a script into a `.sfdm`. That tool lives in its own repository.

Git LFS, if you install it with Chocolatey:

1. Install Chocolatey from <https://chocolatey.org/install>.
1. In an administrator PowerShell, run `choco upgrade git-lfs.install`.

If `choco` is not recognized, close PowerShell and open a new administrator window. If branch checkout or push fails around LFS, run that upgrade again.

### Clone

From a terminal:

```powershell
git clone https://github.com/Juansero29/SFDScripts.git
cd SFDScripts
git lfs fetch --all
dotnet fsi tools/SFD.ScriptTools.LibrarySetup.fsx
```

`LibrarySetup` symlinks `lib/SFD.GameScriptInterface.dll` to the Steam copy and refuses to let that file be committed. Re-run it on every fresh clone. If the game is not in the default Steam folder, pass `-f` with the full path to the DLL.

### Build

```powershell
dotnet build SFDScripts.sln
```

The solution targets `net8.0` and `x64`, matching the game DLL. `Game` is null in this build. It is a stand-in so the scripts compile. The game supplies the real `Game` when it compiles the pasted script.

### Generate a script

The game compiler wraps whatever you paste in its own `GameScript` class. Generate the members only.

Hardcore, from the repository root:

```powershell
dotnet msbuild SFDScripts\SFDScripts.csproj -t:GenerateHardcoreScript
```

The welded body is written to `SFDScripts\bin\Debug\net8.0\Hardcore.txt`.

Any other script, naming the files that belong to that one `GameScript`:

```powershell
dotnet msbuild SFDScripts\SFDScripts.csproj -t:GenerateScript -p:ScriptFiles="External\#B.cs" -p:ScriptOutput="bin\Debug\net8.0\Battle.txt"
```

Paste the `.txt` into the map's Script Editor, or deploy Hardcore with the injector as below. Do not paste `GameScriptInterfaceExtended`, `GlobalUsings.cs`, or `Header.txt`.

### Deploy Hardcore into the Custom maps folder

`injectScriptToAllMaps.ps1` generates the Hardcore body, leaves out the template map data, and lets `SFDScriptInjector` merge each map's own `#region Map Dependant Data`. It then copies the `.sfdm` files into the Superfighters Deluxe Custom maps folder (OneDrive Documents, then Documents).

```powershell
.\injectScriptToAllMaps.ps1
.\injectScriptToAllMaps.ps1 -Destination "D:\Some\Other\Maps\Custom"
```

### ScriptUtil.exe

`ScriptUtil.exe` is the older tool from `Utilities\SfdScriptUtil`. It was not rebuilt in the October 2026 migration. With the Map Editor open it can still paste the `CLASS STARTS HERE` regions into the Script Editor:

```powershell
.\ScriptUtil.exe -i SFDScripts\Internal\Hardcore\HardcoreClassic.cs -d false
```

`-d false` pastes and presses F5. The header it writes into `Hardcore.cs` is for Visual Studio only and is not pasted into the game. Prefer `GenerateHardcoreScript` and `injectScriptToAllMaps.ps1` when you want the maps on disk updated from `Source`.

### PUSH COMMANDS

1. `git add --renormalize .`
1. `git lfs fetch --all origin your-branch-name`
1. `git lfs push --all origin your-branch-name`
1. `git commit -m "commit message"`
1. `git push`

## OTHER USEFUL COMMANDS
- `git reset --soft head~` - undo latest commit

## LICENSE

This repository is licensed under the GNU Lesser General Public License v2.1. Primarily used for software libraries, the GNU LGPL requires that derived works be licensed under the same license, but works that only link to it do not fall under this restriction. There are two commonly used versions of the GNU LGPL.

## TRADEMARK NOTICE

Superfighters Deluxe® logos, their taglines and the look, feel and trade dress of the Service are the exclusive trademarks, service marks, trade dress and logos of  MythoLogic Interactive®. All other trademarks, service marks, trade dress, and logos used on this repository are the trademarks, service marks, trade dress, and logos of their respective owners.
