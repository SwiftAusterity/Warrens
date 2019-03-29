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
    [Serializable]
    public class PartOfSpeech
    {
        [NonSerialized()]
        public static Hashtable parts = new Hashtable();
        public string Symbol { get; set; }
        public string Clss { get; set; }
        public int Ident { get; set; }
        public PartsOfSpeech Flag { get; set; }

        public string Key { get; set; }

        private static int uniq = 0;
        internal Hashtable help = new Hashtable(); // string searchtype->string help: see WnHelp

        private PartOfSpeech()
        {
            // empty constructor for serialization
        }

        private PartOfSpeech(string s, string n, string c, PartsOfSpeech f)
        {
            Symbol = s;
            Key = n;
            Clss = c;
            Flag = f;
            Ident = uniq++;
            parts[s] = this;
            if (c == "")
            {
                parts[Key] = this;
            }
        }

        private PartOfSpeech(string s, string n, PartsOfSpeech f)
            : this(s, n, "", f)
        {
        }

        public static PartOfSpeech Of(string s)
        {
            if (uniq == 0)
            {
                Classinit();
            }

            return (PartOfSpeech)parts[s];
        }

        public static PartOfSpeech Of(PartsOfSpeech f)
        {
            if (f == PartsOfSpeech.Noun)
            {
                return Of("noun");
            }

            if (f == PartsOfSpeech.Verb)
            {
                return Of("verb");
            }

            if (f == PartsOfSpeech.Adjective)
            {
                return Of("adj");
            }

            if (f == PartsOfSpeech.Adverb)
            {
                return Of("adv");
            }

            return null;            // unknown or not unique
        }

        private static void Classinit()
        {
            new PartOfSpeech("n", "noun", PartsOfSpeech.Noun); // 0
            new PartOfSpeech("v", "verb", PartsOfSpeech.Verb); // 1
            new PartOfSpeech("a", "adj", PartsOfSpeech.Adjective); // 2
            new PartOfSpeech("r", "adv", PartsOfSpeech.Adverb); // 3
            new PartOfSpeech("s", "adj", "SATELLITE", PartsOfSpeech.Adjective);
        }
    }
}
