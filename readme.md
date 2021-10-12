# osu! Replay Viewer
_Based on osu!lazer_

## Features
- View downloaded replays
- Download replays (if you can log in)
- Render replays to sequence of images (and you can combine them to make a video)

## Requirements
- [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
- Windows (At least for now. Linux and macOS support soon)
- OpenGL 3.0 devices (I think?)

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

Although there're prebuilt NuGet packages, they have .NET Framework as target, which makes Visual Studio
confuses.