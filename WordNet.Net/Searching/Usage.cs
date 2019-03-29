/*
 * This file is a part of the WordNet.Net open source project.
 * 
 * Copyright (C) 2005 Malcolm Crowe, Troy Simpson 
 * 
 * Project Home: http://www.ebswift.com
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * 
 * */

using System;
using System.Collections;
using System.IO;
using System.Text;
using WordNet.Net.WordNet;

namespace WordNet.Net.Searching
{
    public class Usage
    {
        private static ArrayList frames = new ArrayList();

        public static Usage Frame(int i)
        {
            if (frames.Count == 0)
            {
                Classinit();
            }

            return (Usage)frames[i];
        }

        public string Str { get; }

        private Usage(string f)
        {
            Str = f;
            frames.Add(this);
        }

        private static void Classinit()
        {
            new Usage("");
            new Usage("Something ----s");
            new Usage("Somebody ----s");
            new Usage("It is ----ing");
            new Usage("Something is ----ing PP");
            new Usage("Something ----s something Adjective/Noun");
            new Usage("Something ----s Adjective/Noun");
            new Usage("Somebody ----s Adjective");
            new Usage("Somebody ----s something");
            new Usage("Somebody ----s somebody");
            new Usage("Something ----s somebody");
            new Usage("Something ----s something");
            new Usage("Something ----s to somebody");
            new Usage("Somebody ----s on something");
            new Usage("Somebody ----s somebody something");
            new Usage("Somebody ----s something to somebody");
            new Usage("Somebody ----s something from somebody");
            new Usage("Somebody ----s somebody with something");
            new Usage("Somebody ----s somebody of something");
            new Usage("Somebody ----s something on somebody");
            new Usage("Somebody ----s somebody PP");
            new Usage("Somebody ----s something PP");
            new Usage("Somebody ----s PP");
            new Usage("Somebody's (body part) ----s");
            new Usage("Somebody ----s somebody to INFINITIVE");
            new Usage("Somebody ----s somebody INFINITIVE");
            new Usage("Somebody ----s that CLAUSE");
            new Usage("Somebody ----s to somebody");
            new Usage("Somebody ----s to INFINITIVE");
            new Usage("Somebody ----s whether INFINITIVE");
            new Usage("Somebody ----s somebody into V-ing something");
            new Usage("Somebody ----s something with something");
            new Usage("Somebody ----s INFINITIVE");
            new Usage("Somebody ----s VERB-ing");
            new Usage("It ----s that CLAUSE");
            new Usage("Something ----s INFINITIVE");
        }
    }
}
