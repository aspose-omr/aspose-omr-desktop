using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace AsposeOmrGui.Services;

/// <summary>Converts <see cref="System.Drawing.Bitmap"/> crops into frozen WPF image sources.</summary>
public static class ImageUtil
{
    public static BitmapSource? ToBitmapSource(Bitmap? bitmap)
    {
        if (bitmap is null)
            return null;

        try
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;   // fully load so the stream can be disposed
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();                                 // usable across threads
            return image;
        }
        catch
        {
            return null;
        }
    }
}
