$scriptFile = Get-Item "..\Hardcore.cs"
$files = Get-ChildItem "."
$myshell = New-Object -com "Wscript.Shell"

foreach ($file in $files) {
    if ($file.PSIsContainer) { continue }
    SFDScriptInjector.exe $scriptFile.FullName $file.FullName 
    $myshell.sendkeys("{ENTER}")
} 