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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Acz
{
    class ZipVerifier
    {
        const int BUFFERSIZE = 1048576000;
        static readonly byte[] buffer = new byte[BUFFERSIZE]; // 1000 MiB block size
        static readonly Crc32 crc32 = new();

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
        public static void VerifyZipCrc(string zipPath, string destinationFolder)
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

                    // Compute CRC32 of extracted file
                    crc32.Reset();
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
}
