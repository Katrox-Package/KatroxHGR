using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Katrox
{
	public class HGRConfig : BasePluginConfig
	{
        public string Prefix { get; set; } = " {darkred}Katrox";

        [JsonIgnore]
        public Hook Hook { get; set; } = new Hook();

        [JsonIgnore]
        public Grab Grab { get; set; } = new Grab();

		[JsonIgnore]
		public Rope Rope { get; set; } = new Rope();
	}

}
