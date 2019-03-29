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
    public class PointerType
    {
        private static Hashtable ptypes = new Hashtable();

        private PointerType()
        {
            // empty constructor for serialization
        }

        public static PointerType Of(string s) // lookup by symbol or mnemonic
        {
            if (Count == 0)
            {
                Classinit();
            }

            return (PointerType)ptypes[s];
        }

        public static PointerType Of(int id) // lookup by ident
        {
            if (Count == 0)
            {
                Classinit();
            }

            return (PointerType)ptypes[id];
        }

        public static int Count { get; set; }

        public string Symbol { get; set; }

        public string Mnemonic { get; set; }

        public int Ident { get; set; }

        public string Label { get; set; }

        private PointerType(string s, string m, string h)
        {
            Symbol = s;
            Mnemonic = m;
            Label = h;
            Ident = ++Count;
            ptypes[m] = this;
            ptypes[s] = this;
            ptypes[Ident] = this;
        }

        public static PointerType operator +(PointerType a, int b)
        {
            return (PointerType)ptypes[a.Ident + b];
        }

        private static void Classinit()
        {
            new PointerType("!", "ANTPTR", "Antonyms"); // 1
            new PointerType("@", "HYPERPTR", "Synonyms/Hypernyms"); // 2
            new PointerType("~", "HYPOPTR", "Hyponyms");  // 3
            new PointerType("*", "ENTAILPTR", "Entailment");  // 4
            new PointerType("&", "SIMPTR", "Similarity"); // 5
            new PointerType("#m", "ISMEMBERPTR", "Member Holonyms"); // 6
            new PointerType("#s", "ISSTUFFPTR", "Substance Holonyms");  // 7
            new PointerType("#p", "ISPARTPTR", "Part Holonyms");  // 8
            new PointerType("%m", "HASMEMBERPTR", "Member Meronyms"); // 9
            new PointerType("%s", "HASSTUFFPTR", "Substance Meronyms"); // 10
            new PointerType("%p", "HASPARTPTR", "Part Meronyms"); // 11
            new PointerType("%", "MERONYM", "Meronyms"); // 12
            new PointerType("#", "HOLONYM", "Holonyms"); // 13
            new PointerType(">", "CAUSETO", "'Cause To'"); // 14
            new PointerType("<", "PPLPTR", ""); // 15
            new PointerType("^", "SEEALSOPTR", "See also"); // 16
            new PointerType("\\", "PERTPTR", "Pertainyms"); // 17
            new PointerType("=", "ATTRIBUTE", "Attributes"); // 18
            new PointerType("$", "VERBGROUP", ""); // 19
            new PointerType("+", "DERIVATION", ""); // 20 // TDMS 26/8/05 - removed deviation
            new PointerType(";", "CLASSIFICATION", "Domain"); // 21
            new PointerType("-", "CLASS", "Domain Terms"); // 22
            new PointerType("", "SYNS", "Synonyms"); // 23
            new PointerType("", "FREQ", "Frequency"); // 24
            new PointerType("", "FRAMES", "Sample Sentences"); // 25 // TDMS 05/03/09 - fix suggested by Scott Zhang - reported 09/01/2009
            new PointerType("", "COORDS", "Coordinate Terms"); // 26
            new PointerType("", "RELATIVES", "Relatives"); // 27
            new PointerType("", "HMERONYM", "Meronyms"); // 28
            new PointerType("", "HHOLONYM", "Holonyms"); // 29
            new PointerType("", "WNGREP", "Grep"); // 30
            new PointerType("", "OVERVIEW", "Overview"); // 31
            new PointerType(";c", "CLASSIF_CATEGORY", "Classification Category"); // 32
            new PointerType(";u", "CLASSIF_USAGE", "Classification Usage"); // 33
            new PointerType(";r", "CLASSIF_REGIONAL", "Classification Regional"); // 34
            new PointerType("-c", "CLASS_CATEGORY", "Class Category"); // 35
            new PointerType("-u", "CLASS_USAGE", "Class Usage"); // 36
            new PointerType("-r", "CLASS_REGIONAL", "Class Regional"); // 37

            // WN2.1 - TDMS
            new PointerType("@i", "INSTANCE", "Instance"); // 38
            new PointerType("~i", "INSTANCES", "Instances"); // 39
        }

        public static bool operator >=(PointerType a, string s)
        {
            return a.Ident >= Of(s).Ident;
        }

        public static bool operator <=(PointerType a, string s)
        {
            return a.Ident <= Of(s).Ident;
        }
    }
}
