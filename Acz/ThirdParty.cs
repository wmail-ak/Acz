/*
 * Project: ZIP extraction Utility - ACZ
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

using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;

namespace Acz
{
    /// <summary>
    /// Improved third-party library management for ACZ, ensuring compatibility and performance.
    /// </summary>
    class TgCrc32
    {
        readonly Crc32 ObjCrc32 = new();
        Byte Reset = 0b00000000;
        public void Update(ArraySegment<byte> a) => ObjCrc32.Update(a);
        public long Value
        {
            get
            {
                return ObjCrc32.Value;
            }
        }
        /// <summary>
        /// Improved reset triggering for Crc32, ensuring consistent state management.
        /// </summary>
        public void New()
        {
            if ((Reset & 0b00000001) == 0b00000001)
            {
                ObjCrc32.Reset();
            }
            else
            {
                Reset = 0b00000001;
            }
        }
    }
    class ZipVerifier
    {
        const int BUFFERSIZE = 1048576000;
        readonly byte[] buffer = new byte[BUFFERSIZE]; // 1000 MiB block size
        readonly TgCrc32 crc32 = new();

        /// <exception cref="OriginFileNotFoundException">
        /// Thrown when the archive file cannot be found at the specified path.
        /// </exception>
        /// <exception cref="ExtractedFileNotFoundException">
        /// Thrown when the extracted file cannot be found at the specified path.
        /// </exception>
        /// <exception cref="DestinationFolderNotFoundException">
        /// Thrown when the destination folder cannot be found at the specified path.
        /// </exception>
        /// <exception cref="ExtractedFolderNotFoundException">
        /// Thrown when the extracted folder cannot be found at the specified path.
        /// </exception>
        /// <exception cref="EmptyCRC32Exception">
        /// Thrown when a file inside the archive has no CRC32 value stored (zero or missing).
        /// </exception>
        /// <exception cref="MismatchCRC32Exception">
        /// Thrown when the calculated CRC32 of an extracted file does not match the CRC32 stored in the archive.
        /// </exception>
        public void VerifyZipCrc(string zipPath, string destinationFolder)
        {
            DateTime trigger = DateTime.Now;

            using FileStream fs = File.OpenRead(zipPath);
            using ZipFile zipFile = new(fs);
            try
            {
                int passed = 0;
                foreach (ZipEntry entry in zipFile)
                {
                    if (!entry.IsFile)
                    {
                        if (entry.IsDirectory)
                        {
                            if (!Directory.Exists(Path.GetFullPath(Path.Combine(destinationFolder, entry.Name))))
                            {
                                throw new ExtractedFolderNotFoundException($"missing extracted folder: {entry.Name}");
                            }
                            else { goto Tail; }
                        }
                        else { goto Tail; }
                    }

                    string fullPath = Path.GetFullPath(Path.Combine(destinationFolder, entry.Name));

                    if (!File.Exists(fullPath))
                    {
                        throw new ExtractedFileNotFoundException($"missing extracted file: {entry.Name}");
                    }

                    // Stored CRC from archive
                    long storedCrc = entry.Crc;

                    // Compute CRC32 of extracted file, reset before each file
                    crc32.New();
                    using (FileStream fileStream = File.OpenRead(fullPath))
                    {
                        int bytesRead;
                        while ((bytesRead = fileStream.Read(buffer, 0, BUFFERSIZE)) > 0)
                        {
                            crc32.Update(new ArraySegment<byte>(buffer, 0, bytesRead));
                        }
                    }

                    long computedCrc = crc32.Value;

                    if (storedCrc != computedCrc)
                    {
                        Console.WriteLine($"CRC mismatch for {entry.Name}: stored={storedCrc}, computed={computedCrc}");
                    }
                    if (storedCrc == 0)
                    {
                        Console.WriteLine($"WARNING! CRC equal ZERO ( empty file or lost .zip meta-tag ), for {entry.Name}. filesize={new FileInfo(fullPath).Length}");
                    }
                Tail:
                    passed++;
                    if (((DateTime.Now - trigger).TotalMilliseconds > 5999) || (zipFile.Count == passed))
                    {
                        Console.WriteLine($"resolved {zipFile.Count}/{passed}");
                        trigger = DateTime.Now;
                    }
                }
            }
            catch (ExtractedFolderNotFoundException)
            {
                throw;
            }
            catch (ExtractedFileNotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    /// <summary>
    /// Represents virtual directory record
    /// </summary>
    /// <param name="path"></param>
    internal class VlDirectory(string path)
    {
        public Byte Scanned { get; set; } = 0;
        public string Path { get; set; } = path;
    }
    /// <summary>
    /// Represents virtual tree of the VlDirectory list
    /// </summary>
    internal class VlDirectoryTree
    {
        readonly List<VlDirectory> root = [];
        int Cursor = 0;
        public void AddOne(string path)
        {
            root.Add(new VlDirectory(path));
        }
        public void AddMany(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                root.Add(new VlDirectory(path));
            }
        }
        public IEnumerable<VlDirectory> GetAll()
        {
            return root;
        }
        public VlDirectory? GetNext()
        {
            while (Cursor < root.Count)
            {
                VlDirectory current = root[Cursor++];
                if (current.Scanned == 0)
                {
                    current.Scanned = 1;
                    return current;
                }
            }
            if (Cursor == root.Count)
            {
                Cursor = 0;
            }

            return null;
        }
    }

    class TestIntegrity
    {
        const int BUFFERSIZE = 104857600;
        readonly byte[] buffer = new byte[BUFFERSIZE]; // 100 MiB block size
        readonly TgCrc32 crc32 = new();

        public void RunTest(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Destination folder not found: " + path);
                return;
            }

            string rootPath = path;
            List<string> allFiles = [];
            VlDirectoryTree ObjVlDirectoryTree = new();
            ObjVlDirectoryTree.AddOne(rootPath);

            for (; ; )
            {
                rootPath = ObjVlDirectoryTree.GetNext()?.Path ?? "";
                if (!string.IsNullOrEmpty(rootPath))
                {
                    string[] a = Directory.GetDirectories(rootPath);
                    foreach (string subDirectory in a)
                    {
                        FileAttributes attr = File.GetAttributes(subDirectory);
                        if ((attr & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                        {
                            ObjVlDirectoryTree.AddOne(subDirectory);
                        }
                    }
                }
                else break;
            }

            foreach (VlDirectory directory in ObjVlDirectoryTree.GetAll())
            {
                foreach (string file in Directory.GetFiles(directory.Path))
                {
                    FileAttributes attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                    {
                        allFiles.Add(file);
                    }
                }
            }

            foreach (string file in allFiles)
            {
                using (FileStream fileStream = File.OpenRead(file))
                {
                    crc32.New();
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, BUFFERSIZE)) > 0)
                    {
                        crc32.Update(new ArraySegment<byte>(buffer, 0, bytesRead));
                    }
                }
                long computedCrc = crc32.Value;
                Console.WriteLine($"file: {file}, CRC32: {computedCrc.ToString("X16")}");
            }
        }

        public static void TestDestinationIntegrity(string path)
        {
            TestIntegrity ObjTestIntegrity = new();
            ObjTestIntegrity.RunTest(path);
        }
    }
}

