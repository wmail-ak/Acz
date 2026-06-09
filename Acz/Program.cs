/*
 * Project: ZIP extraction utility - ACZ
 * License: Custom Open Source License Agreement
 *
 * Copyright (c) 2026 Arsenii and Contributors
 *
 * Permission is hereby granted to use this software freely for personal
 * and commercial purposes. Private modifications are allowed, provided
 * copyright notices remain intact. Public redistribution of modified
 * versions requires prior written approval from the authors.
 *
 * DISCLAIMER: This software is provided "as is" without warranty of any kind.
 * See the LICENSE file distributed with this source code for full terms.
 *
 * IMPORTANT NOTICE:
 * - Free use for self and commercial projects is permitted.
 * - Public modifications must be approved by the authors.
 * - Always review the LICENSE file before distribution.
 */

using Acz;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Acz
{
    class Program
    {
        static void Main(string[] args)
        {

            Assembly assembly = Assembly.GetExecutingAssembly();
            Version? version = assembly.GetName().Version;
            string? fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            string? infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            Console.WriteLine($"Hello, Acz v{version}");
            Console.WriteLine($"Useful Tool to Extract && Verify .zip archives.");

            Console.WriteLine($"File Version: {fileVersion}");
            Console.WriteLine($"Informational Version: {infoVersion}");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string[] filePaths = [];
            string rootFolder = string.Empty;

            for (ushort i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg == "-h" || arg == "--help" || arg == "/?")
                {
                    ShowHelp();
                    return;
                }
                else if (arg == "-fs")
                {
                    // Collect subsequent arguments until next option or end
                    List<string> paths = [];
                    for (ushort j = (ushort)(i + 1); j < args.Length && !args[j].StartsWith('-'); j++)
                    {
                        paths.Add(args[j].Trim('"', '\'', ' '));
                        i = j; // advance index
                    }
                    filePaths = [.. paths];
                }
                else if (arg == "-r")
                {
                    if (i + 1 < args.Length)
                    {
                        rootFolder = args[i + 1].Trim('"', '\'', ' ');
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing root folder after -r");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Unknown option '{arg}'");
                    ShowHelp();
                    return;
                }
            }

            if (filePaths.Length == 0)
            {
                Console.WriteLine("Error: No files provided with -fs");
                ShowHelp();
                return;
            }

            if (string.IsNullOrEmpty(rootFolder))
            {
                Console.Write("Enter root folder to extract archives: ");
                rootFolder = Console.ReadLine()?.Trim('"', '\'', ' ') ?? "";
                if (string.IsNullOrEmpty(rootFolder))
                {
                    Console.WriteLine("Error: Root folder not specified.");
                    return;
                }
            }

            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            ushort Errors = 0;

            foreach (string file in filePaths)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Warning: File not found '{file}'");
                    Errors++;
                    continue;
                }

                if (Path.GetExtension(file).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    string archiveName = Path.GetFileNameWithoutExtension(file);
                    string extractPath = Path.Combine(rootFolder, archiveName);

                    try
                    {
                        Console.WriteLine($"Begin for '{file}'");
                        Directory.CreateDirectory(extractPath);
                        ZipFile.ExtractToDirectory(file, extractPath, entryNameEncoding: System.Text.Encoding.UTF8, overwriteFiles: true);
                        Console.WriteLine($"Verify...");
                        ZipVerifier.VerifyZipCrc(file, extractPath);
                    }
                    catch (ExtractedFileNotFoundException ex)
                    {
                        Console.WriteLine($"FileNotFoundException extracting '{file}', {ex.Message}");
                        Errors++;
                    }
                    catch (ExtractedFolderNotFoundException ex)
                    {
                        Console.WriteLine($"FolderNotFoundException extracting '{file}', {ex.Message}");
                        Errors++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error extracting '{file}', {ex.GetType().Name} - {ex.Message}");
                        Errors++;
                    }
                    Console.WriteLine($"Completed '{file}' to '{extractPath}'");
                }
                else
                {
                    Console.WriteLine($"Skipping non-zip file '{file}'");
                    Errors++;
                }
            }
            Console.WriteLine($"Extract && Verify finished.");
            Console.WriteLine($"Errors: {Errors}");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  program.exe -fs <files...> -r <rootFolder>");
            Console.WriteLine("Options:");
            Console.WriteLine("  -fs <files...>   List of .zip files (quoted paths allowed, separated by spaces)");
            Console.WriteLine("  -r <folder>      Root folder to extract archives into");
            Console.WriteLine("  -h, --help, /?   Show this help message");
        }
    }
}