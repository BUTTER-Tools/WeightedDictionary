using PluginContracts;
using System;
using System.Linq;
using System.Collections.Generic;

namespace WeightedDictionary
{


    public partial class WeightedDictionary : Plugin
    {



        private Dictionary<string, string> AnalyzeText(DictionaryData DictData, int DictionaryNumberForTrackingMatches, string[] Words)
        {


            ulong totalNumberOfMatches = 0;

            int[] NumberOfMatches = new int[DictData.NumCats];
            for (int i = 0; i < DictData.NumCats; i++) NumberOfMatches[i] = 0;

            int TotalStringLength = Words.Length;



            Dictionary<string, double> DictionaryResults = new Dictionary<string, double>();
            for (int i = 0; i < DictData.NumCats; i++) DictionaryResults.Add(DictData.CatNames[i], 0);
            

            for (int i = 0; i < TotalStringLength; i++)
            {



                //iterate over n-grams, starting with the largest possible n-gram (derived from the user's dictionary file)
                for (int NumberOfWords = DictData.MaxWords; NumberOfWords > 0; NumberOfWords--)
                {



                    //make sure that we don't overextend past the array
                    if (i + NumberOfWords - 1 >= TotalStringLength) continue;

                    //make the target string

                    string TargetString;

                    if (NumberOfWords > 1)
                    {
                        TargetString = String.Join(" ", Words.Skip(i).Take(NumberOfWords).ToArray());
                    }
                    else
                    {
                        TargetString = Words[i];
                    }


                    //look for an exact match

                    if (DictData.FullDictionary.ContainsKey(NumberOfWords))
                    {
                        if (DictData.FullDictionary[NumberOfWords].ContainsKey(TargetString))
                        {


                            totalNumberOfMatches += 1;

                            //add in the number of words found
                            for (int j = 0; j < DictData.NumCats; j++)
                            {

                                //if we actually have a value for this word in the dictionary, then we do the incrementations
                                if (DictData.FullDictionary[NumberOfWords][TargetString][j] != Double.Epsilon)
                                {
                                    NumberOfMatches[j] += 1;
                                    DictionaryResults[DictData.CatNames[j]] += DictData.FullDictionary[NumberOfWords][TargetString][j];
                                }

                            }
                            //manually increment the for loop so that we're not testing on words that have already been picked up
                            i += NumberOfWords - 1;
                            //break out of the lower level for loop back to moving on to new words altogether
                            break;
                        }
                    }
                    
                }

            }


            Dictionary<string, string> ResultsToReturn = new Dictionary<string, string>();

            for (int i = 0; i < DictData.NumCats; i++)
            {
                if (NumberOfMatches[i] > 0)
                {
                    ResultsToReturn.Add(DictData.CatNames[i], ((DictionaryResults[DictData.CatNames[i]] / NumberOfMatches[i]) + DictData.InterceptWeights[i]).ToString());
                }
                else
                {
                    ResultsToReturn.Add(DictData.CatNames[i], "");
                }
            }

            ResultsToReturn[DictData.totalNumberOfMatchesDictName] = (((double)totalNumberOfMatches / TotalStringLength) * 100).ToString();

            return ResultsToReturn;


        }









    }






}
