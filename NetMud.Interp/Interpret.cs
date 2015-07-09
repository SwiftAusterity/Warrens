using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMud.Data.Behaviors.Rendering;

namespace NetMud.Interp
{
    public static class Interpret
    {
        public static string Render(string commandString, IActor actor)
        {
            var returnString = new StringBuilder();

            //Need some way to build a context object to work off of
                        


            //return returnString.ToString();
            return "<p>Pong</p><p>pong2</p>";
        }
    }
}
