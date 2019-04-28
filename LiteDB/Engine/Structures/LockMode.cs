﻿using System;
using System.Collections.Generic;
using System.IO;

namespace LiteDB.Engine
{
    /// <summary>
    /// Represents a snapshot lock mode
    /// </summary>
    public enum LockMode
    {
        /// <summary>
        /// Read only snap with read lock
        /// </summary>
        Read,

        /// <summary>
        /// Read/Write snapshot with reserved lock
        /// </summary>
        Write
    }
}