using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using PluginContracts;
using OutputHelperLib;
using System.Xml;


namespace WeightedDictionary
{
    public partial class WeightedDictionary : Plugin
    {


        public string[] InputType { get; } = { "Tokens" };
        public string OutputType { get; } = "OutputArray";

        public Dictionary<int, string> OutputHeaderData { get; set; } = new Dictionary<int, string>() { { 0, "TokenCount" } };
        public bool InheritHeader { get; } = false;

        #region Plugin Details and Info

        public string PluginName { get; } = "Weighted Dictionary";
        public string PluginType { get; } = "Language Analysis";
        public string PluginVersion { get; } = "1.2.11";
        public string PluginAuthor { get; } = "Ryan L. Boyd (ryan@ryanboyd.io)";
        public string PluginDescription { get; } = "Scores texts using weighted dictionaries. For dictionaries that have intercept betas, these are also accounted for in the final scores." + Environment.NewLine + Environment.NewLine + 
                                                   "Note that each dictionary also receives a \"DictPct\" score in the output. This score reflects the number of matches each text contained, divided by Token Count. Put another way, this number tells the quantity of each text that was able to be scored for each weighted dictionary, respectively.";
        public bool TopLevel { get; } = false;
        public string PluginTutorial { get; } = "https://youtu.be/EUc5bonNNTQ";

        List<DictionaryMetaObject> DictionaryList { get; set; }
        HashSet<string> ListOfBuiltInDictionaries { get; set; }

        public Icon GetPluginIcon
        {
            get
            {
                return Properties.Resources.icon;
            }
        }

        #endregion



        public void ChangeSettings()
        {

            using (var form = new SettingsForm_WeightedDictionary(DictionaryList))
            {

                form.Icon = Properties.Resources.icon;
                form.Text = PluginName;


                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    DictionaryList = form.DictDataToReturn;
                }
            }

        }


        public Payload RunPlugin(Payload Input)
        {



            Payload pData = new Payload();
            pData.FileID = Input.FileID;
            pData.SegmentID = Input.SegmentID;

            for (int i = 0; i < Input.StringArrayList.Count; i++)
            {

                string[] OutputArray = new string[TotalCatCount];
                OutputArray[0] = Input.StringArrayList[i].Length.ToString();
                //for (int j = 0; j < TotalCatCount; j++) OutputArray[j] = "0";

                for (int j = 0; j < DictionaryList.Count; j++)
                {

                    if (DictionaryList[j].UseDictionary)
                    { 
                        Dictionary<string, string> Results = AnalyzeText(DictionaryList[j].DictData, j, Input.StringArrayList[i]);

                        //gets us the total number of matches and plops it into the output
                        OutputArray[
                                    OutputDataMap[
                                        DictionaryList[j].DictData.totalNumberOfMatchesDictName]
                                        ] = Results[DictionaryList[j].DictData.totalNumberOfMatchesDictName];

                        for (int k = 0; k < DictionaryList[j].DictData.CatNames.Length; k++)
                        {


                                OutputArray[
                                    OutputDataMap[
                                        DictionaryList[j].DictionaryCategoryPrefix + DictionaryList[j].DictData.CatNames[k]
                                                 ]
                                        ] = Results[DictionaryList[j].DictData.CatNames[k]];
                            
                        }
                    }

                }

                pData.SegmentNumber.Add(Input.SegmentNumber[i]);
                pData.StringArrayList.Add(OutputArray);

            }

            return (pData);
        }




