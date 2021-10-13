# osu! Replay Viewer
_Based on osu!lazer_

This replay viewer allow you to view imported replays (yes you have to import them in osu!lazer
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

## Build
To build this project, you need:

- .NET 5.0 SDK
- NuGet Package: ``ppy.osu.Game 0.0.0`` (build from source)
- NuGet Package: ``ppy.osu.Game.Rulesets.Osu 1.0.0`` (build from source)

Build NuGet packages from source please. Get source code [here](https://github.com/ppy/osu)

### Make ``ppy.osu.Game`` NuGet packages
To make packages, simply type ``dotnet pack osu.Game`` and ``dotnet pack osu.Game.Rulesets.Osu``, then
install those packages to your local packages source (Eg: ``nuget add ppy.osu.Game.0.0.0.nupkg -Source
~/nuget``)
