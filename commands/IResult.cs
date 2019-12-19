namespace commands
{
    internal interface IResult<out TResult>
    {
        TResult Result { get; }
    }
}