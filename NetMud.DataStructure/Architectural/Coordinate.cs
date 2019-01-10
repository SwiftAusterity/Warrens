using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural
{
    public class Coordinate
    {
        [DataType(DataType.Text)]
        public int X { get; set; }

        [DataType(DataType.Text)]
        public int Y { get; set; }

        [DataType(DataType.Text)]
        public int Z { get; set; }

        public Coordinate()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Coordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
