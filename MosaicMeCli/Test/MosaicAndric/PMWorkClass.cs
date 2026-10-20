using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace MosaicMeCli.Test.MosaicAndric;

public class PMWorkClass
{
    private static ArrayList m_DB;
    public static Color[,] SourceImage;
    private static Size SourceImageSize;
    private static AlgorithmParameters m_Param;
    public static Bitmap OutputImage;
    public static ArrayList OutputInfo;
    public static EventHandler WorkFinished;
    public static PMWorkDelegate WorkStep;
    public static PMSetupDelegate WorkSetup;
    public static RenderMatching rm;
    public static TimeSpan PreprocessingTime;
    public static TimeSpan SearchTime;
    public static TimeSpan RenderingTime;
    public static volatile int WorkCurrent = 0;
    public static volatile int WorkTotal = 0;
    public static volatile bool WorkIsFinished = false;

    public static ArrayList DB => PMWorkClass.m_DB;

    public static void SetUp(ArrayList p_DB, Bitmap p_SourceImage, AlgorithmParameters p_Param)
    {
        PMWorkClass.m_DB = p_DB;
        PMWorkClass.SourceImage = BitmapConvert.ToBidimensionalArray(p_SourceImage);
        PMWorkClass.SourceImageSize = ((Image) p_SourceImage).Size;
        PMWorkClass.m_Param = p_Param;
    }

    public static void Release()
    {
        PMWorkClass.m_DB = (ArrayList) null;
        PMWorkClass.SourceImage = (Color[,]) null;
        PMWorkClass.m_Param = (AlgorithmParameters) null;
        PMWorkClass.OutputImage = (Bitmap) null;
    }

    public static void DoMatching() => PMWorkClass.DoMatching((Mosaic) null);

    public static void DoMatching(Mosaic p_mosaic)
    {
        DateTime now1 = DateTime.Now;
        PMWorkClass.WorkCurrent = 0;
        PMWorkClass.WorkIsFinished = false;
        ArrayList arrayList = (ArrayList) null;
        lock (PMWorkClass.SourceImage)
            arrayList = new BlockTransform(PMWorkClass.SourceImage, PMWorkClass.m_Param.SourceBlockSize).MakeBlocks(PMWorkClass.m_Param.rank);
        PMWorkClass.WorkTotal = 2 * arrayList.Count;
        DateTime now2 = DateTime.Now;
        PMWorkClass.PreprocessingTime = now2 - now1;
        Mosaic mosaic = p_mosaic == null ? (!PMWorkClass.m_Param.UseAssociativeMemory ? new Mosaic(PMWorkClass.m_DB) : (Mosaic) new MosaicAM(PMWorkClass.m_DB)) : p_mosaic;
        mosaic.MinTileDistance = PMWorkClass.m_Param.MinTileDistance;
        if (PMWorkClass.m_Param.db_images_usage == EDbImageUsage.BlockWithDistance)
            mosaic.AvoidUsingNearEqualTiles = true;
        ArrayList p_MatchingList = mosaic.MatchBlockList(arrayList);
        DateTime now3 = DateTime.Now;
        PMWorkClass.SearchTime = now3 - now2;
        PMWorkClass.rm = new RenderMatching(PMWorkClass.SourceImageSize, PMWorkClass.m_DB, p_MatchingList, arrayList, PMWorkClass.m_Param);
        PMWorkClass.OutputImage = PMWorkClass.rm.DoRender();
        PMWorkClass.OutputInfo = PMWorkClass.rm.InfoList;
        PMWorkClass.RenderingTime = DateTime.Now - now3;
        PMWorkClass.WorkIsFinished = true;
    }

    public static ImageFormat GetImageFormatFromExtension(string extension)
    {
        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return ImageFormat.Jpeg;
            case ".bmp":
                return ImageFormat.Bmp;
            case ".png":
                return ImageFormat.Png;
            case ".gif":
                return ImageFormat.Gif;
            case ".tif":
            case ".tiff":
                return ImageFormat.Tiff;
            default:
                return ImageFormat.Jpeg;
        }
    }

    public static string GetMimeTypeFromExtension(string extension)
    {
        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".bmp":
                return "image/bmp";
            case ".png":
                return "image/png";
            case ".gif":
                return "image/gif";
            case ".tif":
            case ".tiff":
                return "image/tiff";
            default:
                return "image/jpg";
        }
    }

    public static void SaveImage(Image img, string filename, int quality)
    {
        FileInfo fileInfo = new FileInfo(filename);
        EncoderParameters encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long) quality);
        string typeFromExtension = PMWorkClass.GetMimeTypeFromExtension(fileInfo.Extension);
        img.Save(filename, PMWorkClass.GetEncoderInfo(typeFromExtension), encoderParameters);
    }

    private static ImageCodecInfo GetEncoderInfo(string mimeType)
    {
        ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
        for (int index = 0; index < imageEncoders.Length; ++index)
        {
            if (imageEncoders[index].MimeType == mimeType)
                return imageEncoders[index];
        }
        return (ImageCodecInfo) null;
    }
}