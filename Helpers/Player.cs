using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katrox
{
	public partial class Katrox
	{
		public static bool PlayerIsValid([NotNullWhen(true)] CCSPlayerController? x)
		{
			if ((x != null && x.IsValid && x.PlayerPawn.IsValid) == false)
			{
				return false;
			}

			if (x == null) return false;
			if (x.IsBot) return false;
			if (x.Connected == PlayerConnectedState.PlayerConnected && x.Index != 32767 && !x.IsHLTV) return true;

			return false;
		}

		private static IEnumerable<CCSPlayerController> GetPlayers(CsTeam? team = null)
		{
			HashSet<CCSPlayerController> result = new();

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
					x.Team == CsTeam.Spectator) == false)
					continue;

				if ((team.HasValue ? team.Value == x.Team : true) == false)
					continue;

				result.Add(x);
			}

			return result;
		}
	}
}
