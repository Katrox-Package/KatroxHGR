using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Katrox
{
    public partial class Katrox
    {
        private static Dictionary<ulong, GrabState> GrabDatas = new();
        private static List<ulong> HasGrabPlayers = new();

        private class GrabState
        {
            public float? InitialDistance { get; set; }
            public CEnvBeam? Beam { get; set; }
            public CBaseEntity? Entity { get; set; } = null;
        }

        public void Grab_Load()
        {
            if (!string.IsNullOrWhiteSpace(Config.Grab.Grab1))
                AddCommand(Config.Grab.Grab1, "", GrabOne);
            if (!string.IsNullOrWhiteSpace(Config.Grab.Grab0))
                AddCommand(Config.Grab.Grab0, "", GrabZero);

            foreach (var xC in Config.Grab.GiveTempGrab.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", GrabVer);
            foreach (var xC in Config.Grab.RemoveTempGrab.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()) AddCommand(xC, "", GrabSil);
        }

        private void Grab_OnTick(CCSPlayerController? player)
        {
            if (!PlayerIsValid(player)) return;
            if (!GrabDatas.ContainsKey(player.SteamID)) return;

            if (GrabDatas[player.SteamID].Entity == null || !GrabDatas[player.SteamID].InitialDistance.HasValue)
            {
                MoveType_t[] allowedMoveTypes = new[]
                {
                    MoveType_t.MOVETYPE_VPHYSICS,
                    MoveType_t.MOVETYPE_WALK,
                    MoveType_t.MOVETYPE_OBSOLETE,
                    MoveType_t.MOVETYPE_NONE
                };

                player.PrintToCenterHtml(Localizer["GrabSearchingPlayer"]);
                var target = player.GetAimTarget<CBaseEntity>();
                if (target != null
                    && target.IsValid
                    && allowedMoveTypes.Contains(target.MoveType))
                {
                    var initialDistance = (player.PlayerPawn.Value!.AbsOrigin ?? VEC_ZERO).Distance(target.AbsOrigin ?? VEC_ZERO);
                    GrabDatas[player.SteamID] = new GrabState
                    {
                        InitialDistance = initialDistance,
                        Beam = null,
                        Entity = target
                    };

                    if (target.As<CCSPlayerPawn>()?.OriginalController?.Value?.IsValid == true)
                        target.As<CCSPlayerPawn>()?.OriginalController?.Value?.PlayerPawn?.Value?.Teleport(null, null, VEC_ZERO);
                    else
                        target.Teleport(null, null, VEC_ZERO);

                    player.PrintToCenter(Localizer["GrabbedPlayer", target.DesignerName]);
                }
            }
            else
            {
                if (GrabDatas[player.SteamID].Entity != null)
                {
                    Grab_EntityGrab(player);
                }
            }
        }

        private void Grab_EntityGrab(CCSPlayerController player)
        {
            var gEntity = GrabDatas[player.SteamID].Entity;
            if (gEntity == null
                || !gEntity.IsValid
                || !player.PawnIsAlive
                || (gEntity.As<CCSPlayerPawn>()?.OriginalController?.Value?.IsValid == true && gEntity.As<CCSPlayerPawn>()?.OriginalController?.Value?.PawnIsAlive != true))
            {
                ReleaseGrabbedPlayer(player);
                return;
            }

            var start = player.PlayerPawn?.Value?.AbsOrigin ?? VEC_ZERO;

            var eyeAngles = player.PlayerPawn?.Value?.EyeAngles;
            if (eyeAngles == null) return;

            Vector forward = new Vector();
            NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, IntPtr.Zero, IntPtr.Zero);

            var initialDistance = GrabDatas[player.SteamID].InitialDistance;
            var end = start + forward * initialDistance!.Value;

            bool isMouse1ButtonPressed = (player.Pawn.Value?.MovementServices!.Buttons.ButtonStates[0] & 1) != 0;
            bool isMouse2ButtonPressed = (player.Pawn.Value?.MovementServices!.Buttons.ButtonStates[0] & 2048) != 0;

            if (gEntity != null && gEntity.IsValid)
            {
                if (isMouse1ButtonPressed)
                {
                    initialDistance -= 15;
                    if (initialDistance <= 30) initialDistance = 30;
                }
                else if (isMouse2ButtonPressed)
                {
                    initialDistance += 15;
                }

                GrabDatas[player.SteamID].InitialDistance = initialDistance;

                end = start + forward * initialDistance.Value;
                var velocity = (end - gEntity.AbsOrigin!) * 7;

                if (gEntity.As<CCSPlayerPawn>()?.OriginalController?.Value?.IsValid == true)
                    gEntity.As<CCSPlayerPawn>()?.OriginalController?.Value?.PlayerPawn?.Value?.Teleport(null, null, velocity);
                else
                    gEntity.Teleport(null, null, velocity);

                var color = Color.FromArgb(255, 255, 0, 0);
                var width = 3;
                if (GrabDatas.TryGetValue(player.SteamID, out GrabState? grabState) && grabState != null && grabState.Beam != null)
                {
                    var beamState = grabState.Beam;
                    TeleportBeam(beamState, start, gEntity.AbsOrigin!);
                }
                else
                {
                    var a = CreateBeam(start, gEntity.AbsOrigin!, color, width);
                    if (a != null && a.IsValid && grabState != null)
                    {
                        grabState.Beam = a;
                    }
                }
            }
        }

        private void ReleaseGrabbedPlayer(CCSPlayerController player)
        {
            var gEntity = GrabDatas[player.SteamID].Entity;
            if (gEntity != null)
                player.PrintToCenter(Localizer["ReleasedGrabbedPlayer", gEntity?.DesignerName ?? ""]);

            if (GrabDatas.TryGetValue(player.SteamID, out GrabState? grabState) && grabState.Beam != null)
            {
                grabState.Beam.AcceptInput("Kill");
            }
            GrabDatas.Remove(player.SteamID);
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
