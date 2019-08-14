using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using PushbulletSharp;

namespace Transmission.PushbulletImport.Configuration
{
    public sealed class Configuration
    {
        private const string ConfigFileName = "appsettings.json";
        
        public static Configuration Default { get; private set; }
        
        public IConfigurationRoot ApplicationConfig { get; private set; }

        #region Pushbullet Settings
        public PushbulletClient PushbulletClient { get; private set; }
        private const string ApiEnvironmentKey = "TPI_PBAPI";
        private const string ApiConfigKey = "Pushbullet API key";
        #endregion
        
        static Configuration()
        {
            Default = new Configuration(ConfigFileName);
        }

        private Configuration(string settingsFile)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(ConfigFileName)
                .AddEnvironmentVariables(prefix: "TPI_");

            ApplicationConfig = config.Build();

            PushbulletSetup();
        }

        private void PushbulletSetup()
        {
            /*
             * API key expected in file (Pushbullet API key or TPI_PBAPI)
             */
            var apiKey = ApplicationConfig[ApiEnvironmentKey];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = ApplicationConfig[ApiConfigKey];
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ApplicationException("Please specify a Pushbullet API key");
            }
            
            PushbulletClient = new PushbulletClient(apiKey, TimeZoneInfo.Local);
            
            //TODO: read channel tag from config, subscribe to channel
        }
    }
}