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

        public ExternalFFmpegEncoder(int fps, string imageFormat, string outputPath, string preset = "veryslow")
        {
            FFmpeg = new Process()
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    FileName = "ffmpeg",
                    Arguments = string.Join(" ",
                        "-f image2pipe",
                        "-vcodec " + imageFormat,
                        "-framerate " + fps,
                        "-i pipe:",
                        "-preset " + preset,
                        outputPath
                    ),
                    RedirectStandardInput = true
                }
            };
            FFmpeg.Start();
            InputStream = FFmpeg.StandardInput.BaseStream;
        }
    }
}
