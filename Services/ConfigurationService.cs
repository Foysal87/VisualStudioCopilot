using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace XatBotVisualStudioCopilot.Services
{
    public class ConfigurationService
    {
        private static readonly object LockObject = new object();
        private static ConfigurationService _instance;
        private readonly Dictionary<string, Dictionary<string, List<Config>>> _configs;

        private ConfigurationService()
        {
            _configs = LoadConfigs();
        }

        public static ConfigurationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigurationService();
                        }
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, Dictionary<string, List<Config>>> LoadConfigs()
        {
            var configFilePath = @"C:\LocalStore\XatBotVisualStudioCopilot\AiConfigs.json";
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException($"Configuration file not found at path: {configFilePath}");

            var jsonContent = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<Config>>>>(jsonContent);
        }

        public Config GetConfig(string serviceName = "AzureOpenAi", string modelName = "GPT-4o-mini")
        {
            if (_configs.TryGetValue(serviceName, out var modelConfigs))
            {
                if (modelConfigs.TryGetValue(modelName, out var configs) && configs.Count > 0)
                {
                    return configs.First();
                }
            }
            return null;
        }
    }
}
