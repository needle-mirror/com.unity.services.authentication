using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Services.Authentication
{
    // Read by the serializer only: stripping removes getters nothing calls, so these are fields; a property here needs [Preserve].

    /// <summary>
    /// Contextual attributes attached to an authentication request, such as user-defined
    /// and application-defined metadata.
    /// </summary>
    [Serializable]
    public class Attributes
    {
        [JsonProperty("unity")]
        internal readonly UnityAttributes Unity = new UnityAttributes();
        [JsonProperty("device")]
        internal readonly DeviceAttributes Device = new DeviceAttributes();
        [JsonProperty("os")]
        internal readonly OsAttributes Os = new OsAttributes();
        /// <summary>
        /// User-defined attributes to include with the authentication request.
        /// </summary>
        [Preserve]
        [JsonProperty("user")]
        public object User { get; internal set; }
        /// <summary>
        /// Application-defined attributes to include with the authentication request.
        /// </summary>
        [Preserve]
        [JsonProperty("app")]
        public object App { get; internal set; }
    }

    [Serializable]
    internal class UnityAttributes
    {
        [JsonProperty("app")]
        public readonly UnityAppAttributes App = new UnityAppAttributes();
        [JsonProperty("runtime_platform")]
        public readonly string RuntimePlatform;
        // Stays a property: read at serialization time, not construction time.
        [Preserve]
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
        public readonly string BundleId;
        [JsonProperty("name")]
        public readonly string Name;
        [JsonProperty("install_mode")]
        public readonly string InstallMode;
        [JsonProperty("installer")]
        public readonly string Installer;
        [JsonProperty("version")]
        public readonly string Version;

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
        public readonly DeviceCpuAttributes Cpu = new DeviceCpuAttributes();
        [JsonProperty("gpu")]
        public readonly DeviceGpuAttributes Gpu = new DeviceGpuAttributes();
        [JsonProperty("memory")]
        public readonly DeviceMemoryAttributes Memory = new DeviceMemoryAttributes();
        [JsonProperty("screen")]
        public readonly DeviceScreenAttributes Screen = new DeviceScreenAttributes();
        [JsonProperty("max_texture_size")]
        public readonly int MaxTextureSize;
        [JsonProperty("model")]
        public readonly string Model;
        [JsonProperty("rooted_or_jailbroken")]
        public readonly bool RootedOrJailbroken;

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
        public readonly int Count;
        [JsonProperty("frequency_mhz")]
        public readonly int FrequencyMhz;
        [JsonProperty("model")]
        public readonly string Model;
        [JsonProperty("type")]
        public readonly string Type;

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
        public readonly int Id;
        [JsonProperty("memory_size_mb")]
        public readonly int MemorySizeMb;
        [JsonProperty("model")]
        public readonly string Model;
        [JsonProperty("vendor")]
        public readonly string Vendor;
        [JsonProperty("vendor_id")]
        public readonly int VendorId;
        [JsonProperty("version")]
        public readonly string Version;
        [JsonProperty("shader_level")]
        public readonly int ShaderLevel;

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
        public readonly int TotalSizeMb;

        public DeviceMemoryAttributes()
        {
            TotalSizeMb = SystemInfo.systemMemorySize;
        }
    }

    [Serializable]
    internal class DeviceScreenAttributes
    {
        [JsonProperty("dpi")]
        public readonly int Dpi;
        [JsonProperty("fullscreen")]
        public readonly int Fullscreen;
        [JsonProperty("refresh_rate_hz")]
        public readonly double RefreshRateHz;
        [JsonProperty("size")]
        public readonly string Size;

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
        public readonly string Language;
        [JsonProperty("version")]
        public readonly string Version;

        public OsAttributes()
        {
            Language = Application.systemLanguage.ToString();
            Version = SystemInfo.operatingSystem;
        }
    }
}
