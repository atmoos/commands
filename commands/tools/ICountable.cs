using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools
{
    internal interface ICountable
    {
        Int32 Count { get; }
    }
}
