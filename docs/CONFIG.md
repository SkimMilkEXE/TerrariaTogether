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

The first player to join a world (the host, for Host & Play) is added to **Admins** automatically, by their player ID. This only happens once per world, so removing them from the list afterwards sticks. On a world from before this was added, the next player to join is the one added, so join first yourself.

The file only appears once settings have been saved, so on a fresh server it won't exist yet. The world owner can add admins in game. To add one without joining, either:
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
| **Unknown bosses** | Gate | Modded NPCs flagged as bosses that neither Boss Checklist nor their own mod registered. **Gate** uses the shared threshold (but **First kill only** can't apply, and their summon items aren't blocked). **Ignore** lets them spawn freely. |
| **Mid-fight** | Continue | What happens when a boss becomes locked during its fight (someone left). **Continue** lets it finish. **Despawn after warning** warns everyone, then sends the boss away if the group is still short when time runs out. |
| **Mid-fight warning (seconds)** | 30 | How long the boss stays after the warning. |
| **Gated bosses** | all on | One on/off switch per boss, by name. Turn a boss off to never lock it. Bosses from other mods appear here automatically. |
| **Simulated online** (Debug) | -1 | Pretend this many members are online, for testing thresholds solo. -1 turns it off. |
| **Simulated members** (Debug) | -1 | Pretend the member total is this. -1 turns it off. |

Admin overrides from `/smp gating lock|unlock` beat every setting above, including **Gated bosses**. They're saved with the world, while the config applies to every world on the server.

Order a boss's state is decided in: command override → **Gated bosses** → **First kill only** → threshold.

Members needed = the larger of **Minimum online** and **Threshold** × member total (rounded up).
For example, with 6 members at 75%: 6 × 0.75 = 4.5 → **5 needed**.
Only members count as online: excluded members and players who aren't on the roster don't.

When a boss or event is locked:
- its summon item's tooltip says "Locked: 1/2 online (need 2)" (or "Unlocked")
- its summon item can't be used, so it isn't lost, and you see "Eye of Cthulhu is locked: 1/2 online (need 2)."
- if it spawns some other way, it's sent away straight after spawning and the whole server sees why. If a player summoned it (Lihzahrd Power Cell on Golem's altar, Truffle Worm, or a summon item used before their client knew about the lock), the item is given back. Natural spawns (Eye of Cthulhu at night, mechanical bosses) are simply sent away.
- a Guide Voodoo Doll thrown into lava floats on the surface instead of sinking, so the Guide survives and Wall of Flesh doesn't spawn.
- an event (Goblin Army, Frost Legion, Pirates, Martian Madness, Pumpkin Moon, Frost Moon, Old One's Army) is stopped the moment it starts. Events that started while unlocked always run to the end. Event summon items are blocked like boss summon items, except the Eternia Crystal, which is used up.

### Bosses from other mods

- **With [Boss Checklist](https://steamcommunity.com/sharedfiles/filedetails/?id=2669644269) installed**, every modded boss it knows about is gated automatically and shows up in **Gated bosses** and `/smp gating status`. Most big content mods (Calamity, Thorium, ...) register with Boss Checklist.
- **Mods can also register directly** through `Mod.Call` (see `API.md`).
- **Anything else** flagged as a boss follows **Unknown bosses**.

A locked boss only stops new fights. Minions and segments spawned during a fight that's already running are never removed.

## Welcome and rules

Greets players when they join and shows the server rules.

File: `ModConfigs/TerrariaTogether_RulesConfig.json` (admins can change it in game).

| Option | Default | What it does |
|--------|---------|--------------|
| **Show welcome message** | on | Sends the welcome message to each player every time they join. |
| **Welcome message** | `Welcome to {world}, {name}!` | `{name}` becomes the player's name, `{world}` the world's name. |
| **Show rules** | on | Shows the rules on a player's first join, and again whenever the rules change. |
| **Rules** | 3 starter rules | One rule per line, numbered when shown. |
| **Must accept rules to build** | off | Players can't place or break blocks or walls until they type `/smp rules accept`. The rules show on every join until they do. When the rules change, everyone has to accept again. Single-player worlds are never blocked. |

Who has seen and accepted which version of the rules is saved with the world.

## Discord log

Posts server activity to a Discord channel, so people can follow along without being in game.

**Setup (one time):**
1. In Discord: channel settings → **Integrations** → **Webhooks** → **New Webhook** → **Copy Webhook URL**.
2. As an admin in game (or in the server console without the `/`), type `/smp discord url <paste the url>`. Posts start straight away.

Or, instead of step 2, create
`Documents/My Games/Terraria/tModLoader/ModConfigs/TerrariaTogether_DiscordWebhook.txt` on the server machine,
paste the URL into it (nothing else) and restart the server. The command writes this same file.

The URL is a password for the channel: anyone with it can post there. That's why it lives in its own file on the server and not in the settings, which get sent to every player. The command is only seen by the server (and stays in your own chat history until you leave). Never paste the URL into normal chat. If it leaks, delete the webhook in Discord and make a new one.

**Don't share `server.log` without editing it first.** tModLoader writes every web request any mod makes into the log, so each Discord post leaves a `Web Request: https://discord.com/api/webhooks/...` line with the full URL. Delete those lines before posting the log anywhere, e.g. when asking for help.

File: `ModConfigs/TerrariaTogether_DiscordConfig.json` (admins can change it in game).

| Option | Default | What it does |
|--------|---------|--------------|
| **Post joins and leaves** | on | "**Steve** joined the server" / "left the server" |
| **Post chat** | on | Public chat only. Party chat and commands are never posted. |
| **Post deaths** | on | The death message, e.g. "Steve was slain by Zombie." |
| **Post first boss kills** | on | "**Eye of Cthulhu** defeated for the first time by Steve, Alex!" Only the first kill of each boss in this world, with everyone who hit it. |

Nothing is posted in single player. Mentions are switched off, so `@everyone` typed in game doesn't ping anyone.
Bosses beaten before this module was turned on are announced once more on their next kill.

## Stats

Tracks each player's playtime, deaths, boss kills and PvP kills in this world. No settings, turn it on or off under **Modules**.
Stats are saved with the world and kept while the module is off.

- **Playtime** counts while the player is in the world.
- **Boss kills** go to every player who hit the boss, the same rule the game uses for loot. Bosses made of several parts (The Twins, Eater of Worlds) count once.
- Stats follow the player's ID, so renaming a character keeps them. Anyone who joins gets stats, members or not.

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
| `/smp rules` | everyone | Shows the server rules |
| `/smp rules accept` | everyone | Accepts the rules, so you can build when **Must accept rules to build** is on |
| `/smp discord` or `/smp discord status` | admins | Says whether a Discord webhook is set (never shows the URL) |
| `/smp discord url <webhook url>` | admins | Saves the webhook and starts posting |
| `/smp discord url clear` | admins | Removes the webhook, nothing is posted |
| `/smp stats [player]` | everyone | Shows your stats, or another player's |
| `/smp stats top [playtime\|deaths\|bosses\|pvp]` | everyone | Top 5 players for that stat (playtime if left out) |
| `/smp <module> ...` | depends | Runs a module's commands. Turned-off modules refuse their commands. |
