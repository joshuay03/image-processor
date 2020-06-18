/*
 * Author: Joshua Young
 */

using System;
using SixLabors.ImageSharp.PixelFormats;

namespace Project
{
    /// <summary>
    /// A greyscale image node
    /// </summary>
    class N_Greyscale : Node
    {
        /// <summary>
        /// Converts image to greyscale
        /// </summary>
        /// <param name="image">The input image</param>
        /// <param name="logging">The true or false value of the -verbose option</param>
        /// <param name="directory">The location where each node's output should be saved for the -saveall option</param>
        /// <param name="imageNumber">The current image number if multiple images are being processed</param>
        /// <returns>Greyscaled image</returns>
        public override Image GetOutput(Image image, bool logging, string directory, int imageNumber)
        {
            Image PrevImage = base.GetOutput(image, logging, directory, imageNumber);

            NewImage = Image.ToGrayscale(PrevImage);

            return NewImage;
        }

        public override string ToString()
        {
            return "Greyscaling image";
        }
    }

    /// <summary>
    /// A crop image node
    /// </summary>
    class N_Crop : Node
    {
        private int OriginX { get; }
        private int OriginY { get; }
        private int CropWidth { get; }
        private int CropHeight { get; }

        public N_Crop(int originX, int originY, int cropWidth, int cropHeight)
        {
            OriginX = originX;
            OriginY = originY;
            CropWidth = cropWidth;
            CropHeight = cropHeight;
        }

        /// <summary>
        /// Crops image
        /// </summary>
        /// <param name="image">The input image</param>
        /// <param name="logging">The true or false value of the -verbose option</param>
        /// <param name="directory">The location where each node's output should be saved for the -saveall option</param>
        /// <param name="imageNumber">The current image number if multiple images are being processed</param>
        /// <returns>Cropped image</returns>
        public override Image GetOutput(Image image, bool logging, string directory, int imageNumber)
        {
            Image PrevImage = base.GetOutput(image, logging, directory, imageNumber);

            NewImage = new Image(CropWidth, CropHeight);

            try
            {
                for (int i = OriginX; i < OriginX + CropWidth; i++)
                {
                    for (int j = OriginY; j < OriginY + CropHeight; j++)
                    {
                        NewImage[i - OriginX, j - OriginY] = PrevImage[i, j];
                    }
                }

            } catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Error! Image dimensions are incompatible with the given crop parameters\n");
                Environment.Exit(0);
            }

            return NewImage;
        }

