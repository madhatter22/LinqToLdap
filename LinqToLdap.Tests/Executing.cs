using System;

namespace LinqToLdap.Tests
{
    internal class Executing
    {
        public static Func<T> This<T>(Func<T> func)
        {
            return func;
        }

        public static Action This(Action func)
        {
            return func;
        }
    }
}
