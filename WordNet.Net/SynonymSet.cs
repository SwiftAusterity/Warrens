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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using WordNet.Net.Searching;
using WordNet.Net.WordNet;

namespace WordNet.Net
{
	/// <summary>
	/// Summary description for SynSet.
	/// </summary>
	[Serializable]
	public class SynonymSet
	{
		/* directly correlates to classinit in util.cs */
		// This should remain a duplicated list to the one
		// in util.cs for performance reasons.  Using a hashtable
		// in loops will slow searches down.
		private const int ANTPTR = 1;	/* ! */
		private const int HYPERPTR = 2;	/* @ */
		private const int HYPOPTR = 3;	/* ~ */
		private const int ENTAILPTR = 4;	/* * */
		private const int SIMPTR = 5;	/* & */
		private const int ISMEMBERPTR = 6; /* #m */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int ISSTUFFPTR = 7; /* #s */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int ISPARTPTR = 8; /* #p */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int HASMEMBERPTR = 9; /* %m */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int HASSTUFFPTR = 10; /* %s */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int HASPARTPTR = 11; /* %p */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int MERONYM = 12; /* % */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int HOLONYM = 13; /* # */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int CAUSETO = 14; /* > */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int PPLPTR = 15; /* < */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int SEEALSOPTR = 16; /* ^ */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int PERTPTR = 17; /* \\ */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int ATTRIBUTE = 18; /* = */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int VERBGROUP = 19; /* $ */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int NOMINALIZATIONS = 20; /* + */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int CLASSIFICATION = 21; /* ; */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int CLASS = 22;	/* - */
		private const int SYNS = 23; /* */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int FREQ = 24; /* */ // TDMS 11 JUL 2006 - Added new ident matcher
		// TODO: FRAMES symbol is the same as NOMINALIZATIONS - check
		private const int FRAMES = 25; /* + */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int COORDS = 26; /* */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int RELATIVES = 27; /* */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int HMERONYM = 28; /* */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int HHOLONYM = 29; /* */ // TDMS 11 JUL 2006 - Added new ident matcher
		private const int WNGREP = 30; /* */ // TDMS 11 JUL 2006 - Added new ident matcher

		/* Additional searches, but not pointers.  */
		private const int LASTTYPE = CLASS;
		private const int OVERVIEW = LASTTYPE + 9;
		private const int MAXSEARCH = OVERVIEW;
		private const int CLASSIF_START = MAXSEARCH + 1;
		private const int CLASSIF_REGIONAL = CLASSIF_START + 2;    // ;r
		private const int CLASSIF_END = CLASSIF_REGIONAL;
		private const int CLASS_START = CLASSIF_END + 1;
		private const int CLASS_REGIONAL = CLASS_START + 2;      // -r
		private const int CLASS_END = CLASS_REGIONAL;
		private const int INSTANCE = CLASS_END + 1;        // @i
		private const int INSTANCES = CLASS_END + 2;        // ~i

		public bool isDirty = false;

		public int hereiam;
		public int fnum;
		public PartOfSpeech pos;	/* part of speech */
		public Lexeme[] words;		/* words in synset */
		public int whichword = 0;     /* 1.. of the words array */
		public AdjSynSetType sstype;
		public int sense; // "global" variable: will match search.sense-1 if this is nonzero
		public IList<SynonymSet> senses = null; // of SynSet (creates our hierarchy) - TDMS 6 Oct 2005
        private Search search;
		public Pointer[] ptrs;		/* number of pointers */
		public Pointer thisptr; // the current pointertype - TDMS 17 Nov 2005
		public ArrayList frames = new ArrayList(); /* of SynSetFrame */
		public string defn;		/* synset gloss (definition) */
		public AdjMarker adj_marker;

		public SynonymSet()
		{
		} // for serialization

		public SynonymSet(int offset, PartOfSpeech p, string wd, Search sch, int sens)
		{
			pos = p;
			hereiam = offset;
			search = sch;
			sense = sens;
			StreamReader f = WordNetData.Data(p);
			f.DiscardBufferedData();
			f.BaseStream.Position = offset;
			string rec = f.ReadLine();
			if (!rec.StartsWith(offset.ToString("D8")))
			{
				Console.WriteLine("Error reading " + p.Key + " file! " + offset + ": " + rec);
				WordNetData.Reopen(p);
				f = WordNetData.Data(p);
				f.DiscardBufferedData();
				f.BaseStream.Position = offset;
				rec = f.ReadLine();
			}
			Parse(rec, pos, wd);
		}

