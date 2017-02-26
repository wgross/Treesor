namespace Treesor.PSDriveProvider
{
    public static class Reference
    {
        public static Reference<T> To<T>(T value) where T : struct
        {
            return new Reference<T>(value);
        }
    }

    public class Reference<T> where T : struct
    {
        public Reference(T referencedValue)
        {
            Value = referencedValue;
        }

        public T Value { get; }
    }
}