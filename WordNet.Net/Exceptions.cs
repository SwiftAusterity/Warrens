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
using System.IO;
using System.Collections;
using WordNet.Net.Searching;

namespace WordNet.Net
{
    public class Exceptions
	{
        // exception list files
        private static Hashtable excfps = new Hashtable();

		static Exceptions()
		{
			IDictionaryEnumerator d = PartOfSpeech.parts.GetEnumerator();
			while (d.MoveNext())
			{
				PartOfSpeech p = (PartOfSpeech)d.Value;
				if (!excfps.ContainsKey(p.Key))
                {
                    excfps[p.Key] = WordNetData.GetStreamReader(WordNetData.ExcFile(p));
                }
            }
		}

        private string line = null;
        private int beglp = 0, endlp = -1;

		public Exceptions(string word, string p)
			: this(word, PartOfSpeech.Of(p))
		{
		}

		public Exceptions(string word, PartOfSpeech pos)
		{
			line = WordNetData.BinSearch(word, (StreamReader)excfps[pos.Key]);
			if (line != null)
            {
                endlp = line.IndexOf(' ');
            }
        }

		public string Next()
		{
			if (endlp >= 0 && endlp + 1 < line.Length)
			{
				beglp = endlp + 1;
				while (beglp < line.Length && line[beglp] == ' ')
                {
                    beglp++;
                }

                endlp = beglp;
				while (endlp < line.Length && line[endlp] != ' ' && line[endlp] != '\n')
                {
                    endlp++;
                }

                if (endlp != beglp)
                {
                    return line.Substring(beglp, endlp - beglp);
                }
            }
			endlp = -1;
			return null;
		}
	}
}
