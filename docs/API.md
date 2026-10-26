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
