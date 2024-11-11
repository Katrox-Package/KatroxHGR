using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace Katrox
{
    public partial class Katrox
    {
        private void CallOnTick()
        {
            RegisterListener<Listeners.OnTick>(OnTick);
        }

        private void RemoveOnTick()
        {
            RemoveListener<Listeners.OnTick>(OnTick);
        }

        private void OnTick()
        {
            try
            {
                for (int i = 1; i <= Server.MaxPlayers; i++)
                {
                    var ent = NativeAPI.GetEntityFromIndex(i);
                    if (ent == 0)
                        continue;

                    var player = new CCSPlayerController(ent);
                    if (player == null || !player.IsValid)
                        continue;

                    Hook_OnTick(player);
                    Grab_OnTick(player);
                    Rope_OnTick(player);

                }
            }
            catch (Exception e)
            {
                _Logger?.LogError($"OnTick Error: {e.Message}");
            }
        }
    }
}
