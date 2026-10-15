using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        using (var bmp = new Bitmap(@"D:\Lomba Hackaton\My project (2)\Assets\KawanDengarEdu\Sprites\Textures\07_reward_card_3d.png"))
        {
            Console.WriteLine($"Size: {bmp.Width} x {bmp.Height}");
            // Find bounding box of purple button (around purple color #8A2BE2 / #7C3AED / etc.)
            int minX = bmp.Width, maxX = 0, minY = bmp.Height, maxY = 0;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    // purple button has R around 120-160, G around 40-80, B around 200-250
                    if (c.R > 100 && c.R < 180 && c.G < 100 && c.B > 180)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }
            Console.WriteLine($"Purple Button Pixels: x={minX}..{maxX}, y={minY}..{maxY}");
            float svgX = minX * 412f / bmp.Width;
            float svgY = minY * 850f / bmp.Height;
            float svgW = (maxX - minX) * 412f / bmp.Width;
            float svgH = (maxY - minY) * 850f / bmp.Height;
            Console.WriteLine($"Purple Button SVG 412x850: x={svgX:F1}, y={svgY:F1}, w={svgW:F1}, h={svgH:F1}");

            // Also check dark card bounding box (dark blue/black R<40, G<40, B<50)
            int cMinX = bmp.Width, cMaxX = 0, cMinY = bmp.Height, cMaxY = 0;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    if (y > 200 && y < 1600 && c.R < 40 && c.G < 40 && c.B > 20 && c.B < 60)
                    {
                        if (x < cMinX) cMinX = x;
                        if (x > cMaxX) cMaxX = x;
                        if (y < cMinY) cMinY = y;
                        if (y > cMaxY) cMaxY = y;
                    }
                }
            }
            float cSvgX = cMinX * 412f / bmp.Width;
            float cSvgY = cMinY * 850f / bmp.Height;
            float cSvgW = (cMaxX - cMinX) * 412f / bmp.Width;
            float cSvgH = (cMaxY - cMinY) * 850f / bmp.Height;
            Console.WriteLine($"Dark Card SVG 412x850: x={cSvgX:F1}, y={cSvgY:F1}, w={cSvgW:F1}, h={cSvgH:F1}");
        }
    }
}