		public SynonymSet(int off, PartOfSpeech p, string wd, SynonymSet fr)
			: this(off, p, wd, fr.search, fr.sense)
		{
		}

		public SynonymSet(Index idx, int sens, Search sch)
			: this(idx.offs[sens], idx.pos, idx.wd, sch, sens)
		{
		}

		public SynonymSet(int off, PartOfSpeech p, SynonymSet fr)
			: this(off, p, "", fr)
		{
		}

        private void Parse(string s, PartOfSpeech fpos, string word)
		{
			int j;
			var st = s.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            var stI = 0;
			int off = int.Parse(st[stI++]);
			fnum = int.Parse(st[stI++]);
			string f = st[stI++];
			PartOfSpeech pos = PartOfSpeech.Of(f);
			if (pos.Clss == "SATELLITE")
            {
                sstype = AdjSynSetType.IndirectAnt;
            }

            int wcnt = int.Parse(st[stI++], NumberStyles.HexNumber);
			words = new Lexeme[wcnt];
			for (j = 0; j < wcnt; j++)
			{
                words[j] = new Lexeme
                {
                    word = st[stI++],
                    uniq = int.Parse(st[stI++], NumberStyles.HexNumber)
                };

                // Thanh Dao 7 Nov 2005 - Added missing word sense values
                int ss = Getsearchsense(j + 1);
				words[j].wnsns = ss;

				if (words[j].word.ToLower() == word)
                {
                    whichword = j + 1;
                }
            }
			int pcnt = int.Parse(st[stI++]);
			ptrs = new Pointer[pcnt];
			for (j = 0; j < pcnt; j++)
			{
				string p = st[stI++];
				ptrs[j] = new Pointer(p);
				if (fpos.Key == "adj" && sstype == AdjSynSetType.DontKnow)
				{
					if (ptrs[j].ptp.Ident == ANTPTR) // TDMS 11 JUL 2006 - change comparison to int //.mnemonic=="ANTPTR")
                    {
                        sstype = AdjSynSetType.DirectAnt;
                    }
                    else if (ptrs[j].ptp.Ident == PERTPTR) // TDMS 11 JUL 2006 - change comparison to int //mnemonic=="PERTPTR")
                    {
                        sstype = AdjSynSetType.Pertainym;
                    }
                }
				ptrs[j].off = int.Parse(st[stI++]);
				ptrs[j].pos = PartOfSpeech.Of(st[stI++]);
				int sx = int.Parse(st[stI++], NumberStyles.HexNumber);
				ptrs[j].sce = sx >> 8;
				ptrs[j].dst = sx & 0xff;
			}
			f = st[stI++];
			if (f != "|")
			{
				int fcnt = int.Parse(f);
				for (j = 0; j < fcnt; j++)
				{
					f = st[stI++]; // +
					Usage fr = Usage.Frame(int.Parse(st[stI++]));
					frames.Add(new SynSetFrame(fr, int.Parse(st[stI++], NumberStyles.HexNumber)));
				}
				f = st[stI++];
			}
			defn = s.Substring(s.IndexOf('|') + 1);
		}

		internal bool Has(PointerType p)
		{
			for (int i = 0; i < ptrs.Length; i++)
            {
                if (ptrs[i].ptp == p)
                {
                    return true;
                }
            }

            return false;
		}

		internal void TracePtrs(PointerType ptp, PartOfSpeech p, int depth)
		{
			TracePtrs(new SearchType(false, ptp), p, depth);
		}

