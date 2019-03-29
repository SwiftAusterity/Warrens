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
using WordNet.Net.Searching;

namespace WordNet.Net
{
    /// <summary>
    /// Summary description for SemCor.
    /// </summary>
    [Serializable]
    public class SemanticCorrelation
    {
        public int semcor = 0;

        public SemanticCorrelation()
        {
            // empty constructor for serialization
        }

        public SemanticCorrelation(Lexeme lex, int hereiam)
        {
            // left-pad the integer with 0's into a string
            string key = hereiam.ToString("d8") + " " + lex.wnsns;

            using (StreamReader indexFile = WordNetData.GetStreamReader(WordNetData.path + @"\index.sense"))
            {
                // locate our word and key via a binary search
                string[] lexinfo = WordNetData.BinSearchSemCor(key, lex.word, indexFile).Split(' ');

                semcor = Convert.ToInt16(lexinfo[lexinfo.GetUpperBound(0)]);
            }
        }
    }
}
