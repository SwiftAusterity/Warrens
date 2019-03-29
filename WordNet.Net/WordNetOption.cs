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

namespace WordNet.Net
{
    public class WordNetOption
	{
        private static Hashtable opts = new Hashtable();

		public static WordNetOption Opt(string a)
		{
			return (WordNetOption)opts[a];
		}

		public string arg;
		public string help;
		public bool flag;

        private WordNetOption(string a, string h, bool i)
		{
			arg = a;
			help = h;
			flag = i;
			opts.Add(a, this);
		}

		static WordNetOption()
		{
			new WordNetOption("-l", "license and copyright", false);
			new WordNetOption("-h", "help text on each search", false);
			new WordNetOption("-g", "gloss", true);
			new WordNetOption("-a", "lexicographer file information", false);
			new WordNetOption("-o", "synset offset", false);
			new WordNetOption("-s", "sense numbers", false);
			new WordNetOption("-A", "lexicog file info for overview", false);
			new WordNetOption("-O", "synset offset for overview", false);
			new WordNetOption("-S", "sense numbers for overview", false);
		}
	}
}
