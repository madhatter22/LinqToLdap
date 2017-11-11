namespace LinqToLdap.Collections
{
    internal sealed class Nothing
    {
        public static readonly Nothing Value = new Nothing();
        private Nothing() { }
    }
}
