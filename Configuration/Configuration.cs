using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using PushbulletSharp;
using PushbulletSharp.Models.Requests;
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
        public Device PBServerDevice { get; private set; }
        private const string PBDeviceName = "Transmisson Server";
        
        private const string ApiEnvironmentKey = "PBAPI";
        private const string DeviceEnvironmentKey = "PBDEVICE";
        
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
        private const string TransmissionHostEnvironmentKey = "THOST";
        
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
                .AddJsonFile(ConfigFileName, optional: true)
                .AddEnvironmentVariables(prefix: EnvironmentConfigPrefix);

            ApplicationConfig = config.Build();
        }

        #region Pushbullet
        private void PushbulletSetup()
        {
            var apiKey = GetPBApiKey();

            _pbClient = new PushbulletClient(accessToken:apiKey, TimeZoneInfo.Local);
            PBTargetDeviceId = GetTargetDeviceId();
            PBServerDeviceSetup();
        }

        private void PBServerDeviceSetup()
        {
            PBServerDevice = DoesPBDeviceAlreadyExist(PBDeviceName)
                ? GetPBDevice(PBDeviceName)
                : CreatePBDevice(PBDeviceName);
        }

        private bool DoesPBDeviceAlreadyExist(string nickname = null, string deviceId = null)
        {
            return PBClient.CurrentUsersDevices(true).Devices
                .Any(d => d.Nickname.Equals(nickname) || d.Iden.Equals(deviceId));
        }

        private Device GetPBDevice(string nickname = null, string deviceId = null, bool showActiveOnly = true)
        {
            return PBClient.CurrentUsersDevices(showActiveOnly).Devices
                .First(d => d.Nickname.Equals(nickname) || d.Iden.Equals(deviceId));
        }

        private Device CreatePBDevice(string nickname, string model = null, string manufacturer = null, int? appVersion = null)
        {
            var newDevice = new Device()
            {
                Nickname = nickname,
                Model = model,
                Manufacturer = manufacturer
            };

            if (appVersion.HasValue)
            {
                newDevice.AppVersion = appVersion.Value;
            }

            return PBClient.CreateDevice(newDevice);
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

        private string GetTargetDeviceId()
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
            var hostname = ApplicationConfig[TransmissionHostEnvironmentKey];
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