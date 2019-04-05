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
using WordNet.Net.Searching;
using WordNet.Net.WordNet;

namespace WordNet.Net
{
	/// <summary>
	/// Summary description for Index.
	/// </summary>
	public class Index
	{
		public PartOfSpeech pos = null;
		public string wd;
		public int sense_cnt = 0;		/* sense (collins) count */
		public PointerType[] ptruse = null; /* pointer data in index file */
		public int tagsense_cnt = 0;	/* number senses that are tagged */
		public int[] offs = null;		/* synset offsets */
		public SynonymSet[] syns = null;   /* cached */
		public Index next = null;
        private WordNetData netData;

		/* From search.c:
		 * Find word in index file and return parsed entry in data structure.
		 * Input word must be exact match of string in database. 
         */

        public Index(WordNetData netdata)
        {
            netData = netdata;
        }

		// From the WordNet Manual (http://wordnet.princeton.edu/man/wnsearch.3WN.html)
		// index_lookup() finds searchstr in the index file for pos and returns a pointer 
		// to the parsed entry in an Index data structure. searchstr must exactly match the 
		// form of the word (lower case only, hyphens and underscores in the same places) in 
		// the index file. NULL is returned if a match is not found.
		public Index(string word, PartOfSpeech partOfSpeech, WordNetData netdata)
		{
            netData = netdata;
            int j;
			if (word == "" || (!char.IsLetter(word[0]) && !char.IsNumber(word[0])))
            {
                return;
            }

            string line = netData.BinSearch(word, partOfSpeech);
			if (line == null)
            {
                return;
            }

            string[] st = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            if (st.Length >= 6)
            {
                int stI = 0;
                wd = st[stI++]; /* the word */
                pos = PartOfSpeech.Of(st[stI++]); /* the part of speech */
                sense_cnt = int.Parse(st[stI++]); /* collins count */

                int ptruse_cnt = int.Parse(st[stI++]); /* number of pointers types */
                ptruse = new PointerType[ptruse_cnt];
                for (j = 0; j < ptruse_cnt; j++)
                {
                    ptruse[j] = PointerType.Of(st[stI++]);
                }

                int off_cnt = int.Parse(st[stI++]);
                offs = new int[off_cnt];
                tagsense_cnt = int.Parse(st[stI++]);
                for (j = 0; j < off_cnt; j++)
                {
                    offs[j] = int.Parse(st[stI++]);
                }
            }
		}

		public bool HasHoloMero(string s, Search search)
		{
			return HasHoloMero(PointerType.Of(s), search);
		}

		public bool HasHoloMero(PointerType p, Search search)
		{
			PointerType pbase;
			if (p.Mnemonic == "HMERONYM")
            {
                pbase = PointerType.Of("HASMEMBERPTR");
            }
            else
            {
                pbase = PointerType.Of("ISMEMBERPTR");
            }

            if (offs != null)
            {
                for (int i = 0; i < offs.Length; i++)
                {
                    SynonymSet s = new SynonymSet(offs[i], PartOfSpeech.Of("noun"), "", search, 0, netData);

                    if (s.Has(pbase) || s.Has(pbase + 1) || s.Has(pbase + 2))
                    {
                        return true;
                    }
                }
            }

			return false;
		}
	}
}
