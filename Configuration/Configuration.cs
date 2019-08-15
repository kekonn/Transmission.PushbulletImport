using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using PushbulletSharp;
using PushbulletSharp.Models.Responses;

namespace Transmission.PushbulletImport.Configuration
{
    public sealed class Configuration
    {
        private const string ConfigFileName = "appsettings.json";
        private const string EnvironmentConfigPrefix = "TPI_";
        public static Configuration Default { get; private set; }
        
        public IConfigurationRoot ApplicationConfig { get; private set; }

        #region Pushbullet Settings
        public PushbulletClient PBClient { get; private set; }
        public Subscription PBChannelSubscription { get; private set; }
        
        private const string ApiEnvironmentKey = EnvironmentConfigPrefix + "PBAPI";
        private const string ChannelEnvironmentKey = EnvironmentConfigPrefix + "CHANNEL";
        
        private const string PBSectionKey = "Pushbullet";
        private const string ApiKeyConfigKey = "Pushbullet API key";
        private const string ChannelConfigKey = "Channel tag";

        private IConfigurationSection PushbulletConfigSection
        {
            get
            {
                var pbConfigSection = ApplicationConfig.GetSection(PBSectionKey);
                if (pbConfigSection == null)
                {
                    throw new ApplicationException($"Section {PBSectionKey} is missing from {ConfigFileName}.");
                }

                return pbConfigSection;
            }
        }
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
                .AddEnvironmentVariables(prefix: EnvironmentConfigPrefix);

            ApplicationConfig = config.Build();

            PushbulletSetup();
        }

        private void PushbulletSetup()
        {
            var apiKey = GetPBApiKey();

            PBClient = new PushbulletClient(apiKey, TimeZoneInfo.Local);

            PBChannelSubscription = PBClient.SubscribeToChannel(GetChannelTag());
        }

        private string GetPBApiKey()
        {
            var apiKey = ApplicationConfig[ApiEnvironmentKey];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = PushbulletConfigSection[ApiKeyConfigKey];
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ApplicationException("Please specify a Pushbullet API key");
            }

            return apiKey;
        }

        private string GetChannelTag()
        {
            var channelTag = ApplicationConfig[ChannelEnvironmentKey];

            if (string.IsNullOrWhiteSpace(channelTag))
            {
                channelTag = PushbulletConfigSection[ChannelConfigKey];
            }

            if (string.IsNullOrEmpty(channelTag))
            {
                throw new ApplicationException("Please specify a Pushbullet channel tag");
            }

            return channelTag;
        }
    }
}