using osu.Framework;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using System;
using System.Collections.Generic;
using System.IO;

namespace osu_replay_renderer_netcore.CustomHosts
{
    public static class CrossPlatform
    {
        public static IEnumerable<string> GetUserStoragePaths()
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    yield return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;

                case RuntimeInfo.Platform.Linux:
                case RuntimeInfo.Platform.macOS:
                    // https://github.com/ppy/osu-framework/blob/master/osu.Framework/Platform/Linux/LinuxGameHost.cs
                    string xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                    if (!string.IsNullOrEmpty(xdg)) yield return xdg;
                    yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local", "share");
                    // foreach (string path in baseHost.UserStoragePaths) yield return path;
                    break;

                default: throw new InvalidOperationException($"Unknown platform: {Enum.GetName(typeof(RuntimeInfo.Platform), RuntimeInfo.OS)}");
            }
        }

        public static IWindow GetWindow()
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows: return new WindowsWindow();
                case RuntimeInfo.Platform.Linux:
                case RuntimeInfo.Platform.macOS:
                    return new SDL2DesktopWindow();

                default: throw new InvalidOperationException($"Unknown platform: {Enum.GetName(typeof(RuntimeInfo.Platform), RuntimeInfo.OS)}");
            }
        }
    }
}
