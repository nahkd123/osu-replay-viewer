using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input.Handlers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.Timing;
using osu_replay_renderer_netcore.Audio;
using osu_replay_renderer_netcore.CustomHosts.Record;
using osu_replay_renderer_netcore.Patching;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CustomHosts
{
    /// <summary>
    /// Game host that's designed to record the game. This will spawn an OpenGL window, but this
    /// will be changed in the future (maybe we'll hide it, or maybe we'll implement entire
    /// fake window from scratch to make it render offscreen)
    /// </summary>
    public class WindowsRecordGameHost : DesktopGameHost
    {
        public override IEnumerable<string> UserStoragePaths => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Yield();

        public override void OpenFileExternally(string filename) => Logger.Log($"Application has requested file \"{filename}\" to be opened.");
        public override void OpenUrlExternally(string url) => Logger.Log($"Application has requested URL \"{url}\" to be opened.");

        private RecordClock recordClock;
        protected override IFrameBasedClock SceneGraphClock => recordClock;
        protected override IWindow CreateWindow() => new WindowsWindow();
        protected override IEnumerable<InputHandler> CreateAvailableInputHandlers() => new InputHandler[] { };

        public System.Drawing.Size Resolution { get; set; } = new System.Drawing.Size { Width = 1280, Height = 600 };
        public ExternalFFmpegEncoder Encoder { get; set; }
        public bool UsingEncoder { get; set; } = true;

        public WindowsRecordGameHost(string gameName = null, int frameRate = 60) : base(gameName, false)
        {
            recordClock = new RecordClock(frameRate);
            PrepareAudioRendering();
        }

        public AudioJournal AudioJournal { get; set; } = new();
        public AudioBuffer AudioTrack { get; set; } = null;
        public string AudioOutput { get; set; } = null;

        private void PrepareAudioRendering()
        {
            AudioPatcher.OnTrackPlay += track =>
            {
                Console.WriteLine($"Audio Rendering: Track played at frame #{recordClock.CurrentFrame}");
                Console.WriteLine(track.CurrentTime);
                if (AudioTrack == null) return;
                AudioJournal.BufferAt(recordClock.CurrentTime / 1000.0 + 2.0, AudioTrack);
            };

            AudioPatcher.OnSamplePlay += sample =>
            {
                Console.WriteLine($"Audio Rendering: Sample played at frame #{recordClock.CurrentFrame}: Freq = {sample.Frequency.Value}:{sample.AggregateFrequency.Value} | Volume = {sample.Volume}:{sample.AggregateVolume} | {recordClock.CurrentTime}s");
                AudioJournal.SampleAt(recordClock.CurrentTime / 1000.0, sample, buff =>
                {
                    buff = buff.CreateCopy();
                    if (sample.AggregateFrequency.Value != 1) buff.SoundTouchAll(p => p.Pitch = sample.Frequency.Value * sample.AggregateFrequency.Value);
                    buff.Process(x => x * sample.Volume.Value * sample.AggregateVolume.Value);
                    return buff;
                });
            };
        }

        public AudioBuffer FinishAudio()
        {
            AudioBuffer buff = AudioBuffer.FromSeconds(new AudioFormat
            {
                Channels = 2,
                SampleRate = 44100,
                PCMSize = 2
            }, AudioJournal.LongestDuration + 3.0);
            AudioJournal.MixSamples(buff);
            buff.Process(x => Math.Tanh(x));
            return buff;
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
                if (UsingEncoder && Encoder != null)
                {
                    if (ss.Width == Encoder.Resolution.Width && ss.Height == Encoder.Resolution.Height) Encoder.WriteFrame(ss);
                }
            }
            previousScreenshotTask = TakeScreenshotAsync();
        }
    }
}
