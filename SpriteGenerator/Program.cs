using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Drawing.Imaging;

namespace SpriteGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = @"C:\temp\Sprite";
            string cssStyles = "";
            Bitmap spriteImage = null;
            List<ColorMap> colorMapList = null;
            List<Size> sizeList = null;

            colorMapList = new List<ColorMap>();

            colorMapList.Add(new ColorMap()
            {
                OldColor = Color.Black,
                NewColor = Color.Black
            });

            colorMapList.Add(new ColorMap()
            {
                OldColor = Color.Black,
                NewColor = Color.Red
            });

            colorMapList.Add(new ColorMap()
            {
                OldColor = Color.Black,
                NewColor = Color.Green
            });

            colorMapList.Add(new ColorMap()
            {
                OldColor = Color.Black,
                NewColor = Color.Blue
            });

            sizeList = new List<Size>();

            sizeList.Add(new Size(50, 50));
            sizeList.Add(new Size(30, 30));
            sizeList.Add(new Size(15, 15));

            foreach(Size size in sizeList)
            {
                spriteImage = DrawSpriteColors(folder, colorMapList, out cssStyles, size);

                if (spriteImage != null)
                {
                    spriteImage.Save(@"C:\temp\Sprites" + size.Width + ".png", ImageFormat.Png);

                    File.WriteAllText(@"C:\temp\ProviderBase.Sprites" + size.Width + ".less", cssStyles);
                }
            }
        }

        private static Bitmap DrawSpriteColors(string folder, List<ColorMap> colorMapList, out string cssStyles)
        {
            return DrawSpriteColors(folder, colorMapList, out cssStyles, new Size());
        }

        private static Bitmap DrawSpriteColors(string folder, List<ColorMap> colorMapList, out string cssStyles, Size iconSizeScale)
        {
            Bitmap spriteImage = null;
            cssStyles = "";
            
            
            if (Directory.Exists(folder))
            {
                string[] fileList = null;

                fileList = Directory.GetFiles(folder);

                if (fileList != null && fileList.Count() > 0)
                {
                    Bitmap iconImage = null;
                    Size iconSize = new Size();
                    Point spritePoint = new Point();
                    string fileNameFirst = fileList[0];

                    iconImage = new Bitmap(fileNameFirst);

                    iconSize = (iconSizeScale.Width == 0) ? iconImage.Size : iconSizeScale;
                    spriteImage = new Bitmap(((iconSize.Width * colorMapList.Count()) + (colorMapList.Count() - 1)), (iconSize.Height * fileList.Count()));
                    spritePoint = new Point(0, 0);

                    cssStyles += ".sprite() {";
                    cssStyles += Environment.NewLine;
                    cssStyles += "\t" + "float: left;";
                    cssStyles += Environment.NewLine;
                    cssStyles += "\t" + $"width: {iconSize.Width}px;";
                    cssStyles += Environment.NewLine;
                    cssStyles += "\t" + $"height: {iconSize.Height}px;";
                    cssStyles += Environment.NewLine;
                    cssStyles += "\t" + "background: url(/Resource/Images/ProviderBase/Sprites50.png) no-repeat;";
                    cssStyles += Environment.NewLine;
                    cssStyles += "}";
                    cssStyles += Environment.NewLine;

                    cssStyles += Environment.NewLine;
                    cssStyles += ".spriteHover() {";
                    cssStyles += Environment.NewLine;
                    cssStyles += "\t" + "cursor: pointer;";
                    cssStyles += Environment.NewLine;
                    cssStyles += "}";
                    cssStyles += Environment.NewLine;

                    foreach (ColorMap colorMap in colorMapList)
                    {
                        string colourPrefix = "";

                        switch (colorMap.NewColor.Name.ToLower())
                        {
                            case "black":
                                colourPrefix = "d";
                                break;

                            case "red":
                                colourPrefix = "red";
                                break;

                            case "green":
                                colourPrefix = "g";
                                break;

                            case "blue":
                                colourPrefix = "b";
                                break;
                        }

                        foreach (string fileName in fileList)
                        {
                            spriteImage = DrawSprite(fileName, spriteImage, ref iconSize, ref spritePoint, colorMap);

                            cssStyles += Environment.NewLine;
                            cssStyles += $".icon-{Path.GetFileNameWithoutExtension(fileName)}-{colourPrefix}-{iconSize.Width} ";
                            cssStyles += "{";
                            cssStyles += Environment.NewLine;
                            cssStyles += "\t" + ".sprite();";
                            cssStyles += Environment.NewLine;
                            cssStyles += "\t" + $"background-position: {spritePoint.X} {spritePoint.Y};";
                            cssStyles += Environment.NewLine;
                            cssStyles += "}";
                            cssStyles += Environment.NewLine;

                            cssStyles += Environment.NewLine;
                            cssStyles += $".icon-{Path.GetFileNameWithoutExtension(fileName)}-{colourPrefix}-50:hover ";
                            cssStyles += "{";
                            cssStyles += Environment.NewLine;
                            cssStyles += "\t" + ".spriteHover();";
                            cssStyles += Environment.NewLine;
                            cssStyles += "}";
                            cssStyles += Environment.NewLine;
                        }

                        spritePoint = new Point(spritePoint.X + iconSize.Width + 1, 0);
                    }
                }
            }

            return spriteImage;
        }

        private static Bitmap DrawSpriteColumn(string folder, Bitmap spriteImage, ref Size iconSize, ref Point spritePoint)
        {
            return DrawSpriteColumn(folder, spriteImage, ref iconSize, ref spritePoint, null);
        }

        private static Bitmap DrawSpriteColumn(string folder, Bitmap spriteImage, ref Size iconSize, ref Point spritePoint, ColorMap colorMap)
        {
            if (Directory.Exists(folder))
            {
                string[] fileList = null;

                fileList = Directory.GetFiles(folder);

                if (fileList != null && fileList.Count() > 0)
                {
                    foreach (string fileName in fileList)
                    {
                        iconSize = (iconSize == null) ? new Bitmap(fileName).Size : iconSize;
                        spriteImage =  (spriteImage == null) ? new Bitmap((iconSize.Width), (iconSize.Height * fileList.Count())) : spriteImage;
                        spritePoint = (spritePoint == null) ? new Point(0, 0) : spritePoint;

                        spriteImage = DrawSprite(fileName, spriteImage, ref iconSize, ref spritePoint, colorMap);
                    }
                }
            }

            return spriteImage;
        }

        private static Bitmap DrawSprite(string fileName, Bitmap spriteImage, ref Size iconSize, ref Point spritePoint, ColorMap colorMap)
        {
            Graphics spriteGraphic = null;
            Bitmap iconImage = null;
            Rectangle rectangle = new Rectangle();

            iconImage = new Bitmap(fileName);

            spriteGraphic = Graphics.FromImage(spriteImage);

            rectangle = new Rectangle(spritePoint, iconSize);

            if (colorMap != null)
            {
                ImageAttributes imageAttribute = null;
                ColorMap[] colorMapAttribute = null;

                colorMapAttribute = new ColorMap[1];
                colorMapAttribute[0] = colorMap;

                colorMapAttribute = AddAllBlack(colorMapAttribute, colorMap);

                imageAttribute = new ImageAttributes();
                imageAttribute.SetRemapTable(colorMapAttribute);

                spriteGraphic.DrawImage(iconImage, rectangle, 0, 0, iconImage.Width, iconImage.Height, GraphicsUnit.Pixel, imageAttribute);
            }
            else
            {
                spriteGraphic.DrawImage(iconImage, rectangle);
            }

            spritePoint = new Point(spritePoint.X, (spritePoint.Y + iconSize.Height + 1));

            return spriteImage;
        }

        private static ColorMap[] AddColor(ColorMap[] colorMapAttribute, ColorMap colorMap, Color oldColor)
        {
            ColorMap[] colorMapAttributeTemp = null;
            ColorMap colorMapTemp = null;
            int i = 0;

            colorMapAttributeTemp = new ColorMap[colorMapAttribute.Count() + 1];

            for (i = 0; i < colorMapAttribute.Count(); i++)
            {
                colorMapAttributeTemp[i] = colorMapAttribute[i];
            }

            colorMapTemp = new ColorMap();
            colorMapTemp.OldColor = oldColor;
            colorMapTemp.NewColor = colorMap.NewColor;
            colorMapAttributeTemp[i] = colorMapTemp;

            return colorMapAttributeTemp;
        }

        private static ColorMap[] AddAllBlack(ColorMap[] colorMapAttribute, ColorMap colorMap)
        {
            for(int r = 0; r < 10; r++)
            {
                for (int g = 0; g < 10; g++)
                {
                    for (int b = 0; b < 10; b++)
                    {
                        colorMapAttribute = AddColor(colorMapAttribute, colorMap, Color.FromArgb(r, g, b));
                    }
                }
            }

            return colorMapAttribute;
        }
    }
}
