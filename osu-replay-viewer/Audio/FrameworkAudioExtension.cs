using ManagedBass;
using osu.Framework.Audio.Mixing;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Audio;
using System;
using System.Reflection;

namespace osu_replay_renderer_netcore.Audio
{
    public static class FrameworkAudioExtension
    {
        public static ISample GetUnderlaying(this DrawableSample drawable)
        {
            var field = typeof(DrawableSample).GetField("sample", BindingFlags.NonPublic | BindingFlags.Instance);
            return (ISample)field.GetValue(drawable);
        }

        /// <summary>
        /// Check if the sample is SampleBass, which is inaccessable without
        /// reflection. SampleBass is not designed to be accessable outside
        /// framework, so this method might break in the future
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static bool IsSampleBass(this ISample sample) => sample.GetType().IsAssignableTo(SampleBassAdapter.SampleBass);

        /// <summary>
        /// Convert sample interface to SampleBassAdapter, which contains methods
        /// for easy access to SampleId and various methods
        /// </summary>
        /// <param name="sample">The sample object</param>
        /// <returns>null if the sample is not SampleBass</returns>
        public static SampleBassAdapter AsSampleBass(this ISample sample)
        {
            if (!sample.IsSampleBass()) return null;
            return new SampleBassAdapter(sample);
        }

        private static readonly FieldInfo TrackBass_activeStream = typeof(TrackBass).GetField("activeStream", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int GetActiveStreamHandle(this TrackBass track) => (int)TrackBass_activeStream.GetValue(track);
    }
}
