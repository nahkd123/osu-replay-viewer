using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.Audio
{
    /// <summary>
    /// Offline mixer to mix all samples together. You can also mix it with
    /// music.
    /// </summary>
    public class SamplesMixer
    {
        public readonly AudioBuffer Buffer;
        public AudioFormat Format { get => Buffer.Format; }

        public SamplesMixer(AudioBuffer buffer)
        {
            Buffer = buffer;
        }

        public void Mix(AudioBuffer sample, double startSec)
        {
            if (sample == null) return;

            int bufferStartSample = (int)Math.Floor(startSec * Format.SampleRate);
            if (Format.SampleRate == sample.Format.SampleRate)
            {
                for (int i = 0; i < sample.Samples; i++)
                {
                    if (i >= Buffer.Samples) return;
                    for (int ch = 0; ch < Format.Channels; ch++)
                        Buffer[ch, bufferStartSample + i] += sample[ch, i];
                }
            }
            else
            {
                var duration = sample.Duration;
                for (int i = 0; i < Format.SampleRate * duration; i++)
                {
                    if (i >= Buffer.Samples) return;
                    for (int ch = 0; ch < Format.Channels; ch++)
                        Buffer[ch, bufferStartSample + i] += sample.Resample(ch, Format.SampleRate, i);
                }
            }
        }
    }
}
