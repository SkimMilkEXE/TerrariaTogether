# Changelog

## 1.0.0

First release.

- **Core**: modules that admins switch on or off in **Server Settings**, an **Admins** list (by name or player ID, and the first player to join a world is added automatically), a member roster with **Auto** and **Manual** modes, inactive members dropping off after a set number of days, and `/smp help`, `/smp modules`, `/smp whoami` and `/smp roster`.
- **Gating**: bosses and events stay locked until enough members are online. Summon items aren't used up while locked. Includes a first-kill-only option, a per-boss on/off table, admin lock and unlock commands, tooltips, announcements, a mid-fight policy, and refunds for summons that get through. Modded bosses are gated through Boss Checklist, `Mod.Call` or the unknown-boss fallback.
- **Stats**: playtime, deaths, boss kills and PvP kills per player, with `/smp stats` and a top 5 leaderboard.
- **Welcome and rules**: a welcome message, server rules on first join and whenever they change, and an optional step where players must accept the rules before building.
- **Discord log**: joins, leaves, public chat, deaths and first boss kills are posted to a Discord channel through a webhook, which admins set with `/smp discord url`.
