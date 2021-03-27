$scriptFile = Get-Item ".\SFDScripts\Internal\Hardcore\Hardcore.cs"
$maps = Get-ChildItem ".\SFDScripts\Internal\Hardcore\Maps\"

foreach ($map in $maps) {
    if ($map.PSIsContainer) { continue }
    SFDScriptInjector.exe $scriptFile.FullName $map.FullName 
    Copy-Item $map.FullName "C:\Users\juans\OneDrive\Documents\Superfighters Deluxe\Maps\Custom"
} 