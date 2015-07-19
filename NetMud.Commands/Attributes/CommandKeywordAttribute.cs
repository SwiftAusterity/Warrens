using System;

namespace NutMud.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandKeywordAttribute : Attribute
    {
        public string Keyword { get; private set; }
        public bool IsAlsoSubject { get; private set; }

        public CommandKeywordAttribute(string keyword, bool isAlsoSubject)
        {
            //Way easier just to load them all into lowercase so we don't have to move the cost to runtime
            if (String.IsNullOrWhiteSpace(keyword))
                throw (new ArgumentNullException(String.Format("{0} Command accessor keyword blank on implimentation.", this.GetType().ToString())));

            Keyword = keyword.ToLower();
            IsAlsoSubject = isAlsoSubject;
        }
    }
}
