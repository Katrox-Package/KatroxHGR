using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace Katrox
{
	public partial class Katrox
	{

		private static CEnvBeam? CreateBeam(Vector start, Vector end, Color color, float width, float? removeAfter = null)
		{
			CEnvBeam? beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam");

			if (beam != null)
			{
				beam.Render = color;
				beam.Width = width;
				beam.Teleport(start, ANGLE_ZERO, VEC_ZERO);
				beam.EndPos.X = end.X;
				beam.EndPos.Y = end.Y;
				beam.EndPos.Z = end.Z;
				beam.DispatchSpawn();
				if (removeAfter != null && removeAfter.HasValue)
				{
					_Global?.AddTimer(removeAfter.Value, () => beam.AcceptInput("Kill"));
				}
			}

			return beam;
		}

		private static void TeleportBeam(CBeam beam, Vector start, Vector end)
		{
			if (beam != null && beam.IsValid)
			{
				beam.Teleport(start, ANGLE_ZERO, VEC_ZERO);
				beam.EndPos.X = end.X;
				beam.EndPos.Y = end.Y;
				beam.EndPos.Z = end.Z;
				Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
			}
		}

	}
}
