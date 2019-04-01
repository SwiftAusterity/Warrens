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
    public class WordNetData
    {
        public string path;
        private Hashtable indexfps = new Hashtable();
        private Hashtable datafps = new Hashtable();

        public WordNetData(string dictPath)
        {
            path = dictPath;
            PartOfSpeech.Of(PartsOfSpeech.Noun);
            IDictionaryEnumerator d = PartOfSpeech.parts.GetEnumerator();
            while (d.MoveNext())
            {
                PartOfSpeech p = (PartOfSpeech)d.Value;
                if (!indexfps.ContainsKey(p.Key))
                {
                    indexfps[p.Key] = GetStreamReader(IndexFile(p));
                }

                if (!datafps.ContainsKey(p.Key))
                {
                    datafps[p.Key] = GetStreamReader(DataFile(p));
                }
            }
        }

        public StreamReader GetStreamReader(string filePath)
        {
            // copy file to memory stream
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = File.OpenRead(filePath))
            {
                ms.SetLength(fs.Length);
                fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                ms.Flush();
            }

            return new StreamReader(ms, Encoding.ASCII, false, 128);
        }

        public StreamReader Index(PartOfSpeech p)
        {
            return (StreamReader)indexfps[p.Key];
        }

        public StreamReader Data(PartOfSpeech p)
        {
            return (StreamReader)datafps[p.Key];
        }

        public void Reopen(PartOfSpeech p)
        {
            Index(p).Close();
            Data(p).Close();
            indexfps[p.Key] = GetStreamReader(IndexFile(p));
            datafps[p.Key] = GetStreamReader(DataFile(p));
        }

        public string[] lexfiles =
        {
            "adj.all",			    /* 0 */
			"adj.pert",			    /* 1 */
			"adv.all",			    /* 2 */
			"noun.Tops",		    /* 3 */
			"noun.act",			    /* 4 */
			"noun.animal",		    /* 5 */
			"noun.artifact",		/* 6 */
			"noun.attribute",		/* 7 */
			"noun.body",		    /* 8 */
			"noun.cognition",		/* 9 */
			"noun.communication",   /* 10 */
			"noun.event",		    /* 11 */
			"noun.feeling",		    /* 12 */
			"noun.food",		    /* 13 */
			"noun.group",		    /* 14 */
			"noun.location",		/* 15 */
			"noun.motive",		    /* 16 */
			"noun.object",		    /* 17 */
			"noun.person",		    /* 18 */
			"noun.phenomenon",		/* 19 */
			"noun.plant",		    /* 20 */
			"noun.possession",		/* 21 */
			"noun.process",		    /* 22 */
			"noun.quantity",		/* 23 */
			"noun.relation",		/* 24 */
			"noun.shape",		    /* 25 */
			"noun.state",		    /* 26 */
			"noun.substance",		/* 27 */
			"noun.time",		    /* 28 */
			"verb.body",		    /* 29 */
			"verb.change",		    /* 30 */
			"verb.cognition",		/* 31 */
			"verb.communication",	/* 32 */
			"verb.competition",		/* 33 */
			"verb.consumption",		/* 34 */
			"verb.contact",		    /* 35 */
			"verb.creation",		/* 36 */
			"verb.emotion",		    /* 37 */
			"verb.motion",		    /* 38 */
			"verb.perception",		/* 39 */
			"verb.possession",		/* 40 */
			"verb.social",		    /* 41 */
			"verb.stative",		    /* 42 */
			"verb.weather",		    /* 43 */
			"adj.ppl",			    /* 44 */
		};

        internal string ExcFile(PartOfSpeech n)
        {
            return path + n.Key + ".exc";
        }

        internal string IndexFile(PartOfSpeech n)
        {
            return path + "index." + n.Key; // WN2.1 - TDMS
        }

        internal string DataFile(PartOfSpeech n)
        {
            return path + "data." + n.Key; // WN2.1 - TDMS
        }

        public string BinSearch(string searchKey, char marker, StreamReader fp)
        {
            long bot = fp.BaseStream.Seek(0, SeekOrigin.End);
            long top = 0;
            long mid = (bot - top) / 2 + top;
            long diff = 666; // ???
            string key = "";
            string line = "";

            do
            {
                fp.DiscardBufferedData();
                fp.BaseStream.Position = mid - 1;

                if (mid != 1)
                {
                    fp.ReadLine();
                }

                line = fp.ReadLine();
                if (line == null)
                {
                    return null;
                }

                line = line.Replace("\0", "");
                int n = Math.Max(line.IndexOf(marker), 0);
                key = line.Substring(0, n);

                int co = string.CompareOrdinal(key, searchKey);
                if (co < 0)
                {
                    // key is alphabetically less than the search key
                    top = mid;
                    diff = (bot - top) / 2;
                    mid = top + diff;
                }
                if (co > 0)
                {
                    // key is alphabetically greater than the search key
                    bot = mid;
                    diff = (bot - top) / 2;
                    mid = top + diff;
                }
            }
            while (key != searchKey && diff != 0);

            if (key == searchKey)
            {
                return line;
            }

            return null;
        }

        public string BinSearch(string searchKey, StreamReader fp)
        {
            return BinSearch(searchKey, ' ', fp);
        }

        public string BinSearch(string word, PartOfSpeech pos)
        {
            return BinSearch(word, Index(pos));
        }

        public string BinSearchSemCor(string uniqueid, string searchKey, StreamReader fp)
        {
            int n;
            searchKey = searchKey.ToLower(); // for some reason some WordNet words are stored with a capitalised first letter, whilst all words in the sense index are lowercase
            string key = "";
            string line = BinSearch(searchKey, '%', fp);

            // we have found an exact match (or no match)
            if (line == null || line.IndexOf(uniqueid, 0) > 0)
            {
                return line;
            }

            // set the search down the list and work up
            fp.DiscardBufferedData();
            fp.BaseStream.Position -= 4000;

            // move down until we find the first matching key
            do
            {
                line = fp.ReadLine();
                if (line == null)
                {
                    return null;
                }

                n = Math.Max(line.IndexOf('%'), 0);
                key = line.Substring(0, n);
            }
            while (key != searchKey);

            // scroll through matching words until the exact identifier is found
            do
            {
                if (line.IndexOf(uniqueid, 0) > 0)
                {
                    return line;
                }

                line = fp.ReadLine();
                if (line == null)
                {
                    return null;
                }

                n = Math.Max(line.IndexOf('%'), 0);
                key = line.Substring(0, n);
            }
            while (key == searchKey);

            return null;
        }

        /// <summary>
        /// Determines if a word is defined in the WordNet database and returns
        /// all possible searches of the word.
        /// </summary>
        /// <example> This sample displays a message stating whether the 
        /// word "car" exists as the part of speech "noun".
        /// <code>
        /// WNCommon.path = "C:\Program Files\WordNet\2.1\dict\"
        /// Dim wrd As String = "car"
        /// Dim POS As String = "noun"
        /// Dim b As Boolean = WNDB.is_defined(wrd, PartOfSpeech.of(POS)).NonEmpty.ToString
        /// 
        /// If b Then
        /// 	MessageBox.Show("The word " &amp; wrd &amp; " exists as a " &amp; POS &amp; ".")
        /// Else
        /// 	MessageBox.Show("The word " &amp; wrd &amp; " does not exist as a " &amp; POS &amp; ".")
        /// End If
        /// </code>
        /// </example>
        /// <param name="searchstr">The word to search for</param>
        /// <param name="fpos">Part of Speech (noun, verb, adjective, adverb)</param>
        /// <returns>A SearchSet or null if the word does not exist in the dictionary</returns>
        public SearchSet Is_defined(string searchstr, PartOfSpeech fpos)
        {
            Indexes ixs = new Indexes(searchstr, fpos, this);
            Index index;
            int i;
            int CLASS = 22; /* - */
            int LASTTYPE = CLASS;

            Search s = new Search(searchstr, fpos, new SearchType(false, "FREQ"), 0, this);
            SearchSet retval = new SearchSet();
            while ((index = ixs.Next()) != null)
            {
                retval = retval + "SIMPTR" + "FREQ" + "SYNS" + "WNGREP" + "OVERVIEW"; // added WNGREP - TDMS
                for (i = 0; index.ptruse != null && i < index.ptruse.Length; i++)
                {
                    PointerType pt = index.ptruse[i];

                    // WN2.1 - TDMS
                    if (pt.Ident <= LASTTYPE)
                    {
                        retval = retval + pt;
                    }
                    else if (pt.Mnemonic == "INSTANCE")
                    {
                        retval = retval + "HYPERPTR";
                    }
                    else if (pt.Mnemonic == "INSTANCES")
                    {
                        retval = retval + "HYPOPTR";
                    }

                    // WN2.1 - TDMS
                    if (pt.Mnemonic == "SIMPTR")
                    {
                        retval = retval + "ANTPTR";
                    }

                    if (fpos.Key == "noun")
                    {
                        /* set generic HOLONYM and/or MERONYM bit if necessary */
                        if (pt >= "ISMEMBERPTR" && pt <= "ISPARTPTR")
                        {
                            retval = retval + "HOLONYM";
                        }
                        else if (pt >= "HASMEMBERPTR" && pt <= "HASPARTPTR")
                        {
                            retval = retval + "MERONYM";
                        }
                    }
                }

                if (fpos.Key == "noun")
                {
                    retval = retval + "RELATIVES";
                    if (index.HasHoloMero("HMERONYM", s))
                    {
                        retval = retval + "HMERONYM";
                    }

                    if (index.HasHoloMero("HHOLONYM", s))
                    {
                        retval = retval + "HHOLONYM";
                    }

                    if (retval["HYPERPTR"])
                    {
                        retval = retval + "COORDS";
                    }
                }
                else if (fpos.Key == "verb")
                {
                    retval = retval + "RELATIVES" + "FRAMES"; // added frames - TDMS
                }
            }
            return retval;
        }

        internal ArrayList Wngrep(string wordPassed, PartOfSpeech pos)
        {
            ArrayList r = new ArrayList();
            StreamReader fp = Index(pos);
            fp.BaseStream.Position = 0;
            fp.DiscardBufferedData();
            string word = wordPassed.Replace(" ", "_");
            string line;

            while ((line = fp.ReadLine()) != null)
            {
                int lineLen = line.IndexOf(' ');
                line = line.Substring(0, lineLen);
                try
                {
                    if (line.IndexOf(word) >= 0)
                    {
                        r.Add(line.Replace("_", " "));
                    }
                }
                catch
                {
                }
            }

            return r;
        }
    }
}