		/// <summary>
		/// Traces pointer hierarchy.
		/// </summary>
		/// <param name="stp"></param>
		/// <param name="fpos"></param>
		/// <param name="depth"></param>
		internal void TracePtrs(SearchType stp, PartOfSpeech fpos, int depth)
		{
			int i;
			SynonymSet cursyn;
			PointerType ptp = stp.ptp;
			string prefix;
			int realptr; // WN2.1

			for (i = 0; i < ptrs.Length; i++)
			{
				Pointer pt = ptrs[i];
				// following if statement is WN2.1 - TDMS
				if ((ptp.Ident == HYPERPTR && (pt.ptp.Ident == HYPERPTR ||
					pt.ptp.Ident == INSTANCE)) ||
					(ptp.Ident == HYPOPTR && (pt.ptp.Ident == HYPOPTR ||
					pt.ptp.Ident == INSTANCES)) ||
					((pt.ptp == ptp) &&
					((pt.sce == 0) ||
					(pt.sce == whichword))))
				{
					realptr = pt.ptp.Ident; /* WN2.1 deal with INSTANCE */
					if (!search.prflag) // print sense number and synset
                    {
                        Strsns(sense + 1);
                    }

                    search.prflag = true;
					Spaces("TRACEP", depth + (stp.rec ? 2 : 0));

                    switch (pt.ptp.Ident) // TDMS 11 JUL 2006 - changed switch to ident
					{
						case PERTPTR:
							if (fpos.Key == "adv") // TDMS "adverb")
                            {
                                prefix = "Derived from ";
                            }
                            else
                            {
                                prefix = "Pertains to ";
                            }

                            prefix += pt.pos.Key + " ";

                            break;
						case ANTPTR: // TDMS 26/8/05
							if (fpos.Key == "adj") //TODO: which adjective will fall into the below?
                            {
                                prefix = "Antonym of ";
                            }
                            else
                            {
                                prefix = "";
                            }

                            break;
						case PPLPTR:
							prefix = "Participle of verb";
							break;
						case INSTANCE:
							prefix = "INSTANCE OF=> ";
							break;
						case INSTANCES:
							prefix = "HAS INSTANCE=> ";
							break;
						case HASMEMBERPTR:
							prefix = "   HAS MEMBER: ";
							break;
						case HASSTUFFPTR:
							prefix = "   HAS SUBSTANCE: ";
							break;
						case HASPARTPTR:
							prefix = "   HAS PART:  ";
							break;
						case ISMEMBERPTR:
							prefix = "   MEMBER OF:  ";
							break;
						case ISSTUFFPTR: // TDMS 26/8/05
							prefix = "   SUBSTANCE OF: ";
							break;
						case ISPARTPTR: // TDMS 26/8/05
							prefix = "   PART OF: ";
							break;
						default:
							prefix = "=> ";
							break;
					}

					/* Read synset pointed to */
					cursyn = new SynonymSet(pt.off, pt.pos, this);
					search.WordsFrom(cursyn);

					// TDMS 6 Oct 2005 - build hierarchical results

					if (senses == null)
                    {
                        senses = new List<SynonymSet>();
                    }

                    cursyn.thisptr = pt;  // TDMS 17 Nov 2005 - add this pointer type
					senses.Add(cursyn);

					/* For Pertainyms and Participles pointing to a specific
					   sense, indicate the sense then retrieve the synset
					   pointed to and other info as determined by type.
					   Otherwise, just print the synset pointed to. */
					if ((ptp.Ident == PERTPTR || ptp.Ident == PPLPTR) &&
						pt.dst != 0)
					{
						string tbuf = " (Sense " + cursyn.Getsearchsense(pt.dst) + ")";
						cursyn.Str(prefix, tbuf, 0, pt.dst, 0, 1);
						if (ptp.Ident == PPLPTR) // adj pointing to verb
						{
							cursyn.Str("     =>", "\n", 1, 0, 1, 1);
							cursyn.TracePtrs(PointerType.Of("HYPERPTR"), cursyn.pos, 0);
						}
						else if (fpos.Key == "adv") // adverb pointing to adjective
						{
							cursyn.Str("     =>", "\n", 0, 0, (pos.Clss == "SATELLITE") ? 0 : 1, 1);
						}
						else  // adjective pointing to noun
						{
							cursyn.Str("     =>", "\n", 1, 0, 1, 1);
							cursyn.TracePtrs(PointerType.Of("HYPERPTR"), pos, 0);
						}
					}
					else
                    {
                        cursyn.Str(prefix, "\n", 1, 0, 1, 1);
                    }

                    /* For HOLONYMS and MERONYMS, keep track of last one printed in buffer so results can be truncated later. */
                    if (ptp.Ident >= PointerType.Of("ISMEMBERPTR").Ident &&
						ptp.Ident <= PointerType.Of("HASPARTPTR").Ident)
					{
						search.Mark();
					}

					if (depth > 0)
					{
						depth = cursyn.Depthcheck(depth);
						cursyn.TracePtrs(ptp, cursyn.pos, depth + 1);
					}
				}
			}
		}

