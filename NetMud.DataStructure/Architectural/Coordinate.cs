using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural
{
    public class Coordinate
    {
        [DataType(DataType.Text)]
        public short X { get; set; }

        [DataType(DataType.Text)]
        public short Y { get; set; }

        public Coordinate()
        {
            X = 0;
            Y = 0;
        }

        public Coordinate(short x, short y)
        {
            X = x;
            Y = y;
        }

        public Coordinate(int x, int y)
        {
            X = (short)x;
            Y = (short)y;
        }


        public Coordinate(short x, int y)
        {
            X = x;
            Y = (short)y;
        }

        public Coordinate(int x, short y)
        {
            X = (short)x;
            Y = y;
        }
    }
}
