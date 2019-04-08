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
    internal class Indexes
    {
        // TDMS - 14 Aug 2005 - added a new index count
        // so that we could patch more possibilities into
        // the strings array below
        private static readonly int stringscount = 7;
        private readonly Index[] offsets = new Index[stringscount]; // of Index
        private int offset = 0;
        private readonly PartOfSpeech fpos;

        public Indexes(string str, PartOfSpeech pos, WordNetData netdata)
        {
            string[] strings = new string[stringscount];
            str = str.ToLower();
            strings[0] = str;
            strings[1] = str.Replace('_', '-');
            strings[2] = str.Replace('-', '_');
            strings[3] = str.Replace("-", "").Replace("_", "");
            strings[4] = str.Replace(".", "");
            // TDMS - 14 Aug 2005 - added two more possibilities
            // to allow for spaces to be transformed
            // an example is a search for "11 plus", without this
            // transformation no result would be found
            strings[5] = str.Replace(' ', '-');
            strings[6] = str.Replace(' ', '_');
            offsets[0] = new Index(str, pos, netdata);
            // TDMS - 14 Aug 2005 - changed 5 to 7 to allow for two
            // new possibilities
            for (int i = 1; i < stringscount; i++)
            {
                if (str != strings[i])
                {
                    offsets[i] = new Index(strings[i], pos, netdata);
                }
            }

            fpos = pos;
        }

        public Index Next()
        {
            for (int i = offset; i < stringscount; i++)
            {
                if (offsets[i] != null)
                {
                    offset = i + 1;
                    return offsets[i];
                }
            }

            return null;
        }
    }
}
