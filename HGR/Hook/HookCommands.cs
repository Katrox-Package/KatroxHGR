using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katrox
{
    public partial class Katrox
    {
        [CommandHelper(1, "<target>")]
        public void HookVer(CCSPlayerController? player, CommandInfo info)
        {
            if (PlayerIsValid(player) == false)
                return;

            if (!AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            var target = info.GetArgTargetResult(1);

            if (target == null)
            {
                player!.PrintToChat(Config.Prefix + ChatColors.White + Localizer["TargetIsWrong"]);
                return;
            }

            target
                .ToList()
                .ForEach(x =>
                {
                    if (!HasHookPlayers.Contains(x.SteamID))
                    {
                        HasHookPlayers.Add(x.SteamID);
                    }
                    Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminGave", player!.PlayerName, x.PlayerName, "hook"]);
                });

        }

        [CommandHelper(1, "<target>")]
        public void HookSil(CCSPlayerController? player, CommandInfo info)
        {
            if (PlayerIsValid(player) == false)
                return;

            if (!AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            var target = info.GetArgTargetResult(1);

            if (target == null)
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["TargetIsWrong"]);
                return;
            }

            target
                .ToList()
                .ForEach(x =>
                {
                    if (HasHookPlayers.Contains(x.SteamID))
                    {
                        HasHookPlayers.RemoveAll(y => y == x.SteamID);
                    }
                    Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminDelete", player.PlayerName, x.PlayerName, "hook"]);
                });

        }

        [CommandHelper(1, "<speed>")]
        public void HookHiz(CCSPlayerController? player, CommandInfo info)
        {
            if (PlayerIsValid(player) == false)
                return;

            if (!AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            if (!int.TryParse(info.GetArg(1), out int x))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["HookSpeedIsWrong"]);
                return;
            }

            Config.Hook.DefaultSpeed = x;

            Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["NamedAdminChangedHookSpeed", player.PlayerName, x]);
        }

        public void HookAc(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;

            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            HookEnabledForT = true;
            HookEnabledForCt = true;

            Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["OpenedForAll", callerName, "hook"]);
        }

        public void HookAcT(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;

            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            HookEnabledForT = true;

            Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["OpenedHookForT", callerName]);
        }

        public void HookAcCt(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;

            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            HookEnabledForCt = true;

            Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["OpenedHookForCT", callerName]);
        }

        public void HookKapa(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;

            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            HookEnabledForT = false;
            HookEnabledForCt = false;

            Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["DisabledForAll", callerName, "hook"]);
        }

        public void HookKapaT(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;

            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            HookEnabledForT = false;

            Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["DisabledHookForT", callerName]);
        }

        public void HookKapaCT(CCSPlayerController? player, CommandInfo info)
        {
            var callerName = player == null ? "Console" : player.PlayerName;

            if (player != null && !AdminManager.PlayerHasPermissions(player, Config.Hook.GivePermission))
            {
                player!.PrintToChat(Config.Prefix + ChatColors.White + Localizer["NotEnoughPermission"]);
                return;
            }

            HookEnabledForCt = false;

            Server.PrintToChatAll(Config.Prefix + ChatColors.White + Localizer["DisabledHookForCT", callerName]);
        }
    }
}
