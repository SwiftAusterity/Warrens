using System;

namespace NetMud.Lexica.DeepLex
{
    [Serializable]
    public class DictionaryEntry
    {
        /// <summary>
        /// Metadata for the api
        /// </summary>
        public Meta meta { get; set; }

        /// <summary>
        /// Homograph
        /// </summary>
        public int hom { get; set; }

        /// <summary>
        /// Headwords
        /// </summary>
        public Headword hwi { get; set; }

        /// <summary>
        /// Alternate headwords
        /// </summary>
        public Headword[] ahws { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public Variant[] vrs { get; set; }

        /// <summary>
        /// Functional Label, word form
        /// </summary>
        public string fl { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public string[] lbs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public string[] sls { get; set; }

        /// <summary>
        /// Inflections
        /// </summary>
        public Inflection[] ins { get; set; }

        /// <summary>
        /// When a headword is a less common spelling of another word with the same meaning, there will be a cognate cross-reference pointing to the headword with the more common spelling.
        /// </summary>
        public CognateCrossReference cxs { get; set; }

        /// <summary>
        /// The definition section groups together all the sense sequences and verb dividers for a headword or defined run-on phrase.
        /// </summary>
        public Definition[] def { get; set; }
    }

    /// <summary>
    /// The definition section groups together all the sense sequences and verb dividers for a headword or defined run-on phrase.
    /// </summary>
    [Serializable]
    public class Definition
    {
        public SenseSequence[] sseq { get; set; }

        /// <summary>
        /// Verb Divider: The verb divider acts as a functional label in verb entries, introducing the separate sense sequences for transitive and intransitive meanings of the verb.
        /// </summary>
        public string vd { get; set; }
    }

    [Serializable]
    public class Sense
    {
        public DefiningText dt { get; set; }

        /// <summary>
        /// The sense number identifies a sense or subsense within the entry. 
        /// </summary>
        public string sn { get; set; }

        /// <summary>
        /// A sense may be divided into two parts to show a particular relationship between two closely related meanings. 
        /// The second part of the sense is contained in an sdsense, consisting of an sense divider sd along with a second dt.
        /// </summary>
        public DividedSense sdsense { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public string[] lbs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public string[] sls { get; set; }

        /// <summary>
        /// Inflections
        /// </summary>
        public Inflection[] ins { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public Variant[] vrs { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public Pronounciation[] prs { get; set; }
        //et, sgram
    }

    /// <summary>
    /// The defining text is the text of the definition or translation for a particular sense. 
    /// </summary>
    [Serializable]
    public class DefiningText
    {
        /// <summary>
        /// definition content
        /// </summary>
        public string text { get; set; }

        //bnw, ca, ri, snote, uns, or vis
    }

    /// <summary>
    /// A sense may be divided into two parts to show a particular relationship between two closely related meanings. 
    /// The second part of the sense is contained in an sdsense, consisting of an sense divider sd along with a second dt.
    /// </summary>
    [Serializable]
    public class DividedSense
    {
        /// <summary>
        /// Sense divider
        /// </summary>
        public string sd { get; set; }

        /// <summary>
        /// Defining Text
        /// </summary>
        public DefiningText dt { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public string[] lbs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public string[] sls { get; set; }

        /// <summary>
        /// Inflections
        /// </summary>
        public Inflection[] ins { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public Variant[] vrs { get; set; }

        /// <summary>
        /// Pronounciations
        /// </summary>
        public Pronounciation[] prs { get; set; }

        //et, sgram
    }
}