		/// <summary>
		/// Trace coordinate terms.
		/// </summary>
		/// <param name="ptp"></param>
		/// <param name="fpos"></param>
		/// <param name="depth"></param>
		internal void TraceCoords(PointerType ptp, PartOfSpeech fpos, int depth)
		{
			for (int i = 0; i < ptrs.Length; i++)
			{
				Pointer pt = ptrs[i];

				// WN2.1 if statement change - TDMS
				if ((pt.ptp.Ident == HYPERPTR || pt.ptp.Ident == INSTANCE) &&
					((pt.sce == 0) ||
					(pt.sce == whichword)))
				{
					if (!search.prflag)
					{
						Strsns(sense + 1);
						search.prflag = true;
					}
					Spaces("TRACEC", depth);
					SynonymSet cursyn = new SynonymSet(pt.off, pt.pos, this);
					search.WordsFrom(cursyn);
					cursyn.Str("-> ", "\n", 1, 0, 0, 1);
					cursyn.TracePtrs(ptp, cursyn.pos, depth);
					// TDMS 6 Oct 2005 - build hierarchical results
					if (senses == null)
                    {
                        senses = new List<SynonymSet>();
                    }

                    cursyn.thisptr = pt;  // TDMS 17 Nov 2005 - add this pointer type
					senses.Add(cursyn);

					if (depth > 0)
					{
						depth = Depthcheck(depth);
						cursyn.TraceCoords(ptp, cursyn.pos, depth + 1);
						// TDMS 6 Oct 2005 - build hierarchical results
						// TODO: verify this
						if (senses == null)
                        {
                            senses = new List<SynonymSet>();
                        }

                        cursyn.thisptr = pt;  // TDMS 17 Nov 2005 - add this pointer type
						senses.Add(cursyn);
					}
				}
			}
		}

		/// <summary>
		/// Trace classification.
		/// </summary>
		/// <param name="ptp"></param>
		/// <param name="stp"></param>
		internal void Traceclassif(PointerType ptp, SearchType stp) //,PartOfSpeech fpos)
		{
			int j;
			int idx = 0;
			string head = "";
			int LASTTYPE = PointerType.Of("CLASS").Ident;
			int OVERVIEW = LASTTYPE + 9;
			int MAXSEARCH = OVERVIEW;
			int CLASSIF_START = MAXSEARCH + 1;
			int CLASSIF_CATEGORY = CLASSIF_START;        /* ;c */
			int CLASSIF_USAGE = CLASSIF_START + 1;    /* ;u */
			int CLASSIF_REGIONAL = CLASSIF_START + 2;    /* ;r */
			int CLASSIF_END = CLASSIF_REGIONAL;
			int CLASS_START = CLASSIF_END + 1;
			int CLASS_CATEGORY = CLASS_START;          /* -c */
			int CLASS_USAGE = CLASS_START + 1;      /* -u */
			int CLASS_REGIONAL = CLASS_START + 2;      /* -r */
			int CLASS_END = CLASS_REGIONAL;
			ArrayList prlist = new ArrayList();

			for (int i = 0; i < ptrs.Length; i++)
			{
				Pointer pt = ptrs[i];
				if (((pt.ptp.Ident >= CLASSIF_START) &&
					(pt.ptp.Ident <= CLASSIF_END) && stp.ptp.Ident == PointerType.Of("CLASSIFICATION").Ident) ||

					((pt.ptp.Ident >= CLASS_START) &&
					(pt.ptp.Ident <= CLASS_END) && stp.ptp.Ident == PointerType.Of("CLASS").Ident))
				{
					if (!search.prflag)
					{
						Strsns(sense + 1);
						search.prflag = true;
					}

					SynonymSet cursyn = new SynonymSet(pt.off, pt.pos, this);
					// TDMS 6 Oct 2005 - build hierarchical results
					// TODO: verify this
					if (senses == null)
                    {
                        senses = new List<SynonymSet>();
                    }

                    cursyn.thisptr = pt;  // TDMS 17 Nov 2005 - add this pointer type
					senses.Add(cursyn);

					for (j = 0; j < idx; j++)
					{
						if (pt.off == Convert.ToInt16(prlist[j]))
						{
							break;
						}
					}

					if (j == idx)
					{
						prlist.Add(pt.off);
						Spaces("TRACEP", 0);

						if (pt.ptp.Ident == CLASSIF_CATEGORY)
                        {
                            head = "TOPIC->("; // WN2.1 - TDMS
                        }
                        else if (pt.ptp.Ident == CLASSIF_USAGE)
                        {
                            head = "USAGE->(";
                        }
                        else if (pt.ptp.Ident == CLASSIF_REGIONAL)
                        {
                            head = "REGION->(";
                        }
                        else if (pt.ptp.Ident == CLASS_CATEGORY)
                        {
                            head = "TOPIC_TERM->("; // WN2.1 - TDMS
                        }
                        else if (pt.ptp.Ident == CLASS_USAGE)
                        {
                            head = "USAGE_TERM->(";
                        }
                        else if (pt.ptp.Ident == CLASS_REGIONAL)
                        {
                            head = "REGION_TERM->(";
                        }

                        head += pt.pos.Key;
						head += ") ";
						cursyn.Str(head, "\n", 0, 0, 0, 0);
					}
				}
			}
		}

