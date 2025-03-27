using System.Diagnostics.CodeAnalysis;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Katrox
{
    public static class PlayerExtensions
    {
        public static Vector? GetAimVector(this CCSPlayerController player)
        {
            if (player == null)
                return null;

            if (RayTrace.TraceShape(player, out var result))
            {
                return result?.EndPos;
            }
            return null;
        }

        public static T? GetAimTarget<T>(this CCSPlayerController player, string designerName = "", string excludeDesignerName = "") where T : CBaseEntity
        {
            if (player == null)
                return null;

            if (typeof(T) == typeof(CCSPlayerController))
            {
                var ent = GetAimTarget<CBaseEntity>(player, designerName, excludeDesignerName);
                if (ent?.IsValid == true && ent.DesignerName.Contains("player"))
                {
                    return ent.As<CCSPlayerPawn>().OriginalController.Value as T;
                }
                return null;
            }

            if (RayTrace.TraceShape(player, out var result) && result?.HitEntity is { DesignerName: not "worldent" } targetEntity)
            {
                return targetEntity as T;
            }

            return null;
        }
    }

    public partial class Katrox
    {
        public static bool PlayerIsValid([NotNullWhen(true)] CCSPlayerController? x, bool bot = false)
        {
            if ((x != null && x.IsValid && x.PlayerPawn.IsValid) == false)
            {
                return false;
            }

            if (x == null) return false;
            if (bot ? false : x.IsBot) return false;
            if (x.Connected == PlayerConnectedState.PlayerConnected && x.Index != 32767 && !x.IsHLTV) return true;

            return false;
        }

        private static IEnumerable<CCSPlayerController> GetPlayers(CsTeam? team = null)
        {
            List<CCSPlayerController> result = new();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                var ent = NativeAPI.GetEntityFromIndex(i);
                if (ent == 0)
                    continue;

                var x = new CCSPlayerController(ent);
                if (PlayerIsValid(x) == false)
                    continue;

                if ((x.Team == CsTeam.Terrorist ||
                    x.Team == CsTeam.CounterTerrorist ||
                    x.Team == CsTeam.Spectator ||
                    x.Team == CsTeam.None) == false)
                    continue;

                if ((team.HasValue ? team.Value == x.Team : true) == false)
                    continue;

                result.Add(x);
            }

            return result;
        }
    }
}
