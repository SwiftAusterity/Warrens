using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Height, Width, Length/depth
    /// </summary>
    public class Dimensions
    {
        [DataType(DataType.Text)]
        public int Height { get; set; }

        [DataType(DataType.Text)]
        public int Width { get; set; }

        [DataType(DataType.Text)]
        public int Length { get; set; }

        public Dimensions()
        {
            Height = -1;
            Width = -1;
            Length = -1;
        }

        public Dimensions(int height, int width, int length)
        {
            Height = height;
            Width = width;
            Length = length;
        }

        public override string ToString()
        {
            return string.Format("{0}x{1}x{2}", Height, Width, Length);
        }
    }
}
