# Config and commands

## Server Settings

Edit in game from **Settings** → **Mod Configuration** → **Terraria Together** → **Server Settings** (admins only), or on the server in
`Documents/My Games/Terraria/tModLoader/ModConfigs/TerrariaTogether_CoreServerConfig.json`.

| Option | Default | What it does |
|--------|---------|--------------|
| **Admins** | empty | Player names that can run admin commands and change server settings in game. Names must match exactly. |

The server console is always an admin.

The file only appears once settings have been saved, so on a fresh server it won't exist yet. To add the first admin, either:
- create the file yourself:
  ```json
  {
    "Admins": [ "YourCharacterName" ]
  }
  ```
- or open **Server Settings** in a single-player world (you're always admin there), add the name and click **Save**.

Restart the server afterwards.

## Commands

Commands run on the server in multiplayer and also work in the server console (without the leading `/`).

| Command | Who | What it does |
|---------|-----|--------------|
| `/smp help` | everyone | Lists the commands |
| `/smp modules` | everyone | Lists modules and whether each one is on |
| `/smp whoami` | everyone | Shows your name and whether you're an admin |
| `/smp <module> ...` | depends | Runs a module's commands. Turned-off modules refuse their commands. |
