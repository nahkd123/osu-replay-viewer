using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CustomHosts.Record
{
    /// <summary>
    /// FFmpeg video encoder with actual FFmpeg executable instead of FFmpeg.AutoGen
    /// </summary>
    public class ExternalFFmpegEncoder
    {
        public Process FFmpeg { get; private set; }
        public Stream InputStream { get; private set; }

        public int FPS { get; set; } = 60;
        public System.Drawing.Size Resolution { get; set; }
        public string OutputPath { get; set; } = "output.mp4";
        public string Preset { get; set; } = "slow";
        public string Encoder { get; set; } = "libx264";
        public bool MotionInterpolation { get; set; } = false;

        /// <summary>
        /// Blend multiple frames. Values that's lower than or equals to 1 will disable frames
        /// blending. Frames blending makes encoding process way slower
        /// </summary>
        public int FramesBlending { get; set; } = 1;

        public string FFmpegArguments
        {
            get
            {
                int actualFramesBlending = Math.Max(FramesBlending, 1);

                string inputParameters = $"-f rawvideo -pixel_format rgb24 -video_size {Resolution.Width}x{Resolution.Height} -framerate {FPS * actualFramesBlending} -i pipe:";
                string inputEffect;
                if (actualFramesBlending > 1) inputEffect = $"-vf tblend=all_mode=average -r {FPS}";
                else if (MotionInterpolation) inputEffect = $"-vf minterpolate=fps={FPS * 4}";
                else inputEffect = null;
                string outputParameters = $"-c:v {Encoder} -preset {Preset} {OutputPath}";

                /*if (actualFramesBlending > 1) return $"-f image2pipe -vcodec {ImageFormat} -framerate {FPS * actualFramesBlending} -i pipe: -vf tblend=all_mode=average -r {FPS} -preset {Preset} {OutputPath}";
                else if (MotionInterpolation) return $"-f image2pipe -vcodec {ImageFormat} -framerate {FPS} -i pipe: -vf minterpolate=fps={FPS * 4} -preset {Preset} {OutputPath}";
                else return $"-f image2pipe -vcodec {ImageFormat} -framerate {FPS} -i pipe: -preset {Preset} {OutputPath}";*/
                return inputParameters + (inputEffect != null? (" " + inputEffect) : "") + " " + outputParameters;
            }
        }

        private byte[] buffer;

        public void StartFFmpeg()
        {
            buffer = new byte[Resolution.Width * Resolution.Height * 3];
            Console.WriteLine("Starting FFmpeg process with arguments: " + FFmpegArguments);
            FFmpeg = new Process()
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    FileName = "ffmpeg",
                    /*Arguments = string.Join(" ",
                        "-f image2pipe",
                        "-vcodec " + imageFormat,
                        "-framerate " + fps,
                        "-i pipe:",
                        "-preset " + preset,
                        outputPath
                    ),*/
                    Arguments = FFmpegArguments,
                    RedirectStandardInput = true
                }
            };
            FFmpeg.Start();
            InputStream = FFmpeg.StandardInput.BaseStream;
        }

        public void WriteRGBA(Image<Rgba32> image)
        {
            if (image.Width != Resolution.Width || image.Height != Resolution.Height) throw new ArgumentException("Invaild image size");
            if (image.TryGetSinglePixelSpan(out var span))
            {
                for (int i = 0; i < span.Length; i++)
                {
                    Rgba32 pixel = span[i];
                    buffer[i * 3] = pixel.R;
                    buffer[i * 3 + 1] = pixel.G;
                    buffer[i * 3 + 2] = pixel.B;
                }
            }
            InputStream.Write(buffer);
        }
    }
}
