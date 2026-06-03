using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contextual attributes attached to an authentication request, such as user-defined
    /// and application-defined metadata.
    /// </summary>
    [Serializable]
    public class Attributes
    {
        [JsonProperty("unity")]
        internal UnityAttributes Unity { get; } = new();
        [JsonProperty("device")]
        internal DeviceAttributes Device { get; } = new();
        [JsonProperty("os")]
        internal OsAttributes Os { get; } = new();
        /// <summary>
        /// User-defined attributes to include with the authentication request.
        /// </summary>
        [JsonProperty("user")]
        public object User { get; internal set; }
        /// <summary>
        /// Application-defined attributes to include with the authentication request.
        /// </summary>
        [JsonProperty("app")]
        public object App { get; internal set; }
    }

    [Serializable]
    internal class UnityAttributes
    {
        [JsonProperty("app")]
        public UnityAppAttributes App { get; } = new();
        [JsonProperty("runtime_platform")]
        public string RuntimePlatform { get; }
        [JsonProperty("time_since_start")]
        public double TimeSinceStart => Time.realtimeSinceStartupAsDouble * 1000;

        public UnityAttributes()
        {
            RuntimePlatform = Application.platform.ToString();
        }
    }

    [Serializable]
    internal class UnityAppAttributes
    {
        [JsonProperty("bundle_id")]
        public string BundleId { get; }
        [JsonProperty("name")]
        public string Name { get; }
        [JsonProperty("install_mode")]
        public string InstallMode { get; }
        [JsonProperty("installer")]
        public string Installer { get; }
        [JsonProperty("version")]
        public string Version { get; }

        public UnityAppAttributes()
        {
            BundleId = Application.identifier;
            Name = Application.productName;
            InstallMode = Application.installMode switch
            {
                ApplicationInstallMode.Store          => "store",
                ApplicationInstallMode.DeveloperBuild => "developer_build",
                ApplicationInstallMode.Adhoc          => "adhoc",
                ApplicationInstallMode.Enterprise     => "enterprise",
                ApplicationInstallMode.Editor         => "editor",
                _                                     => "unknown"
            };
            Installer = Application.installerName;
            Version = Application.version;
        }
    }

    [Serializable]
    internal class DeviceAttributes
    {
        [JsonProperty("cpu")]
        public DeviceCpuAttributes Cpu { get; } = new();
        [JsonProperty("gpu")]
        public DeviceGpuAttributes Gpu { get; } = new();
        [JsonProperty("memory")]
        public DeviceMemoryAttributes Memory { get; } = new();
        [JsonProperty("screen")]
        public DeviceScreenAttributes Screen { get; } = new();
        [JsonProperty("max_texture_size")]
        public int MaxTextureSize { get; }
        [JsonProperty("model")]
        public string Model { get; }
        [JsonProperty("rooted_or_jailbroken")]
        public bool RootedOrJailbroken { get; }

        public DeviceAttributes()
        {
            MaxTextureSize = SystemInfo.maxTextureSize;
            Model = SystemInfo.deviceModel;
            RootedOrJailbroken = Application.sandboxType == ApplicationSandboxType.SandboxBroken;
        }
    }

    [Serializable]
    internal class DeviceCpuAttributes
    {
        [JsonProperty("count")]
        public int Count { get; }
        [JsonProperty("frequency_mhz")]
        public int FrequencyMhz { get; }
        [JsonProperty("model")]
        public string Model { get; }
        [JsonProperty("type")]
        public string Type { get; }

        public DeviceCpuAttributes()
        {
            Count = SystemInfo.processorCount;
            FrequencyMhz = SystemInfo.processorFrequency;
#if UNITY_6000_0_OR_NEWER
            Model = SystemInfo.processorModel;
#else
            Model = "<unknown>";
#endif
            Type = SystemInfo.processorType;
        }
    }

    [Serializable]
    internal class DeviceGpuAttributes
    {
        [JsonProperty("id")]
        public int Id { get; }
        [JsonProperty("memory_size_mb")]
        public int MemorySizeMb { get; }
        [JsonProperty("model")]
        public string Model { get; }
        [JsonProperty("vendor")]
        public string Vendor { get; }
        [JsonProperty("vendor_id")]
        public int VendorId { get; }
        [JsonProperty("version")]
        public string Version { get; }
        [JsonProperty("shader_level")]
        public int ShaderLevel { get; }

        public DeviceGpuAttributes()
        {
            Id = SystemInfo.graphicsDeviceID;
            MemorySizeMb = SystemInfo.graphicsMemorySize;
            Model = SystemInfo.graphicsDeviceName;
            Vendor = SystemInfo.graphicsDeviceVendor;
            VendorId = SystemInfo.graphicsDeviceVendorID;
            Version = SystemInfo.graphicsDeviceVersion;
            ShaderLevel = SystemInfo.graphicsShaderLevel;
        }
    }

    [Serializable]
    internal class DeviceMemoryAttributes
    {
        [JsonProperty("total_size_mb")]
        public int TotalSizeMb { get; }

        public DeviceMemoryAttributes()
        {
            TotalSizeMb = SystemInfo.systemMemorySize;
        }
    }

    [Serializable]
    internal class DeviceScreenAttributes
    {
        [JsonProperty("dpi")]
        public int Dpi { get; }
        [JsonProperty("fullscreen")]
        public int Fullscreen { get; }
        [JsonProperty("refresh_rate_hz")]
        public double RefreshRateHz { get; }
        [JsonProperty("size")]
        public string Size { get; }

        public DeviceScreenAttributes()
        {
            Dpi = (int)Screen.dpi;
            Fullscreen = Screen.fullScreen ? 1 : 0;
#if UNITY_2022_2_OR_NEWER
            RefreshRateHz = Screen.currentResolution.refreshRateRatio.value;
#else
            RefreshRateHz = 0;
#endif
            Size = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
        }
    }

    [Serializable]
    internal class OsAttributes
    {
        [JsonProperty("language")]
        public string Language { get; }
        [JsonProperty("version")]
        public string Version { get; }

        public OsAttributes()
        {
            Language = Application.systemLanguage.ToString();
            Version = SystemInfo.operatingSystem;
        }
    }
}
