using System;
using System.Collections.Generic;

namespace ESRI.SilverlightViewer.Utility
{
    public partial class StringHelper
    {
        /// <summary>
        /// Convert a string value to bool (non-case-sensitive)
        /// </summary>
        /// <param name="s">A string to be converted</param>
        /// <returns>Returns true if the string equals "T", "Y", "True" or "Yes", and false for others</returns>
        public static bool ConvertToBool(string s)
        {
            bool b = false;

            if (!string.IsNullOrEmpty(s))
            {
                s = s.ToUpper();
                if (s.Equals("TRUE") || s.Equals("T") || s.Equals("YES") || s.Equals("Y"))
                {
                    b = true;
                }
            }

            return b;
        }

        /// <summary>
        /// Convert a string into a dictionary 
        /// </summary>
        /// <param name="value">A string value, e.g. "State=New Jersey|Capital=Trenton|Nickname=The Garden State"</param>
        /// <param name="separator1">The first separator. e.g. "|" in the example</param>
        /// <param name="separator2">The second separator. e.g. "=" in the example</param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertToDictionary(string value, char separator1, char separator2)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(value))
            {
                string[] arrParam = null;
                string[] arrPair = value.Split(separator1);

                for (int i = 0; i < arrPair.Length; i++)
                {
                    arrParam = arrPair[i].Split(separator2);
                    if (!dict.ContainsKey(arrParam[0])) dict.Add(arrParam[0], arrParam[1]);
                }
            }

            return dict;
        }

        /// <summary>
        /// Convert Keys and Values into a dictionary, e.g. Keys="NAME,ADRESS" Values="Frank,100 Main St"
        /// </summary>
        /// <param name="keys">The keys string</param>
        /// <param name="values">The values string</param>
        /// <param name="separator">Separator of keys and values</param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertToDictionary(string keys, string values, char separator)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(keys) && !string.IsNullOrEmpty(values))
            {
                string[] arrKeys = keys.Split(separator);
                string[] arrValues = values.Split(separator);

                int count = Math.Min(arrKeys.Length, arrValues.Length);

                for (int i = 0; i < count; i++)
                {
                    if (!dict.ContainsKey(arrKeys[i])) dict.Add(arrKeys[i], arrValues[i]);
                }
            }

            return dict;
        }
    }
}
