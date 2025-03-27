using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace Katrox
{
    public partial class Katrox
	{
		private static Dictionary<ulong, CEnvBeam> PlayersGrapples = new();
		private static List<ulong> HasHookPlayers = new();

		private static bool HookEnabledForCt = true;
		private static bool HookEnabledForT = true;

		private void Hook_Load()
		{
			if (!string.IsNullOrWhiteSpace(Config.Hook.Hook1))
            	AddCommand(Config.Hook.Hook1, "", HookOne);
			if (!string.IsNullOrWhiteSpace(Config.Hook.Hook0))
            	AddCommand(Config.Hook.Hook0, "", HookZero);

            foreach (var xC in Config.Hook.OpenHookForAll.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookAc);
            foreach (var xC in Config.Hook.OpenHookForT.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookAcT);
            foreach (var xC in Config.Hook.OpenHookForCT.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookAcCt);
            foreach (var xC in Config.Hook.DisableHookForAll.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookKapa);
            foreach (var xC in Config.Hook.DisableHookForT.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookKapaT);
            foreach (var xC in Config.Hook.DisableHookForCT.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookKapaCT);
            foreach (var xC in Config.Hook.ChangeHookSpeed.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookHiz);
            foreach (var xC in Config.Hook.GiveTempHook.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookVer);
            foreach (var xC in Config.Hook.RemoveTempHook.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", HookSil);
        }

		public void HookOne(CCSPlayerController? player, CommandInfo info)
		{
			if (PlayerIsValid(player) == false)
				return;

			if (!HasHookPlayers.Contains(player!.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.Hook.UsePermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			if (!player.PawnIsAlive)
				return;

			if (PlayersGrapples.TryGetValue(player.SteamID, out var laser) && laser.IsValid)
				return;

			var team = player.Team;
			if (team == CsTeam.Terrorist)
			{
				if (HookEnabledForT == false)
				{
					player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForT"]);
					return;
				}
			}
			else if (team == CsTeam.CounterTerrorist)
			{
				if (HookEnabledForCt == false)
				{
					player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForCT"]);
					return;
				}
			}

			HookStartXX(player);
		}

		public void HookZero(CCSPlayerController? player, CommandInfo info)
		{
			if (PlayerIsValid(player) == false)
				return;

			if (!HasHookPlayers.Contains(player!.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.Hook.UsePermission))
			{
				player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
				return;
			}

			if (!player.PawnIsAlive)
				return;

			if (PlayersGrapples.TryGetValue(player.SteamID, out var laser))
			{
				laser.AcceptInput("Kill");
				PlayersGrapples.Remove(player.SteamID);
			}
		}

		private void Hook_OnTick(CCSPlayerController? player)
		{
			if (PlayerIsValid(player) == false) return;

			if (PlayersGrapples.TryGetValue(player.SteamID, out CEnvBeam? laser))
			{
				var pPawn = player.PlayerPawn.Value;
				if (pPawn == null) return;

				if (pPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
				{
					laser.AcceptInput("Kill");
					PlayersGrapples.Remove(player.SteamID);
					return;
				}

				if (!HasHookPlayers.Contains(player.SteamID) && !AdminManager.PlayerHasPermissions(player, Config.Hook.UsePermission))
				{
					return;
				}

				var team = player.Team;
				if (team == CsTeam.Terrorist)
				{
					if (HookEnabledForT == false)
					{
						player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForT"]);
						return;
					}
				}
				else if (team == CsTeam.CounterTerrorist)
				{
					if (HookEnabledForCt == false)
					{
						player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookIsDisabledForCT"]);
						return;
					}
				}

				var grappleTarget = laser.EndPos;
				var playerPosition = pPawn.AbsOrigin;
				var direction = new Vector(grappleTarget.X - playerPosition?.X, grappleTarget.Y - playerPosition?.Y, grappleTarget.Z - playerPosition?.Z);
				var distanceToTarget = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);

				if (distanceToTarget < 40f)
				{
					return;
				}

				direction = new Vector(direction.X / distanceToTarget, direction.Y / distanceToTarget, direction.Z / distanceToTarget);

				var newVelocity = new Vector(
					direction.X * Config.Hook.DefaultSpeed,
					direction.Y * Config.Hook.DefaultSpeed,
					direction.Z * Config.Hook.DefaultSpeed
                );

				if (pPawn.AbsVelocity != null)
				{
					pPawn.AbsVelocity.X = newVelocity.X;
					pPawn.AbsVelocity.Y = newVelocity.Y;
					pPawn.AbsVelocity.Z = newVelocity.Z;
				}

				laser.Teleport(playerPosition);
			}
		}

		private void HookStartXX(CCSPlayerController player)
		{
			if (player.PlayerPawn.Value?.AbsOrigin == null || player.PlayerPawn.Value.CBodyComponent?.SceneNode == null)
			{
				return;
			}

			if (player.GetAimVector() is not { } endPos)
				return;

			var playerX = player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.X;
			var playerY = player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Y;
			var playerZ = player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Z;

			Vector grappleTarget = endPos;
			Vector playerPosition = new Vector(playerX, playerY, playerZ);
			float thresholdDistance = 40.0f;

			var direction = new Vector(grappleTarget.X - playerPosition.X, grappleTarget.Y - playerPosition.Y, grappleTarget.Z - playerPosition.Z);
			var distanceToTarget = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);

			if (distanceToTarget < thresholdDistance)
			{
				return;
			}

			Color color = Color.FromArgb(255, 0, 0, 255);
			int width = 3;
			var laser = CreateBeam(new Vector(playerX, playerY, playerZ), endPos, color, width);
			if (laser != null && laser.IsValid)
			{
				if (PlayersGrapples.ContainsKey(player.SteamID) == false)
				{
					PlayersGrapples.Add(player.SteamID, laser);
				}
				else
				{
					PlayersGrapples[player.SteamID].AcceptInput("Kill");
					PlayersGrapples[player.SteamID] = laser;
				}
			}
		}

	}
}
