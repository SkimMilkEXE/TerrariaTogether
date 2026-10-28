# Mod.Call API

Other mods talk to Terraria Together through `Mod.Call`, so they never need a hard dependency on it.

```csharp
if (ModLoader.TryGetMod("TerrariaTogether", out Mod smp))
{
    int version = (int)smp.Call("GetApiVersion");
    bool gating = (bool)smp.Call("IsModuleEnabled", "gating");
}
```

**Current API version: 1**. The version goes up when a call is renamed or removed. Adding a call doesn't change it.

Unknown calls return `null` and log a warning.

## Core calls

| Call | Arguments | Returns |
|------|-----------|---------|
| `GetApiVersion` | none | `int`: the API version |
| `IsModuleEnabled` | `string moduleId` (e.g. `"gating"`) | `bool`: `true` if the module exists and is turned on |

## Module calls

Module calls are named `"<Module>.<Call>"`, for example `"Gating.RegisterBoss"`. They are documented below as each module is added.

Module calls still work while that module is turned off, so a mod's registrations aren't lost if the server owner turns the module back on later.

## Gating

Most mods don't need any code: if your boss is in [Boss Checklist](https://steamcommunity.com/sharedfiles/filedetails/?id=2669644269), Terraria Together imports it automatically. Modded NPCs with `NPC.boss = true` that aren't registered anywhere fall under the server's **Unknown bosses** setting.

Register directly when you want a boss gated without Boss Checklist, or when the import gets something wrong. A direct registration wins over the Boss Checklist import for the same NPCs.

```csharp
public override void PostSetupContent()
{
    if (!ModLoader.TryGetMod("TerrariaTogether", out Mod smp))
        return;

    smp.Call("Gating.RegisterBoss",
        "MyMod/StormDragon",                                  // key: "ModName/BossName", never change it
        Mod.GetLocalization("NPCs.StormDragon.DisplayName"),  // LocalizedText or plain string
        new[] { ModContent.NPCType<StormDragonHead>(), ModContent.NPCType<StormDragonBody>() },
        new[] { ModContent.ItemType<StormCaller>() },          // optional: summon items
        (Func<bool>)(() => DownedSystem.downedStormDragon));  // optional: has it been beaten
}
```

Call it in `PostSetupContent` (or `Load` of a `ModSystem`). Registrations made after `AddRecipes` may miss the config's boss list.

| Call | Arguments | Returns |
|------|-----------|---------|
| `Gating.RegisterBoss` | `string key` (`"ModName/BossName"`), `LocalizedText` or `string` display name, NPC types (`int` or any `IEnumerable<int>`), optional summon items (same), optional `Func<bool>` downed check | `bool`: `false` if the key is taken or arguments are missing (a warning is logged) |
| `Gating.IsLocked` | `string key` | `bool`: whether the boss is locked right now (`false` in single player or with gating off); `null` if the key isn't registered |

Notes:
- **NPC types:** list every NPC that belongs to the fight. While a boss is locked, any of them is stopped from spawning unless a fight with that boss is already running, so segments and minions spawned mid-fight are never touched.
- **Summon items** let players keep their item when the boss is locked. Without them the boss is still stopped, but the item is used up.
- **Downed check:** without one, the server's **First kill only** setting can't apply to your boss.
- **Keys** are saved in worlds and configs. Changing one loses admin overrides and config switches for that boss.