		/// <summary>
		/// Trace nominalizations.
		/// </summary>
		/// <param name="ptp"></param>
		internal void Tracenomins(PointerType ptp) //,PartOfSpeech fpos)
		{
			for (int i = 0; i < ptrs.Length; i++)
			{
				Pointer pt = ptrs[i];
				// TDMS 26/8/05 changed DERIVATION to NOMINALIZATIONS - verify this
				if (pt.ptp.Ident == NOMINALIZATIONS && (pt.sce == 0 || pt.sce == whichword))
				{
					if (!search.prflag)
					{
						Strsns(sense + 1);
						search.prflag = true;
					}
					Spaces("TRACEP", 0);
					SynonymSet cursyn = new SynonymSet(pt.off, pt.pos, this);
					search.WordsFrom(cursyn);
					cursyn.Str("RELATED TO-> ", "\n", 0, 0, 0, 0);
					// TDMS 6 Oct 2005 - build hierarchical results
					// TODO: verify this
					if (senses == null)
                    {
                        senses = new List<SynonymSet>();
                    }

                    cursyn.thisptr = pt;  // TDMS 17 Nov 2005 - add this pointer type
					senses.Add(cursyn);

					cursyn.TracePtrs(ptp, cursyn.pos, 0);
				}
			}
		}

        /// <summary>
        /// Trace meronyms.
        /// </summary>
        /// <param name="pbase"></param>
        /// <param name="fpos"></param>
        /// <param name="depth"></param>
        private void TraceInherit(PointerType pbase, PartOfSpeech fpos, int depth)
		{
			for (int i = 0; i < ptrs.Length; i++)
			{
				Pointer pt = ptrs[i];
				if (pt.ptp.Ident == HYPERPTR && (pt.sce == 0 || pt.sce == whichword))
				{
					Spaces("TRACEI", depth);
					SynonymSet cursyn = new SynonymSet(pt.off, pt.pos, this);
					search.WordsFrom(cursyn);
					cursyn.Str("=> ", "\n", 1, 0, 0, 1);
					// TDMS 6 Oct 2005 - build hierarchical results
					// TODO: verify this
					if (senses == null)
                    {
                        senses = new List<SynonymSet>();
                    }

                    cursyn.thisptr = pt;  // TDMS 17 Nov 2005 - add this pointer type
					// TODO: This is adding senses incorrectly
					senses.Add(cursyn);

					cursyn.TracePtrs(pbase, PartOfSpeech.Of("noun"), depth);
					cursyn.TracePtrs(pbase + 1, PartOfSpeech.Of("noun"), depth);
					cursyn.TracePtrs(pbase + 2, PartOfSpeech.Of("noun"), depth);

					if (depth > 0)
					{
						depth = Depthcheck(depth);
						cursyn.TraceInherit(pbase, cursyn.pos, depth + 1);
					}
				}
			}
			search.Trunc();
		}

		/// <summary>
		/// Trace adjective antonyms.
		/// </summary>
		internal void TraceAdjAnt()
		{
			SynonymSet newsynptr;
			int i, j;
			AdjSynSetType anttype = AdjSynSetType.DirectAnt;
			SynonymSet simptr, antptr;
			string similar = "        => ";

			/* This search is only applicable for ADJ synsets which have
			   either direct or indirect antonyms (not valid for pertainyms). */
			if (sstype == AdjSynSetType.DirectAnt || sstype == AdjSynSetType.IndirectAnt)
			{
				Strsns(sense + 1);
				search.buf += "\n";
				/* if indirect, get cluster head */
				if (sstype == AdjSynSetType.IndirectAnt)
				{
					anttype = AdjSynSetType.IndirectAnt;

					i = 0;
					while (ptrs[i].ptp.Ident != SIMPTR)
                    {
                        i++;
                    }

                    newsynptr = new SynonymSet(ptrs[i].off, PartOfSpeech.Of("adj"), this);
				}
				else
                {
                    newsynptr = this;
                }
                /* find antonyms - if direct, make sure that the antonym ptr we're looking at is from this word */
                for (i = 0; i < newsynptr.ptrs.Length; i++)
				{
					if (newsynptr.ptrs[i].ptp.Ident == ANTPTR && // TDMS 11 JUL 2006 // mnemonic=="ANTPTR" &&
						((anttype == AdjSynSetType.DirectAnt &&
						newsynptr.ptrs[i].sce == newsynptr.whichword) ||
						anttype == AdjSynSetType.IndirectAnt))
					{
						/* read the antonym's synset and print it.  if a direct antonym, print it's satellites. */
						antptr = new SynonymSet(newsynptr.ptrs[i].off, PartOfSpeech.Of("adj"), this);
						search.WordsFrom(antptr);
						// TDMS 6 Oct 2005 - build hierarchical results
						if (senses == null)
                        {
                            senses = new List<SynonymSet>();
                        }
                        //TODO: check the ptrs reference
                        antptr.thisptr = newsynptr.ptrs[i];  // TDMS 17 Nov 2005 - add this pointer type
						senses.Add(antptr);
						if (anttype == AdjSynSetType.DirectAnt)
						{
							antptr.Str("", "\n", 1, 0, 1, 1);
							for (j = 0; j < antptr.ptrs.Length; j++)
                            {
                                if (antptr.ptrs[j].ptp.Ident == SIMPTR) // TDMS 11 JUL 2006 - changed to INT //.mnemonic=="SIMPTR")
								{
									simptr = new SynonymSet(antptr.ptrs[j].off, PartOfSpeech.Of("adj"), this);
									search.WordsFrom(simptr);
									simptr.Str(similar, "\n", 1, 0, 0, 1);
									// TDMS 6 Oct 2005 - build hierarchical results
									if (antptr.senses == null)
                                    {
                                        antptr.senses = new List<SynonymSet>();
                                    }

                                    antptr.senses.Add(simptr);
								}
                            }
                        }
						else
                        {
                            antptr.StrAnt("\n", anttype, 1);
                        }
                    }
				}
			}
		}