        public override string ToString()
        {
            return "Cropping image";
        }
    }

    /// <summary>
    /// A resize image node
    /// </summary>
    class N_Resize : Node
    {
        private float ResizeWidthPercent { get; }
        private float ResizeHeightPercent { get; }

        public N_Resize(float resizeWidthPercent, float resizeHeightPercent)
        {
            ResizeWidthPercent = resizeWidthPercent;
            ResizeHeightPercent = resizeHeightPercent;
        }

        /// <summary>
        /// Resizes image
        /// </summary>
        /// <param name="image">The input image</param>
        /// <param name="logging">The true or false value of the -verbose option</param>
        /// <param name="directory">The location where each node's output should be saved for the -saveall option</param>
        /// <param name="imageNumber">The current image number if multiple images are being processed</param>
        /// <returns>Resized image</returns>
        public override Image GetOutput(Image image, bool logging, string directory, int imageNumber)
        {
            Image PrevImage = base.GetOutput(image, logging, directory, imageNumber);

            int resizeWidth = (int)(ResizeWidthPercent * PrevImage.Width);
            int resizeHeight = (int)(ResizeHeightPercent * PrevImage.Height);

            NewImage = Image.Resize(PrevImage, resizeWidth, resizeHeight);

            return NewImage;
        }

        public override string ToString()
        {
            return "Resizing image";
        }
    }

    /// <summary>
    /// A normalise image node
    /// </summary>
    class N_Normalise : Node
    {
        string IncludeZero { get; set; }

        public N_Normalise(string includeZero)
        {
            IncludeZero = includeZero;
        }

        /// <summary>
        /// Normalises image
        /// </summary>
        /// <param name="image">The input image</param>
        /// <param name="logging">The true or false value of the -verbose option</param>
        /// <param name="directory">The location where each node's output should be saved for the -saveall option</param>
        /// <param name="imageNumber">The current image number if multiple images are being processed</param>
        /// <returns>Normalised image</returns>
        public override Image GetOutput(Image image, bool logging, string directory, int imageNumber)
        {
            Image PrevImage = base.GetOutput(image, logging, directory, imageNumber);

            Image GrayImage = Image.ToGrayscale(PrevImage);

            float OldMin = 256;
            float OldMax = -1;

            for (int i = 0; i < GrayImage.Width; i++)
            {
                for (int j = 0; j < GrayImage.Height; j++)
                {
                    if (IncludeZero == "no")
                    {
                        if (GrayImage.GetGreyIntensity(i, j) < OldMin && GrayImage.GetGreyIntensity(i, j) > 0)
                        {
                            OldMin = GrayImage.GetGreyIntensity(i, j);
                        }
                        if (GrayImage.GetGreyIntensity(i, j) > OldMax && GrayImage.GetGreyIntensity(i, j) > 0)
                        {
                            OldMax = GrayImage.GetGreyIntensity(i, j);
                        }
                    }
                    else
                    {
                        if (GrayImage.GetGreyIntensity(i, j) < OldMin)
                        {
                            OldMin = GrayImage.GetGreyIntensity(i, j);
                        }
                        if (GrayImage.GetGreyIntensity(i, j) > OldMax)
                        {
                            OldMax = GrayImage.GetGreyIntensity(i, j);
                        }
                    }
                }
            }

            float newMin;
            float newMax;

            newMin = 0;
            newMax = 255;

            float numOldRed;
            float numOldGreen;
            float numOldBlue;
            Rgba32 pixel = new Rgba32
            {
                A = 255
            };

            for (int i = 0; i < PrevImage.Width; i++)
            {
                for (int j = 0; j < PrevImage.Height; j++)
                {
                    numOldRed = PrevImage[i, j].R;
                    pixel.R = (byte)(newMin + ((numOldRed - OldMin) / (OldMax - OldMin) * (newMax - newMin)));

                    numOldGreen = PrevImage[i, j].G;
                    pixel.G = (byte)(newMin + ((numOldGreen - OldMin) / (OldMax - OldMin) * (newMax - newMin)));

                    numOldBlue = PrevImage[i, j].B;
                    pixel.B = (byte)(newMin + ((numOldBlue - OldMin) / (OldMax - OldMin) * (newMax - newMin)));

                    NewImage[i, j] = pixel;
                }
            }

            return NewImage;
        }

        public override string ToString()
        {
            return "Normalising image";
        }
    }

    /// <summary>
    /// A convoluted image node
    /// </summary>
    class N_Convolve : Node
    {
        private readonly double[,] EdgeDetection = { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
        private readonly double[,] Sharpen = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        private readonly double[,] Blur = { { 1, 4, 6, 4, 1 }, { 4, 16, 24, 16, 4 }, { 6, 24, 36, 24, 6 }, { 4, 16, 24, 16, 4 }, { 1, 4, 6, 4, 1 } };
        private readonly double[,] Kernel;

        public N_Convolve(string kernel)
        {
            switch (kernel)
            {
                case "edge":
                    Kernel = EdgeDetection;
                    break;
                case "sharpen":
                    Kernel = Sharpen;
                    break;
                case "blur":
                    double BlurMultiplier = 0.00390625;
                    for (int i = 0; i < Blur.GetLength(0); i++)
                    {
                        for (int j = 0; j < Blur.GetLength(1); j++)
                        {
                            Blur[i, j] = BlurMultiplier * Blur[i, j];
                        }
                    }
                    Kernel = Blur;
                    break;
            }
        }

        /// <summary>
        /// Convolutes image
        /// </summary>
        /// <param name="image">The input image</param>
        /// <param name="logging">The true or false value of the -verbose option</param>
        /// <param name="directory">The location where each node's output should be saved for the -saveall option</param>
        /// <param name="imageNumber">The current image number if multiple images are being processed</param>
        /// <returns>Convoluted image</returns>
        public override Image GetOutput(Image image, bool logging, string directory, int imageNumber)
        {
            Image PrevImage = base.GetOutput(image, logging, directory, imageNumber);

            int imageWidth = PrevImage.Width;
            int imageHeight = PrevImage.Height;

            if (Kernel.GetLength(0) > PrevImage.Width)
            {
                imageWidth = Kernel.GetLength(0);
            }

            if (Kernel.GetLength(1) > PrevImage.Height)
            {
                imageHeight = Kernel.GetLength(1);
            }

            NewImage = new Image(imageWidth, imageHeight);

            for (int i = Kernel.GetLength(0) / 2; i < PrevImage.Width - Kernel.GetLength(0) / 2; i++)
            {
                for (int j = Kernel.GetLength(1) / 2; j < PrevImage.Height - Kernel.GetLength(1) / 2; j++)
                {
                    double red = 0;
                    double green = 0;
                    double blue = 0;
                    Rgba32 pixel = new Rgba32
                    {
                        A = 255
                    };

                    for (int k = -Kernel.GetLength(0) / 2; k <= Kernel.GetLength(0) / 2; k++)
                    {
                        for (int l = -Kernel.GetLength(1) / 2; l <= Kernel.GetLength(1) / 2; l++)
                        {
                            red += Kernel[k + Kernel.GetLength(0) / 2, l + Kernel.GetLength(1) / 2] * PrevImage[i + k, j + l].R;
                            green += Kernel[k + Kernel.GetLength(0) / 2, l + Kernel.GetLength(1) / 2] * PrevImage[i + k, j + l].G;
                            blue += Kernel[k + Kernel.GetLength(0) / 2, l + Kernel.GetLength(1) / 2] * PrevImage[i + k, j + l].B;
                        }
                    }

                    pixel.R = (byte)red;
                    pixel.G = (byte)green;
                    pixel.B = (byte)blue;

                    NewImage[i, j] = pixel;
                }
            }

            return NewImage;
        }

        public override string ToString()
        {
            return "Convoluting image";
        }
    }
}