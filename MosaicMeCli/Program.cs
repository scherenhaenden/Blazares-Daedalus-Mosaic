using ImageMagick;
using MosaicMeCli.Test;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: MosaicMeCli <image-path>");
    return 1;
}

var imagePath = Path.GetFullPath(args[0]);
if (!File.Exists(imagePath))
{
    Console.Error.WriteLine($"Image file not found: {imagePath}");
    return 1;
}

try
{
    using var image = new MagickImage(imagePath);
    ImagePrintingV2.DrawImage(image);
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Could not render image: {exception.Message}");
    return 1;
}
