/*
 * Author: Joshua Young
 */

using System;
using System.IO;

namespace Project
{
    static class PipelineLoader
    {
        /// <summary>
        /// Finds and creates the node object in the given string
        /// </summary>
        /// <param name="nodeString">The string which contains a node and its parameters</param>
        /// <returns>The created node</returns>
        public static Node CreateNode(string nodeString)
        {
            Node node = null;

            if (nodeString.Contains("crop"))
            {
                int OriginX = int.Parse(nodeString.Split(" ")[1].Split("=")[1].Split("x")[0]);
                int originY = int.Parse(nodeString.Split(" ")[1].Split("=")[1].Split("x")[1]);
                int cropWidth = int.Parse(nodeString.Split(" ")[2].Split("=")[1].Split("x")[0]);
                int cropHeight = int.Parse(nodeString.Split(" ")[2].Split("=")[1].Split("x")[1]);

                node = new N_Crop(OriginX, originY, cropWidth, cropHeight);
            }
            else if (nodeString.Contains("greyscale"))
            {
                node = new N_Greyscale();
            }
            else if (nodeString.Contains("resize"))
            {
                float resizeWidthPercent = float.Parse(nodeString.Split(" ")[1].Split("=")[1].Split("x")[0]);
                float resizeHeightPercent = float.Parse(nodeString.Split(" ")[1].Split("=")[1].Split("x")[1]);

                node = new N_Resize(resizeWidthPercent, resizeHeightPercent);
            }
            else if (nodeString.Contains("normalise"))
            {
                string includeZero = nodeString.Split(" ")[1].Split("=")[1].Split("x")[0];

                node = new N_Normalise(includeZero);
            }
            else if (nodeString.Contains("convolve"))
            {
                string kernel = nodeString.Split(" ")[1].Split("=")[1];

                node = new N_Convolve(kernel);
            }

            return node;
        }

        /// <summary>
        /// Links all the created nodes with each other in sequence
        /// </summary>
        /// <param name="pipeFile">The text file with the nodes</param>
        /// <returns>The final node of the sequence</returns>
        public static Node LoadPipeline(string pipeFile)
        {
            string[] linesArr = null;

            try
            {
                linesArr = File.ReadAllLines(pipeFile);
            } catch (FileNotFoundException)
            {
                Console.WriteLine("\nError! Pipefile does not exists!\n");
                Environment.Exit(0); 
            } catch (DirectoryNotFoundException)
            {
                Console.WriteLine("\nError! Pipefile directory does not exists!\n");
                Environment.Exit(0);
            }

            Node previousNode = CreateNode(linesArr[0]);

            for (int i = 1; i <= linesArr.Length - 1; i++)
            {
                Node newNode = CreateNode(linesArr[i]);
                newNode.SetInput(previousNode);
                previousNode = newNode;
            }

            return previousNode;
        }
    }
}