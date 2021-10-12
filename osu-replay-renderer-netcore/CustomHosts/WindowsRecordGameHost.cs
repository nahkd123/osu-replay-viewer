using JetBrains.Annotations;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CustomHosts
{
    public class WindowsRecordGameHost : DesktopGameHost
    {
        public override IEnumerable<string> UserStoragePaths => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Yield();

        public override void OpenFileExternally(string filename) => Logger.Log($"Application has requested file \"{filename}\" to be opened.");
        public override void OpenUrlExternally(string url) => Logger.Log($"Application has requested URL \"{url}\" to be opened.");

        public WindowsRecordGameHost(string gameName = null, int frameRate = 60) : base(gameName, false)
        {}

        public class RecordClock : IFrameBasedClock
        {
            public double FrameTime { get; private set; }
            public int CurrentFrame { get; private set; } = 0;
            private int FPS;

            public RecordClock(int frameRate)
            {
                FPS = frameRate;
                FrameTime = 1000.0 / FPS;
            }

            public double ElapsedFrameTime => FrameTime;
            public double FramesPerSecond => FPS;
            FrameTimeInfo IFrameBasedClock.TimeInfo => new() { Elapsed = FrameTime, Current = CurrentTime };
            public double CurrentTime => 1000.0 * CurrentFrame / FPS;
            public double Rate => 1.00;
            public bool IsRunning => true;

            public void ProcessFrame() { CurrentFrame++; }
        }
    }
}
