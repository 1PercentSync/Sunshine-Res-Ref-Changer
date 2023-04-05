using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace ScreenResolutionChanger
{
    class Program
    {
        const int DM_PELSWIDTH = 0x80000;
        const int DM_PELSHEIGHT = 0x100000;
        const int DM_DISPLAYFREQUENCY = 0x400000;


        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && args[0].ToLower() == "list"))
            {
                ListAvailableResolutions();
            }
            else if (args.Length == 1)
            {
                var input = args[0];
                var splitted = input.Split(new[] { 'x', '@' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitted.Length == 3 && int.TryParse(splitted[0], out int width) && int.TryParse(splitted[1], out int height) && int.TryParse(splitted[2], out int frequency))
                {
                    ChangeResolution(width, height, frequency);
                }
                else
                {
                    Console.WriteLine("Invalid input format. Example: 1920x1080@60");
                }
            }
            else
            {
                Console.WriteLine("Invalid arguments.");
            }
        }

        static void ListAvailableResolutions()
        {
            var resolutions = new List<Tuple<int, int, int>>();

            DEVMODE vDevMode = new DEVMODE();
            int i = 0;

            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                resolutions.Add(Tuple.Create(vDevMode.dmPelsWidth, vDevMode.dmPelsHeight, vDevMode.dmDisplayFrequency));
                i++;
            }

            var distinctResolutions = resolutions.Distinct().OrderByDescending(r => r.Item1 * r.Item2).ThenByDescending(r => r.Item3);

            Console.WriteLine("Available screen resolutions and refresh rates:");
            foreach (var resolution in distinctResolutions)
            {
                Console.WriteLine($"{resolution.Item1}x{resolution.Item2}@{resolution.Item3}");
            }
        }

        static void ChangeResolution(int width, int height, int frequency)
        {
            DEVMODE current = new DEVMODE();
            current.dmSize = (short)Marshal.SizeOf(current);
            EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current);

            DEVMODE newDevMode = new DEVMODE();
            newDevMode.dmSize = (short)Marshal.SizeOf(newDevMode);
            newDevMode.dmDeviceName = current.dmDeviceName;
            newDevMode.dmPelsWidth = width;
            newDevMode.dmPelsHeight = height;
            newDevMode.dmDisplayFrequency = frequency;
            newDevMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY;

            int result = ChangeDisplaySettings(ref newDevMode, 0);
            if (result == 0)
            {
                Console.WriteLine("Screen resolution and refresh rate successfully changed.");
            }
            else
            {
                Console.WriteLine("Error: Unable to change the screen resolution and refresh rate.");
            }
        }
    }
}