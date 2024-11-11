using CounterStrikeSharp.API;
using System.Text.Json;

namespace Katrox
{
    public partial class Katrox
    {
        private static string _customConfigPath = "";

        public void ConfigReadPath()
        {
            var pathComponents = new string[]
            {
                Server.GameDirectory,
                "csgo",
                "addons",
                "counterstrikesharp",
                "configs",
                "plugins",
                ModuleName
            };

            var fullPath = Path.Combine(pathComponents);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            _customConfigPath = fullPath;
        }

        public T LoadConfig<T>(string fileName) where T : new()
        {
            var filePath = Path.Combine(_customConfigPath, fileName);

            if (!File.Exists(filePath))
            {
                var defaultConfig = new T();
                var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                Server.PrintToConsole($"{fileName} dosyasi olusturuldu ve kaydedildi.");
                return defaultConfig;
            }

            var fileContent = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<T>(fileContent);

            return config ?? new T();
        }


        private void LoadAllConfigs()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_customConfigPath))
                {
                    ConfigReadPath();
                }

                var configFiles = new List<(string FileName, Func<object> Loader, Action<object> Setter)>
                {
                   ("Hook.json", () => LoadConfig<Hook>("Hook.json"), config => _Config.Hook = Config.Hook = (Hook)config),
                   ("Grab.json", () => LoadConfig<Grab>("Grab.json"), config => _Config.Grab = Config.Grab = (Grab)config),
                   ("Rope.json", () => LoadConfig<Rope>("Rope.json"), config => _Config.Rope = Config.Rope = (Rope)config)
                };

                foreach (var (fileName, loader, setter) in configFiles)
                {
                    try
                    {
                        var config = loader.Invoke();
                        if (config != null)
                        {
                            setter(config);
                        }
                    }
                    catch (Exception e)
                    {
                        Server.PrintToConsole($"{fileName} yuklenirken hata: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Server.PrintToConsole($"Configler yuklenirken hata: {e.Message}");
            }
        }


    }
}
