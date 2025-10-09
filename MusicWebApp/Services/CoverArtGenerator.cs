using SkiaSharp;
using System.Security.Cryptography;
using System.Reflection;
using Bogus;

namespace MusicWebApp.Services;

public class CoverArtGenerator
{
    public byte[] Generate(string title, string artist, ulong seed)
    {
        int intSeed = seed.GetHashCode() ^ title.GetHashCode() ^ artist.GetHashCode();
        var rng = new Random(intSeed);

        int size = 256;
        float halfSize = size / 2.0f;
        float threeQuarterSize = 3.0f * size / 4.0f;
        var info = new SKImageInfo(size, size);
        using var surface = SKSurface.Create(info);
        SKCanvas canvas = surface.Canvas;

        canvas.Clear(SKColors.Black);

        var goodColors = new SKColor[]
        {
            SKColors.Red,
            SKColors.Green,
            SKColors.Blue,
            SKColors.Gold,
            SKColors.Crimson,
            SKColors.OrangeRed,
            SKColors.Tomato,
            SKColors.Coral,
            SKColors.Orange,
            SKColors.Gold,
            SKColors.Goldenrod,
            SKColors.Khaki,
            SKColors.YellowGreen,
            SKColors.LimeGreen,
            SKColors.MediumSeaGreen,
            SKColors.SeaGreen,
            SKColors.Teal,
            SKColors.DarkTurquoise,
            SKColors.Turquoise,
            SKColors.SteelBlue,
            SKColors.DodgerBlue,
            SKColors.RoyalBlue,
            SKColors.MediumSlateBlue,
            SKColors.MediumPurple,
            SKColors.Indigo,
            SKColors.MediumVioletRed,
            SKColors.HotPink,
            SKColors.PaleVioletRed,
            SKColors.Sienna,
            SKColors.Chocolate,
            SKColors.SlateGray
        };

        var c1 = goodColors[rng.Next(goodColors.Length)];
        var c2 = goodColors[rng.Next(goodColors.Length)];
        var c3 = goodColors[rng.Next(goodColors.Length)];
        var asd = new[] { c1, c2, c3, c1 };

        var sweep = SKShader.CreateSweepGradient(new SKPoint(halfSize, halfSize), asd, null);

        float freqX = 0.01f * (1.0f + (float)rng.NextDouble());
        float freqY = 0.01f * (1.0f + (float)rng.NextDouble());
        int octaves = 1 + rng.Next(3);

        var turbulence = SKShader.CreatePerlinNoiseTurbulence(freqX, freqY, octaves, intSeed);

        var shader = SKShader.CreateCompose(sweep, turbulence, SKBlendMode.SrcOver);

        var paint = new SKPaint
        {
            Shader = shader
        };
        canvas.DrawPaint(paint);

        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(0, 0, 0),
            Style = SKPaintStyle.Fill
        };
        using var textFont = new SKFont
        {
            Typeface = SKTypeface.FromFile("./wwwroot/fonts/Comic Sans MS.ttf"),
            Size = size,
        };

        var textPoint = new SKPoint(0.0f, size / 2);
        float actualLength = textFont.MeasureText(title);
        textFont.Size *= size / actualLength;
        canvas.DrawText(title, textPoint, textFont, textPaint);

        textPoint = new SKPoint(0.0f, 0.9f * size);
        string byArtist = "by " + artist;
        actualLength = textFont.MeasureText(byArtist);
        textFont.Size *= threeQuarterSize / actualLength;
        canvas.DrawText("by " + artist, textPoint, textFont, textPaint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);

        return data.ToArray();
    }
}
