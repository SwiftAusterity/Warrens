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
            Low = default;
            High = default;
        }

        public ValueRange(T low, T high)
        {
            Low = low;
            High = high;
        }

        public override string ToString()
        {
            return string.Format("{0} to {1}", Low, High);
        }
    }
}
