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
    /// <summary>
    /// WordNet search code morphology functions
    /// </summary>
    public class Morph
	{
        private static readonly string[] sufx = {
								   /* Noun suffixes */
								   "s", "ses", "xes", "zes", "ches", "shes", "men", "ies",
								   /* Verb suffixes */
								   "s", "ies", "es", "es", "ed", "ed", "ing", "ing",
								   /* Adjective suffixes */
								   "er", "est", "er", "est"
							   };
        private static readonly string[] addr = { 
								   /* Noun endings */
								   "", "s", "x", "z", "ch", "sh", "man", "y",
								   /* Verb endings */
								   "", "y", "e", "", "e", "", "e", "",
								   /* Adjective endings */
								   "", "", "e", "e"
							   };
        private static readonly int[] offsets = { 0, 8, 8, 16 };
        private static readonly int[] cnts = { 8, 8, 8, 4 }; // 0 changed to 8 - Troy

        private static string[] prepositions = 
		{
			"to", "at", "of", "on", "off", "in", "out", "up", "down", "from", 
			"with", "into", "for", "about", "between" 
		};
        private string searchstr, str;
        private int svcnt, svprep;
        private PartOfSpeech pos;
        private Exceptions e;
        private bool firsttime;
        private readonly int cnt;

		public Morph(string s, string p)
			: this(s, PartOfSpeech.Of(p))
		{
		}

		public Morph(string s, PartOfSpeech p)
		{
			string origstr = s;
			pos = p;
			if (pos.Clss == "SATELLITE")
            {
                pos = PartOfSpeech.Of("adj");
            }
            /* Assume string hasnt had spaces substitued with _ */
            str = origstr.Replace(' ', '_').ToLower();
			searchstr = "";
			cnt = str.Split('_').Length;
			svprep = 0;
			firsttime = true;
		}

		public string Next()
		{
			string word, tmp;
			int prep = 0, cnt, st_idx = 0, end_idx = 0, end_idx1, end_idx2;
			string append = "";

			/* first time through for this string */
			if (firsttime)
			{
				firsttime = false;
				cnt = str.Split('_').Length;
				svprep = 0;

				/* first try exception list */
				e = new Exceptions(str, pos);
				if ((tmp = e.Next()) != null && tmp != str)
				{
					svcnt = 1; /* force next time to pass NULL */
					return tmp;
				}
				/* then try simply morph on original string */
				if (pos.Key != "verb" && ((tmp = Morphword(str)) != null) && str != tmp)
                {
                    return tmp;
                }

                if (pos.Key == "verb" && cnt > 1 && (prep = Hasprep(str, cnt)) != 0)
				{
					svprep = prep;
					return Morphprep(str);
				}
				else
				{
					svcnt = cnt = str.Split('_').Length;
					while (--cnt > 0)
					{
						end_idx1 = str.Substring(st_idx).IndexOf('_') + st_idx;
						end_idx2 = str.Substring(st_idx).IndexOf('-') + st_idx;
						if (end_idx1 >= st_idx && end_idx2 >= st_idx)
						{
							if (end_idx1 < end_idx2)
							{
								end_idx = end_idx1;
								append = "_";
							}
							else
							{
								end_idx = end_idx2;
								append = "-";
							}
						}
						else if (end_idx1 >= st_idx)
						{
							end_idx = end_idx1;
							append = "_";
						}
						else
						{
							end_idx = end_idx2;
							append = "-";
						}
						if (end_idx < 0)
                        {
                            return null;
                        }

                        word = str.Substring(st_idx, end_idx - st_idx);
						if ((tmp = Morphword(word)) != null)
                        {
                            searchstr += tmp;
                        }
                        else
                        {
                            searchstr += word;
                        }

                        searchstr += append;
						st_idx = end_idx + 1;
					}
					word = str.Substring(st_idx);
					if ((tmp = Morphword(word)) != null)
                    {
                        searchstr += tmp;
                    }
                    else
                    {
                        searchstr += word;
                    }

                    if (searchstr != str && WordNetData.Is_defined(searchstr, pos).NonEmpty)
                    {
                        return searchstr;
                    }
                    else
                    {
                        return null;
                    }
                }
			}
			else  // not firsttime
			{
				if (svprep > 0)
				{
					svprep = 0;
					return null;
				}
				else if (svcnt == 1)
                {
                    return e.Next();
                }
                else
				{
					svcnt = 1;
					e = new Exceptions(str, pos);
					if ((tmp = e.Next()) != null && tmp != str)
                    {
                        return tmp;
                    }

                    return null;
				}
			}
		}

        private string Morphword(string word)
		{
			string end = "";
			string tmpbuf = "";
			if (word == null)
            {
                return null;
            }

            Exceptions e = new Exceptions(word, pos);
			string tmp = e.Next();
			if (tmp != null)
            {
                return tmp;
            }

            if (pos.Key == "adverb")
            {
                return null;
            }

            if (pos.Key == "noun")
			{
				if (word.EndsWith("ful"))
				{
					tmpbuf = word.Substring(0, word.Length - 3);
					end = "ful";
				}
				else if (word.EndsWith("ss") || word.Length <= 2)
                {
                    return null;
                }
            }
			if (tmpbuf.Length == 0)
            {
                tmpbuf = word;
            }

            int offset = offsets[pos.Ident];
			int cnt = cnts[pos.Ident];
			for (int i = 0; i < cnt; i++)
            {
                if (tmpbuf.EndsWith(sufx[i + offset]))
				{
					// TDMS 11 Oct 2005 - bug fix - "word" substituted with "tmpbuf" as per
					// wordnet code morph.c
					string retval = tmpbuf.Substring(0, tmpbuf.Length - sufx[i + offset].Length) + addr[i + offset];
					if (WordNetData.Is_defined(retval, pos).NonEmpty)
                    {
                        return retval + end;
                    }
                }
            }

            return null;
		}

        /* Find a preposition in the verb string and return its
		   corresponding word number. */
        private int Hasprep(string s, int wdcnt)
		{
			int i, wdnum;
			int pos = 0;
			for (wdnum = 2; wdnum <= wdcnt; wdnum++)
			{
				pos = s.IndexOf('_');
				for (pos++, i = 0; i < prepositions.Length; i++)
				{
					int len = prepositions[i].Length;
					if (len <= s.Length - pos && s.Substring(pos, len) == prepositions[i]
						&& (len == s.Length - pos || s[pos + len] == '_'))
                    {
                        return wdnum;
                    }
                }
			}
			return 0;
		}

        private string Morphprep(string s)
		{
			string excWord, lastwd = null;
			int i, offset, cnt, rest, last;
			string word, end, retval;

			/* Assume that the verb is the first word in the phrase.  Strip it
			   off, check for validity, then try various morphs with the
			   rest of the phrase tacked on, trying to find a match. */

			rest = s.IndexOf('_');
			last = s.LastIndexOf('_');
			end = "";
			if (rest != last)
			{ // more than 2 words
				lastwd = Morphword(s.Substring(last + 1));
				if (lastwd != null)
                {
                    end = s.Substring(rest, last - rest + 1) + lastwd;
                }
            }
			word = s.Substring(0, rest);
			for (i = 0; i < word.Length; i++)
            {
                if (!char.IsLetterOrDigit(word[i]))
                {
                    return null;
                }
            }

            offset = offsets[PartOfSpeech.Of("verb").Ident];
			cnt = cnts[PartOfSpeech.Of("verb").Ident];
			/* First try to find the verb in the exception list */
			Exceptions e = new Exceptions(word, PartOfSpeech.Of("verb"));
			while ((excWord = e.Next()) != null && excWord != word)
			{
				retval = excWord + s.Substring(rest);
				if (WordNetData.Is_defined(retval, PartOfSpeech.Of("verb")).NonEmpty)
                {
                    return retval;
                }
                else if (lastwd != null)
				{
					retval = excWord + end;
					if (WordNetData.Is_defined(retval, PartOfSpeech.Of("verb")).NonEmpty)
                    {
                        return retval;
                    }
                }
			}
			for (i = 0; i < cnt; i++)
            {
                if ((excWord = Wordbase(word, i + offset)) != null && excWord != word) // ending is different
				{
					retval = excWord + s.Substring(rest);
					if (WordNetData.Is_defined(retval, PartOfSpeech.Of("verb")).NonEmpty)
                    {
                        return retval;
                    }
                    else if (lastwd != null)
					{
						retval = excWord + end;
						if (WordNetData.Is_defined(retval, PartOfSpeech.Of("verb")).NonEmpty)
                        {
                            return retval;
                        }
                    }
				}
            }

            retval = word + s.Substring(rest);
			if (s != retval)
            {
                return retval;
            }

            if (lastwd != null)
			{
				retval = word + end;
				if (s != retval)
                {
                    return retval;
                }
            }
			return null;
		}

        private string Wordbase(string word, int ender)
		{
			if (word.EndsWith(sufx[ender]))
            {
                return word.Substring(0, word.Length - sufx[ender].Length) + addr[ender];
            }

            return word;
		}
	}
}
