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
| Auto mode: join, `/smp roster` | You're listed as online, "1 of 1 members online" |
| Leave, console: `smp roster` | Listed with "last seen 0 days ago", "0 of 1" |
| Restart server, console: `smp roster` | Roster is still there |
| Manual mode: new player joins | Not added; `/smp roster add <name>` adds them |
| `/smp roster exclude <name>` | Member no longer counted; again to undo |
| Non-admin: `/smp roster add <name>` | Refused |
