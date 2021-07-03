using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB.Engine
{
    internal enum PageType 
    { 
        Empty = 0, 
        Header = 1, 
        Collection = 2, 
        Index = 3, 
        Data = 4
    }
}
