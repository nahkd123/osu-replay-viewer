using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.Audio
{
    /// <summary>
    /// Audio buffer. Can be the music or the sample. However, the internal sample datas
    /// are represented as float (single precision float), which uses about 27.5 MiB (about
    /// 28.8 MB) of system memory to store 5 minutes stereo buffer with 48kHz
    /// </summary>
    public class AudioBuffer
    {
        /// <summary>
        /// Buffer audio format
        /// </summary>
        public readonly AudioFormat Format;

        /// <summary>
        /// Number of samples in this buffer. Divide by sample rate and you got
        /// the buffer duration.
        /// </summary>
        public readonly int Samples;

        /// <summary>
        /// The buffer duration in seconds
        /// </summary>
        public double Duration { get => ((double)Samples) / Format.SampleRate; }

        /// <summary>
        /// Buffer data. [c * n] is the channel #1 data, [c * n + 1] is the channel #2
        /// data, etc...
        /// </summary>
        public readonly float[] Data;

        public AudioBuffer(AudioFormat format, int samples)
        {
            Format = format;
            Samples = samples;
            Data = new float[format.Channels * samples];
        }

        /// <summary>
        /// Get the PCM value at index x in channel y
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="index">Sample index</param>
        /// <returns></returns>
        public float this[int channel, int index]
        {
            get => Data[Format.Channels * index + channel];
            set => Data[Format.Channels * index + channel] = value;
        }

        public float InterpolateAt(double t, int channel)
        {
            int sample = (int)Math.Floor(t * Format.SampleRate);
            float a = sample > 0? this[channel, sample - 1] : this[channel, sample];
            float b = this[channel, sample];
            float c = sample < Samples - 1? this[channel, sample + 1] : this[channel, sample];
            return (a + b * 2f + c) / 4f;
        }

        public float Resample(int channel, int dstSampleRate, int dstIndex)
        {
            double srcIndexFloat = Format.SampleRate * ((double)dstIndex) / dstSampleRate;
            int srcIndex = (int)Math.Floor(srcIndexFloat);
            float a = srcIndex >= 0? this[channel, srcIndex] : this[channel, 0];
            float b = srcIndex < Samples - 1? this[channel, srcIndex + 1] : this[channel, Samples - 1];
            float mix = (float)(srcIndexFloat - srcIndex);
            return a * (1f - mix) + b * mix;
        }

        /// <summary>
        /// Write buffer data as RIFF Wave to stream
        /// </summary>
        /// <param name="stream"></param>
        public void WriteWave(Stream stream)
        {
            stream.Write(new byte[] { 0x52, 0x49, 0x46, 0x46 }); // RIFF
            stream.Write(BitConverter.GetBytes(36 + Samples * Format.Channels * Format.PCMSize)); // int: ChunkSize
            stream.Write(new byte[] { 0x57, 0x41, 0x56, 0x45 }); // WAVE

            stream.Write(new byte[] { 0x66, 0x6d, 0x74, 0x20 }); // "fmt "
            stream.Write(BitConverter.GetBytes(16)); // int: Subchunk1Size
            stream.Write(BitConverter.GetBytes((short)1)); // short: AudioFormat (1 = PCM)
            stream.Write(BitConverter.GetBytes((short)Format.Channels)); // short: NumChannels
            stream.Write(BitConverter.GetBytes(Format.SampleRate)); // int: SampleRate
            stream.Write(BitConverter.GetBytes(Format.SampleRate * Format.Channels * Format.PCMSize)); // int: ByteRate
            stream.Write(BitConverter.GetBytes((short)(Format.Channels * Format.PCMSize))); // short: BlockAlign
            stream.Write(BitConverter.GetBytes((short)(Format.PCMSize * 8))); // short: BitsPerSample

            stream.Write(new byte[] { 0x64, 0x61, 0x74, 0x61 }); // "data"
            stream.Write(BitConverter.GetBytes(Samples * Format.Channels * Format.PCMSize)); // int: Subchunk2Size
            for (int i = 0; i < Data.Length; i++) stream.Write(Format.AmpToBytes(Data[i]));
        }

        public static AudioBuffer NoiseBuffer(AudioFormat format, int samples, double amp = 0.78)
        {
            var buff = new AudioBuffer(format, samples);
            Random rng = new();
            for (int i = 0; i < samples; i++)
            {
                for (int ch = 0; ch < format.Channels; ch++)
                    buff[ch, i] = (float)((1.0 - rng.NextDouble() * 2.0) * amp);
            }
            return buff;
        }

        /// <summary>
        /// Create new audio buffer with data generated from given function
        /// </summary>
        /// <param name="format">Audio format to generate</param>
        /// <param name="samples">Number of samples</param>
        /// <param name="func">Generator function, where the given parameter is the
        /// time in second</param>
        /// <returns></returns>
        public static AudioBuffer FromFunction(AudioFormat format, int samples, Func<double, double> func)
        {
            var buff = new AudioBuffer(format, samples);
            double a;
            for (int i = 0; i < samples; i++)
            {
                a = func((double)i / samples);
                for (int ch = 0; ch < format.Channels; ch++) buff[ch, i] = (float)a;
            }
            return buff;
        }
    }
}
