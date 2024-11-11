using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Katrox;
using System.Drawing;
using static System.Formats.Asn1.AsnWriter;

namespace Katrox
{
	public partial class Katrox
	{
		private static Dictionary<ulong, RopeState> _ropeStates = new();
		private static List<ulong> HasRopePlayers = new();

		private class RopeState
		{
			public bool IsEnabled { get; set; }
			public Vector? TargetIndex { get; set; }
			public float? Distance { get; set; }
			public CEnvBeam? Beam { get; set; }
		}

		private void Rope_Load()
		{
			AddCommand(Config.Rope.Rope1, "", RopeOne);
			AddCommand(Config.Hook.Hook0, "", RopeZero);

			foreach (var xC in Config.Rope.GiveTempRope) AddCommand(xC, "", RopeVer);
			foreach (var xC in Config.Rope.RemoveTempRope) AddCommand(xC, "", RopeSil);
		}

		[CommandHelper(1, "<target>")]
		public void RopeVer(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;
			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Grab.GivePermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			var target = info.GetArgTargetResult(1);

			if (target == null)
			{
				player?.PrintToChat(Config.Prefix + ChatColors.White + Localizer["TargetIsWrong"]);
				return;
			}

			target
				.ToList()
				.ForEach(x =>
				{
					if (!HasRopePlayers.Contains(x.SteamID))
					{
						HasRopePlayers.Add(x.SteamID);
					}
					Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminGave", callerName, x.PlayerName, "rope"]);
				});

		}

		[CommandHelper(1, "<target>")]
		public void RopeSil(CCSPlayerController? player, CommandInfo info)
		{
			var callerName = player == null ? "Console" : player.PlayerName;
			if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Grab.GivePermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			var target = info.GetArgTargetResult(1);

			if (target == null)
			{
				player?.PrintToChat(Config.Prefix + ChatColors.White + Localizer["TargetIsWrong"]);
				return;
			}

			target
				.ToList()
				.ForEach(x =>
				{
					if (HasRopePlayers.Contains(x.SteamID))
					{
						HasRopePlayers.RemoveAll(y => y == x.SteamID);
					}
					Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminDelete", callerName, x.PlayerName, "rope"]);
				});
		}

		public void RopeOne(CCSPlayerController? player, CommandInfo info)
		{
			if (PlayerIsValid(player) == false)
				return;
			
			if (!HasRopePlayers.Contains(player.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.Rope.UsePermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			if (!player.PawnIsAlive)
				return;

			if (_ropeStates.ContainsKey(player.SteamID) && _ropeStates[player.SteamID].IsEnabled)
			{
				return;
			}

			var clientloc = player.PlayerPawn?.Value?.AbsOrigin ?? VEC_ZERO;

			if (CustomRayTrace(player, out Vector? endPos) == false)
			{
				return;
			}

			_ropeStates[player.SteamID] = new RopeState
			{
				IsEnabled = true,
				TargetIndex = endPos,
				Distance = (clientloc - endPos).Length(),
				Beam = null
			};
		}

		public void RopeZero(CCSPlayerController? player, CommandInfo info)
		{
			if (PlayerIsValid(player) == false)
			{
				return;
			}

			DetachRope(player);
		}

		private void Rope_OnTick(CCSPlayerController player)
		{
			if (!_ropeStates.ContainsKey(player.SteamID) || !_ropeStates[player.SteamID].IsEnabled || !player.PawnIsAlive)
			{
				DetachRope(player);
				return;
			}

			var clientloc = player.PlayerPawn?.Value?.AbsOrigin ?? VEC_ZERO;
			Vector velocity = VEC_ZERO;
			Vector direction = VEC_ZERO;
			Vector ascension = VEC_ZERO;
			float climb = 3.0f;

			var ropeState = _ropeStates[player.SteamID];
			if (ropeState == null || !ropeState.Distance.HasValue)
			{
				return;
			}

			float distance = ropeState.Distance.Value;
			Vector? end = ropeState.TargetIndex;

			if (end == null)
			{
				return;
			}

			direction = end - clientloc;

			if (direction.Length() - 5 >= distance - (5 * 4))
			{
				velocity = player.PlayerPawn?.Value?.AbsVelocity ?? VEC_ZERO;
				direction = direction.Normalize();

				ascension.X = direction.X * climb;
				ascension.Y = direction.Y * climb;
				ascension.Z = direction.Z * climb;

				direction = direction *= 60.0f;
				velocity.X += direction.X + ascension.X;
				velocity.Y += direction.Y + ascension.Y;

				if (ascension.Z > 0.0f)
				{
					velocity.Z += direction.Z + ascension.Z;
				}

				if (end.Z - clientloc.Z >= distance && velocity.Z < 0.0f)
				{
					velocity.Z *= -1;
				}

				if (player.PlayerPawn != null && player.PlayerPawn.Value != null)
				{
					player.PlayerPawn.Value.AbsVelocity.Change(velocity);
				}
			}

			var color = Color.FromArgb(255, 0, 255, 0);
			var width = 3;

			if (ropeState != null && ropeState.Beam != null)
			{
				var beamState = ropeState.Beam;

				TeleportBeam(beamState, clientloc, end);
			}
			else
			{
				var a = CreateBeam(clientloc, end, color, width);
				if (a != null && a.IsValid && ropeState != null)
				{
					ropeState.Beam = a;
				}
			}
		}

		private void DetachRope(CCSPlayerController player)
		{
			if (!_ropeStates.ContainsKey(player.SteamID))
			{
				return;
			}

			var ropeState = _ropeStates[player.SteamID];
			if (ropeState != null && ropeState.IsEnabled)
			{
				ropeState.IsEnabled = false;
				ropeState.TargetIndex = null;

				if (ropeState.Beam != null)
				{
					ropeState.Beam.Remove();
					ropeState.Beam = null;
				}
				_ropeStates.Remove(player.SteamID);
			}
		}

	}
}