using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace WeightedDictionary
{
    internal partial class SettingsForm_WeightedDictionary : Form
    {


        #region Get and Set Options

        public List<DictionaryMetaObject> DictDataToReturn;
        public Dictionary<string, string> DictDescriptions;
        public Dictionary<string, string> DictPrefixes;
        public bool RawFreqs;

       #endregion



        public SettingsForm_WeightedDictionary(List<DictionaryMetaObject> Dicts)
        {
            InitializeComponent();

            DictDataToReturn = Dicts;
            DictDescriptions = new Dictionary<string, string>();
            DictPrefixes = new Dictionary<string, string>();


            foreach (DictionaryMetaObject Dict in Dicts)
            {
                SelectedDictionariesCheckedListbox.Items.Add(Dict.DictionaryName);

                if (Dict.UseDictionary) SelectedDictionariesCheckedListbox.SetItemChecked(
                                                    SelectedDictionariesCheckedListbox.Items.IndexOf(Dict.DictionaryName), Dict.UseDictionary);
                try
                { 
                    DictDescriptions.Add(Dict.DictionaryName, Dict.DictionaryDescription);
                    DictPrefixes.Add(Dict.DictionaryName, Dict.DictionaryCategoryPrefix);
                }
                catch
                {
                }
            }

            SelectedDictionariesCheckedListbox.SelectedIndex = 0;

            
        }










        private void OKButton_Click(object sender, System.EventArgs e)
        {

            for (int i = 0; i < DictDataToReturn.Count; i++)
            {
                DictDataToReturn[i].UseDictionary = SelectedDictionariesCheckedListbox.GetItemChecked(SelectedDictionariesCheckedListbox.Items.IndexOf(DictDataToReturn[i].DictionaryName));
            }

            this.DialogResult = DialogResult.OK;

        }

        private void SelectedDictionariesCheckedListbox_Click(object sender, System.EventArgs e)
        {
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            if (SelectedDictionariesCheckedListbox.SelectedItem != null)
            {
                DictionaryDescriptionTextbox.Text = SelectedDictionariesCheckedListbox.SelectedItem.ToString() + Environment.NewLine + Environment.NewLine +
                    "Output Prefix: " + DictPrefixes[SelectedDictionariesCheckedListbox.SelectedItem.ToString()] + Environment.NewLine + Environment.NewLine +
                    DictDescriptions[SelectedDictionariesCheckedListbox.SelectedItem.ToString()];
            }
                
        }

        private void SelectedDictionariesCheckedListbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            UpdateDescription();
        }

        private void SelectedDictionariesCheckedListbox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            UpdateDescription();
        }

        private void CheckAllButton_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < SelectedDictionariesCheckedListbox.Items.Count; i++) SelectedDictionariesCheckedListbox.SetItemChecked(i, true);
        }

        private void UncheckAllButton_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < SelectedDictionariesCheckedListbox.Items.Count; i++) SelectedDictionariesCheckedListbox.SetItemChecked(i, false);
        }







        private void LoadDictionaryButton_Click(object sender, System.EventArgs e)
        {


            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.ValidateNames = true;
                dialog.Title = "Please choose the Dictionary file that you would like to read";
                dialog.FileName = "Weighted Dictionary.txt";
                dialog.Filter = "Weighted Dictionary File (*.txt)|*.txt";
                if (dialog.ShowDialog() == DialogResult.OK)
                {


                    try
                    {
                        string DicText = File.ReadAllText(dialog.FileName, Encoding.UTF8);
                        DictionaryMetaObject InputDictData = new DictionaryMetaObject(Path.GetFileName(dialog.FileName), "User-loaded dictionary", "", DicText);
                        DictParser DP = new DictParser();
                        InputDictData.DictData = DP.ParseDict(InputDictData);
                        DictDataToReturn.Add(InputDictData);
                        SelectedDictionariesCheckedListbox.Items.Add(InputDictData.DictionaryName);
                        SelectedDictionariesCheckedListbox.SetItemChecked(SelectedDictionariesCheckedListbox.Items.IndexOf(InputDictData.DictionaryName), true);
                        DictPrefixes.Add(InputDictData.DictionaryName, "");
                        DictDescriptions.Add(InputDictData.DictionaryName, InputDictData.DictionaryDescription);


                    }
                    catch
                    {
                        MessageBox.Show("There was an error while trying to read/load your dictionary file. Is your dictionary correctly formatted? Have you already added this dictionary to this plugin?", "Error reading dictionary", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                }



            }



        }




























    }
}
