using System.IO;
using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;

namespace TerrariaTogether.Core.Networking
{
	// every packet starts with (moduleId, msgType) so it can be routed to its module
	public static class SmpPackets
	{
		// starts a packet for a module, the caller writes the body and calls Send
		public static ModPacket Create(SmpModule module, byte msgType)
		{
			ModPacket packet = ModContent.GetInstance<TerrariaTogether>().GetPacket();
			packet.Write(module.Id);
			packet.Write(msgType);
			return packet;
		}

		public static void Handle(Mod mod, BinaryReader reader, int fromWho)
		{
			string moduleId = reader.ReadString();
			byte msgType = reader.ReadByte();

			SmpModule module = ModuleManager.Instance.Get(moduleId);
			if (module == null)
			{
				mod.Logger.Warn($"packet for unknown module '{moduleId}' (type {msgType}) from {fromWho}");
				return;
			}

			// disabled modules ignore their packets
			if (!module.Enabled)
				return;

			module.HandlePacket(msgType, reader, fromWho);
		}
	}
}
