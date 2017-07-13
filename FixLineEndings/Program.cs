using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FixLineEndings
{
    /// <summary>
    /// A program to modify all files in a git repo recursivley to fix line endings from linux to windows style
    /// </summary>
    class Program
    {
        private static HashSet<string> TextFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".ds",
            ".dc",
            ".sln",
            ".props",
            ".sources",
            ".targets",
            ".csproj",
            ".rptproj",
            ".vcxproj",
            ".proj",
            ".filters",
            ".cs",
            ".vb",
            ".h",
            ".cpp",
            ".resx",
            ".xaml",
            ".html",
            ".css",
            ".js",
            ".cmd",
            ".bat",
            ".ps1",
            ".txt",
            ".config",
            ".xml",
            ".xsd",
            ".ini",
            ".snippet",
            ".XML",
            ".vsixmanifest",
            ".ruleset",
            ".nuspec",
            ".StyleCop",
            ".DotSettings",
            ".rdl",
            ".rsd",
            ".def",
            ".gitattributes",
            ".gitignore",
            ".rds",
        };
        private static string repositoryRoot;

        static int Main(string[] args)
        {
            if (!GetAndValidateArguments(args))
            {
                return 1;
            }

            DetectBadFiles(repositoryRoot);
            //TryFixBadFiles(repositoryRoot);

            return 0;
        }

        private static bool GetAndValidateArguments(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: <repo directory>");
                return false;
            }

            // get and validate repo directory
            if (!ValidateDirectory(args[0]))
            {
                return false;
            }

            repositoryRoot = args[0].TrimEnd('\\');

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

        private static IEnumerable<string> FindFilesWithCrLf(string repositoryRoot)
        {
            return Directory.EnumerateFiles(repositoryRoot, "*.*", SearchOption.AllDirectories).Where(f => !f.Contains(@"\.git\") && TextFileExtensions.Contains(Path.GetExtension(f)));
        }

        private static void DetectBadFiles(string repositoryRoot)
        {
            foreach (var file in FindFilesWithCrLf(repositoryRoot))
            {
                if (!HasProperLineEndings(file))
                {
                    Console.Error.WriteLine("BAD FILE:" + file);
                }
            }
        }

        private static bool HasProperLineEndings(string filePath)
        {
            var text = File.ReadAllText(filePath);
            char previous = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                var current = text[i];
                if (current == '\n' && previous != '\r')
                {
                    return false;
                }
                previous = current;
            }
            return true;
        }

        private static void TryFixBadFiles(string repositoryRoot)
        {
            foreach (var file in FindFilesWithCrLf(repositoryRoot))
            {
                TryFixLineEndings(file);
            }
        }

        private static void TryFixLineEndings(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fs, true))
            {
                var text = reader.ReadToEnd();
                // non-performent but brainless implementation :)
                var result = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
                File.WriteAllText(file, result, reader.CurrentEncoding);
            }
        }
    }
}
