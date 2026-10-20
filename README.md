# Blazares Daedalus Mosaic (MosaicMe)

An experimental .NET console application that renders an image in the terminal using colored block characters. The repository also contains earlier rendering experiments and mosaic algorithm prototypes.

## Requirements

- .NET 10 SDK
- A terminal that supports ANSI colors and Unicode block characters
- An image file supported by ImageMagick/Magick.NET

## Run

From the repository root:

```sh
dotnet run --project MosaicMeCli -- "/path/to/image.jpg"
```

Or build and run the compiled application:

```sh
dotnet build MosaicMe.sln
dotnet run --project MosaicMeCli -- "/path/to/image.jpg"
```

The image path is required. The application prints a terminal-sized preview; it does not write or modify the source image.

## Docker

The project includes a Dockerfile. Build the image from the repository root:

```sh
docker build -f MosaicMeCli/Dockerfile -t mosaicme .
```

Pass an image from the host through a read-only mount:

```sh
docker run --rm -it -v "$PWD:/images:ro" mosaicme /images/image.jpg
```

## Project status

This is an early-stage experiment. `MosaicMeCli` currently uses Magick.NET 15 and .NET 10; the `Test/MosaicAndric` directory contains additional algorithm prototypes that are not wired into the command-line flow.
