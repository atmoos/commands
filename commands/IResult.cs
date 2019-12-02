using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands
{
    internal interface IResult<out TResult>
    {
        TResult Result {get;}
    }
}