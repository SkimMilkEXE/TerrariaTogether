using System.IO;
using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;

namespace TerrariaTogether.Modules.Gating
{
	// bosses stay locked until enough members are online
	public class GatingModule : SmpModule
	{
		public const string ModuleId = "gating";

		public override string Id => ModuleId;

		// for globals, so they don't look the module up every call
		public static bool IsEnabled => ModuleManager.IsEnabled(ModuleId);

		public override void HandleCommand(CommandCaller caller, string[] args)
		{
			GateCommands.Handle(caller, args);
		}

		public override void HandlePacket(byte msgType, BinaryReader reader, int fromWho)
		{
			GatingSystem.Instance.HandlePacket(msgType, reader, fromWho);
		}
	}
}
