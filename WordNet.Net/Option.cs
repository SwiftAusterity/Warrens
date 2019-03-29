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
using System.Text;
using System.Collections;
using WordNet.Net.WordNet;
using WordNet.Net.Searching;

[assembly: CLSCompliant(true)]
namespace WordNet.Net
{
    [CLSCompliant(true)]
	public class Option
	{
		public string arg;
		public SearchType sch;
		public PartOfSpeech pos;
		public int helpx;
		public string label;
		public int id;

		public static int Count
		{
			get
			{
				return opts.Count;
			}
		}

		public static Option At(int ix)
		{
			return (Option)opts[ix];
		}

		private static ArrayList opts = new ArrayList();

        private Option(string a, string m, string p, int h, string b)
		{
			arg = a;
			if (m[0] == '-')
            {
                sch = new SearchType(true, m.Substring(1));
            }
            else
            {
                sch = new SearchType(false, m);
            }

            pos = PartOfSpeech.Of(p.ToLower());
			helpx = h;
			label = b;
			id = opts.Count;
			opts.Add(this);
		}

		static Option()
		{
			new Option("-synsa", "SIMPTR", "ADJ", 0, "Similarity");   // 0
			new Option("-antsa", "ANTPTR", "ADJ", 1, "Antonyms");
			new Option("-perta", "PERTPTR", "ADJ", 0, "Pertainyms");
			new Option("-attra", "ATTRIBUTE", "ADJ", 2, "Attributes");
			new Option("-domna", "CLASSIFICATION", "ADJ", 3, "Domain");
			new Option("-domta", "CLASS", "ADJ", 4, "Domain Terms");  // 5
			new Option("-famla", "FREQ", "ADJ", 5, "Familiarity");
			new Option("-grepa", "WNGREP", "ADJ", 6, "Grep");

			new Option("-synsn", "HYPERPTR", "NOUN", 0, "Synonyms/Hypernyms (Ordered by Estimated Frequency): brief");
			// WN1.6			new Opt( "-simsn", "RELATIVES", "NOUN", 1, "Synonyms (Grouped by Similarity of Meaning)" );
			new Option("-antsn", "ANTPTR", "NOUN", 2, "Antonyms");
			new Option("-coorn", "COORDS", "NOUN", 3, "Coordinate Terms (sisters)");   // 10
			new Option("-hypen", "-HYPERPTR", "NOUN", 4, "Synonyms/Hypernyms (Ordered by Estimated Frequency): full");
			new Option("-hypon", "HYPOPTR", "NOUN", 5, "Hyponyms");
			new Option("-treen", "-HYPOPTR", "NOUN", 6, "Hyponyms Tree");
			new Option("-holon", "HOLONYM", "NOUN", 7, "Holonyms");
			new Option("-sprtn", "ISPARTPTR", "NOUN", 7, "Part Holonyms");  // 15
			new Option("-smemn", "ISMEMBERPTR", "NOUN", 7, "Member Holonyms");
			new Option("-ssubn", "ISSTUFFPTR", "NOUN", 7, "Substance Holonyms");
			new Option("-hholn", "-HHOLONYM", "NOUN", 8, "Holonyms Tree");
			new Option("-meron", "MERONYM", "NOUN", 9, "Meronyms");
			new Option("-subsn", "HASSTUFFPTR", "NOUN", 9, "Substance Meronyms");  // 20
			new Option("-partn", "HASPARTPTR", "NOUN", 9, "Part Meronyms");
			new Option("-membn", "HASMEMBERPTR", "NOUN", 9, "Member Meronyms");
			new Option("-hmern", "-HMERONYM", "NOUN", 10, "Meronyms Tree");

			// TDMS 26/8/05 - this has been modified inline with WordNet 2.1, however it is not verified
			// the derin and nomn codes need to be verified as being identified correctly
			new Option( "-nomnn", "DERIVATION", "NOUN", 11, "Derived Forms" ); // TDMS 26/8/05 - replaced by above
			new Option( "-derin", "DERIVATION", "NOUN", 11, "Derived Forms" ); // TDMS 26/8/05 - replaced by above

			new Option("-domnn", "CLASSIFICATION", "NOUN", 13, "Domain");
			new Option("-domtn", "CLASS", "NOUN", 14, "Domain Terms");
			new Option("-attrn", "ATTRIBUTE", "NOUN", 12, "Attributes");
			new Option("-famln", "FREQ", "NOUN", 15, "Familiarity");
			new Option("-grepn", "WNGREP", "NOUN", 16, "Grep");  // 30

			new Option("-synsv", "HYPERPTR", "VERB", 0, "Synonyms/Hypernyms (Ordered by Estimated Frequency): brief");
			new Option("-simsv", "RELATIVES", "VERB", 1, "Synonyms (Grouped by Similarity of Meaning)");
			new Option("-antsv", "ANTPTR", "VERB", 2, "Antonyms");
			new Option("-coorv", "COORDS", "VERB", 3, "Coordinate Terms (sisters)");
			new Option("-hypev", "-HYPERPTR", "VERB", 4, "Synonyms/Hypernyms (Ordered by Estimated Frequency): full");  // 35
			new Option("-hypov", "HYPOPTR", "VERB", 5, "Troponyms (hyponyms)");
			new Option("-treev", "-HYPOPTR", "VERB", 5, "Troponyms (hyponyms)");
			new Option("-tropv", "-HYPOPTR", "VERB", 5, "Troponyms (hyponyms)");
			new Option("-entav", "ENTAILPTR", "VERB", 6, "Entailment");
			new Option("-causv", "CAUSETO", "VERB", 7, "\'Cause To\'");  // 40

			// TDMS 26/8/05 - this has been modified inline with WordNet 2.1, however it is not verified
			// the nomnv and deriv codes need to be verified as being identified correctly
			new Option( "-nomnv", "DERIVATION", "VERB", 8, "Derived Forms" );  // TDMS 26/8/05 - replaced by above
			new Option( "-deriv", "DERIVATION", "VERB", 8, "Derived Forms" );  // TDMS 26/8/05 - replaced by above

			new Option("-domnv", "CLASSIFICATION", "VERB", 10, "Domain");
			new Option("-domtv", "CLASS", "VERB", 11, "Domain Terms");
			new Option("-framv", "FRAMES", "VERB", 9, "Sample Sentences");  // 45
			new Option("-famlv", "FREQ", "VERB", 12, "Familiarity");
			new Option("-grepv", "WNGREP", "VERB", 13, "Grep");

			new Option("-synsr", "SYNS", "ADV", 0, "Synonyms");
			new Option("-antsr", "ANTPTR", "ADV", 1, "Antonyms");
			new Option("-pertr", "PERTPTR", "ADV", 0, "Pertainyms");  //  50
			new Option("-domnr", "CLASSIFICATION", "ADV", 2, "Domain");
			new Option("-domtr", "CLASS", "ADV", 3, "Domain Terms");
			new Option("-famlr", "FREQ", "ADV", 4, "Familiarity");
			new Option("-grepr", "WNGREP", "ADV", 5, "Grep");

			new Option("-over", "OVERVIEW", "ALL_POS", -1, "Overview");  // 55
		}
	}
}
