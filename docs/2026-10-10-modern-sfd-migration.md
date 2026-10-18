# Migration to modern Superfighters Deluxe scripts

10 October 2026

The scripts in this repository stopped compiling after Superfighters Deluxe changed how it compiles map scripts. This note records what changed in the game, what this repository does about it, and how a script gets from Visual Studio back into a map.

## What changed in the game

`GameScriptInterface` is still the type the game derives a script from, but it no longer exposes `Game`. The game compiler in `Superfighters Deluxe.dll` wraps the text from the script editor before compiling it.

The wrapper it adds is a class named `GameScript` in the namespace `SFDScript`. That class inherits `GameScriptInterface` and declares `public static IGame Game`. The editor text is inserted as members of that class. The game then constructs `SFDScript.GameScript` with the live `IGame`.

The same compiler rewrites the old event registration calls. `Events.UpdateCallback.Start(...)` becomes `Game.Events.StartUpdateCallback(...)`, and the same pattern applies to the other `Events.*Callback.Start` and `.Stop` calls.

A script that is pasted into the game must therefore be the members only: methods, fields, and nested types. It must not declare its own `GameScript` class, and it must not include a constructor. The game already supplies `GameScript(IGame game)`.

The script API assembly, `SFD.GameScriptInterface.dll`, is a 64-bit .NET 8 library shipped with the game. It is proprietary and is not committed to this repository.

## What this repository does

The layout follows [SFD.Templates](https://github.com/dsafxP/SFD.Templates) by HAZARD (dsafxP). The tools under `tools/` are that project's `SFD.ScriptTools`, included under their MIT license.

Visual Studio compiles against a stand-in base class that is never written into a map:

```csharp
public abstract class GameScriptInterfaceExtended : GameScriptInterface
{
    protected static readonly IGame Game;
}
```

`Game` is null when the project runs under Visual Studio. It exists so call sites compile. The game assigns the real static `Game` when it compiles the pasted script.

Each script that should be exported is a `public partial class GameScript : GameScriptInterfaceExtended`.

- Hardcore's editable source stays split across `SFDScripts/Internal/Hardcore/Source`. Those files share the namespace `SFDScript`, so they compile as one class and the generator welds them into one script.
- Every other script has its own namespace. They are all named `GameScript`, and a shared namespace would merge them into one class.
- `GlobalUsings.cs` carries the usings the game itself prepends (`System`, `System.Linq`, `System.Collections`, `System.Collections.Generic`, `System.Text`, `System.Text.RegularExpressions`, `SFDGameScriptInterface`). Files that contain `GameScript` do not repeat those usings. The generator rejects any other `using` in those files.
- Placeholder constructors (`base(null)`, `base(game)`, empty constructors on the script class) were removed so they are not welded into the game's constructor.

`SFDScripts/SFDScripts.csproj` and `Tests/SFDScripts.UnitTests` target `net8.0` and `x64`. The unit-test project previously targeted .NET Framework 4.5.2, which cannot reference a net8.0 project.

`lib/SFD.GameScriptInterface.dll` is a symlink to the copy in the Steam install. `tools/SFD.ScriptTools.LibrarySetup.fsx` creates it and the path is listed in `.gitignore`. `References/SFD.GameScriptInterface.dll` is no longer part of the repository. The project records the product version it was set up against in `<RequiredGameSdkVersion>`.

`SFD.ScriptTools.ScriptGenerator.fsx` reads one or more C# files and writes only the members declared inside `GameScript`. Two MSBuild targets call it:

- `GenerateHardcoreScript` welds `Internal/Hardcore/Source`.
- `GenerateScript` welds an explicit file list into the path given by `ScriptOutput`.

`ScriptUtil.exe` is the older weld tool from `Utilities/SfdScriptUtil`. It still pastes the text between the `CLASS STARTS HERE` / `CLASS ENDS HERE` comments into an open Script Editor, and it does not paste `Header.txt`. That header is only for the Visual Studio copy of `Hardcore.cs`. New work should use the generator. The executable itself was not rebuilt in this migration.

## Deploying Hardcore

`injectScriptToAllMaps.ps1` generates the Hardcore body from `Source`, leaves out `MapDependentData.cs` (each map supplies its own `#region Map Dependant Data`), wraps the body in `#region Script To Copy`, and passes that file to `SFDScriptInjector`. The injector still merges the matching `Hardcore*.cs` map data and writes the `.sfdm`. The script then copies those maps into the local Custom maps folder.
