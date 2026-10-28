# Config and commands

## Server Settings

Edit in game from **Settings** → **Mod Configuration** → **Terraria Together** → **Server Settings** (admins only), or on the server in
`Documents/My Games/Terraria/tModLoader/ModConfigs/TerrariaTogether_CoreServerConfig.json`.

| Option | Default | What it does |
|--------|---------|--------------|
| **Modules** | all on | One on/off switch per module (currently **Gating**). New modules appear here automatically. |
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

## Gating

Bosses stay locked until enough members are online. Single-player worlds are never gated.

File: `ModConfigs/TerrariaTogether_GatingConfig.json` (admins can change it in game, same as Server Settings).

| Option | Default | What it does |
|--------|---------|--------------|
| **Threshold (%)** | 75 | Percent of members that must be online to fight a boss. |
| **Minimum online** | 2 | Bosses stay locked with fewer than this many members online, whatever the percent says. |
| **First kill only** | on | Once a boss has been beaten it stays unlocked, so it can be farmed with any number of players. |
| **Announce changes** | on | Server chat message when bosses lock or unlock, and when an admin locks or unlocks a boss. |
| **Gated bosses** | all on | One on/off switch per boss, by name. Turn a boss off to never lock it. Bosses from other mods appear here automatically. |
| **Simulated online** (Debug) | -1 | Pretend this many members are online, for testing thresholds solo. -1 turns it off. |
| **Simulated members** (Debug) | -1 | Pretend the member total is this. -1 turns it off. |

Admin overrides from `/smp gating lock|unlock` beat every setting above, including **Gated bosses**. They're saved with the world, while the config applies to every world on the server.

Order a boss's state is decided in: command override → **Gated bosses** → **First kill only** → threshold.

Members needed = the larger of **Minimum online** and **Threshold** × member total (rounded up).
For example, with 6 members at 75%: 6 × 0.75 = 4.5 → **5 needed**.
Only members count as online: excluded members and players who aren't on the roster don't.

When a boss is locked:
- its summon item's tooltip says "Locked: 1/2 online (need 2)" (or "Unlocked")
- its summon item can't be used, so it isn't lost, and you see "Eye of Cthulhu is locked: 1/2 online (need 2)."
- if it spawns some other way, it's sent away straight after spawning and the whole server sees why. Items used this way (power cell, truffle worm, voodoo doll) are lost for now.

## Commands

Boss names can be typed as the name (`eye of cthulhu`), without spaces (`eyeofcthulhu`) or as the full key (`Terraria/EyeOfCthulhu`). Case doesn't matter.

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
| `/smp gating` or `/smp gating status` | everyone | Shows online/needed and whether each boss is locked, and why |
| `/smp gating lock <boss>` | admins | Keeps a boss locked whatever the numbers say |
| `/smp gating unlock <boss>` | admins | Keeps a boss unlocked whatever the numbers say |
| `/smp gating clear <boss>` | admins | Puts a boss back on the normal rules |
| `/smp <module> ...` | depends | Runs a module's commands. Turned-off modules refuse their commands. |
