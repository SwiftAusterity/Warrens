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
using WordNet.Net;
using WordNet.Net.Searching;
using WordNet.Net.WordNet;

namespace WordNet.Net
{
    /// <summary>
    /// Summary description for WordNetClasses.
    /// </summary>
    public class WordNetEngine
    {
        public static bool hasmatch = false; // determines whether morphs are considered

        public WordNetEngine(string dictpath)
        {
            WNCommon.Path = dictpath;
        }

        public void OverviewFor(string t, string p, ref bool b, ref SearchSet obj, ArrayList list)
        {
            PartOfSpeech pos = PartOfSpeech.Of(p);
            SearchSet ss = WordNetData.Is_defined(t, pos);
            MorphStr ms = new MorphStr(t, pos);
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
                        ss = ss + WordNetData.Is_defined(m, pos);
                        AddSearchFor(m, pos, list);
                    }
                }
            }

            b = ss.NonEmpty;
            obj = ss;
        }

        private bool AddSearchFor(string s, PartOfSpeech pos, ArrayList list)
        {
            Search se = new Search(s, false, pos, new SearchType(false, "OVERVIEW"), 0);
            if (se.lexemes.Count > 0)
            {
                list.Add(se);
            }

            return se.lexemes.Count > 0; // results were found
        }
    }
}