        private void Spaces(string trace, int n)
		{
			for (int j = 0; j < n; j++)
            {
                search.buf += "     ";
            }

            switch (trace)
			{
				case "TRACEP": // traceptrs
					if (n > 0)
                    {
                        search.buf += "   ";
                    }
                    else
                    {
                        search.buf += "       ";
                    }

                    break;
				case "TRACEC": // tracecoords
					if (n == 0)
                    {
                        search.buf += "    ";
                    }

                    break;
				case "TRACEI": // traceinherit
					if (n == 0)
                    {
                        search.buf += "\n    ";
                    }

                    break;
			}
		}

		public void Print(string head, string tail, int definition, int wdnum, int antflag, int markerflag)
		{
			string keep = search.buf;
			search.buf = "";
			Str(head, tail, definition, wdnum, antflag, markerflag);
			Console.Write(search.buf);
			search.buf = keep;
		}

		internal void Str(string head, string tail, int definition, int wdnum, int antflag, int markerflag)
		{
			int i, wdcnt;
			search.buf += head;
			/* Precede synset with additional information as indicated
			   by flags */
			if (WordNetOption.Opt("-o").flag)
            {
                search.buf += "(" + hereiam + ") ";
            }

            search.prlexid = WordNetOption.Opt("-a").flag;
            if (search.prlexid)
			{
				search.buf += "<" + WordNetData.lexfiles[fnum] + "> ";
			}

            if (wdnum > 0)
            {
                Catword(wdnum - 1, markerflag, antflag);
            }
            else
            {
                for (i = 0, wdcnt = words.Length; i < wdcnt; i++)
				{
					Catword(i, markerflag, antflag);
					if (i < wdcnt - 1)
                    {
                        search.buf += ", ";
                    }
                }
            }

            if (definition != 0 && WordNetOption.Opt("-g").flag && defn != null)
			{
				search.buf += " -- " + defn;

				isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
				// populates buf to the logic that defines whether the 
				// synset is populated with relevant information
			}

			search.buf += tail;
		}

        private void StrAnt(string tail, AdjSynSetType attype, int definition)
		{
			int i, wdcnt;
			bool first = true;
			if (WordNetOption.Opt("-o").flag)
            {
                search.buf += "(" + hereiam + ") ";
            }

            search.prlexid = WordNetOption.Opt("-a").flag;
            if (search.prlexid)
			{
				search.buf += "<" + WordNetData.lexfiles[fnum] + "> ";
				search.prlexid = true;
			}

            /* print antonyms from cluster head (of indirect ant) */
            search.buf += "INDIRECT (VIA ";
			for (i = 0, wdcnt = words.Length; i < wdcnt; i++)
			{
				if (first)
				{
					StrAnt(PartOfSpeech.Of("adj"), i + 1, "", ", ");
					first = false;
				}
				else
                {
                    StrAnt(PartOfSpeech.Of("adj"), i + 1, ", ", ", ");
                }
            }
			search.buf += ") -> ";

			/* now print synonyms from cluster head (of indirect ant) */
			for (i = 0, wdcnt = words.Length; i < wdcnt; i++)
			{
				Catword(i, 0, 0);
				if (i < wdcnt - 1)
                {
                    search.buf += ", ";
                }
            }

			if (WordNetOption.Opt("-g").flag && defn != null && definition != 0)
			{
				search.buf += " -- " + defn;

				isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
				// populates buf to the logic that defines whether the 
				// synset is populated with relevant information
			}
			search.buf += tail;
		}

