using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PluginContracts;


namespace WeightedDictionary
{
    //partial class because we're splitting this bad boy up between files
    internal class DictParser
    {






        public DictionaryData ParseDict(DictionaryMetaObject DictionaryToParse)
        {



            DictionaryData DictData = DictionaryToParse.DictData;

            //  ____                   _       _         ____  _      _   ____        _           ___  _     _           _   
            // |  _ \ ___  _ __  _   _| | __ _| |_ ___  |  _ \(_) ___| |_|  _ \  __ _| |_ __ _   / _ \| |__ (_) ___  ___| |_ 
            // | |_) / _ \| '_ \| | | | |/ _` | __/ _ \ | | | | |/ __| __| | | |/ _` | __/ _` | | | | | '_ \| |/ _ \/ __| __|
            // |  __/ (_) | |_) | |_| | | (_| | ||  __/ | |_| | | (__| |_| |_| | (_| | || (_| | | |_| | |_) | |  __/ (__| |_ 
            // |_|   \___/| .__/ \__,_|_|\__,_|\__\___| |____/|_|\___|\__|____/ \__,_|\__\__,_|  \___/|_.__// |\___|\___|\__|
            //            |_|                                                                             |__/               




            //parse out the the dictionary file
            DictData.MaxWords = 0;
            DictData.FullDictionary = new Dictionary<int, Dictionary<string, double[]>>();



            string[] DicLines = DictionaryToParse.DictionaryRawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            string[] HeaderLines = DicLines[0].Split(new[] { '\t' });
            string[] InterceptLines = DicLines[1].Split(new[] { '\t' });

            DictData.NumCats = HeaderLines.Length - 1;

            //now that we know the number of categories, we can fill out the arrays
            DictData.CatNames = new string[DictData.NumCats];
            DictData.InterceptWeights = new double[DictData.NumCats];
                

            //Map Out the Categories
            for (int i = 0; i < DictData.NumCats; i++)
            {
                DictData.CatNames[i] = HeaderLines[i+1];
                DictData.InterceptWeights[i] = Double.Parse(InterceptLines[i + 1]);
            }


            //Map out the dictionary entries
            for (int i = 2; i < DicLines.Length; i++)
            {

                string EntryLine = DicLines[i];

                string[] EntryRow = EntryLine.Split(new char[] { '\t' }, StringSplitOptions.None);

                if (EntryRow.Length > 1 && !String.IsNullOrWhiteSpace(EntryRow[0])) { 

                    int Words_In_Entry = EntryRow[0].Split(' ').Length;
                    if (Words_In_Entry > DictData.MaxWords) DictData.MaxWords = Words_In_Entry;

                    double[] WordWeights = new double[DictData.NumCats];

                //this is where we actually map out the word weights from the dictionary file
                for (int j = 0; j < DictData.NumCats; j++)
                {
                    if (!String.IsNullOrWhiteSpace(EntryRow[j + 1]))
                    {
                        try
                        {
                            WordWeights[j] = Double.Parse(EntryRow[j + 1].Trim());
                        }
                        catch
                        {
                            WordWeights[j] = Double.Epsilon;
                        }
                            
                    }
                    else
                    {
                        WordWeights[j] = Double.Epsilon;
                    }

                }
                        
                    if (DictData.FullDictionary.ContainsKey(Words_In_Entry))
                    {
                        if (!DictData.FullDictionary[Words_In_Entry].ContainsKey(EntryRow[0].ToLower()))
                            DictData.FullDictionary[Words_In_Entry].Add(EntryRow[0].ToLower(), WordWeights);
                    }
                    else
                    {
                        DictData.FullDictionary.Add(Words_In_Entry, new Dictionary<string, double[]> { { EntryRow[0].ToLower(), WordWeights } });
                    }

                        
                }
            }


            DictData.DictionaryLoaded = true;

            return DictData;

            //MessageBox.Show("Your dictionary has been successfully loaded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);




        }








    }
}