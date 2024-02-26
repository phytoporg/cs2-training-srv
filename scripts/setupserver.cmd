@echo off
setlocal
set DEFAULT_SERVER_ROOT=C:\cs2-server-temp
set CONFIG_ROOT=%~dp0..\config
set GAMEINFO_CONFIG=%CONFIG_ROOT%\gameinfo.gi
set FAKE_RCON_CONFIG=%CONFIG_ROOT%\fake_rcon_config.gi
set GAMEMODE_TRAINING_CONFIG=%CONFIG_ROOT%\gamemode_training.cfg

if not exist "%CONFIG_ROOT%" (
  echo Config path not found: %CONFIG_ROOT%
  exit /b -1
)

set /p SERVER_ROOT=Input server root or press Enter for default (default=%DEFAULT_SERVER_ROOT%):
if "%SERVER_ROOT%"=="" set SERVER_ROOT=%DEFAULT_SERVER_ROOT%
echo Server Root = %SERVER_ROOT%

set DEL_SERVER_ROOT=
if exist %SERVER_ROOT% (
  set /p DEL_SERVER_ROOT=Server root already exists. Nuke and start fresh (Y/N^)?:
)

if /I "%DEL_SERVER_ROOT%"=="Y" (
  <NUL set /p="Deleting %SERVER_ROOT%... "
  rmdir /s /q %SERVER_ROOT%
  echo done!
)

set STEAMCMD_ARCHIVE_TARGET=%TMP%\steamcmd.zip
<NUL set /p="Downloading and extracting steamcmd... "
del %STEAMCMD_ARCHIVE_TARGET%
powershell -C "wget https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip -O %STEAMCMD_ARCHIVE_TARGET%" || goto Failed
powershell -C "Expand-Archive -LiteralPath '%STEAMCMD_ARCHIVE_TARGET%' -DestinationPath %TMP% -Force" || goto Failed
echo done!

echo Installing cs2 server to %DEFAULT_SERVER_ROOT%. This may take a while...
call "%TMP%\steamcmd.exe" +force_install_dir %SERVER_ROOT% +login anonymous +app_update 730 +quit
echo %ERRORLEVEL%
echo done!

set METAMOD_ARCHIVE_TARGET=%TMP%\steamcmd.zip
<NUL set /p="Downloading, extracting and installing metamod... "
del %METAMOD_ARCHIVE_TARGET%
powershell -C "wget https://mms.alliedmods.net/mmsdrop/2.0/mmsource-2.0.0-git1282-windows.zip -O %METAMOD_ARCHIVE_TARGET%" || goto Failed
powershell -C "Expand-Archive -LiteralPath '%METAMOD_ARCHIVE_TARGET%' -DestinationPath %SERVER_ROOT%\game\csgo -Force" || goto Failed
echo done!

<NUL set /p="Configuring metamod... "
copy "%GAMEINFO_CONFIG%" "%SERVER_ROOT%\game\csgo\gameinfo.gi"
echo done!

set CSS_ARCHIVE_TARGET=%TMP%\css.zip
<NUL set /p="Downloading and installing CounterStrikeSharp... "
del %CSS_ARCHIVE_TARGET%
powershell -C "wget https://github.com/roflmuffin/CounterStrikeSharp/releases/download/v175/counterstrikesharp-build-175-windows-a5399dd.zip -O %CSS_ARCHIVE_TARGET%" || goto Failed
powershell -C "Expand-Archive -LiteralPath '%CSS_ARCHIVE_TARGET%' -DestinationPath %SERVER_ROOT%\game\csgo -Force" || goto Failed
echo done!

set FAKERCON_ARCHIVE_TARGET=%TMP%\fake_rcon.zip
<NUL set /p="Downloading and installing fake_rcon... "
del %FAKERCON_ARCHIVE_TARGET%
powershell -C "wget https://github.com/Salvatore-Als/cs2-fake-rcon/releases/download/1.2.1a/windows.zip -O %FAKERCON_ARCHIVE_TARGET%" || goto Failed
powershell -C "Expand-Archive -LiteralPath '%FAKERCON_ARCHIVE_TARGET%' -DestinationPath %SERVER_ROOT%\game\csgo -Force" || goto Failed
echo done!

<NUL set /p="Configuring fake_rcon... "
copy "%GAMEMODE_TRAINING_CONFIG%" "%SERVER_ROOT%\game\csgo\cfg\gamemode_training.cfg" || goto Failed
echo done!

set CS2_EXE=%SERVER_ROOT%\game\bin\win64\cs2.exe
<NUL set /p="Creating shortcuts... "
powershell -C "cd %~dp0; .\set-shortcut.ps1 -SourceExe '%CS2_EXE%' -DestinationPath '%HOMEPATH%\Desktop\cs2_server_inferno_training.lnk' -Arguments '-dedicated -insecure -usercon +game_type 2 +game_mode 0 +map de_inferno'" || goto Failed
powershell -C "cd %~dp0; .\set-shortcut.ps1 -SourceExe '%CS2_EXE%' -DestinationPath '%HOMEPATH%\Desktop\cs2_server_mirage_training.lnk' -Arguments '-dedicated -insecure -usercon +game_type 2 +game_mode 0 +map de_mirage'" || goto Failed
echo done!

exit /b 0

:Failed
echo Command failed
exit /b -1