        private void Catword(int wdnum, int adjmarker, int antflag)
		{
			search.buf += Deadjify(words[wdnum].word);
			/* Print additional lexicographer information and WordNet sense
			   number as indicated by flags */
			if (words[wdnum].uniq != 0)
			{
				search.buf += "" + words[wdnum].uniq;
				isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
				// populates buf to the logic that defines whether the 
				// synset is populated with relevant information
			}

			int s = Getsearchsense(wdnum + 1);
			words[wdnum].wnsns = s;
			if (WordNetOption.Opt("-s").flag)
			{
				search.buf += "#" + s;
				isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
				// populates buf to the logic that defines whether the 
				// synset is populated with relevant information
			}
			/* For adjectives, append adjective marker if present, and
			   print antonym if flag is passed */
			if (pos.Key == "adj")
			{
				if (adjmarker > 0)
				{
					search.buf += "" + adj_marker.mark;
					isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
					// populates buf to the logic that defines whether the 
					// synset is populated with relevant information
				}
				if (antflag > 0)
                {
                    StrAnt(PartOfSpeech.Of("adj"), wdnum + 1, "(vs. ", ")");
                }
            }
		}

		internal void StrAnt(PartOfSpeech pos, int wdnum, string head, string tail)
		{
			int i, j, wdoff;

			/* Go through all the pointers looking for anotnyms from the word
			   indicated by wdnum.  When found, print all the antonym's
			   antonym pointers which point back to wdnum. */
			for (i = 0; i < ptrs.Length; i++)
			{
				Pointer pt = ptrs[i];
				if (pt.ptp.Ident == ANTPTR && pt.sce == wdnum)
				{
					SynonymSet psyn = new SynonymSet(pt.off, pos, this);
					for (j = 0; j < psyn.ptrs.Length; j++)
					{
						Pointer ppt = psyn.ptrs[j];
						if (ppt.ptp.Ident == ANTPTR &&
							ppt.dst == wdnum &&
							ppt.off == hereiam)
						{
							wdoff = ppt.sce > 0 ? ppt.sce - 1 : 0;
							search.buf += head;
							/* Construct buffer containing formatted antonym,
							   then add it onto end of return buffer */
							search.buf += Deadjify(psyn.words[wdoff].word);
							/* Print additional lexicographer information and
							   WordNet sense number as indicated by flags */
							isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
							// populates buf to the logic that defines whether the 
							// synset is populated with relevant information

							if (search.prlexid && psyn.words[wdoff].uniq != 0)
                            {
                                search.buf += psyn.words[wdoff].uniq;
                            }

                            int s = Getsearchsense(wdoff + 1);
							psyn.words[wdoff].wnsns = s;
							if (WordNetOption.Opt("-s").flag)
							{
								search.buf += "#" + s;
								isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
								// populates buf to the logic that defines whether the 
								// synset is populated with relevant information
							}
							search.buf += tail;
						}
					}
				}
			}
		}

        private string Deadjify(string word)
		{
			string tmpword = word + " ";
			adj_marker = AdjMarker.Of("UNKNOWN_MARKER");
			for (int j = 0; j < word.Length; j++)
            {
                if (word[j] == '(')
				{
					if (tmpword.Substring(j, 3) == "(a)")
                    {
                        adj_marker = AdjMarker.Of("ATTRIBUTIVE");
                    }
                    else if (tmpword.Substring(j, 4) == "(ip)")
                    {
                        adj_marker = AdjMarker.Of("IMMED_POSTNOMINAL");
                    }
                    else if (tmpword.Substring(j, 3) == "(p)")
                    {
                        adj_marker = AdjMarker.Of("PREDICATIVE");
                    }

                    return word.Substring(0, j);
				}
            }

            return word;
		}

		internal void Strsns(int sense)
		{
			Strsense(sense);
			Str("", "\n", 1, 0, 1, 1);
		}

        private void Strsense(int sense)
		{
			/* Append lexicographer filename after Sense # if flag is set. */
			if (WordNetOption.Opt("-a").flag)
            {
                search.buf += "\nSense " + sense + " in file \"" + WordNetData.lexfiles[fnum] + "\"\n";
            }
            else
            {
                search.buf += "\nSense " + sense + "\n";
            }

            isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
			// populates buf to the logic that defines whether the 
			// synset is populated with relevant information
		}

