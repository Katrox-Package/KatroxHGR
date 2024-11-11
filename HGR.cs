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
	public partial class Katrox : BasePlugin, IPluginConfig<HGRConfig>
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

			Hook_Load();
			Grab_Load();

            CallOnTick();
        }

		public override void Unload(bool hotReload)
		{
			RemoveOnTick();
		}

        public HGRConfig Config { get; set; } = new HGRConfig();

        public void OnConfigParsed(HGRConfig config)
        {
            Config = config;
            _Config = config;
        }

        public static HGRConfig _Config { get; set; } = new HGRConfig();
	}
}
