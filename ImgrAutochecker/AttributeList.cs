using System.Collections.Generic;

namespace ImgrAutochecker
{
    public class AttributeList
    {
        public bool Use;
        public string Attribute;
        public bool All;
        public string Value;
        public string[] AllValues;
        public string ValidationRules;
        public List<string> Attributes = new List<string>(); 

        public AttributeList(string valAttribute, string valRules)
        {
            Attribute = valAttribute;
            ValidationRules = valRules;
        }

        //public bool use
        //{
        //    get { return Use; }
        //    set { Use = value; }
        //}

        public string attribute
        {
            get { return Attribute; }
        }

        public bool all
        {
            get { return All; }
            set { All = value; }
        }

        public string value
        {
            get { return Value; }
            set { Value = value; }
        }

    }
}
