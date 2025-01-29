using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
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
		public override string ModuleVersion => "0.0.6";
		public override string ModuleAuthor => "Roxy & Katarina";

		public HGRConfig Config { get; set; } = new HGRConfig();
		public static HGRConfig _Config { get; set; } = new HGRConfig();


		public static Katrox? _Global;
		internal static ILogger? _Logger;

		public static readonly Vector VEC_ZERO = Vector.Zero;
		public static readonly QAngle ANGLE_ZERO = QAngle.Zero;

		public override void Load(bool hotReload)
		{
			_Global = this;
			_Logger = Logger;

			LoadAllConfigs();

			Hook_Load();
			Grab_Load();
			Rope_Load();

            CallOnTick();
        }

		public override void Unload(bool hotReload)
		{
			RemoveOnTick();
		}

        public void OnConfigParsed(HGRConfig config)
        {
			config.Prefix = StringExtensions.ReplaceColorTags(config.Prefix);

            Config = config;
            _Config = config;
        }

	}
}
