# Modules

Every feature of Terraria Together is a module that server owners can switch on or off in
**Server Settings** → **Modules**. This page lists what exists and what might come next.

Ideas here aren't promises. A module gets built once it's been picked and added to the roadmap.
New modules follow Gating's shape: setup → MVP → config and commands → edge cases.

| Status | Meaning |
|--------|---------|
| **Done** | Shipped and switchable |
| **Next** | Picked for the next release |
| **Idea** | Possible, not planned yet |

## Done

### Gating (`gating`)
Bosses and events stay locked until enough of the group is online, so nobody gets ahead of
everyone else. Works with modded bosses through Boss Checklist and `Mod.Call`.
See `CONFIG.md` for its settings.

### Playtime and stats (`stats`)
Playtime, deaths, boss kills and PvP kills per player, shown with `/smp stats [player]`
and a top 5 with `/smp stats top <stat>`.

### Welcome and rules (`rules`)
A configurable welcome message on every join, server rules on first join and whenever they change,
and an optional accept-rules step (`/smp rules accept`) before players can build.

### Discord log (`discord`)
Posts joins, leaves, public chat, deaths and each boss's first kill to a Discord channel through a webhook.

## Small

### AFK detection (Core) — Idea
Members who haven't moved or acted for a set number of minutes stop counting as online.
Stops a character idling at spawn from unlocking bosses. This lives in Core (presence)
rather than in a module, so Gating and anything else that counts online members benefit.

### Spawn protection (`spawn`) — Idea
Only admins can break or place blocks within a radius of world spawn.
A small first use of the block-protection code that land claims would need.

### Death drop protection (`deathdrops`) — Idea
Items dropped on death can only be picked up by their owner for the first few minutes.

### PvP rules (`pvp`) — Idea
Force PvP off, make it opt-in with a cooldown before switching, or turn it off near spawn.

### Persistent teams (`teams`) — Idea
Remembers each member's team color and puts them back on it when they join.

## Medium

### Grief log (`grieflog`) — Idea
Records who broke or placed blocks and who opened chests, by player ID and position.
Admins look things up with `/smp log here` or `/smp log <player>`.
Doesn't stop anything on its own; it settles disputes, and claims and rollback build on it later.
Main challenge: keeping the saved data small.

### Votes (`votes`) — Idea
`/smp vote <thing>` for group decisions: skip the night, start an event, let someone fight a boss early.

## Large

### Chest locking (`chests`) — Idea
Lock a single chest to yourself and your team. Covers most theft complaints without full claims.

### Land claims (`claims`) — Idea
Members claim rectangles. Only the owner and players they trust can build or open chests inside.
The hardest module here: Terraria clients change blocks instantly, so the server has to reject
edits and send the correct blocks back. Explosives, liquids and wiring all need handling.
Best built after the grief log.

### Progression extensions (`progression`) — Idea
Lock more than bosses: Hardmode entry, biome keys, specific events.
Wall of Flesh gating already covers most of Hardmode, so this waits until there's a need.
