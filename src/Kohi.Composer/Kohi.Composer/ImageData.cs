// Copyright (c) Kohi Art Community, Inc.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Kohi.Composer;

public static class ImageData
{
    public static void Save(string filename, Graphics2D graphics)
    {
        using var fs = new FileStream(filename, FileMode.OpenOrCreate);
        Save(fs, Path.GetExtension(filename), graphics);
    }

    public static string SaveAsUri(Graphics2D graphics)
    {
        using var ms = new MemoryStream();
        Save(ms, ".png", graphics);
        return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
    }

    public static void Save(Stream stream, string extension, Graphics2D graphics)
    {
        var image = ImageBufferToImage32(graphics);
        IImageFormat imageFormat;
        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                imageFormat = JpegFormat.Instance;
                break;
            case ".png":
                imageFormat = PngFormat.Instance;
                break;
            case ".gif":
                imageFormat = GifFormat.Instance;
                break;
            default:
                throw new NotImplementedException();
        }

        image.Save(stream, imageFormat);
    }

    private static Image<Rgba32> ImageBufferToImage32(Graphics2D g)
    {
        var source = g.Buffer;
        var invertedBuffer = new byte[source.Length];
        var index = 0;
        for (var y = g.Height - 1; y >= 0; y--)
        {
            var line = Graphics2D.GetBufferOffsetY(g, y);
            for (var x = 0; x < g.Width; x++)
            {
                var pix = x * 4;
                invertedBuffer[index++] = source[line + pix + 2];
                invertedBuffer[index++] = source[line + pix + 1];
                invertedBuffer[index++] = source[line + pix + 0];
                invertedBuffer[index++] = source[line + pix + 3];
            }
        }

        var image2 = Image.LoadPixelData<Rgba32>(invertedBuffer,
            g.Width,
            g.Height);
        return image2;
    }

    public static bool Load(string fileName, Graphics2D destImage)
    {
        if (File.Exists(fileName))
        {
            var temp = Image.Load<Rgba32>(fileName);
            return ConvertImageToImageBuffer(destImage, temp);
        }

        throw new Exception($"Image file not found: {fileName}");
    }

    private static bool ConvertImageToImageBuffer(Graphics2D g, Image<Rgba32> image)
    {
        if (image.DangerousTryGetSinglePixelMemory(out var pixelSpan))
        {
            var pixelArray = pixelSpan.ToArray();

            return ConvertImageToImageBuffer(g, pixelArray);
        }

        return false;
    }

    public static bool ConvertImageToImageBuffer(Graphics2D g, Rgba32[] pixelArray)
    {
        var sourceIndex = 0;
        var destBuffer = g.Buffer;
        for (var y = 0; y < g.Height; y++)
        {
            var destIndex = Graphics2D.GetBufferOffsetXy(g, 0, g.Height - 1 - y);
            for (var x = 0; x < g.Width; x++)
            {
                destBuffer[destIndex++] = pixelArray[sourceIndex].B;
                destBuffer[destIndex++] = pixelArray[sourceIndex].G;
                destBuffer[destIndex++] = pixelArray[sourceIndex].R;
                destBuffer[destIndex++] = pixelArray[sourceIndex].A;
                sourceIndex++;
            }
        }

        return true;
    }
}