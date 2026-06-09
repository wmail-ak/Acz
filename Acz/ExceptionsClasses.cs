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

using System;
using System.Collections.Generic;
using System.Text;

namespace Acz
{
    public class AczPathExceptionException : Exception
    {
        public AczPathExceptionException() { }
        public AczPathExceptionException(string message) : base(message) { }
        public AczPathExceptionException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class OriginFileNotFoundException : AczPathExceptionException
    {
        public OriginFileNotFoundException() { }
        public OriginFileNotFoundException(string message)
            : base(message) { }
        public OriginFileNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class ExtractedFileNotFoundException : AczPathExceptionException
    {
        public ExtractedFileNotFoundException() { }
        public ExtractedFileNotFoundException(string message)
            : base(message) { }
        public ExtractedFileNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class DestinationFolderNotFoundException : AczPathExceptionException
    {
        public DestinationFolderNotFoundException() { }
        public DestinationFolderNotFoundException(string message)
            : base(message) { }
        public DestinationFolderNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class ExtractedFolderNotFoundException : AczPathExceptionException
    {
        public ExtractedFolderNotFoundException() { }
        public ExtractedFolderNotFoundException(string message)
            : base(message) { }
        public ExtractedFolderNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class CRC32Exception : Exception
    {
        public CRC32Exception() { }
        public CRC32Exception(string message) : base(message) { }
        public CRC32Exception(string message, Exception inner)
            : base(message, inner) { }
    }

    public class EmptyCRC32Exception : CRC32Exception
    {
        public EmptyCRC32Exception() { }
        public EmptyCRC32Exception(string message) : base(message) { }
        public EmptyCRC32Exception(string message, Exception inner)
            : base(message, inner) { }

    }

    public class MismatchCRC32Exception : CRC32Exception
    {
        public MismatchCRC32Exception() { }
        public MismatchCRC32Exception(string message) : base(message) { }
        public MismatchCRC32Exception(string message, Exception inner)
            : base(message, inner) { }
    }

}
