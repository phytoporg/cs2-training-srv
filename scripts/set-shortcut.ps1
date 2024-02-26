param ( [string]$SourceExe, [string]$DestinationPath, [string]$Arguments)

$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($DestinationPath)
$Shortcut.TargetPath = $SourceExe
$Shortcut.Arguments = $Arguments
$Shortcut.Save()
