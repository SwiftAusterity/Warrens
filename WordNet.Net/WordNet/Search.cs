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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using WordNet.Net.Searching;

namespace WordNet.Net.WordNet
{
    // C# interface to WordNet data
    // WordNet is from Princton University
    // this interface by Malcolm Crowe
    // update to latest WordNet version by Troy Simpson
    [Serializable]
    public class Search
    {
        public string word;
        public SearchType sch;
        public PartOfSpeech pos;
        public int whichsense;  // which sense to search for
        public Hashtable parts = null; // string->Search filled in by the two-parameter form of do_search
        public Hashtable morphs = null; // string->Search: filled in by do_search(true)
        public IList<SynonymSet> senses = null; // of SynSet: filled in by findtheinfo 
        public ArrayList countSenses = null; // of int: filled in by findtheinfo for FREQ
        public ArrayList strings = null; // of string: filled in by findtheinfo for WNGREP
        public Hashtable lexemes = new Hashtable(); // results of search: Lexeme -> true
        public string buf = ""; // results of search as text string
        internal bool prflag = true;
        internal bool prlexid = false;
        public int taggedSenses;
        public const int ALLSENSES = 0;
        private CustomGrep customgrep = null;
        private WordNetData netData;
        private int lastholomero = 0;

        public Search(string theWord, bool doMorphs, string thePartOfSpeech, string theSearchType, int sn, WordNetData netdata)
            : this(theWord, doMorphs, PartOfSpeech.Of(thePartOfSpeech), new SearchType(theSearchType), sn, netdata)
        {
        }

        public Search(string w, bool doMorphs, PartOfSpeech p, SearchType s, int sn, WordNetData netdata)
            : this(w, p, s, sn, netdata)
        {
            if (p != null)
            {
                Do_search(doMorphs);
            }
        }

        public Search(string w, bool doMorphs, PartOfSpeech p, SearchType s, int sn, CustomGrep cg, WordNetData netdata)
            : this(w, p, s, sn, netdata)
        {
            customgrep = cg;
            if (p != null)
            {
                Do_search(doMorphs);
            }
        }

        internal Search(string w, PartOfSpeech p, SearchType s, int sn, WordNetData netdata)
        {
            netData = netdata;
            word = w;
            pos = p;
            sch = s;
            whichsense = sn;
        }

        internal void Mark()
        {
            lastholomero = buf.Length;
        }

        internal void Trunc()
        {
            buf = buf.Substring(0, lastholomero);
        }

        /// <summary>
        /// Performs a search based on the parameters setup
        /// in the Search constructor.
        /// </summary>
        /// <param name="doMorphs">Specify if morphs should be searched</param>
        /// <param name="thePartOfSpeech">The Part Of Speech to perform the search on</param>
        public void Do_search(bool doMorphs, string thePartOfSpeech)
        {
            if (parts == null)
            {
                parts = new Hashtable();
            }

            Search s = new Search(word, PartOfSpeech.Of(thePartOfSpeech), sch, whichsense, netData);
            s.Do_search(doMorphs);
            parts[thePartOfSpeech] = s;
            buf += s.buf;
        }

        /// <summary>
        /// Performs a search based on the parameters setup
        /// in the Search constructor.
        /// </summary>
        /// <param name="doMorphs">Specifies whether to perform a search on morphs.  
        /// This parameter is retrieved in the first
        /// call from do_search(bool m, string p).  If morph searching is specified
        /// and a morph is found, then on a recursive call to this method
        /// morphing will be turned off to prevent unnecessary morph searching.</param>
        internal void Do_search(bool doMorphs)
        {
            Findtheinfo();
            if (buf.Length > 0)
            {
                buf = "\n" + sch.Label + " of " + pos.Key + " " + word + "\n" + buf;
            }

            if (!doMorphs)
            {
                return;
            }

            morphs = new Hashtable();
            Morph st = new Morph(word, pos, netData);
            string morphword;

            // if there are morphs then perform iterative searches
            // on each morph, filling the morph tree in the search
            // object.
            while ((morphword = st.Next()) != null)
            {
                Search s = new Search(morphword, pos, sch, whichsense, netData);
                s.Do_search(false);

                // Fill the morphlist - eg. if verb relations of 'drunk' are requested, none are directly 
                // found, but the morph 'drink' will have results.  The morph hashtable will be populated 
                // into the search results and should be iterated instead of the returned synset if the 
                // morphs are non-empty
                morphs[morphword] = s;
                buf += s.buf;
            }
        }

