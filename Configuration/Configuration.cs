using System.IO;
using Microsoft.Extensions.Configuration;

namespace Transmission.PushbulletImport.Configuration
{
    public sealed class Configuration
    {
        private const string ConfigFileName = "appsettings.json";
        
        public static Configuration Default { get; private set; }
        
        public IConfigurationRoot ApplicationConfig { get; private set; }
        
        static Configuration()
        {
            Default = new Configuration(ConfigFileName);
        }

        private Configuration(string settingsFile)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(ConfigFileName)
                .AddEnvironmentVariables(prefix: "TPI_")
                .Build();

            ApplicationConfig = config;
        }
    }
}