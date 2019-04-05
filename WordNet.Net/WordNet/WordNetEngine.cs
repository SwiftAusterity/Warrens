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
 */

using System.Collections;
using System.Collections.Generic;
using WordNet.Net.Searching;
using WordNet.Net.WordNet;

namespace WordNet.Net
{
    /// <summary>
    /// Summary description for WordNetClasses.
    /// </summary>
    public class WordNetEngine
    {
        public bool hasmatch = false; // determines whether morphs are considered
        private WordNetData netData;

        public WordNetEngine(string dictPath)
        {
            netData = new WordNetData(dictPath);
        }

        public void OverviewFor(string t, string p, ref bool b, ref SearchSet obj, List<Search> list)
        {
            SearchSet ss = null;

            if (string.IsNullOrWhiteSpace(p))
            {
                OverviewFor(t, PartsOfSpeech.Adjective.ToString(), ref b, ref obj, list);
                OverviewFor(t,  PartsOfSpeech.Adverb.ToString(), ref b, ref obj, list);
                OverviewFor(t,  PartsOfSpeech.Noun.ToString(), ref b, ref obj, list);
                OverviewFor(t, PartsOfSpeech.Verb.ToString(), ref b, ref obj, list);

                ss = obj;
            }
            else
            { 
                PartOfSpeech pos = PartOfSpeech.Of(p);

                if (!string.IsNullOrWhiteSpace(pos?.Key))
                {
                    ss = netData.Is_defined(t, pos);
                    Morph ms = new Morph(t, pos, netData);
                    hasmatch = AddSearchFor(t, pos, list);

                    //TODO: if this isn't reset then morphs aren't checked on subsequent searches - check for side effects of resetting this manually
                    if (!hasmatch)
                    {
                        string m;
                        // loop through morphs (if there are any)
                        while ((m = ms.Next()) != null)
                        {
                            if (m != t)
                            {
                                ss += netData.Is_defined(m, pos);
                                AddSearchFor(m, pos, list);
                            }
                        }
                    }
                }
            }

            b = ss?.NonEmpty ?? false;
            obj = ss;
        }

        private bool AddSearchFor(string s, PartOfSpeech pos, List<Search> list)
        {
            Search se = new Search(s, false, pos, new SearchType(false, "OVERVIEW"), 0, netData);
            if (se.lexemes.Count > 0)
            {
                list.Add(se);
            }

            return se.lexemes.Count > 0; // results were found
        }
    }
}
