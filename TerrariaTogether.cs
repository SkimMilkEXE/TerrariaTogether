using System.IO;
using Terraria.ModLoader;
using TerrariaTogether.Core.Api;
using TerrariaTogether.Core.Networking;

namespace TerrariaTogether
{
	public class TerrariaTogether : Mod
	{
		public override object Call(params object[] args) => ApiDispatcher.Call(this, args);

		public override void HandlePacket(BinaryReader reader, int whoAmI) => SmpPackets.Handle(this, reader, whoAmI);
	}
}
