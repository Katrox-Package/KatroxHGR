using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Katrox
{
    public partial class Katrox
    {
        public void GrabOne(CCSPlayerController? player, CommandInfo info)
        {
            if (PlayerIsValid(player) == false)
            {
                return;
            }

            if (!AdminManager.PlayerHasPermissions(player, "@css/cvar") &&
                !HasGrabPlayers.Contains(player.SteamID))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            if (_grabStates.ContainsKey(player!.SteamID) && _grabStates[player.SteamID].GrabbedPlayer != null)
            {
                player.PrintToCenter(Localizer["AlreadyGrabbedPlayer"]);
                return;
            }

            if (!player.PawnIsAlive)
            {
                return;
            }

            _grabStates[player.SteamID] = new GrabState
            {
                IsEnabled = true,
                GrabbedPlayer = null,
                InitialDistance = null,
                Beam = null
            };
        }

        public void GrabZero(CCSPlayerController? player, CommandInfo cmd)
        {
            if (PlayerIsValid(player) == false)
            {
                return;
            }

            if (!AdminManager.PlayerHasPermissions(player, "@css/cvar") &&
                !HasGrabPlayers.Contains(player.SteamID))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            if (!player.PawnIsAlive || !_grabStates.ContainsKey(player.SteamID) || _grabStates[player.SteamID].GrabbedPlayer == null)
            {
                _grabStates.Remove(player.SteamID);
                return;
            }

            ReleaseGrabbedPlayer(player);
        }


        [CommandHelper(1, "<isim>")]
        public void GrabVer(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;
            if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/cvar"))
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
                    if (!HasGrabPlayers.Contains(x.SteamID))
                    {
                        HasGrabPlayers.Add(x.SteamID);
                    }
                    Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminGave", callerName, x.PlayerName, "grab"]);
                });

        }

        [CommandHelper(1, "<isim>")]
        public void GrabSil(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;
            if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/cvar"))
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
                    if (HasGrabPlayers.Contains(x.SteamID))
                    {
                        HasGrabPlayers.RemoveAll(y => y == x.SteamID);
                    }
                    Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminDelete", callerName, x.PlayerName, "grab"]);
                });
        }
    }
}
