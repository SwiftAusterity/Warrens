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
    public class SearchSet
    {
        private BitSet b;
        internal SearchSet()
        {
            b = new BitSet(30);
        }
        internal SearchSet(SearchSet s)
        {
            b = new BitSet(s.b);
        }

        public static SearchSet operator +(SearchSet a, string s)
        {
            SearchSet r = new SearchSet(a);
            r.b[PointerType.Of(s).Ident] = true;
            return r;
        }

        public static SearchSet operator +(SearchSet a, PointerType p)
        {
            SearchSet r = new SearchSet(a);
            r.b[p.Ident] = true;
            return r;
        }

        public static SearchSet operator +(SearchSet a, SearchSet b)
        {
            SearchSet r = new SearchSet(a)
            {
                b = a.b.Or(b.b)
            };
            return r;
        }

        public bool this[int i]
        {
            get
            {
                return b[i];
            }
        }
        public bool this[string s]
        {
            get
            {
                return b[PointerType.Of(s).Ident];
            }
        }
        public bool NonEmpty
        {
            get
            {
                return b.GetHashCode() != 0;
            }
        }
    }
}
