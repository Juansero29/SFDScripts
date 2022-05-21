# SFDScripts

![Build Status](https://github.com/Juansero29/SFDScripts/actions/workflows/dotnet-desktop.yml/badge.svg)

## My Superfighters Deluxe Scripts

This is a collection of the scripts I use when hosting, most of the time I take existing maps along with their scripts, then I modify them to "enhace" them: upgrade the performances, refactor them, make them more readable, remove useless code, change the naming of classes and variables, etc...

As for scripts I have made "myself":

- "MarioPanadero.cs" which is largely inspired on the Battle Of Teams script used by Nets in most of his maps.
- Hardcore script modifications and updates, which are here thanks to @Antonikon

This repo is here solely as a collection of modified scripts. I, in no case, declare the author of all of the code that this repo contains.

## REPO STRUCTURE

```text
•
├── Backup (a backup of a collection of maps a used 3 years ago on the server I hosted)
│
├── SFDScripts (folder containing the actual scripts project)
│   │
│   ├── EXTERNAL (fodler containing external scripts: scripts that you can activate on any map, not map dependant)
│   │
│   ├── INTERNAL (folder containing internal scripts: scripts that are map dependant, and need to be the script inside the map they're supposed to use)
│   │
│   └── TESTS (some tests I have made, not too many ^^)
│
├── docs (folder containing a backup of the script documentation, also used for a webhook available here: https://juansero29.github.io/SFDScripts/)
│
├── LICENSE (the LGPL-2.1 License for this repo)
│
└── SFDScripts.sln (the Visual Studio 2019 solution)
```

## PROJECTS

I'm currently working on two main projects on SFD:

1. Making all battle of teams map bot-enabled (available to be played with bots)
2. Restoring Antonikon's Hardcore server using his open-sourced script and remastering his maps.

You can follow these projects in here: <https://github.com/Juansero29/SFDScripts/projects>

## CONTRIBUTING

You're free to make comments, create issues and PRs! I'm open to suggestions.

Check [CONTRIBUTING.MD](https://github.com/Juansero29/SFDScripts/blob/master/CONTRIBUTING.md) for very detailed information on how to start contributing.

## PUSHING NEW MAPS

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
