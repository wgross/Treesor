namespace Treesor.PSDriveProvider
{
    public class Reference<T> where T : struct
    {
        public Reference(T referencedValue)
        {
            Value = referencedValue;
        }

        public T Value { get; }
    }
}