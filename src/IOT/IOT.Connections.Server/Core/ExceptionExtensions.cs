namespace IOT.Connections.Server;

internal static class ExceptionExtensions
{
    public static T TraverseFind<T>(this Exception e)
        where T : Exception
    {
        var result = e.TraverseFind(x => x is not T);

        return (T)result!;
    }

    public static Exception? TraverseFind(this Exception e, Func<Exception, bool> lambda)
    {
        if(lambda is null)
        {
            return default;
        }

        var exception = e;

        while(true)
        {
            if(lambda.Invoke(exception))
            {
                return exception;
            }

            if(exception.InnerException == null)
            {
                break;
            }

            exception = exception.InnerException;
        }

        return default;
    }
}