        // From the WordNet Manual (http://wordnet.princeton.edu/man/wnsearch.3WN.html)
        // findtheinfo() is the primary search algorithm for use with database interface 
        // applications. Search results are automatically formatted, and a pointer to the 
        // text buffer is returned. All searches listed in WNHOME/include/wnconsts.h can be 
        // done by findtheinfo().
        private void Findtheinfo()
        {
            Indexes ixs = new Indexes(word, pos, netData);
            int depth = sch.rec ? 1 : 0;
            senses = new List<SynonymSet>();
            Index idx;
            switch (sch.ptp.Mnemonic)
            {
                case "OVERVIEW":
                    WNOverview();
                    break;
                case "FREQ":
                    if (countSenses == null)
                    {
                        countSenses = new ArrayList();
                    }

                    while ((idx = ixs.Next()) != null)
                    {
                        countSenses.Add(idx.offs.Length);
                        buf += "Sense " + countSenses.Count + ": " + idx.offs.Length;
                    }
                    break;
                case "WNGREP":
                    if (!(customgrep == null))
                    {
                        strings = customgrep.Wngrep(word, pos);
                    }
                    else
                    {
                        strings = netData.Wngrep(word, pos);
                    }

                    for (int wi = 0; wi < strings.Count; wi++)
                    {
                        buf += (string)strings[wi] + "\n";
                    }

                    break;
                case "VERBGROUP":
                case "RELATIVES":
                    while ((idx = ixs.Next()) != null)
                    {
                        Relatives(idx);
                    }

                    break;
                default:
                    /* look at all spellings of word */
                    while ((idx = ixs.Next()) != null)
                    {
                        /* Print extra sense msgs if looking at all senses */
                        if (whichsense == ALLSENSES)
                        {
                            buf += "\n";
                        }

                        /* Go through all of the searchword's senses in the
						   database and perform the search requested. */
                        for (int sense = 0; sense < idx.offs.Length; sense++)
                        {
                            if (whichsense == ALLSENSES || whichsense == sense + 1)
                            {
                                prflag = false;
                                /* Determine if this synset has already been done
								   with a different spelling. If so, skip it. */
                                var skipToEnd = false;
                                for (int j = 0; j < senses.Count; j++)
                                {
                                    SynonymSet ss = senses[j];
                                    if (ss.hereiam == idx.offs[sense])
                                    {
                                        skipToEnd = true;
                                        break;
                                    }
                                }

                                if (!skipToEnd)
                                {
                                    SynonymSet cursyn = new SynonymSet(idx, sense, this, netData);

                                    //TODO: moved senses.add(cursyn) from here to each case and handled it differently according to search - this handling needs to be verified to ensure the filter is not to limiting
                                    switch (sch.ptp.Mnemonic)
                                    {
                                        case "ANTPTR":
                                            if (pos.Key == "adj")
                                            {
                                                cursyn.TraceAdjAnt();
                                            }
                                            else
                                            {
                                                cursyn.TracePtrs(sch.ptp, pos, depth);
                                            }

                                            if (cursyn.isDirty)
                                            { // TDMS 25 Oct 2005 - restrict to relevant values
                                                cursyn.frames.Clear(); // TDMS 03 Jul 2006 - frames get added in wordnet.cs after filtering
                                                senses.Add(cursyn);
                                            }
                                            break;
                                        case "COORDS":
                                            //eg. search for 'car', select Noun -> 'Coordinate Terms'
                                            cursyn.TraceCoords(PointerType.Of("HYPOPTR"), pos, depth);

                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;
                                        case "FRAMES":
                                            //eg. search for 'right', select Verb -> 'Sample Sentences'
                                            cursyn.StrFrame(true);
                                            // TDMS 03 JUL 2006 fixed relevancy check										if (cursyn.sense != 0) // TDMS 25 Oct 2005 - restrict to relevant values
                                            if (cursyn.isDirty)
                                            {
                                                senses.Add(cursyn);
                                            }

                                            break;
                                        case "MERONYM":
                                            //eg. search for 'car', select Noun -> 'Meronym'
                                            cursyn.TracePtrs(PointerType.Of("HASMEMBERPTR"), pos, depth);
                                            cursyn.TracePtrs(PointerType.Of("HASSTUFFPTR"), pos, depth);
                                            cursyn.TracePtrs(PointerType.Of("HASPARTPTR"), pos, depth);

                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }

                                            break;
                                        case "HOLONYM":
                                            //eg. search for 'car', select Noun -> 'Holonyms'
                                            cursyn.TracePtrs(PointerType.Of("ISMEMBERPTR"), pos, depth);
                                            cursyn.TracePtrs(PointerType.Of("ISSTUFFPTR"), pos, depth);
                                            cursyn.TracePtrs(PointerType.Of("ISPARTPTR"), pos, depth);
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;
                                        case "HMERONYM":
                                            //eg. search for 'car', select Noun -> 'Meronyms Tree'
                                            cursyn.PartsAll(sch.ptp);
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;
                                        case "HHOLONYM":
                                            cursyn.PartsAll(sch.ptp);
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;
                                        case "SEEALSOPTR":
                                            cursyn.Seealso();
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;
                                        case "SIMPTR":
                                        case "SYNS":
                                        case "HYPERPTR":
                                            //eg. search for 'car', select Noun -> 'Synonyms/Hypernyms, ordered by estimated frequency'
                                            WordsFrom(cursyn);
                                            cursyn.Strsns(sense + 1);
                                            prflag = true;
                                            cursyn.TracePtrs(sch.ptp, pos, depth);
                                            if (pos.Key == "adj")
                                            {
                                                cursyn.TracePtrs(PointerType.Of("PERTPTR"), pos, depth);
                                                cursyn.TracePtrs(PointerType.Of("PPLPTR"), pos, depth);
                                            }
                                            else if (pos.Key == "adv")
                                            {
                                                cursyn.TracePtrs(PointerType.Of("PERTPTR"), pos, depth);
                                            }

                                            if (pos.Key == "verb")
                                            {
                                                cursyn.StrFrame(false);
                                            }

                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            //												senses.Add(cursyn);
                                            break;
                                        case "NOMINALIZATIONS": // 26/8/05 - changed "DERIVATION" to "NOMINALIZATIONS" - this needs to be verified
                                                                // derivation - TDMS
                                            cursyn.Tracenomins(sch.ptp);
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;
                                        //WN3.0
                                        case "PERTPTR":
                                            cursyn.Strsns(sense + 1);
                                            prflag = true;
                                            cursyn.TracePtrs(PointerType.Of("PERTPTR"), pos, depth);
                                            break;

                                        case "CLASSIFICATION":
                                        case "CLASS":
                                            //eg. search for 'car', select Noun -> 'Domain Terms'
                                            cursyn.Traceclassif(sch.ptp, new SearchType(false, sch.ptp));
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;

                                        case "HYPOPTR":
                                            //eg. search for 'car', select Noun -> 'Hyponyms'
                                            cursyn.TracePtrs(sch.ptp, pos, depth);
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;

                                        default:
                                            cursyn.TracePtrs(sch.ptp, pos, depth);
                                            if (cursyn.isDirty) // TDMS 25 Oct 2005 - restrict to relevant values
                                            {
                                                senses.Add(cursyn);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public void WordsFrom(SynonymSet s)
        {
            for (int j = 0; j < s.words.Length; j++)
            {
                Lexeme lx = s.words[j];
                lexemes[lx] = true;
            }
        }

        // TDMS - relatives - synonyms of verb - grouped by similarity of meaning
        private void Relatives(Index idx)
        {
            RelationalList rellist = null;
            switch (pos.Key)
            {
                case "verb":
                    rellist = FindVerbGroups(idx, rellist);
                    DoRelList(idx, rellist);
                    break;
            }
        }

        private RelationalList FindVerbGroups(Index idx, RelationalList rellist)
        {
            int i, j, k;
            /* Read all senses */
            for (i = 0; i < idx.offs.Length; i++)
            {
                SynonymSet synset = new SynonymSet(idx.offs[i], pos, idx.wd, this, i, netData);
                /* Look for VERBGROUP ptr(s) for this sense.  If found,
				   create group for senses, or add to existing group. */
                for (j = 0; j < synset.ptrs.Length; j++)
                {
                    Pointer p = synset.ptrs[j];
                    if (p.ptp.Mnemonic == "VERBGROUP")
                    {
                        /* Need to find sense number for ptr offset */
                        for (k = 0; k < idx.offs.Length; k++)
                        {
                            if (p.off == idx.offs[k])
                            {
                                rellist = AddRelatives(idx, i, k, rellist);
                                break;
                            }
                        }
                    }
                }
            }
            return rellist;
        }

        private RelationalList AddRelatives(Index idx, int rel1, int rel2, RelationalList rellist)
        {
            /* If either of the new relatives are already in a relative group,
			   then add the other to the existing group (transitivity).
				   Otherwise create a new group and add these 2 senses to it. */
            RelationalList rel, last = null;
            for (rel = rellist; rel != null; rel = rel.next)
            {
                if (rel.senses[rel1] || rel.senses[rel2])
                {
                    rel.senses[rel1] = rel.senses[rel2] = true;
                    /* If part of another relative group, merge the groups */
                    for (RelationalList r = rellist; r != null; r = r.next)
                    {
                        if (r != rel && r.senses[rel1] || r.senses[rel2])
                        {
                            rel.senses = rel.senses.Or(r.senses);
                        }
                    }

                    return rellist;
                }
                last = rel;
            }
            rel = new RelationalList();
            rel.senses[rel1] = rel.senses[rel2] = true;
            if (rellist == null)
            {
                return rel;
            }

            last.next = rel;
            return rellist;
        }

        private void DoRelList(Index idx, RelationalList rellist)
        {
            int i;
            bool flag;
            SynonymSet synptr;
            BitSet outsenses = new BitSet(300);
            prflag = true;
            for (RelationalList rel = rellist; rel != null; rel = rel.next)
            {
                flag = false;
                for (i = 0; i < idx.offs.Length; i++)
                {
                    if (rel.senses[i] && !outsenses[i])
                    {
                        flag = true;
                        synptr = new SynonymSet(idx.offs[i], pos, "", this, i, netData);
                        synptr.Strsns(i + 1);
                        synptr.TracePtrs(PointerType.Of("HYPERPTR"), pos, 0);
                        synptr.frames.Clear(); // TDMS 03 Jul 2006 - frames get added in wordnet.cs after filtering
                                               // TDMS 11 Oct 2005 - build hierarchical results
                        senses.Add(synptr);
                        outsenses[i] = true;
                    }
                }

                if (flag)
                {
                    buf += "--------------\n";
                }
            }
            for (i = 0; i < idx.offs.Length; i++)
            {
                if (!outsenses[i])
                {
                    synptr = new SynonymSet(idx.offs[i], pos, "", this, i, netData);
                    synptr.Strsns(i + 1);
                    synptr.TracePtrs(PointerType.Of("HYPERPTR"), pos, 0);
                    synptr.frames.Clear(); // TDMS 03 Jul 2006 - frames get added in wordnet.cs after filtering
                                           // TDMS 11 Oct 2005 - build hierarchical results
                    senses.Add(synptr);
                    buf += "---------------\n";
                }
            }
        }

        private void WNOverview()
        {
            Index idx;
            //senses = new ArrayList();
            senses = new List<SynonymSet>();
            Indexes ixs = new Indexes(word, pos, netData);
            while ((idx = ixs.Next()) != null)
            {
                buf += "\n";
                /* Print synset for each sense.  If requested, precede
				   synset with synset offset and/or lexical file information.*/
                for (int sens = 0; sens < idx.offs.Length; sens++)
                {
                    var skipToEnd = false;
                    for (int j = 0; j < senses.Count; j++)
                    {
                        SynonymSet ss = senses[j];
                        if (ss.hereiam == idx.offs[sens])
                        {
                            skipToEnd = true;
                            break;
                        }
                    }
                    if (!skipToEnd)
                    {
                        SynonymSet cursyn = new SynonymSet(idx, sens, this, netData);

                        bool svdflag = WordNetOption.Opt("-g").flag;
                        WordNetOption.Opt("-g").flag = true;
                        bool svaflag = WordNetOption.Opt("-a").flag;
                        WordNetOption.Opt("-a").flag = WordNetOption.Opt("-A").flag;
                        bool svoflag = WordNetOption.Opt("-o").flag;
                        WordNetOption.Opt("-o").flag = WordNetOption.Opt("-O").flag;

                        cursyn.Str("" + (sens + 1) + ". ", "\n", 1, 0, 0, 0);

                        WordNetOption.Opt("-g").flag = svdflag;
                        WordNetOption.Opt("-a").flag = svaflag;
                        WordNetOption.Opt("-o").flag = svoflag;
                        WordsFrom(cursyn);
                        cursyn.frames.Clear(); // TDMS 03 Jul 2006 - frames get added in wordnet.cs after filtering
                        senses.Add(cursyn);
                    }
                }
                /* Print sense summary message */
                if (senses.Count > 0)
                {
                    buf += string.Format("\nThe {0} {1} has {2} sense{3}", pos.Key, idx.wd, senses.Count, senses.Count > 1 ? "s" : "");

                    taggedSenses = 0;
                    if (idx.tagsense_cnt > 0)
                    {
                        taggedSenses = idx.tagsense_cnt;
                        buf += " (first " + idx.tagsense_cnt + " from tagged texts)\n";
                    }
                    else
                    {
                        buf += " (no senses from tagged texts)\n";
                    }
                }
            }
        }
    }
}
