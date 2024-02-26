# cs2-training-srv
Experimenting with CS2 training server plugin stuff.

## SETUP
### References
- [Valve SteamCMD wiki page](https://developer.valvesoftware.com/wiki/SteamCMD#Downloading_SteamCMD)
- [CounterStrikeSharp "Getting Started" guide](https://docs.cssharp.dev/docs/guides/getting-started.html)

### Instructions
- Download and extract [steamcmd](https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip) to your favorite directory
- run `steamcmd.exe` from the CLI
- From the steamcmd prompt: `login <your_steam_username>` and provide your steam login credentials
- `force_install_dir <directory_for_server>` to set the server install directory. You'll get a warning about calling this before login. Ignore it.
- `app_update 730 validate` to install the server. Let it download, it might take a bit.
- Download the latest build of [metamod](https://www.sourcemm.net/downloads.php/?branch=master)
- Extract the `addons` directory into `<directory_for_server>/game/csgo/addons`
- Open `<directory_for_server>/game/csgo/gameinfo.gi` in your favorite text editor
- Create a new line underneath `Game_LowViolence  csgo_lv` and add `Game  csgo/addons/metamod`. Your `gameinfo.gi` should look like [this](https://docs.cssharp.dev/images/gameinfogi-example.png)
- Download [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases) and copy the `/addons` directory to `/game/csgo/`
- Once connected to the server, verify that `CounterStrikeSharp` is loaded with the `meta list` command.
