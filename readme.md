# osu! Replay Viewer
_Based on osu!lazer_

This replay viewer allow you to view imported replays (yes you have to import **beatmaps & replays (skins are not supported now)** in osu!lazer
client) without launching the actual game, and you can also render replays to video files, thanks
to FFmpeg.

This project aims to make replay viewer without modifying the official game code or write entire
thing from scratch, but uses components from the game instead. Because of this, it's much more easy
to upgrade to make UI matches with actual game;

> This project somewhat implemented [this](https://github.com/ppy/osu/discussions/12986) idea (except
  we're running outside the official client)

## Features
- View downloaded replays
- Download replays (if you can log in)
- ~~Render replays to sequence of images~~ (it actually render replay to a video now)

## To-dos
- Record audio (seems impossible until osu!framework allow us to replace AudioThread)
- Clean up messy code
- Allow user to choose different osu!lazer application directory

## Requirements
- [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
- Windows (At least for now. Linux and macOS support soon)
- OpenGL 3.0 device (I think?)
- FFmpeg installed as command (a.k.a you must be able to run ``ffmpeg`` without including an entire
  path to it) if you want to render the replay to video

## Installing FFmpeg
1. Grab FFmpeg binaries [here](https://www.ffmpeg.org/download.html)
  > Linux users can also install FFmpeg from package manager included in their distribution

  > Windows users can download FFmpeg [here](https://www.gyan.dev/ffmpeg/builds/) or
    [here](https://github.com/BtbN/FFmpeg-Builds/releases)

2. Include ``ffmpeg`` in command line path
3. Confirm that it's working by running ``ffmpeg`` alone

## Command Line arguments
> You can view all command line arguments by running the executable alone

Usage: ``[...Options] Subcommand <...Subcommand Arguments>``

### Subcommands
- ``view <Local Replay ID>``, ``view online:<Online Replay ID>`` or ``view file:<File.osr>``:
  View replay
- ``list``: View all downloaded scores/replays as list, including ruleset, selected mods and ranked
  score
- ``download <Online Replay Id>``: Attempt to download replay from osu.ppy.sh (broken right now
  because you need to log in in order to download replays)

### Options
- ``--headless``: Run replay viewer without window. Designed to test some features
- ``--record`` or ``-R``: Run replay viewer and record it to video file
- ``--record-resolution <Width> <Height>`` or ``-RS <Width> <Height>``: Set record resolution
- ``--record-fps <FPS>`` or ``-RF <FPS>``: Set record final frame rate
- ``--record-frames-blending <Amount>`` or ``-RB <Amount>``: Smooth recorded gameplay by blending
  multiple frames together
- ``--record-minterpolate`` or ``-RMI``: Apply x4 motion interpolation to recorded gameplay
- ``--record-ffmpeg-preset <Preset>`` or ``-RP <Preset>``: Set FFmpeg encoding preset
- ``--record-output <Output.mp4>`` or ``-O <Output.mp4>``: Set recorded gameplay output

## Build
To build this project, you need:

- .NET 5.0 SDK
- NuGet Package: ``ppy.osu.Game``
- NuGet Package: ``ppy.osu.Game.Rulesets.Osu``
