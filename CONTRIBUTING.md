
# Contributing to SFDScripts

A big welcome and thank you for considering contributing to this project! *More will get added from time to time so check this document frequently if you will be a constant contributor*.

Reading and following these guidelines will help everyone make the contribution process easy and effective for everyone involved. It also communicates that you agree to respect my time managing and developing these open source project. In return, I will reciprocate that respect by addressing your issues, assessing changes, and helping you finalize your pull requests.

## Quicklinks

* [Code of Conduct](#code-of-conduct)
* [General Information](#getting-information)
* [Issues](#issues)
* [Pull Requests](#pull-requests)
* [Projects, Milestones and Labels](#projects,-milestones-and-labels)
* [Getting Started](#getting-started)
  * [Prerequisites](#prerequisites)
  * [Starting Up](#starting-up)
  * [Developing](#developing)
  * [Debugging](#debugging)
  * [Testing](#testing)
* [Other Useful Links & Info](#other-useful-links-&-info)
* [Contributing Maps](#contributing-maps)
* [Getting Help](#getting-help)

## Code of Conduct

Just be nice, try to do your best and test your code before pushing a PR!

Some basic code rules:

* Be sure to respect [common guidelines for C# developers](https://github.com/ktaranov/naming-convention/blob/master/C%23%20Coding%20Standards%20and%20Naming%20Conventions.md) while you develop
* All code is written in English
* Get used to pulling changes from master every time you open your solution to avoid coding from an outdated base-code and having to resolve even more conflicts later on when submitting your PR
* Untested code won't be merged with master
* Avoid commenting code: rather spend time thinking on naming variables and functions correctly
* Prefer adding a field or property to a class rather than passing around lots of arguments in the functions
* Leave the code cleaner than you found it
  * Feel free to do small refactors and name changes whenever you feel it is useful and will leave the code at a better state than without those modifications.
  * Code that gets cleaner over time is way better than code that gets dirtier over time
* Keep your functions as small as possible
* Redact your commit messages well, in clear English, so that anyone can understand what was done in each commit. Do as much commits as you need to, as long as your commit messages are always clear and concise.
* Make your branches have meaningful names. It is also a good idea to write the number of the Issue you are fixing with your PR in the name of your banch. Prefer a long, clear name rather than a concise unclear name: (e.g prefer `34-fixing-airstrike-cost` rather than `patch-1`)
* As scripts are a single file due to SFD's restrictions, please organize related methods, properties, fields, classes and interfaces into regions.
  * Regions can be collapsed, and searcheable, whick makes them very useful in single file scripts.
  * Declare them like this:

  ```csharp
  #region Descriptive Name Of What This Region Contains 

  [... code ...]

  #endregion
   ```

## General Information

Contributions are made to this repo via Issues and Pull Requests (PRs). A few general guidelines that cover both:

* Search for existing Issues and PRs before creating your own.
* For the moment I (juansero29) am the only reviewer. I will review your code when I have the time to, mostly on weekends. I'll try to do it as quick as possible but I am no machine, so don't hesitate to ping me here or on Discord if it's been too long since your contributions have been there without review.
* If you have noticed a bug or an issue with one of the scripts and no Issue treats it, then please add an Issue **before** submitting the associated PR.
* If you've never contributed before, see [the first timer's guide in this blog](https://auth0.com/blog/a-first-timers-guide-to-an-open-source-project/) for resources and tips on how to get started.

## Issues

Issues should be used to report problems with the scripts, request a new feature, report a bug you found or to discuss potential changes before a PR is created.

An issue can be labeled as "meta" meaning it concerns a problem with the code itself (code-style, naming, architecture) or the repo.

If you find an Issue that addresses the problem you're having, please add your own reproduction information to the existing issue rather than creating a new one. Adding a [reaction](https://github.blog/2016-03-10-add-reactions-to-pull-requests-issues-and-comments/) can also help by indicating that a particular problem is affecting more than just the reporter.

Some general rules:

* Choose a good descriptive title for the issue
* Include a clear description of the problem
* (Optional) If possible, describe what a good solution for the problem could be.

## Pull Requests

PRs to this repo are always welcome and can be a quick way to get your fix or improvement slated soon. In general, PRs should:

* Only fix/add the functionality in question **OR** address wide-spread whitespace/style issues, not both.
* Address a single concern in the least number of changed lines as possible.
* This is optional but almost assures your PR will be quickly merged:
  * If what you did can be shown in a video, include a `.gif` file uploaded to the internet (giphy.com, gfycat.com, tenor.com, etc.) that shows that what you did is working. You can use a free tool like <https://www.screentogif.com/> for capturing gifs from your screen and exporting them.
  * If what you did is static but can be shown, include a screenshot show-casing your work.
  * If your work can't be seen, don't bother to include images or gifs to the PR.

In general, try to follow the [fork-and-pull](https://github.com/susam/gitpr) git workflow:

1. Fork the repository to your own Github account
2. Clone the project to your machine
3. Create a branch locally with a succinct but descriptive name
4. Commit changes to the branch
5. Following any formatting and testing guidelines specific to this repo
6. Push changes to your fork
7. Open a PR in our repository and follow the PR template so that we can efficiently review the changes.

## Projects, Milestones and Labels

Issues are assigned to projects, are categorized with labels and are iterated through milestones.

On the issues tab, you can filter the issues by project choosing the `Projects` dropdown, and concentrate in the ones that interest you.

You can also check the iterations with the [Milestones](https://github.com/Juansero29/SFDScripts/milestones) button left to the `New Issue` button. Iterations are there to plan out the work to do in a period of time, settting a "delivery date" to the work that is going to be done in said-period. The issues present in these milestones are usually what I'm working on on my free time. They should be done by the end of the milestone period. Then at the end of the period, a new date should be set, and new issues should be added to it.

To know exactly is a particular issue is being worked on right now, check the `In Progress` issues inside the [Projects](https://github.com/Juansero29/SFDScripts/projects) kanban board.

You can also check the [Labels](https://github.com/Juansero29/SFDScripts/labels) to get a more precise idea on how I sort Issues and PRs.

## Getting Started

This section will help you to get started contributing to these repo.

### Prerequisites

These repo contains Superfighters Deluxe scripts, which means you should have a copy of the game in order to start testing. The preferred tool for developing and testing these scripts is Visual Studio.

You need to have downloaded:

* Git - [Download Link](https://git-scm.com/downloads)
* Superfighters Deluxe - [Buy here](https://store.steampowered.com/app/855860/Superfighters_Deluxe/)
* Visual Studio - [Download Link](https://www.google.com/search?q=download+visual+studio+community).

### Starting Up

1. Startup the solution file "*.sln" at the root of this repo. This should open Visual Studio along with all the scripts.
2. Navigate to the file you want to edit using the Solution Explorer.
3. You can start reading the code and getting familiar with it.

> This topic in the Official Forums will help you get started on developing with Visual Studio for the game if you have any problems at this stage [Developing a script for SFD in Visual Studio](https://www.mythologicinteractiveforums.com/viewtopic.php?t=1588).

### Developing

Once your file is open and you know what you want to do, it is time to start coding!

> The C# API for the game is accesible from the map editor.  Click on "Script" at the top-right corner next to "Common", then press `F1` or click on  `Script API`. You can also find a version online of the documentation tied to this repo [by clicking here](https://juansero29.github.io/SFDScripts/).

Check again the Code of Conduct for the coding rules to keep them in mind while coding, but also have fun :) This is the only fun part of all this mess ^^

### Debugging

You can debug these scripts with breakpoints and variables information by attaching SFD in debug to Visual Studio.
Take a look at the post concerning debug on the official forums: [Debugging a script for SFD in Visual Studio](https://mythologicinteractiveforums.com/viewtopic.php?f=15&t=2366).

Here's a recap of what is in said post:

1. Set the DEVENV in `config.ini` file to your Visual Studio `.exe`
    * `config.ini` is located by default in your `Documents` folder. (e.g `C:\Users\username\Documents\Superfighters Deluxe` or `C:\Users\username\OneDrive\Documents\Superfighters Deluxe`)
    * Your `devenv.exe` is located by default at `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe`
2. Make Sure Superfighters Deluxe and Visual Studio are both booted up.
3. From Visual Studio, attach the Superfighters Deluxe process.
    * Use CTRL + ALT + P or search "Attach to Process..." from the search bar to open the window
    * Then search for the SFD process and validate.
4. Run your map from the SFD Map Editor using `F5`
5. A debuggable file will be generated when you execute the map containing the script in the map executed.
    * To set breakpoints in any early code (like OnStartup) you can write `System.Diagnostics.Debugger.Break();` in the code. This will make the code break (stop) at this location.
    * To set breakpoints from Visual Studio, simply click on the left column between the left tabs and the number of the line in code.

Some work-arounds

* To inspect any variables from the ScriptAPI, you need to first assign any values to a local script variable and then inspect those.

### Testing

#### What does testing mean?

It means you have confirmed that the bug/feature you have worked on is having the expected behavior by executing the script directly in Superfighters Deluxe. It also means you have debuugged the script at least once to confirm the variables you worked on have the expected values and behavior.

#### How to test?

To test your script:

1. Copy the code in the script (inside the `Script To Copy` and paste it in the Script Editor inside the Map Editor of SFD (This is a pain in the ass, but that's how it's done)
2. Compile by pressing `F5` or clicking the compile button to check there's no errors in your script.
3. Close the editor and Test the map by pressing `F5` again or click the `▶` button in the top-left corner of the Map Editor.
4. Look-out for errors and check that everything works as expected.

> You can use `F6` or `Test > Test Options` to add more players to the map you are testing. (very useful for Hardcore)
<!---->
> Internal scripts should have an associated "Template Map" that will allow you to test them by putting your code inside them.

**For the moment no unit test are done in this repo.**

This sucks, but that's how it is for the moment, I haven't got the time to include them. Maybe in the future it will also be necessary to include a unit test to show that the feature/bug you solved, is actually solved. Or, if you're kind enough, you can start adding Unit Tests with a PR!

## Other Useful Links & Info

* The [ScriptLinker](https://github.com/NearHuscarl/ScriptLinker) may be worth checking out for easing out the development/testing process. I haven't used it yet.

## Getting Help

What would be of us, poor developers, if we couldn't get any help at all.

* Please google your question before pinging anyone else, and seek-out for a quick answer when possible.

* If you're still having trouble with C# or an specific C# API, please check the [official documentation]("https://docs.microsoft.com/en-us/dotnet/csharp/").

* If you're having trouble with the SFD API, be sure to check the documentation as pointed out in [developing](#developing).
* The [Superfighters Deluxe Script API](https://mythologicinteractiveforums.com/viewforum.php?f=15) forum may also be helpful. You can explore questions asked, or make a new post so others can help you. You can also make suggestions to developers so that they (maybe) make additions to the API according your needs.

* May you need quick help in your long voyage in the obscure paths of the code in this repo, you can ping me on discord: `Juansero29#8880` directly.

* You can also get help from other fellow scripters in the [Mythologic Interactive Discord Server](https://store.steampowered.com/news/app/855860/view/2880577391820768632).

# Contributing Maps

Hello map contributors! [This video](https://www.youtube.com/watch?v=tJksVsLieJw) will really help you understand how to contribute maps to Hardcore server in a painless way. The video is lenghty, but is well divided in chapters, so you can skim into any parts that specially interest you.
These are the sections explained in this video:

Chapters:

1. Getting Started
1. GitHub Account
1. Fork Repo (install git/git lfs)
1. Clone Repo
1. Create Issue
1. Create New Local Branch Linked To The Issue
1. Open Hardcore Template
1. Understand the only part of script concerning map creators
1. Understand each layer of the maps and its uses.
1. Player Skins
1. Drone Zones
1. AirStrike Markers
1. Menus Text
1. Player Start Point
1. Menu Camera Start Point
1. Maps Camera Position Markers
1. Player Spawn Positions
1. Capture Points
1. Going through each available variable in the Map Dependant Data part
1. Number Of Map Parts
1. Rounds Per Map Part
1. Current Map Part Index
1. World Top
1. Drone Area Begging / Drone Area Size
1. Camera Width / Camera Height
1. Testing map
1. Adding map to the repo
1. Pushing Changes  (git add . / git commit -m "Message" / git push)
1. Creating Pull Request
1. Validating PR (don't forget to put Solves or Closes issue  (number of the issue) in the description)
1. Waiting for review

# Thanks

At last but not least, thank you for wanting to contribute to this project and reading this long file. All my best wishes and have fun coding! :)
