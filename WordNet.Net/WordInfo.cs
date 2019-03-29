/*
 * This file is a part of the WordNet.Net open source project.
 * 
 * Author:	Jeff Martin
 * Date:	7/07/2005
 * 
 * Copyright (C) 2005 Malcolm Crowe, Troy Simpson, Jeff Martin
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

using System;
using System.Collections;
using WordNet.Net.Searching;

namespace WordNet.Net
{
    /// <summary>This class contains information about the word</summary>
    public class WordInfo
	{
		public string text = "";
		public PartsOfSpeech partOfSpeech = PartsOfSpeech.None;
		public int[] senseCounts = null;

		/// <summary>a sum of all the sense counts hints at the commonality of a word</summary>
		public int Strength
		{
			get
			{
                int strength = 0;
				foreach (int i in senseCounts)
                {
                    strength += i;
                }

                return strength;
			}
		}

		public static bool operator ==(WordInfo a, WordInfo b)
		{
			if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            if (a.partOfSpeech != b.partOfSpeech)
            {
                return false;
            }

            if (a.senseCounts != null && b.senseCounts != null)
            {
                return a.senseCounts.Equals(b.senseCounts);
            }

            if (a.senseCounts == null && b.senseCounts == null)
            {
                return true;
            }

            return false;
		}

		public static bool operator !=(WordInfo a, WordInfo b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is WordInfo)
            {
                return this == (WordInfo)obj;
            }
            else
            {
                return false;
            }
        }

		public override int GetHashCode()
		{
			return partOfSpeech.GetHashCode() ^ senseCounts.GetHashCode();
		}
	}
}
