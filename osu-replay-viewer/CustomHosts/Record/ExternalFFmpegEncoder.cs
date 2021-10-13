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
        public string ImageFormat { get; set; } = "png";
        public string OutputPath { get; set; } = "output.mp4";
        public string Preset { get; set; } = "slow";
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

                if (actualFramesBlending > 1) return $"-f image2pipe -vcodec {ImageFormat} -framerate {FPS * actualFramesBlending} -i pipe: -vf tblend=all_mode=average -r {FPS} -preset {Preset} {OutputPath}";
                else if (MotionInterpolation) return $"-f image2pipe -vcodec {ImageFormat} -framerate {FPS} -i pipe: -vf minterpolate=fps={FPS * 4} -preset {Preset} {OutputPath}";
                else return $"-f image2pipe -vcodec {ImageFormat} -framerate {FPS} -i pipe: -preset {Preset} {OutputPath}";
            }
        }

        public void StartFFmpeg()
        {
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
    }
}
