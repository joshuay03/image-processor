/*
 * Author: Joshua Young
 */

using System;
using System.IO;

namespace Project
{
    public static class Program
    {
        /// <summary>
        /// Gets the index of the given arguments
        /// </summary>
        /// <param name="args">The array of arguments</param>
        /// <param name="argument">The argument to be found</param>
        /// <returns>The argument index or -1 if not found</returns>
        public static int GetArgValue(string[] args, string argument)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == argument)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks if an option exists as an argument
        /// </summary>
        /// <param name="args">The array of arguments</param>
        /// <param name="option">The option to be found</param>
        /// <returns>A true or false on whether the argument exists</returns>
        public static bool CheckSingularArg(string[] args, string option)
        {
            bool exists = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == option)
                {
                    exists = true;
                }
            }

            return exists;
        }

        /// <summary>
        /// Gets the index of the given arguments
        /// </summary>
        /// <param name="exceptionMessage">The exception message</param>
        /// <returns>The exception directory</returns>
        public static string GetExceptionDirectory(string exceptionMessage)
        {
            string exceptionDirectory = exceptionMessage.Split("'")[1].Split("'")[0];

            return exceptionDirectory;
        }

        public static void Main(string[] args)
        {
            bool helpExists = CheckSingularArg(args, "-help");

            if (helpExists == true)
            {
                if (args.Length == 1)
                {
                    Console.WriteLine("\nUsage: Image_Pipeline [options] -pipe <path> -input <path> -output <path>");
                    Console.WriteLine("    Required parameters:");
                    Console.WriteLine("       -pipe<path>    : the path to the pipe txt file");
                    Console.WriteLine("       -input <path>  : the path to the input image or image directory");
                    Console.WriteLine("       -output <path> : the path to the output (file or directory)");
                    Console.WriteLine("                        must be a directory if -saveall is enabled or -input is a directory");
                    Console.WriteLine("    Options:");
                    Console.WriteLine("       -help          : display this help");
                    Console.WriteLine("       -verbose       : use this option to enable verbose logging");
                    Console.WriteLine("       -saveall       : use this option to save all intermediate images\n");
                    return;
                }

                Console.WriteLine("\nUsage: Image_Pipeline [options] -pipe <path> -input <path> -output <path>");
                Console.WriteLine("    Required parameters:");
                Console.WriteLine("       -pipe<path>    : the path to the pipe txt file");
                Console.WriteLine("       -input <path>  : the path to the input image or image directory");
                Console.WriteLine("       -output <path> : the path to the output (file or directory)");
                Console.WriteLine("                        must be a directory if -saveall is enabled or -input is a directory");
                Console.WriteLine("    Options:");
                Console.WriteLine("       -help          : display this help");
                Console.WriteLine("       -verbose       : use this option to enable verbose logging");
                Console.WriteLine("       -saveall       : use this option to save all intermediate images");
            }

            bool logging = false;
            string directory = null;
            int pipeIndex = GetArgValue(args, "-pipe");
            int inputIndex = GetArgValue(args, "-input");
            int outputIndex = GetArgValue(args, "-output");

            if (pipeIndex == -1 || inputIndex == -1 || outputIndex == -1)
            {
                Console.WriteLine("\nError!");

                if (pipeIndex == -1)
                {
                    Console.WriteLine("\n-pipe is missing!");
                }

                if (inputIndex == -1)
                {
                    Console.WriteLine("\n-input is missing!");
                }

                if (outputIndex == -1)
                {
                    Console.WriteLine("\n-output is missing!");
                }

                Console.WriteLine("\nUsage: Image_Pipeline [options] -pipe <path> -input <path> -output <path>");
                Console.WriteLine("       Use -help option for more detailed information\n");
                return;
            }

            
            bool verboseExists = CheckSingularArg(args, "-verbose");
            bool saveallExists = CheckSingularArg(args, "-saveall");

            if (verboseExists == true)
            {
                logging = true;
            }

            if (saveallExists == true)
            {
                directory = args[outputIndex];
            }

            Image inputImage;
            int imageNumber = 0;

            if (Directory.Exists(args[inputIndex]))
            {
                string[] files = Directory.GetFiles(args[inputIndex], "*.png");

                if (files.Length == 0)
                {
                    Console.WriteLine("Error! No images found in input directory!");
                    return;
                }

                foreach (string imagePath in files)
                {
                    imageNumber += 1;

                    inputImage = new Image(imagePath);
                    
                    Node endNode = PipelineLoader.LoadPipeline(args[pipeIndex]);

                    if (imageNumber == 1)
                    {
                        Console.WriteLine();
                    }

                    Console.WriteLine("Image number: " + imageNumber);

                    Image result = endNode.GetOutput(inputImage, logging, directory, imageNumber);

                    if (verboseExists == true)
                    {
                        Console.WriteLine("Output image size: " + result + "\n");
                    }

                    if (Directory.Exists(args[outputIndex]))
                    {
                        result.Write(endNode.NodesFolder + "/" + endNode.NumPreviousNodes() + " " + endNode);
                        result.Write(args[outputIndex] + "/" + "output " + imageNumber);
                    }
                    else
                    {
                        DirectoryInfo outputFolder = Directory.CreateDirectory(args[outputIndex]);

                        result.Write(endNode.NodesFolder + "/" + endNode.NumPreviousNodes() + " " + endNode);
                        result.Write(outputFolder.ToString() + imageNumber + ": " + "/output ");
                    }
                }
            }
            else
            {
                try
                {
                    inputImage = new Image(args[inputIndex]);
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("\nError! Input image does not exists!\n");
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine("\nError! Input image directory does not exists!\n");
                    return;
                }

                Node endNode = PipelineLoader.LoadPipeline(args[pipeIndex]);

                Image result = endNode.GetOutput(inputImage, logging, directory);

                if (verboseExists == true)
                {
                    Console.WriteLine("Output image size: " + result + "\n");
                }

                if (Directory.Exists(args[outputIndex]))
                {
                    result.Write(endNode.NodesFolder + "/" + endNode.NumPreviousNodes() + " " + endNode);
                    result.Write(args[outputIndex] + "/output");
                }
                else
                {
                    result.Write(endNode.NodesFolder + "/" + endNode.NumPreviousNodes() + " " + endNode);
                    result.Write(args[outputIndex]);
                }
            }
        }
    }
}