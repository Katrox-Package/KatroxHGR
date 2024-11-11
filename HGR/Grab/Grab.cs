using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace Katrox
{
	public partial class Katrox
	{
		private static Dictionary<ulong, GrabState> _grabStates = new();
		private static List<ulong> HasGrabPlayers = new();

		private class GrabState
		{
			public bool IsEnabled { get; set; }
			public ulong? GrabbedPlayer { get; set; }
			public float? InitialDistance { get; set; }
			public CEnvBeam? Beam { get; set; }
		}

		public void Grab_Load()
		{
            AddCommand(Config.Grab.Grab1, "", GrabOne);
            AddCommand(Config.Grab.Grab0, "", GrabZero);

            foreach (var xC in Config.Grab.GiveTempGrab) AddCommand(xC, "", GrabVer);
            foreach (var xC in Config.Grab.RemoveTempGrab) AddCommand(xC, "", GrabSil);
        }

		private void Grab_OnTick(CCSPlayerController? player)
		{
			if (!_grabStates.ContainsKey(player!.SteamID) || !_grabStates[player.SteamID].IsEnabled || !player.PawnIsAlive)
			{
				return;
			}

			if (_grabStates[player.SteamID].GrabbedPlayer == null || !_grabStates[player.SteamID].InitialDistance.HasValue)
			{
				FindAndGrabTarget(player);
			}
			else
			{
				UpdateGrabbedPlayerPosition(player);
			}
		}

		private void FindAndGrabTarget(CCSPlayerController player)
		{
			player.PrintToCenter("Hedef aranıyor...");

			if (CustomRayTrace(player, out Vector? endPos) == false)
			{
				return;
			}

			var targetPlayer = GetPlayers().FirstOrDefault(x => x.SteamID != player.SteamID && x.PlayerPawn.Value != null && (x.PlayerPawn.Value.AbsOrigin ?? new Vector(0, 0, 0)).Distance(endPos!) < 100);

			if (targetPlayer != null && targetPlayer.PlayerPawn.Value != null && targetPlayer.PawnIsAlive && player.PawnIsAlive)
			{
				var initialDistance = (player.PlayerPawn.Value!.AbsOrigin ?? new Vector(0, 0, 0)).Distance(endPos!);

				_grabStates[player.SteamID] = new GrabState
				{
					IsEnabled = true,
					GrabbedPlayer = targetPlayer.SteamID,
					InitialDistance = initialDistance,
					Beam = null
				};

				targetPlayer.PlayerPawn.Value.AbsVelocity.Change(new Vector(0, 0, 0));
				player.PrintToCenter($"{targetPlayer.PlayerName} yakalandı!");
			}
		}

		private void UpdateGrabbedPlayerPosition(CCSPlayerController player)
		{
			var grabbedPlayer = GetPlayers().FirstOrDefault(x => x.SteamID == _grabStates[player.SteamID].GrabbedPlayer);
			if (grabbedPlayer == null)
			{
				return;
			}
			var grabbedPawn = grabbedPlayer.PlayerPawn.Value;

			if (grabbedPlayer == null || !grabbedPlayer.PawnIsAlive || !player.PawnIsAlive)
			{
				ReleaseGrabbedPlayer(player);
				return;
			}

			var start = player.PlayerPawn?.Value?.AbsOrigin ?? VEC_ZERO;
			var forwardVector = GetForwardVector(player) ?? new Vector(1, 0, 0);
			var initialDistance = _grabStates[player.SteamID].InitialDistance;
			var end = start + forwardVector * initialDistance!.Value;

			bool isMouse1ButtonPressed = (player.Pawn.Value?.MovementServices!.Buttons.ButtonStates[0] & 1) != 0;
			bool isMouse2ButtonPressed = (player.Pawn.Value?.MovementServices!.Buttons.ButtonStates[0] & 2048) != 0;

			if (grabbedPawn != null)
			{
				if (isMouse1ButtonPressed)
				{
					initialDistance -= 15;
					if (initialDistance <= 30)
						initialDistance = 30;
				}
				else if (isMouse2ButtonPressed)
				{
					initialDistance += 15;
				}

				_grabStates[player.SteamID].InitialDistance = initialDistance;
				end = start + forwardVector * initialDistance.Value;

				var velocity = (end - grabbedPawn.AbsOrigin!) * 7;
				grabbedPawn.AbsVelocity.X = velocity.X;
				grabbedPawn.AbsVelocity.Y = velocity.Y;
				grabbedPawn.AbsVelocity.Z = velocity.Z;


				var color = Color.FromArgb(255, 255, 0, 0);
				var width = 3;

                if (_grabStates.TryGetValue(player.SteamID, out GrabState? grabState) && grabState != null && grabState.Beam != null)
				{
					var beamState = grabState.Beam;

					TeleportBeam(beamState,start,end);
				}
				else
				{
                    var a = CreateBeam(start, grabbedPawn.AbsOrigin!, color, width);
					if(a != null && a.IsValid && grabState != null)
					{
						grabState.Beam = a;
					}
                }
            }
        }

		private void ReleaseGrabbedPlayer(CCSPlayerController player)
		{
			var xP = GetPlayers().FirstOrDefault(x => x.SteamID == _grabStates[player.SteamID].GrabbedPlayer);

			if (xP != null)
			{
				player.PrintToCenter($"{xP.PlayerName} bırakıldı!");
			}

			if (_grabStates.TryGetValue(player.SteamID, out GrabState? grabState) && grabState.Beam != null)
			{
				grabState.Beam.Remove();
			}

			_grabStates.Remove(player.SteamID);
		}


		private Vector? GetForwardVector(CCSPlayerController player)
		{
			if (player.PlayerPawn.Value == null)
			{
				return null;
			}
			var eyeAngles = player.PlayerPawn.Value.EyeAngles;
			if (eyeAngles == null)
			{
				return null;
			}

			Vector forward = new Vector();
			NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, IntPtr.Zero, IntPtr.Zero);
			return forward;
		}
	}
}
