using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input.Handlers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu_replay_renderer_netcore.CustomHosts.Record;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CustomHosts
{
    /// <summary>
    /// Game host that's designed to record the game. This will spawn an OpenGL window, but this
    /// will be changed in the future (maybe we'll hide it, or maybe we'll implement entire
    /// fake window from scratch to make it render offscreen)
    /// </summary>
    public class ReplayRecordGameHost : DesktopGameHost
    {
        // public override IEnumerable<string> UserStoragePaths => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Yield();
        public override IEnumerable<string> UserStoragePaths => CrossPlatform.GetUserStoragePaths();

        public override void OpenFileExternally(string filename) => Logger.Log($"Application has requested file \"{filename}\" to be opened.");
        public override void OpenUrlExternally(string url) => Logger.Log($"Application has requested URL \"{url}\" to be opened.");

        private RecordClock recordClock;
        protected override IFrameBasedClock SceneGraphClock => recordClock;
        protected override IWindow CreateWindow() => new WindowsWindow();
        protected override IEnumerable<InputHandler> CreateAvailableInputHandlers() => new InputHandler[] { };

        public System.Drawing.Size Resolution { get; set; } = new System.Drawing.Size { Width = 1280, Height = 600 };
        public ExternalFFmpegEncoder Encoder { get; set; }
        public bool UsingEncoder { get; set; } = true;

        public ReplayRecordGameHost(string gameName = null, int frameRate = 60) : base(gameName, false)
        {
            recordClock = new RecordClock(frameRate);
        }

        protected override void SetupConfig(IDictionary<FrameworkSetting, object> defaultOverrides)
        {
            //defaultOverrides[FrameworkSetting.AudioDevice] = "No sound";
            base.SetupConfig(defaultOverrides);
        }

        protected override void SetupForRun()
        {
            base.SetupForRun();
            // The record procedure is basically like this:
            // 1. Create new OpenGL context
            // 2. Draw to that context
            // 3. Take screenshot (a.k.a read the context buffer)
            // 4. Store that screenshot to file, or feed it to FFmpeg
            // 5. Advance the clock to next frame
            // 6. Jump to step 2 until the game decided to end

            MaximumDrawHz = recordClock.FramesPerSecond;
            MaximumUpdateHz = MaximumInactiveHz = 0;
        }

        private Task<Image<Rgba32>> previousScreenshotTask;
        private bool setupHostInRender = false;

        protected virtual void SetupHostInRender()
        {
            Config.SetValue(FrameworkSetting.FrameSync, FrameSync.Unlimited);
        }

        protected override void DrawFrame()
        {
            // Make sure we're using correct framework config
            Config.SetValue(FrameworkSetting.WindowedSize, Resolution);
            Config.SetValue(FrameworkSetting.WindowMode, WindowMode.Windowed);

            if (Root == null) return;
            if (!setupHostInRender)
            {
                setupHostInRender = true;
                SetupHostInRender();
            }

            // Draw
            base.DrawFrame();

            // Advance the clock
            //Console.WriteLine(recordClock.CurrentFrame + ": " + recordClock.CurrentTime);
            recordClock.CurrentFrame++;

            // Now we'll render it either to image files or feed it directly to FFmpeg
            if (previousScreenshotTask != null)
            {
                Task.WaitAll(previousScreenshotTask);
                Image<Rgba32> ss = previousScreenshotTask.Result;
                //if (!Directory.Exists(@"video")) Directory.CreateDirectory(@"video");
                //ss.SaveAsJpeg(@"./video/" + recordClock.CurrentFrame.ToString().PadLeft(8, '0') + ".jpeg");
                if (UsingEncoder && Encoder != null)
                {
                    if (ss.Width == Encoder.Resolution.Width && ss.Height == Encoder.Resolution.Height) Encoder.WriteRGBA(ss);
                }
            }
            previousScreenshotTask = TakeScreenshotAsync();
        }
    }
}
