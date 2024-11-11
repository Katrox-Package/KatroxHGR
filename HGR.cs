using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katrox
{
	public partial class Katrox : BasePlugin
	{
		public override string ModuleName => "KatroxHGR";
		public override string ModuleVersion => "0.0.1";
		public override string ModuleAuthor => "Roxy & Katarina";


		public static Katrox? _Global;
		internal static ILogger? _Logger;

		public static readonly Vector VEC_ZERO = Vector.Zero;
		public static readonly QAngle ANGLE_ZERO = QAngle.Zero;

		public override void Load(bool hotReload)
		{
			_Global = this;
			_Logger = Logger;
		}

		public override void Unload(bool hotReload)
		{
			
		}
	}
}
