# cs2-training-srv
Experimenting with CS2 training server plugin stuff.

## SETUP
### References
- [Valve SteamCMD wiki page](https://developer.valvesoftware.com/wiki/SteamCMD#Downloading_SteamCMD)
- [CounterStrikeSharp "Getting Started" guide](https://docs.cssharp.dev/docs/guides/getting-started.html)
- [fake_rcon guide](https://forums.alliedmods.net/showpost.php?p=2811082&postcount=15)

### Instructions
- Clone this repository into your favorite directory
- Run `<your_favorite_directory>\cs2-training-srv\scripts\setupserver.cmd`, answer all prompts
- Run any of the shortcuts created on your desktop
- Launch CS2 from steam
- Once the game is up and running, press tilde (`~`) to bring up the console
- Type `connect localhost` and you will connect to the server
- Use `fake_rcon_password password` to authenticate for using admin commands
- Issue admin commands (like `sv_cheat 1`) by prefixing with `fake_rcon`. For example: `fake_rcon sv_cheat 1`.