        public Dictionary<string, int> OutputDataMap { get; set; }
        private int TotalCatCount { get; set; } = 0;
        public void Initialize()
        {

            TotalCatCount = 1;
            Dictionary<string, int> TempOutputDataMap = new Dictionary<string, int>();
            Dictionary<int, string> TempHeaderData = new Dictionary<int, string>();
            TempHeaderData.Add(0, "TokenCount");

            for (int i = 0; i < DictionaryList.Count; i++)
            {
                if (DictionaryList[i].UseDictionary)
                {



                    //this little bit here lets us keep track of the total number of matches that we've found
                    //from each dictionary

                    string totalMatchesTrackerVarName = "";
                    if (!String.IsNullOrWhiteSpace(DictionaryList[i].DictionaryCategoryPrefix) && !TempOutputDataMap.ContainsKey(DictionaryList[i].DictionaryCategoryPrefix + "TotalMatches")) 
                    {
                        totalMatchesTrackerVarName = DictionaryList[i].DictionaryCategoryPrefix + "DictPct";
                    }
                    else
                    {
                        totalMatchesTrackerVarName = DictionaryList[i].DictionaryCategoryPrefix + "DictPct_" + i.ToString();
                    }
                    DictionaryList[i].DictData.totalNumberOfMatchesDictName = totalMatchesTrackerVarName;
                    TempHeaderData.Add(TotalCatCount, totalMatchesTrackerVarName);
                    TempOutputDataMap.Add(totalMatchesTrackerVarName, TotalCatCount);
                    TotalCatCount++;


                    DictParser DP = new DictParser();
                    DictionaryList[i].DictData = DP.ParseDict(DictionaryList[i]);

                    //add all of the categories to the header
                    for (int j = 0; j < DictionaryList[i].DictData.NumCats; j++)
                    {

                        

                        int increment = 1;
                        //makes sure that we don't have duplicate category names
                        string CatNameRoot = DictionaryList[i].DictData.CatNames[j];
                        while (TempOutputDataMap.ContainsKey(DictionaryList[i].DictionaryCategoryPrefix + DictionaryList[i].DictData.CatNames[j]))
                        {
                            DictionaryList[i].DictData.CatNames[j] = CatNameRoot + "_" + increment.ToString();
                            increment++;
                        }

                        TempHeaderData.Add(TotalCatCount,
                           DictionaryList[i].DictionaryCategoryPrefix + DictionaryList[i].DictData.CatNames[j]);

                        TempOutputDataMap.Add(DictionaryList[i].DictionaryCategoryPrefix + DictionaryList[i].DictData.CatNames[j],
                            TotalCatCount);
                        TotalCatCount++;
                    }
                }
            }

            OutputHeaderData = TempHeaderData;
            OutputDataMap = TempOutputDataMap;
        }

        public bool InspectSettings()
        {
            return true;
        }

        



