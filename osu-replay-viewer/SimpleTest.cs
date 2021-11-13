using ManagedBass;
using osu_replay_renderer_netcore.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore
{
    /// <summary>
    /// Simple test runner, using command-line argument to choose test.
    /// Usually used when I'm too lazy.
    /// </summary>
    public class SimpleTest
    {
        public static void ExecuteTest(string type)
        {
            SetUp();
            Console.WriteLine($" --- Test: {type}");
            switch (type)
            {
                case "audio-mixing": AudioMixing(); break;
                default: TestDefault(); break;
            }
        }

        public static void TestDefault()
        {
            Console.Error.WriteLine("Unknown test");
        }

        public static void AudioMixing()
        {
            Console.WriteLine("Cleaning up...");
            CleanUpFile("audiomixing.wav");

            Console.WriteLine("Setting up...");
            var targetBuffer = new AudioBuffer(new AudioFormat { SampleRate = 44100 }, 44100 * 10);
            var sampleBuffer1 = AudioBuffer.NoiseBuffer(new AudioFormat { SampleRate = 44100 }, 44100, 0.42);
            var sampleBuffer2 = AudioBuffer.FromFunction(new AudioFormat { SampleRate = 48000 }, 48000, t =>
            {
                return Math.Sin(t * 440.0 * 2 * Math.PI) * 0.5;
            });
            var mixer = new SamplesMixer(targetBuffer);

            Console.WriteLine("Mixing...");
            mixer.Mix(sampleBuffer1, 0);
            mixer.Mix(sampleBuffer1, 2);
            mixer.Mix(sampleBuffer1, 4);
            mixer.Mix(sampleBuffer1, 8);

            mixer.Mix(sampleBuffer2, 1);
            mixer.Mix(sampleBuffer2, 7);
            mixer.Mix(sampleBuffer2, 7);
            mixer.Mix(sampleBuffer2, 8);

            Console.WriteLine("Writing to testresults/audiomixing.wav...");
            var stream = OpenStream("audiomixing.wav");
            targetBuffer.WriteWave(stream);
            stream.Close();
        }

        private static void SetUp() { if (!Directory.Exists("testresults")) Directory.CreateDirectory("testresults"); }
        private static void CleanUpFile(string file)
        {
            if (File.Exists("testresults" + Path.DirectorySeparatorChar + file))
                File.Delete("testresults" + Path.DirectorySeparatorChar + file);
        }
        private static FileStream OpenStream(string file)
        {
            return new FileStream("testresults" + Path.DirectorySeparatorChar + file, FileMode.OpenOrCreate);
        }
    }
}
