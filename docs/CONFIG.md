# Config and commands

## Server Settings

Edit in game from **Settings** → **Mod Configuration** → **Terraria Together** → **Server Settings** (admins only), or on the server in
`Documents/My Games/Terraria/tModLoader/ModConfigs/TerrariaTogether_CoreServerConfig.json`.

| Option | Default | What it does |
|--------|---------|--------------|
| **Admins** | empty | Player names or player IDs that can run admin commands and change server settings in game. Names must match exactly. An ID is safer, since anyone can name a character after you. Players see their ID with `/smp whoami` (the first 8 characters of their client UUID; the full UUID works too). |
| **Roster mode** | Auto | **Auto** adds anyone who joins as a member. **Manual** only adds players an admin adds with `/smp roster add`. |
| **Inactive after (days)** | 14 | Members not seen for this many days stop counting toward the member total, until they come back. `0` means never. |

The roster itself (who's a member) is saved with the world, not in the config file.

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
| `/smp whoami` | everyone | Shows your name, ID and whether you're an admin |
| `/smp roster` | everyone | Shows "X of Y members online" and each member's status |
| `/smp roster add <name>` | admins | Adds an online player as a member (they must be online so the server knows their UUID) |
| `/smp roster remove <name>` | admins | Removes a member. In Auto mode they're added again next time they join. |
| `/smp roster exclude <name>` | admins | Stops a member counting toward online or total (e.g. an alt account). Run again to undo. |
| `/smp <module> ...` | depends | Runs a module's commands. Turned-off modules refuse their commands. |