		internal int Depthcheck(int depth)
		{
            return depth >= 20 ? -1 : depth;
		}

		internal void PartsAll(PointerType ptp)
		{
			int hasptr = 0;
			PointerType ptrbase = PointerType.Of((ptp.Ident == HMERONYM) ? "HASMEMBERPTR" : "ISMEMBERPTR");
			/* First, print out the MEMBER, STUFF, PART info for this synset */
			for (int i = 0; i < 3; i++)
            {
                if (Has(ptrbase + i))
				{
					TracePtrs(ptrbase + i, PartOfSpeech.Of("noun"), i);
					hasptr++;
				}
            }
            /* Print out MEMBER, STUFF, PART info for hypernyms on HMERONYM search only */
            if (hasptr > 0 && ptp.Ident == HMERONYM)
			{
				search.Mark();

				TraceInherit(ptrbase, PartOfSpeech.Of("noun"), 1);
			}
		}

		internal void StrFrame(bool prsynset)
		{
			int i;
			if (prsynset)
            {
                Strsns(sense + 1);
            }

            if (!FindExample())
            {
                for (i = 0; i < frames.Count; i++)
				{
					SynSetFrame sf = (SynSetFrame)frames[i];
					if (sf.to == whichword || sf.to == 0)
					{
						if (sf.to == whichword)
                        {
                            search.buf += "          => ";
                        }
                        else
                        {
                            search.buf += "          *> ";
                        }

                        search.buf += sf.fr.Str + "\n";

						isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
						// populates buf to the logic that defines whether the 
						// synset is populated with relevant information
					}
				}
            }
        }

        /* find the example sentence references in the example sentence index file */
        private bool FindExample()
		{
			bool retval = false;
			StreamReader fp = WordNetData.GetStreamReader(WordNetData.path + "SENTIDX.VRB");

			int wdnum = whichword - 1;
			Lexeme lx = words[wdnum];
			string tbuf = lx.word + "%" + pos.Ident + ":" + fnum + ":" + lx.uniq + "::";
			string str = WordNetData.BinSearch(tbuf, fp);

			if (str != null)
			{
				str = str.Substring(lx.word.Length + 11);
				var st = str.Split(new char[] { ' ', ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var stI = 0;
				string offset;
				while ((offset = st[stI++]) != null)
				{
					GetExample(offset, lx.word);
					retval = true;
				}
			}

			fp.Close();

			return retval;
		}

        private void GetExample(string off, string wd)
		{
			StreamReader fp = WordNetData.GetStreamReader(WordNetData.path + "SENTS.VRB");
			string line = WordNetData.BinSearch(off, fp);
			line = line.Substring(line.IndexOf(' ') + 1);
			search.buf += "         EX: " + line.Replace("%s", wd);
			fp.Close();

			isDirty = true; // TDMS 19 July 2006 - attempt to tie the logic which 
			// populates buf to the logic that defines whether the 
			// synset is populated with relevant information
		}

		public int Getsearchsense(int which)
		{
			int i;
			string wdbuf = words[which - 1].word.Replace(' ', '_').ToLower();
			Index idx = Index.Lookup(wdbuf, pos);
			if (idx != null)
            {
                for (i = 0; i < idx.offs.Length; i++)
                {
                    if (idx.offs[i] == hereiam)
                    {
                        return i + 1;
                    }
                }
            }

            return 0;
		}

		internal void Seealso()
		{
			/* Find all SEEALSO pointers from the searchword and print the
			   word or synset pointed to. */
			string prefix; // = "      Also See-> ";
			//WN3.0 added updated wording for verb see also
			if (pos.Key == "verb")
            {
                prefix = "      Phrasal Verb-> ";
            }
            else
            {
                prefix = "      Also See-> ";
            }

            for (int i = 0; i < ptrs.Length; i++)
			{
				Pointer p = ptrs[i];
				if (p.ptp.Ident == SEEALSOPTR &&
					(p.sce == 0 || (p.sce == whichword)))
				{
					SynonymSet cursyn = new SynonymSet(p.off, p.pos, "", this);
					bool svwnsnsflag = WordNetOption.Opt("-s").flag;
					WordNetOption.Opt("-s").flag = true;
					cursyn.Str(prefix, "", 0, (p.dst == 0) ? 0 : p.dst, 0, 0);
					prefix = "; ";
				}
			}
		}
	}
}
