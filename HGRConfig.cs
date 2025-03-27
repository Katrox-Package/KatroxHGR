using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Katrox
{
    public class HGRConfig : BasePluginConfig
    {
        public string Prefix { get; set; } = " {darkred}[Katrox]";

        [JsonIgnore]
        public Hook Hook { get; set; } = new Hook();

        [JsonIgnore]
        public Grab Grab { get; set; } = new Grab();

        [JsonIgnore]
        public Rope Rope { get; set; } = new Rope();
    }

}