        //one of the few plugins thus far where I'm actually using a constructor
        //might not be the most efficient way to handle this (especially at runtime)
        //but I don't suspect that it'll be too bad.
        public WeightedDictionary()
        {
            DictionaryList = new List<DictionaryMetaObject>();
            ListOfBuiltInDictionaries = new HashSet<string>();

            DictionaryList.Add(new DictionaryMetaObject("Affective Norms, Warriner",
                                                         "Warriner, A. B., Kuperman, V., & Brysbaert, M. (2013). Norms of valence, arousal, and dominance for 13,915 English lemmas. Behavior Research Methods, 45(4), 1191–1207. https://doi.org/10.3758/s13428-012-0314-x",
                                                         "WAN_",
                                                         Properties.Resources.WarrinerNorms));

            DictionaryList.Add(new DictionaryMetaObject("AFINN-96",
                                                         "AFINN is a list of English words rated for valence with an integer between minus five (negative) and plus five (positive). The words have been manually labeled by Finn Årup Nielsen in 2009-2011." + Environment.NewLine + Environment.NewLine +
                                                         "Hansen, L. K., Arvidsson, A., Nielsen, F. A., Colleoni, E., & Etter, M. (2011). Good Friends, Bad News - Affect and Virality in Twitter. In J. J. Park, L. T. Yang, & C. Lee (Eds.), Future Information Technology (pp. 34–43). Springer Berlin Heidelberg." + Environment.NewLine + Environment.NewLine +
                                                         "Nielsen, F. Å. (2011). A new ANEW: Evaluation of a word list for sentiment analysis in microblogs. ArXiv:1103.2903 [Cs]. Retrieved from http://arxiv.org/abs/1103.2903",
                                                         "AFINN_96_",
                                                         Properties.Resources.AFINN_96));

            DictionaryList.Add(new DictionaryMetaObject("AFINN-111",
                                                         "An updated version of AFINN-96." + Environment.NewLine + Environment.NewLine +
                                                         "Hansen, L. K., Arvidsson, A., Nielsen, F. A., Colleoni, E., & Etter, M. (2011). Good Friends, Bad News - Affect and Virality in Twitter. In J. J. Park, L. T. Yang, & C. Lee (Eds.), Future Information Technology (pp. 34–43). Springer Berlin Heidelberg." + Environment.NewLine + Environment.NewLine +
                                                         "Nielsen, F. Å. (2011). A new ANEW: Evaluation of a word list for sentiment analysis in microblogs. ArXiv:1103.2903 [Cs]. Retrieved from http://arxiv.org/abs/1103.2903",
                                                         "AFINN_111_",
                                                         Properties.Resources.AFINN_111));

            DictionaryList.Add(new DictionaryMetaObject("Authenticity Norms",
                                                         "Kovács, B., Carroll, G. R., & Lehman, D. W. (2013). Authenticity and Consumer Value Ratings: Empirical Tests from the Restaurant Domain. Organization Science. https://doi.org/10.1287/orsc.2013.0843",
                                                         "Authenticity_",
                                                         Properties.Resources.AuthenticityNorms));

            DictionaryList.Add(new DictionaryMetaObject("Concreteness Norms, Brysbaert",
                                                         "Brysbaert, M., Warriner, A. B., & Kuperman, V. (2014). Concreteness ratings for 40 thousand generally known English word lemmas. Behavior Research Methods, 46(3), 904–911. https://doi.org/10.3758/s13428-013-0403-5",
                                                         "BRYS_",
                                                         Properties.Resources.BrysbaertConcreteness));

            DictionaryList.Add(new DictionaryMetaObject("Depeche Mood",
                                                         "Staiano, J., & Guerini, M. (2014). Depeche Mood: A Lexicon for Emotion Analysis from Crowd Annotated News. Proceedings of the 52nd Annual Meeting of the Association for Computational Linguistics (Volume 2: Short Papers), 427–433. https://doi.org/10.3115/v1/P14-2070" + Environment.NewLine + Environment.NewLine +
                                                         "*Note that scores from the original norm set are collapsed across parts of speech.",
                                                         "DM_",
                                                         Properties.Resources.DepecheMood));

            DictionaryList.Add(new DictionaryMetaObject("DIC-LSA Norms",
                                                         "Bestgen, Y., & Vincze, N. (2012). Checking and bootstrapping lexical norms by means of word similarity indexes. Behavior Research Methods, 44(4), 998–1006. https://doi.org/10.3758/s13428-012-0195-z",
                                                         "DICLSA_",
                                                         Properties.Resources.DICLSA));

            DictionaryList.Add(new DictionaryMetaObject("Embodiment Norms",
                                                         "https://psyc.ucalgary.ca/languageprocessing/node/22",
                                                         "EmbNorm_",
                                                         Properties.Resources.Embodiment));

            DictionaryList.Add(new DictionaryMetaObject("Evaluative Lexicon 2.0",
                                                         "Rocklage, M. D., Rucker, D. D., & Nordgren, L. F. (2018). The Evaluative Lexicon 2.0: The measurement of emotionality, extremity, and valence in language. Behavior Research Methods, 50(4), 1327–1344. https://doi.org/10.3758/s13428-017-0975-6",
                                                         "EL2_",
                                                         Properties.Resources.EvaluativeLexicon2));

            DictionaryList.Add(new DictionaryMetaObject("Gender Norms",
                                                         "[PREPRINT] Lewis, M., Borkenhagen, M. C., Converse, E., Lupyan, G., & Seidenberg, M. S. (2020, March 30). What might books be teaching young children about gender?. https://doi.org/10.31234/osf.io/ntgfe",
                                                         "GendNorm_",
                                                         Properties.Resources.Molly_Lewis_Gender_Norms__2020_03_19_));

            DictionaryList.Add(new DictionaryMetaObject("Humor Norms",
                                                         "Engelthaler, T., & Hills, T. T. (2018). Humor norms for 4,997 English words. Behavior Research Methods, 50(3), 1116–1124. https://doi.org/10.3758/s13428-017-0930-6",
                                                         "Humor_",
                                                         Properties.Resources.Humor));

            DictionaryList.Add(new DictionaryMetaObject("LabMT Norms",
                                                         "Dodds, P. S., Harris, K. D., Kloumann, I. M., Bliss, C. A., & Danforth, C. M. (2011). Temporal Patterns of Happiness and Information in a Global Social Network: Hedonometrics and Twitter. PLOS ONE, 6(12), e26752. https://doi.org/10.1371/journal.pone.0026752",
                                                         "LabMT_",
                                                         Properties.Resources.LabMT_Norms));

            DictionaryList.Add(new DictionaryMetaObject("Lancaster Sensorimotor Norms",
                                                         "Lynott, D., Connell, L., Brysbaert, M., Brand, J., & Carney, J. (2019). The Lancaster Sensorimotor Norms: Multidimensional measures of Perceptual and Action Strength for 40,000 English words [Preprint]. https://doi.org/10.31234/osf.io/ktjwp" + Environment.NewLine + Environment.NewLine +
                                                         "Variable Descriptions:" + Environment.NewLine + 
                                                         "----------------------" + Environment.NewLine +
                                                         "Auditory_M: Auditory strength: mean rating (0–5) of how strongly the concept is experienced by hearing" + Environment.NewLine + Environment.NewLine +
                                                        "Gustatory_M: Gustatory strength: mean rating (0–5) of how strongly the concept is experienced by tasting" + Environment.NewLine + Environment.NewLine +
                                                        "Haptic_M: Haptic strength: mean rating (0–5) of how strongly the concept is experienced by feeling through touch" + Environment.NewLine + Environment.NewLine +
                                                        "Interoceptive_M: Interoceptive strength: mean rating (0–5) of how strongly the concept is experienced by sensations inside the body" + Environment.NewLine + Environment.NewLine +
                                                        "Olfactory_M: Olfactory strength: mean rating (0–5) of how strongly the concept is experienced by smelling" + Environment.NewLine + Environment.NewLine +
                                                        "Visual_M: Visual strength: mean rating (0–5) of how strongly the concept is experienced by seeing" + Environment.NewLine + Environment.NewLine +
                                                        "Foot_leg_M: Foot action strength: mean rating (0–5) of how strongly the concept is experienced by performaing an action with the foot / leg" + Environment.NewLine + Environment.NewLine +
                                                        "Hand_arm_M: Hand action strength: mean rating (0–5) of how strongly the concept is experienced by performaing an action with the hand / arm" + Environment.NewLine + Environment.NewLine +
                                                        "Head_M: Head action strength: mean rating (0–5) of how strongly the concept is experienced by performaing an action with the head excluding mouth" + Environment.NewLine + Environment.NewLine +
                                                        "Mouth_M: Mouth action strength: mean rating (0–5) of how strongly the concept is experienced by performaing an action with the mouth / throat" + Environment.NewLine + Environment.NewLine +
                                                        "Torso_M: Torso action strength: mean rating (0–5) of how strongly the concept is experienced by performaing an action with the torso" + Environment.NewLine + Environment.NewLine +
                                                        "Auditory_MSD: Mean of the Standard Deviations of auditory strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Gustatory_MSD: Mean of the Standard Deviations of gustatory strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Haptic_MSD: Mean of the Standard Deviations of haptic strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Interoceptive_MSD: Mean of the Standard Deviations of interoceptive strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Olfactory_MSD: Mean of the Standard Deviations of olfactory strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Visual_MSD: Mean of the Standard Deviations of visual strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Foot_leg_MSD: Mean of the Standard Deviations of foot action strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Hand_arm_MSD: Mean of the Standard Deviations of hand action strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Head_MSD: Mean of the Standard Deviations of head action strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Mouth_MSD: Mean of the Standard Deviations of mouth action strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "Torso_MSD: Mean of the Standard Deviations of torso action strength ratings" + Environment.NewLine + Environment.NewLine +
                                                        "MaxStrength_Perceptual: Perceptual strength in the dominant modality (i.e., highest strength rating across six perceptual modalities)" + Environment.NewLine + Environment.NewLine +
                                                        "Minkowski3_Perceptual: Aggregated perceptual strength in all modalities where the influence of weaker modalities is attenuated, calculated as Minkowski distance (with exponent 3) of the 6-dimension vector of perceptual strength from the origin." + Environment.NewLine + Environment.NewLine +
                                                        "Exclusivity_Perceptual: Modality exclusivity of the concept; the extent to which a concept is experienced though a single perceptual modality (0–1, typically expressed as %), calculated as the range of perceptual strength values divided by their sum" + Environment.NewLine + Environment.NewLine +
                                                        "MaxStrength_Action: Action strength in the dominant effector (i.e., highest strength rating across five action effectors)" + Environment.NewLine + Environment.NewLine +
                                                        "Minkowski3_Action: Aggregated action strength in all effectors where the influence of weaker effectors is attenuated, calculated as Minkowski distance (with exponent 3) of the 5-dimension vector of action strength from the origin." + Environment.NewLine + Environment.NewLine +
                                                        "Exclusivity_Action: Effector exclusivity of the concept; the extent to which a concept is experienced though a single action effector (0–1, typically expressed as %), calculated as the range of action strength values divided by their sum" + Environment.NewLine + Environment.NewLine +
                                                        "MaxStrength_Sensorimotor: Sensorimotor strength in the dominant dimension (i.e., highest strength rating across 11 sensorimotor dimensions)" + Environment.NewLine + Environment.NewLine +
                                                        "Minkowski3_Sensorimotor: Aggregated sensorimotor strength in all dimensions where the influence of weaker dimensions is attenuated, calculated as Minkowski distance (with exponent 3) of the 11-dimension vector of sensorimotor strength from the origin." + Environment.NewLine + Environment.NewLine +
                                                        "Exclusivity_Sensorimotor: Sensorimotor exclusivity of the concept; the extent to which a concept is experienced though a single sensorimotor dimension (0–1, typically expressed as %), calculated as the range of sensorimotor strength values divided by their sum" + Environment.NewLine + Environment.NewLine +
                                                        "N_Known_Perceptual: Number of participants in perceptual strength norming who knew the concept well enough to provide valid ratings (i.e., as opposed to selecting the “don’t know” option)" + Environment.NewLine + Environment.NewLine +
                                                        "List_N_Perceptual: Number of valid participants in perceptual strength norming who completed the item list featuring the concept (i.e., who were presented with the concept for rating)" + Environment.NewLine + Environment.NewLine +
                                                        "Pct_Known_Perceptual: Percentage of participants (0–1) in perceptual strength norming who knew the concept well enough to provide valid ratings, calculated as N_known.perceptual divided by List_N.perceptual" + Environment.NewLine + Environment.NewLine +
                                                        "N_Known_Action: Number of participants in action strength norming who knew the concept well enough to provide valid ratings (i.e., as opposed to selecting the “don’t know” option)" + Environment.NewLine + Environment.NewLine +
                                                        "List_N_Action: Number of valid participants in action strength norming who completed the item list featuring the concept (i.e., who were presented with the concept for rating)" + Environment.NewLine + Environment.NewLine +
                                                        "Pct_Known_Action: Percentage of participants (0–1) in action strength norming who knew the concept well enough to provide valid ratings, calculated as N_known.action divided by List_N.action" + Environment.NewLine + Environment.NewLine +
                                                        "Mean_Age_Perceptual: Average age of participants in perceptual strength norming who completed the item list featuring the concept (i.e., who were presented with the concept for rating)" + Environment.NewLine + Environment.NewLine +
                                                        "Mean_Age_Action: Average age of participants in action strength norming who completed the item list featuring the concept (i.e., who were presented with the concept for rating)",
                                                         "Lanc_",
                                                         Properties.Resources.LancasterSensorimotorNorms));

            DictionaryList.Add(new DictionaryMetaObject("Modality Norms, Adj + Nouns",
                                                      "Lynott, D., & Connell, L. (2009). Modality exclusivity norms for 423 object properties. Behavior Research Methods, 41(2), 558–564. https://doi.org/10.3758/BRM.41.2.558" + Environment.NewLine + Environment.NewLine +
                                                      "Lynott, D., & Connell, L. (2013). Modality exclusivity norms for 400 nouns: The relationship between perceptual experience and surface word form. Behavior Research Methods, 45(2), 516–526. https://doi.org/10.3758/s13428-012-0267-0",
                                                      "MOD_",
                                                      Properties.Resources.ModalityNorms));

            DictionaryList.Add(new DictionaryMetaObject("MoralStrength Lexicon",
                                                      "Araque, O., Gatti, L., & Kalimeri, K. (2020). MoralStrength: Exploiting a moral lexicon and embedding similarity for moral foundations prediction. Knowledge-Based Systems, 191, 105184. https://doi.org/10.1016/j.knosys.2019.105184" + Environment.NewLine + Environment.NewLine +
                                                      "For this dictionary, it is recommended that you lemmatize your input texts.",
                                                      "MorStr_",
                                                      Properties.Resources.MoralStrength));

            DictionaryList.Add(new DictionaryMetaObject("Psycholinguistic Features (Bootstrapped)",
                                                      "Paetzold, G., & Specia, L. (2016). Inferring Psycholinguistic Properties of Words. In Proceedings of the 2016 Conference of the North American Chapter of the Association for Computational Linguistics: Human Language Technologies (pp. 435–440). San Diego, California: Association for Computational Linguistics. https://doi.org/10.18653/v1/N16-1050",
                                                      "BPL_",
                                                      Properties.Resources.PsychoLingFeat));

            DictionaryList.Add(new DictionaryMetaObject("Stereotype Content Dictionary",
                                                      "Nicolas, G., Bai, X., & Fiske, S. (2019). Automated Dictionary Creation for Analyzing Text: An Illustration from Stereotype Content [Preprint]. https://doi.org/10.31234/osf.io/afm8k",
                                                      "SCD_",
                                                      Properties.Resources.StereotypeContentDictionary));

            DictionaryList.Add(new DictionaryMetaObject("Subjectivity Lexicon",
                                                      "Wilson, T., Wiebe, J., & Hoffmann, P. (2005). Recognizing contextual polarity in phrase-level sentiment analysis. Proceedings of the Conference on Human Language Technology and Empirical Methods in Natural Language Processing, 347–354. https://doi.org/10.3115/1220575.1220619",
                                                      "SubjLex_",
                                                      Properties.Resources.Subjectivity_Lexicon_Averaged));

            DictionaryList.Add(new DictionaryMetaObject("Tabooness Norms",
                                                      "Janschewitz, K. (2008). Taboo, emotionally valenced, and emotionally neutral word norms. Behavior Research Methods, 40(4), 1065–1074. https://doi.org/10.3758/BRM.40.4.1065" + Environment.NewLine + Environment.NewLine +
                                                      "Scores texts using the entire set of participant norms, plus the male- and female-derived norms separately. Within the variable names, \"M\" stands for the Mean from the norms, and \"MSD\" stands for the Mean of the norm's Standard Deviation.",
                                                      "TN_",
                                                      Properties.Resources.Tabooness));

            DictionaryList.Add(new DictionaryMetaObject("Weighted Referential Activity Dictionary",
                                                         "Bucci, W., & Maskit, B. (2006). A weighted dictionary for Referential Activity. In J. G. Shanahan, Y. Qu, & J. Wiebe (Eds.), Computing Attitude and Affect in Text (pp. 49–60). Dordrecht, The Netherlands: Springer.",
                                                         "WRAD_",
                                                         Properties.Resources.WRAD));

            DictionaryList.Add(new DictionaryMetaObject("Weighted Reflection-Reorganizing List",
                                                         "Maskit, B. (2012, September). The Discourse Attributes Analysis Program (DAAP) (Series 8) [Computer software]. Unpublished computer software." + Environment.NewLine + Environment.NewLine +
                                                         "Murphy, S.M., Bucci, W. & Maskit (2011, June). The language of psychotherapy process: Cross-linguistic markers of narrative and Referential Activity using the Linguistic Inquiry Word Count (LIWC). In S. Murphy (Moderator), Cross-linguistic studies in narrative, emotional expression and the Referential Process. Panel presented at the 42nd International Annual Meeting of The Society for Psychotherapy Research, Bern, Switzerland.",
                                                         "WRRL_",
                                                         Properties.Resources.WRRL));

            DictionaryList.Add(new DictionaryMetaObject("WWBP Age Estimator",
                                                         "Sap, M., Park, G., Eichstaedt, J., Kern, M., Stillwell, D., Kosinski, M., … Schwartz, H. A. (2014). Developing Age and Gender Predictive Lexica over Social Media. In Proceedings of the 2014 Conference on Empirical Methods in Natural Language Processing (EMNLP) (pp. 1146–1151). Doha, Qatar: Association for Computational Linguistics. https://doi.org/10.3115/v1/D14-1121",
                                                         "WWBP_Age_",
                                                         Properties.Resources.WWBP_Age));

            DictionaryList.Add(new DictionaryMetaObject("WWBP Distress Lexicon",
                                                         "Sedoc, J., Buechel, S., Nachmany, Y., Buffone, A., & Ungar, L. (2020). Learning word ratings for empathy and distress from document-level user responses. Proceedings of The 12th Language Resources and Evaluation Conference, 1664–1673. https://www.aclweb.org/anthology/2020.lrec-1.206",
                                                         "WWBP_Dst_",
                                                         Properties.Resources.WWBP_distress_lexicon));

            DictionaryList.Add(new DictionaryMetaObject("WWBP Empathy Lexicon",
                                                         "Sedoc, J., Buechel, S., Nachmany, Y., Buffone, A., & Ungar, L. (2020). Learning word ratings for empathy and distress from document-level user responses. Proceedings of The 12th Language Resources and Evaluation Conference, 1664–1673. https://www.aclweb.org/anthology/2020.lrec-1.206",
                                                         "WWBP_Emp_",
                                                         Properties.Resources.WWBP_empathy_lexicon));

            DictionaryList.Add(new DictionaryMetaObject("WWBP Gender Estimator",
                                                         "Sap, M., Park, G., Eichstaedt, J., Kern, M., Stillwell, D., Kosinski, M., … Schwartz, H. A. (2014). Developing Age and Gender Predictive Lexica over Social Media. In Proceedings of the 2014 Conference on Empirical Methods in Natural Language Processing (EMNLP) (pp. 1146–1151). Doha, Qatar: Association for Computational Linguistics. https://doi.org/10.3115/v1/D14-1121",
                                                         "WWBP_Gender_",
                                                         Properties.Resources.WWBP_Gender));

            DictionaryList.Add(new DictionaryMetaObject("WWBP Dark Triad",
                                                         "Preotiuc-Pietro, D., Carpenter, J., Giorgi, S., & Ungar, L. (2016). Studying the Dark Triad of Personality Through Twitter Behavior. In Proceedings of the 25th ACM International on Conference on Information and Knowledge Management (pp. 761–770). New York, NY, USA: ACM. https://doi.org/10.1145/2983323.2983822",
                                                         "WWBP_D3_",
                                                         Properties.Resources.WWBP_D3));

            DictionaryList.Add(new DictionaryMetaObject("WWBP PERMA Lexicon",
                                                         "Schwartz, H. A., Sap, M., Kern, M. L., Eichstaedt, J. C., Kapelner, A., Agrawal, M., … Ungar, L. H. (2015). Predicting individual well-being through the language of social media. In Biocomputing 2016 (Vols. 1–0, pp. 516–527). WORLD SCIENTIFIC. https://doi.org/10.1142/9789814749411_0047",
                                                         "WWBP_PERMA_",
                                                         Properties.Resources.WWBP_PERMA));

            DictionaryList.Add(new DictionaryMetaObject("WWBP Prospection Lexicon",
                                                         "http://www.wwbp.org/lexica.html",
                                                         "WWBP_PROSP_",
                                                         Properties.Resources.WWBP_Prospection));


            foreach (DictionaryMetaObject dict in DictionaryList)
            {
                ListOfBuiltInDictionaries.Add(dict.DictionaryName);
            }



        }



