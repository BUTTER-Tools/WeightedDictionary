using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace WeightedDictionary
{


    internal class DictionaryMetaObject
    {
        public string DictionaryName { get; set; }
        public string DictionaryCategoryPrefix { get; set; }
        public string DictionaryDescription { get; set; }
        public string DictionaryRawText { get; set; }
        public bool UseDictionary { get; set; }
        public DictionaryData DictData { get; set; }

        public DictionaryMetaObject(string DictName, string DictDescript, string DictPrefix, string DictContents, bool useDict = true)
        {
            DictionaryName = DictName;
            DictionaryDescription = DictDescript;
            DictionaryRawText = DictContents;
            DictionaryCategoryPrefix = DictPrefix; 
            UseDictionary = useDict;
            DictData = new DictionaryData();
        }

    }


    public class DictionaryData
    {

        public int NumCats { get; set; }
        public int MaxWords { get; set; }

        public string totalNumberOfMatchesDictName { get; set; }

        public string[] CatNames { get; set; }

        //yeah, we're going full inception with this variable. dictionary inside of a dictionary inside of a dictionary
        //while it might seem unnecessarily complicated (and it might be), it makes sense.
        //the first level simply differentiates the wildcard entries from the non-wildcard entries                
        //The second level is purely to refer to the word length -- does each sub-entry include 1-word entries, 2-word entries, etc?
        //the third level contains the actual entries from the user's dictionary file
        public double[] InterceptWeights { get; set; }
        public Dictionary<int, Dictionary<string, double[]>> FullDictionary { get; set; }

        public bool DictionaryLoaded { get; set; }


        public DictionaryData()
        {
            NumCats = 0;
            MaxWords = 0;
            CatNames = new string[] { };
            InterceptWeights = new double[0];
            FullDictionary = new Dictionary<int, Dictionary<string, double[]>>();
            DictionaryLoaded = false;
        }






    }
}
