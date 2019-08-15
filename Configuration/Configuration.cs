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

        private PushbulletClient _pbClient;

        public PushbulletClient PBClient
        {
            get
            {
                if (_pbClient == null)
                {
                    PushbulletSetup();
                }

                return _pbClient;
            }
        }
        public string PBTargetDeviceId { get; private set; }
        
        private const string ApiEnvironmentKey = EnvironmentConfigPrefix + "PBAPI";
        private const string DeviceEnvironmentKey = EnvironmentConfigPrefix + "PBDEVICE";
        
        private const string PBSectionKey = "Pushbullet";
        private const string ApiKeyConfigKey = "Pushbullet API key";
        private const string DeviceConfigKey = "Target device";

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

        #region Transmission Settings

        private const string TransmissionSectionKey = "Transmission";
        private const string TransmissionHostKey = "Host";
        private const string TransmissionHostEnvironmentKey = EnvironmentConfigPrefix + "THOST";
        
        private API.RPC.Client _tClient; 
        public API.RPC.Client TransmissionClient
        {
            get
            {
                if (_tClient == null)
                {
                    TorrentClientSetup();
                }

                return _tClient;
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
        }

        #region Pushbullet
        private void PushbulletSetup()
        {
            var apiKey = GetPBApiKey();

            _pbClient = new PushbulletClient(apiKey, TimeZoneInfo.Local);
            PBTargetDeviceId = GetDeviceId();
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

        private string GetDeviceId()
        {
            var deviceId = ApplicationConfig[DeviceEnvironmentKey];

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = PushbulletConfigSection[DeviceConfigKey];
            }

            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ApplicationException("Please specify a Pushbullet target device id");
            }

            return deviceId;
        }
        #endregion

        #region Transmission

        private void TorrentClientSetup()
        {
            var hostName = GetTransmissionHostname();
            
            _tClient = new API.RPC.Client(hostName);
        }

        private string GetTransmissionHostname()
        {
            var hostname = ApplicationConfig[ApiEnvironmentKey];
            if (string.IsNullOrWhiteSpace(hostname))
            {
                var configSection = ApplicationConfig.GetSection(TransmissionSectionKey);
                hostname = configSection[TransmissionHostKey];
            }

            if (string.IsNullOrEmpty(hostname))
            {
                throw new ApplicationException("Please specify a Transmission hostname.");
            }

            return hostname;
        }

        #endregion
    }
}