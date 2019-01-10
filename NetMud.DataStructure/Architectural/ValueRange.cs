using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural
{
    public class ValueRange<T>
    {
        [DataType(DataType.Text)]
        public T Low { get; set; }

        [DataType(DataType.Text)]
        public T High { get; set; }

        public ValueRange()
        {
            Low = default(T);
            High = default(T);
        }

        public ValueRange(T low, T high)
        {
            Low = low;
            High = high;
        }
    }
}