        public Payload FinishUp(Payload Input)
        {
            return (Input);
        }




        #region Import/Export Settings
        public void ImportSettings(Dictionary<string, string> SettingsDict)
        {
            foreach (DictionaryMetaObject dict in DictionaryList)
            {
                if (SettingsDict.ContainsKey(XmlConvert.EncodeName(dict.DictionaryName)))
                {
                    dict.UseDictionary = Boolean.Parse(SettingsDict[XmlConvert.EncodeName(dict.DictionaryName)]);
                }
                else
                {
                    dict.UseDictionary = false;
                }
            }
        }



        public Dictionary<string, string> ExportSettings(bool suppressWarnings)
        {
            Dictionary<string, string> SettingsDict = new Dictionary<string, string>();
            bool UsingCustomDictionary = false;

            foreach (DictionaryMetaObject dict in DictionaryList)
            {
                if (ListOfBuiltInDictionaries.Contains(dict.DictionaryName))
                {
                    SettingsDict.Add(XmlConvert.EncodeName(dict.DictionaryName), dict.UseDictionary.ToString());
                }
                else
                {
                    //we only show this message if the user has loaded in a custom dictionary
                    if (!UsingCustomDictionary)
                    {
                        UsingCustomDictionary = true;
                        if (!suppressWarnings) MessageBox.Show("Currently, the \"" + PluginName + "\" plugin does not store custom user dictionaries when exported as part of a pipeline. When you load this pipeline later, you will need to reload any custom dictionaries that you are currently using with this plugin. This feature may be added to a later version of the plugin.", "Pipeline Save Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            
            return (SettingsDict);
        }
        #endregion







    }
}
