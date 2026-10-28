using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaTogether.Core.Roster
{
	// tracks which client uuid sits in which player slot, server only
	public class PresenceTracker : ModSystem
	{
		private readonly string[] slotUuids = new string[Main.maxPlayers];

		public static PresenceTracker Instance => ModContent.GetInstance<PresenceTracker>();

		// server only, for modules that react to players joining and leaving
		public event Action<Player> PlayerJoined;
		public event Action<Player> PlayerLeft;

		// first 8 characters of a uuid, short enough to type into the admins list
		public const int ShortIdLength = 8;

		public static string ShortId(string uuid) => uuid.Length > ShortIdLength ? uuid[..ShortIdLength] : uuid;

		// the server normally reads and throws away the uuid message, so peek at it first
		public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			if (messageType != MessageID.ClientUUID || Main.netMode != NetmodeID.Server || playerNumber < 0 || playerNumber >= Main.maxPlayers)
				return false;

			string uuid;
			long start = reader.BaseStream.Position;
			try
			{
				uuid = reader.ReadString();
			}
			catch (Exception)
			{
				return false; // malformed message, let terraria deal with it
			}
			finally
			{
				// rewind so terraria still reads the message normally
				reader.BaseStream.Position = start;
			}

			// clients send whatever they like, only accept real guids
			if (!Guid.TryParse(uuid, out Guid guid))
				return false;

			// stored in one format so ShortId and comparisons line up
			uuid = guid.ToString();
			slotUuids[playerNumber] = uuid;
			RosterSystem.Instance.OnJoin(uuid, Main.player[playerNumber].name);
			PlayerJoined?.Invoke(Main.player[playerNumber]);
			return false;
		}

		public void OnLeave(Player player)
		{
			string uuid = slotUuids[player.whoAmI];
			if (uuid == null)
				return;

			RosterSystem.Instance.OnLeave(uuid, player.name);
			PlayerLeft?.Invoke(player);
			slotUuids[player.whoAmI] = null;
		}

		// null until the player's uuid has arrived
		public string GetUuid(Player player)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				return Main.clientUUID;
			return slotUuids[player.whoAmI];
		}

		public bool IsOnline(string uuid)
		{
			// no server in single player, so the only one online is the local player
			if (Main.netMode == NetmodeID.SinglePlayer)
				return uuid == Main.clientUUID;

			for (int i = 0; i < slotUuids.Length; i++)
			{
				if (slotUuids[i] == uuid && Netplay.Clients[i].IsActive)
					return true;
			}
			return false;
		}

		public override void ClearWorld()
		{
			Array.Clear(slotUuids);
		}
	}

	// PlayerDisconnect runs on the server, PlayerConnect doesn't, so joins come from the uuid message instead
	public class PresencePlayer : ModPlayer
	{
		public override void PlayerDisconnect()
		{
			if (Main.netMode == NetmodeID.Server)
				PresenceTracker.Instance.OnLeave(Player);
		}
	}
}
