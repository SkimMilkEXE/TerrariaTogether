# Testing

Multiplayer rules only run on a server, so most testing needs a dedicated server plus at least one client on the same PC.

## Local server setup

1. Build the mod: tModLoader → **Workshop** → **Develop Mods** → **Build**, or `dotnet build` in this folder.
2. Start the server: run `start-tModLoaderServer.bat` in the tModLoader install folder
   (Steam → tModLoader → **Manage** → **Browse local files**).
3. In the server console, pick the world (or create a small test world), leave the port at `7777`, and skip the password.
   The server uses the same enabled-mods list as the client, so make sure **Terraria Together** is enabled in the client's **Mods** menu first.
4. Start tModLoader normally → **Multiplayer** → **Join via IP** → `127.0.0.1`, port `7777`.

To test with two players, start a second client with `start-tModLoader-FamilyShare.bat` (it skips Steam, so it runs alongside the Steam client).

Logs: `tModLoader-Logs/` in the tModLoader install folder (`server.log` and `client.log`). `Documents/My Games/Terraria/tModLoader/Logs` is a shortcut to it.

## Phase 0 check

- [ ] Server starts with the mod enabled, no errors in `server.log`
- [ ] A client joins and can play
- [ ] Localization file loads without errors in either log

## Test matrix

Filled in as features land. Each row is run on a dedicated server with one admin and one non-admin client.

Note: two clients on the same PC may send the same ID and count as one member. Check `/smp whoami` on both.

### Core

| Test | Expected |
|------|----------|
| Console: `smp modules` | Lists modules, or "No modules are installed yet." |
| Non-admin saves **Server Settings** | Refused with "Only server admins can do that." |
| Join, then `/smp whoami` | Shows name and an 8-character ID (not "not received yet") |
| Admin by ID in **Admins**, `/smp whoami` | Says admin |
| New world, empty **Admins**, first player joins, `/smp whoami` | Says admin; their 8-character ID is now in **Admins**; `server.log` has "added <name> (<id>) to admins" |
| Remove that ID from **Admins**, rejoin | Not admin any more, and not added back |
| Second player joins the same world | Not admin |
| Restart server, owner rejoins | Still admin |
| Auto mode: join, `/smp roster` | You're listed as online, "1 of 1 members online" |
| Leave, console: `smp roster` | Listed with "last seen 0 days ago", "0 of 1" |
| Restart server, console: `smp roster` | Roster is still there |
| Manual mode: new player joins | Not added; `/smp roster add <name>` adds them |
| `/smp roster exclude <name>` | Member no longer counted; again to undo |
| Non-admin: `/smp roster add <name>` | Refused |

### Gating

Solo setup: one member online, defaults (75%, min 2), so everything is locked. Set **Minimum online** to 1 to unlock.

| Test | Expected |
|------|----------|
| Locked: use Suspicious Looking Eye at night | Not used up, chat says "Eye of Cthulhu is locked: 1/1 online (need 2)." (once, not spammed) |
| Unlocked (min online 1): use Suspicious Looking Eye at night | Eye of Cthulhu spawns normally |
| Locked: change min online back to 2 in game, try again | Locked again without rejoining |
| Locked: `/smp roster exclude` yourself, then unlock settings | Still locked (0 online) |
| Locked: talk to the Clothier at night ("Curse") | Skeletron spawns, is sent away at once, server chat explains why |
| Locked: Mechanical Eye | Not used up, "The Twins is locked..." |
| Turn Gating off in **Modules** (Server Settings) | Everything summons normally, `/smp modules` shows gating: off |
| Single player | Nothing is ever locked |
| Hover Suspicious Looking Eye (locked / unlocked) | Tooltip line "Locked: 1/1 online (need 2)" in red / "Unlocked" in green |
| Set **Minimum online** to 1 | Server chat: "Bosses are now unlocked: 1/1 members online (need 1)." Back to 2: "locked again" |
| `/smp gating status` | Header with counts, one line per boss with locked/unlocked and the reason |
| Beat Eye of Cthulhu, then lock settings again | EoC stays unlocked ("already beaten"); turn **First kill only** off and it locks |
| `/smp gating unlock eye of cthulhu` while locked | Announced; EoC can be summoned; status says "unlocked by an admin" |
| `/smp gating lock king slime` while unlocked | Announced; Slime Crown blocked with "King Slime is locked by an admin." |
| Restart server, `/smp gating status` | Overrides are still there |
| `/smp gating clear king slime` | Back to normal rules |
| Non-admin: `/smp gating lock king slime` | Refused |
| **Simulated online** 3, **Simulated members** 4 | Status shows 3/4 online, need 3 |
| **Gated bosses**: turn King Slime off while locked | Slime Crown works; status says "not gated in config" |
| ...then `/smp gating lock king slime` | Locked again (commands beat the table) |
| Open **Server Settings** / **Gating** | **Modules** shows Gating, **Gated bosses** shows all 18 bosses by name, each with a toggle, no text boxes or +/- buttons. Save button lights up after a toggle; **Restore defaults** turns everything back on |

### Other mods (Phase 5)

Enable Calamity and Boss Checklist alongside Terraria Together, on both server and client.

