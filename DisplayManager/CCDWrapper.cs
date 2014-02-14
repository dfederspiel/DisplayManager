using System;
using System.Runtime.InteropServices;

namespace DisplayManager
{
    public class CCDWrapper
    {
        public CCDWrapper()
        {
        }

        [DllImport("User32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern CCDWrapper.StatusCode DisplayConfigGetDeviceInfo(IntPtr requestPacket);

        public static CCDWrapper.StatusCode DisplayConfigGetDeviceInfo<T>(ref T displayConfig)
        where T : CCDWrapper.IDisplayConfigInfo
        {
            return CCDWrapper.MarshalStructureAndCall<T>(ref displayConfig, new Func<IntPtr, CCDWrapper.StatusCode>(CCDWrapper.DisplayConfigGetDeviceInfo));
        }

        [DllImport("User32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern int GetDisplayConfigBufferSizes(CCDWrapper.QueryDisplayFlags flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

        private static CCDWrapper.StatusCode MarshalStructureAndCall<T>(ref T displayConfig, Func<IntPtr, CCDWrapper.StatusCode> func)
        where T : CCDWrapper.IDisplayConfigInfo
        {
            IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(displayConfig));
            Marshal.StructureToPtr(displayConfig, intPtr, false);
            CCDWrapper.StatusCode statusCode = func(intPtr);
            displayConfig = (T)Marshal.PtrToStructure(intPtr, displayConfig.GetType());
            Marshal.FreeHGlobal(intPtr);
            return statusCode;
        }

        [DllImport("User32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern int QueryDisplayConfig(CCDWrapper.QueryDisplayFlags flags, ref uint numPathArrayElements, [Out] CCDWrapper.DisplayConfigPathInfo[] pathInfoArray, ref uint modeInfoArrayElements, [Out] CCDWrapper.DisplayConfigModeInfo[] modeInfoArray, IntPtr z);

        [DllImport("User32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        public static extern int SetDisplayConfig(uint numPathArrayElements, [In] CCDWrapper.DisplayConfigPathInfo[] pathArray, uint numModeInfoArrayElements, [In] CCDWrapper.DisplayConfigModeInfo[] modeInfoArray, CCDWrapper.SdcFlags flags);

        [Flags]
        public enum D3DmdtVideoSignalStandard : uint
        {
            Uninitialized = 0,
            VesaDmt = 1,
            VesaGtf = 2,
            VesaCvt = 3,
            Ibm = 4,
            Apple = 5,
            NtscM = 6,
            NtscJ = 7,
            Ntsc443 = 8,
            PalB = 9,
            PalB1 = 10,
            PalG = 11,
            PalH = 12,
            PalI = 13,
            PalD = 14,
            PalN = 15,
            PalNc = 16,
            SecamB = 17,
            SecamD = 18,
            SecamG = 19,
            SecamH = 20,
            SecamK = 21,
            SecamK1 = 22,
            SecamL = 23,
            SecamL1 = 24,
            Eia861 = 25,
            Eia861A = 26,
            Eia861B = 27,
            PalK = 28,
            PalK1 = 29,
            PalL = 30,
            PalM = 31,
            Other = 255
        }

        public struct DisplayConfig2DRegion
        {
            public uint cx;

            public uint cy;
        }

        public struct DisplayConfigDeviceInfoHeader
        {
            public CCDWrapper.DisplayConfigDeviceInfoType type;

            public int size;

            public CCDWrapper.LUID adapterId;

            public uint id;
        }

        public enum DisplayConfigDeviceInfoType : uint
        {
            GetSourceName = 1,
            GetTargetName = 2,
            GetTargetPreferredMode = 3,
            GetAdapterName = 4,
            SetTargetPersistence = 5
        }

        [Flags]
        public enum DisplayConfigFlags : uint
        {
            Zero,
            PathActive
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DisplayConfigModeInfo
        {
            [FieldOffset(0)]
            public CCDWrapper.DisplayConfigModeInfoType infoType;

            [FieldOffset(4)]
            public uint id;

            [FieldOffset(8)]
            public CCDWrapper.LUID adapterId;

            [FieldOffset(16)]
            public CCDWrapper.DisplayConfigTargetMode targetMode;

            [FieldOffset(16)]
            public CCDWrapper.DisplayConfigSourceMode sourceMode;
        }

        [Flags]
        public enum DisplayConfigModeInfoType : uint
        {
            Zero = 0,
            Source = 1,
            Target = 2,
            ForceUint32 = 4294967295
        }

        public struct DisplayConfigPathInfo
        {
            public CCDWrapper.DisplayConfigPathSourceInfo sourceInfo;

            public CCDWrapper.DisplayConfigPathTargetInfo targetInfo;

            public uint flags;
        }

        public struct DisplayConfigPathSourceInfo
        {
            public CCDWrapper.LUID adapterId;

            public uint id;

            public uint modeInfoIdx;

            public CCDWrapper.DisplayConfigSourceStatus statusFlags;
        }

        public struct DisplayConfigPathTargetInfo
        {
            public CCDWrapper.LUID adapterId;

            public uint id;

            public uint modeInfoIdx;

            public CCDWrapper.DisplayConfigVideoOutputTechnology outputTechnology;

            public CCDWrapper.DisplayConfigRotation rotation;

            public CCDWrapper.DisplayConfigScaling scaling;

            public CCDWrapper.DisplayConfigRational refreshRate;

            public CCDWrapper.DisplayConfigScanLineOrdering scanLineOrdering;

            public bool targetAvailable;

            public CCDWrapper.DisplayConfigTargetStatus statusFlags;
        }

        [Flags]
        public enum DisplayConfigPixelFormat : uint
        {
            Zero = 0,
            Pixelformat8Bpp = 1,
            Pixelformat16Bpp = 2,
            Pixelformat24Bpp = 3,
            Pixelformat32Bpp = 4,
            PixelformatNongdi = 5,
            PixelformatForceUint32 = 4294967295
        }

        public struct DisplayConfigRational
        {
            public uint numerator;

            public uint denominator;
        }

        [Flags]
        public enum DisplayConfigRotation : uint
        {
            Zero = 0,
            Identity = 1,
            Rotate90 = 2,
            Rotate180 = 3,
            Rotate270 = 4,
            ForceUint32 = 4294967295
        }

        [Flags]
        public enum DisplayConfigScaling : uint
        {
            Zero = 0,
            Identity = 1,
            Centered = 2,
            Stretched = 3,
            Aspectratiocenteredmax = 4,
            Custom = 5,
            Preferred = 128,
            ForceUint32 = 4294967295
        }

        [Flags]
        public enum DisplayConfigScanLineOrdering : uint
        {
            Unspecified = 0,
            Progressive = 1,
            Interlaced = 2,
            InterlacedUpperfieldfirst = 2,
            InterlacedLowerfieldfirst = 3,
            ForceUint32 = 4294967295
        }

        public struct DisplayConfigSourceDeviceName : CCDWrapper.IDisplayConfigInfo
        {
            private const int Cchdevicename = 32;

            public CCDWrapper.DisplayConfigDeviceInfoHeader header;

            public string viewGdiDeviceName;
        }

        public struct DisplayConfigSourceMode
        {
            public uint width;

            public uint height;

            public CCDWrapper.DisplayConfigPixelFormat pixelFormat;

            public CCDWrapper.PointL position;
        }

        [Flags]
        public enum DisplayConfigSourceStatus
        {
            Zero,
            InUse
        }

        public struct DisplayConfigTargetMode
        {
            public CCDWrapper.DisplayConfigVideoSignalInfo targetVideoSignalInfo;
        }

        [Flags]
        public enum DisplayConfigTargetStatus : uint
        {
            Zero = 0,
            InUse = 1,
            FORCIBLE = 2,
            ForcedAvailabilityBoot = 4,
            ForcedAvailabilityPath = 8,
            ForcedAvailabilitySystem = 16
        }

        [Flags]
        public enum DisplayConfigTopologyId : uint
        {
            Zero = 0,
            Internal = 1,
            Clone = 2,
            Extend = 4,
            External = 8,
            ForceUint32 = 4294967295
        }

        [Flags]
        public enum DisplayConfigVideoOutputTechnology : uint
        {
            Hd15 = 0,
            Svideo = 1,
            CompositeVideo = 2,
            ComponentVideo = 3,
            Dvi = 4,
            Hdmi = 5,
            Lvds = 6,
            DJpn = 8,
            Sdi = 9,
            DisplayportExternal = 10,
            DisplayportEmbedded = 11,
            UdiExternal = 12,
            UdiEmbedded = 13,
            Sdtvdongle = 14,
            Internal = 2147483648,
            ForceUint32 = 4294967295,
            Other = 4294967295
        }

        public struct DisplayConfigVideoSignalInfo
        {
            public long pixelRate;

            public CCDWrapper.DisplayConfigRational hSyncFreq;

            public CCDWrapper.DisplayConfigRational vSyncFreq;

            public CCDWrapper.DisplayConfig2DRegion activeSize;

            public CCDWrapper.DisplayConfig2DRegion totalSize;

            public CCDWrapper.D3DmdtVideoSignalStandard videoStandard;

            public CCDWrapper.DisplayConfigScanLineOrdering ScanLineOrdering;
        }

        public interface IDisplayConfigInfo
        {

        }

        public struct LUID
        {
            public uint LowPart;

            public uint HighPart;
        }

        public struct PointL
        {
            public int x;

            public int y;
        }

        [Flags]
        public enum QueryDisplayFlags : uint
        {
            Zero = 0,
            AllPaths = 1,
            OnlyActivePaths = 2,
            DatabaseCurrent = 4
        }

        [Flags]
        public enum SdcFlags : uint
        {
            Zero = 0,
            TopologyInternal = 1,
            TopologyClone = 2,
            TopologyExtend = 4,
            TopologyExternal = 8,
            UseDatabaseCurrent = 15,
            TopologySupplied = 16,
            UseSuppliedDisplayConfig = 32,
            Validate = 64,
            Apply = 128,
            NoOptimization = 256,
            SaveToDatabase = 512,
            AllowChanges = 1024,
            PathPersistIfRequired = 2048,
            ForceModeEnumeration = 4096,
            AllowPathOrderChanges = 8192
        }

        public enum StatusCode : uint
        {
            Success = 0,
            AccessDenied = 5,
            GenFailure = 31,
            NotSupported = 50,
            InvalidParameter = 87,
            InSufficientBuffer = 122,
            BadConfiguration = 1610
        }
    }
}