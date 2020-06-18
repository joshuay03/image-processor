/*
 * Author: Joshua Young
 */

using System;
using System.IO;

namespace Project
{
    /// <summary>
    /// An image node
    /// </summary>
    class Node
    {
        public Node PreviousNode { get; set; }
        public Image NewImage { get; set; }
        public DirectoryInfo NodesFolder;

        /// <summary>
        /// Sets the previous node as the input for the current node
        /// </summary>
        /// <param name="previousNode">The previous node</param>
        public void SetInput(Node previousNode)
        {
            PreviousNode = previousNode;
        }

        /// <summary>
        /// Checks if a previous node exists and gets its output or else returns the same image
        /// </summary>
        /// <param name="image">The input image for the previous node</param>
        /// <param name="logging">The true or false value of the -verbose option</param>
        /// <param name="directory">The location where each node's output should be saved for the -saveall option</param>
        /// <param name="imageNumber">The current image number if multiple images are being processed</param>
        /// <returns>Output image of the previous node or the same image</returns>
        public virtual Image GetOutput(Image image, bool logging, string directory, int imageNumber = 0)
        {
            if (PreviousNode != null)
            {
                NewImage = PreviousNode.GetOutput(image, logging, directory, imageNumber);

                if (directory != null)
                {
                    SaveNodeImage(NewImage, directory, imageNumber);
                }

                if (logging == true)
                {
                    LogNodeImage();
                }
            }
            else
            {
                NewImage = image;

                if (logging == true)
                {
                    LogNodeImage();
                }
            }

            return NewImage;
        }

        /// <summary>
        /// Writes the output image of the node
        /// </summary>
        /// <param name="image">The output image of the node</param>
        /// <param name="directory">The location where the image should be saved</param>
        /// <param name="imageNumber">The current image number if multiple images are being processed</param>
        public void SaveNodeImage(Image image, string directory, int imageNumber)
        {
            if (imageNumber > 0)
            {
                NodesFolder = Directory.CreateDirectory(directory + "/" + "output " + imageNumber + " nodes");
                image.Write(NodesFolder + "/" + (NumPreviousNodes() - 1) + " " + PreviousNode);
            }
            else
            {
                if (Directory.Exists(directory))
                {
                    NodesFolder = Directory.CreateDirectory(directory + "/output nodes");
                    image.Write(NodesFolder + "/" + (NumPreviousNodes() - 1) + " " + PreviousNode);

                }
                else
                {
                    NodesFolder = Directory.CreateDirectory(directory + " nodes");
                    image.Write(NodesFolder + "/" + (NumPreviousNodes() - 1) + " " + PreviousNode);
                }
            }
        }

        /// <summary>
        /// Writes the name, inout image size and output image size of the node to the console
        /// </summary>
        public void LogNodeImage()
        {
            if (PreviousNode != null)
            {
                Console.WriteLine("Output image size: " + NewImage);
                Console.WriteLine("\n" + NumPreviousNodes() + ": " + ToString());
                Console.WriteLine("Input image size: " + NewImage);
            }
            else
            {
                Console.WriteLine("\n" + NumPreviousNodes() + ": " + ToString());
                Console.WriteLine("Input image size: " + NewImage);
            }
        }

        /// <summary>
        /// Gets the number of the node in the pipeline sequence
        /// </summary>
        public int NumPreviousNodes()
        {
            if (PreviousNode == null)
            {
                return 1;
            }

            return 1 + PreviousNode.NumPreviousNodes();
        }
    }
}