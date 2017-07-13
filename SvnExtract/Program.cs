using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SvnExtract
{
    /// <summary>
    /// A program to extract out all files recursivley from an svn repository
    /// and copy them to a destination folder without the .svn cruft.
    /// <remarks>Does not copy blank directories.</remarks>
    /// </summary>
    class Program
    {
        private static string sourceDirectory;
        private static string destinationDirectory;
        private static Dictionary<string, string> pathMap = new Dictionary<string, string>();

        static int Main(string[] args)
        {
            if (!GetAndValidateArguments(args))
            {
                return 1;
            }

            PopulatePathMap(sourceDirectory);
            CopyFiles(pathMap);

            return 0;
        }

        private static void CopyFiles(Dictionary<string, string> pathMap)
        {
            foreach(var pair in pathMap)
            {
                Console.WriteLine("Copying {0} -> {1}", pair.Key, pair.Value);

                FileInfo destinationFile = new FileInfo(pair.Value);

                if (!Directory.Exists(destinationFile.DirectoryName))
                {
                    Directory.CreateDirectory(destinationFile.DirectoryName);
                }

                File.Copy(pair.Key, pair.Value, true);
            }

            Console.WriteLine();
            Console.WriteLine("Successfully copied {0} files", pathMap.Count);
        }

        // Recursive call to add files in each directory
        private static void PopulatePathMap(string SourceDirectory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(SourceDirectory);

            // base case: add files in current directory
            List<string> result = (from file in dirInfo.GetFiles() select file.FullName).ToList();

            foreach (var sourceFile in result)
            {
                var destinationFile = sourceFile.Substring(sourceDirectory.Length);
                destinationFile = Path.Combine(destinationDirectory, destinationFile);
                pathMap.Add(sourceFile, destinationFile);
            }

            // recursive case: add files in each directory under current directory
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                // ignore .svn directories and everything in them
                if (!dir.Name.Equals(".svn", StringComparison.CurrentCultureIgnoreCase))
                {
                    PopulatePathMap(dir.FullName);
                }
            }
        }

        private static bool GetAndValidateArguments(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: <source directory> <destination directory>");
                return false;
            }

            // get and validate source directory
            if (!ValidateDirectory(args[0]) || !ValidateDirectory(args[1]))
            {
                return false;
            }

            sourceDirectory = args[0];
            destinationDirectory = args[1];

            if (!sourceDirectory.EndsWith("\\"))
            {
                sourceDirectory += "\\";
            }

            if (!destinationDirectory.EndsWith("\\"))
            {
                destinationDirectory += "\\";
            }

            return true;
        }

        private static bool ValidateDirectory(string Directory)
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                Console.WriteLine("Directory {0} does not exist", Directory);
                return false;
            }

            return true;
        }
    }
}
