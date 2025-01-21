using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Diagnostics.CodeAnalysis;

namespace Katrox
{
	public static class PlayerExtensions
	{
		public static T? GetAimTarget<T>(this CCSPlayerController player, string designerName = "", string excludeDesignerName = "") where T : CBaseEntity
		{
			if (player == null)
				return null;

			if (typeof(T) == typeof(CCSPlayerController))
			{
				var pawn = GetAimTarget<CCSPlayerPawn>(player, designerName, excludeDesignerName);
				if (pawn?.IsValid == true && pawn.DesignerName == "player")
				{
					return pawn.OriginalController.Value as T;
				}
				return null;
			}

			var GameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
			var targetEntity = GameRules?.FindPickerEntity<T>(player);

			if (targetEntity == null
				|| !targetEntity.IsValid
				|| targetEntity.Entity == null
				|| string.IsNullOrWhiteSpace(targetEntity.DesignerName)
				|| (!string.IsNullOrWhiteSpace(designerName) && !targetEntity.DesignerName.Contains(designerName, StringComparison.OrdinalIgnoreCase))
				|| (!string.IsNullOrWhiteSpace(excludeDesignerName) && targetEntity.DesignerName.Contains(excludeDesignerName, StringComparison.OrdinalIgnoreCase)))
			{
				return null;
			}

			if (Katrox.CustomRayTraceData(player, out var traceData) && traceData.HasValue)
			{
				var traceValue = traceData.Value;
				float fraction = traceValue.Fraction;
				Vector endPos = traceValue.EndPos.ToCSVector();

				if (fraction < 1.0f && targetEntity.AbsOrigin != null)
				{
					var collisionProp = targetEntity.Collision;
					if (collisionProp == null)
						return null;

					Vector mins = collisionProp.Mins;
					Vector maxs = collisionProp.Maxs;
					Vector entityOrigin = targetEntity.AbsOrigin;
					Vector boxCenterLocal = (mins + maxs) * 0.5f;
					Vector boxCenterWorld = entityOrigin + boxCenterLocal;
					Vector boxExtents = (maxs - mins) * 0.5f;
					float boundingSphereRadius = boxExtents.Length();

					float distanceHitToCenter = VectorHelper.Distance(endPos, boxCenterWorld);
					if (distanceHitToCenter <= boundingSphereRadius)
					{
						return targetEntity;
					}
				}
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
