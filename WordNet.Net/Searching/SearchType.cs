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
    public class SearchType : IComparable
    {
        public bool rec;
        public PointerType ptp;
        //public static SortedList searchtypes = new SortedList(); // SearchType -> SearchType
        public SearchType(bool r, string t)
            : this(r, PointerType.Of(t))
        {
        }

        public SearchType(bool r, PointerType p)
        {
            ptp = p;
            rec = r;
        }

        public SearchType(string m)
            : this(m[0] == '-', (m[0] == ' ') ? m.Substring(1) : m)
        {
        }

        public string Label
        {
            get
            {
                return ptp.Label;
            }
        }

        public int CompareTo(object a)
        {
            SearchType s = (SearchType)a;
            if (ptp.Ident < s.ptp.Ident)
            {
                return -1;
            }

            if (ptp.Ident > s.ptp.Ident)
            {
                return 1;
            }

            if ((!rec) && s.rec)
            {
                return -1;
            }

            if (rec && !s.rec)
            {
                return 1;
            }

            return 1;
        }

        public override bool Equals(object a)
        {
            return CompareTo(a) == 0;
        }

        public override int GetHashCode()
        {
            return rec.GetHashCode() + ptp.GetHashCode();
        }
    }
}
