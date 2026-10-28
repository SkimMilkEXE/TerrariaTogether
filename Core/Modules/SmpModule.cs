using System.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaTogether.Core.Modules
{
	// base class every smp module derives from
	public abstract class SmpModule
	{
		// short lowercase id used in configs, packets, commands and the api
		public abstract string Id { get; }

		// on/off comes from the Modules table in CoreServerConfig
		public bool Enabled => ModuleManager.IsEnabled(Id);

		// handles /smp <id> <args...>, args has the module id already stripped
		public virtual void HandleCommand(CommandCaller caller, string[] args)
		{
			caller.Reply(Language.GetTextValue("Mods.TerrariaTogether.Commands.NoModuleCommands", Id));
		}

		// handles a packet routed here by SmpPackets
		public virtual void HandlePacket(byte msgType, BinaryReader reader, int fromWho) { }

		// handles Mod.Call("<Module>.<name>", args...), returns null if not handled
		public virtual object Call(string name, object[] args) => null;
	}
}