| Test | Expected |
|------|----------|
| `server.log` after start | "gating: imported N bosses from boss checklist" with N > 0 |
| `/smp gating status` | Calamity bosses listed after the vanilla ones, e.g. Desert Scourge |
| **Gated bosses** in config | Calamity bosses listed with toggles |
| Locked: use Desert Medallion (Desert Scourge) in the desert | Not used up, "Desert Scourge is locked..." |
| Unlocked: Desert Scourge fight | All segments spawn normally, nothing is removed mid-fight |
| Unlock, start a fight, set **Minimum online** back up mid-fight | Fight continues untouched |
| Without Boss Checklist enabled | Log has no import line; Calamity bosses still blocked at spawn via **Unknown bosses** (status doesn't list them) |
| **Unknown bosses** = Ignore, without Boss Checklist | Calamity bosses spawn freely |

### Edge cases (Phase 6)

Bosses locked unless a row says otherwise. Hardmode rows need a hardmode test world.

| Test | Expected |
|------|----------|
| Throw a Guide Voodoo Doll into lava (Guide alive) | Doll floats on the lava, Guide lives, chat says Wall of Flesh is locked |
| Unlock, throw it again | Wall of Flesh spawns as normal |
| Lihzahrd Power Cell on Golem's altar | Golem sent away, the power cell comes back to you |
| Fish with a Truffle Worm in the ocean | Duke sent away, the truffle worm comes back |
| Wait for a night where Eye of Cthulhu spawns naturally (or `/time` cheats) | Eye sent away, no item appears |
| Goblin Battle Standard | Not used up, "Goblin Army is locked..." |
| Start an event while unlocked, then lock mid-event | Event keeps going |
| Save and quit during an unlocked event, lock, reload | Event keeps going |
| **Gated bosses** | Events listed under a "Terraria events" heading, after all the bosses |
| **Mid-fight** = Despawn after warning, warning 10 s: start a fight unlocked, then raise **Minimum online** | Warning in chat; boss and its parts leave 10 s later |
| Same, but lower **Minimum online** again before time runs out | "Enough members are online again, ... stays." |
| **Mid-fight** = Continue | Raising **Minimum online** mid-fight changes nothing |

### Discord log

Set up a webhook in a test channel first (see `CONFIG.md`).

| Test | Expected |
|------|----------|
| Start server without the webhook file | `server.log`: "no webhook file at ...", game works normally |
| File with junk in it | `server.log`: "doesn't contain a discord webhook url" |
| Valid file, restart | `server.log`: "webhook loaded" |
| Join, then leave | "**you** joined the server" / "left the server" in Discord |
| Say something in chat | "**you**: message" |
| Party chat, `/smp help` | Nothing posted |
| Type `@everyone hi` and `**bold**` | Posted as plain text, nobody pinged, no bold |
| Die | Death message posted |
| Kill Eye of Cthulhu (unlocked) | "**Eye of Cthulhu** defeated for the first time by you!" |
| Kill it again | Nothing posted |
| The Twins | One post, "Retinazer & Spazmatism" |
| Turn each **Post ...** option off | That kind of message stops |
| Turn Discord log off in **Modules** | Nothing posted |
| Unplug the internet / wrong URL | Warnings in `server.log`, game keeps running |
| Admin: `/smp discord url clear`, then `/smp discord` | "No Discord webhook is set", file deleted, nothing posted |
| Admin: `/smp discord url <webhook url>` | "Discord webhook saved", next join/chat is posted without a restart, file written |
| Admin: `/smp discord url hello` | "That isn't a Discord webhook URL", old webhook unchanged |
| Non-admin: `/smp discord` | Refused |
| Console: `smp discord url <webhook url>` | Works the same |

### Welcome and rules

| Test | Expected |
|------|----------|
| Join | Gold "Welcome to <world>, <you>!" then "Server rules:" with 3 numbered rules |
| Leave and join again | Welcome only, no rules |
| Change **Welcome message**, rejoin | New text, `{name}` and `{world}` filled in |
| Turn **Show welcome message** / **Show rules** off, rejoin | Nothing shown for the one turned off |
| Edit a rule while online | Everyone online sees "The server rules have changed." and the new rules |
| **Must accept rules to build** on, new player joins | Rules plus "Type /smp rules accept..." |
| ...try to place a block, dig, chop a tree | Blocked, "Accept the rules before building..." |
| ...`/smp rules accept`, then build | "Thanks for accepting..." and building works |
| Rejoin after accepting | Welcome only, building works |
| Edit a rule | Building blocked again until `/smp rules accept` |
| Turn **Must accept rules to build** off | Building works straight away without accepting |
| Single player with it on | Never blocked |
| Console: `smp rules` | Lists the rules |

### Stats

Turn Gating off (or unlock bosses) for the boss rows.

| Test | Expected |
|------|----------|
| Play 2 minutes, `/smp stats` | "Stats for <you>:", playtime 0h 2m |
| Die to anything, `/smp stats` | Deaths 1 |
| Two players, PvP on, one kills the other | Killer: PvP kills 1. Victim: deaths 1 |
| Kill Eye of Cthulhu with two players hitting it | Both get bosses killed 1 |
| Player who never hit the boss | Bosses killed unchanged |
| The Twins, Eater of Worlds, The Destroyer, Golem, Moon Lord | Each counts as **1** boss kill, not one per part |
| `/smp stats <other player>` | Their stats; unknown name says "No stats for a player named ..." |
| `/smp stats top`, `top deaths`, `top bosses`, `top pvp` | Up to 5 names, highest first; `top nonsense` shows usage |
| Console: `smp stats` | Usage; `smp stats <name>` works |
| Restart server | Stats still there |
| Turn Stats off in **Modules** | `/smp stats` refused, nothing counts; on again and old stats are back |
