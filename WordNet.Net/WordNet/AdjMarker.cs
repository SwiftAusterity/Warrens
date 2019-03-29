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
using System.Globalization;
using System.Diagnostics;

namespace WordNet.Net.WordNet
{
    [Serializable]
	public class AdjMarker
	{
        private static Hashtable marks = new Hashtable();
		public string mnem;
		public string mark;

        private AdjMarker()
		{
			// empty constructor for serialization
		}

		public static AdjMarker Of(string s)
		{
			return (AdjMarker)marks[s];
		}

        private AdjMarker(string n, string k)
		{
			mnem = n;
			mark = k;
			marks[n] = this;
		}

		static AdjMarker()
		{
			new AdjMarker("UNKNOWN_MARKER", "");
			new AdjMarker("ATTRIBUTIVE", "(prenominal)");
			new AdjMarker("IMMED_POSTNOMINAL", "(postnominal)");
			new AdjMarker("PREDICATIVE", "(predicate)");
		}
	}
}
