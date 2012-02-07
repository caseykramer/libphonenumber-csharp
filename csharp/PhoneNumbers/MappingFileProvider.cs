﻿/*
 * Copyright (C) 2011 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneNumbers
{
    /**
    * A utility which knows the data files that are available for the geocoder to use. The data files
    * contain mappings from phone number prefixes to text descriptions, and are organized by country
    * calling code and language that the text descriptions are in.
    *
    * @author Shaopeng Jia
    */
    public class MappingFileProvider
    {
        private int numOfEntries = 0;
        private int[] countryCallingCodes;
        private List<HashSet<String>> availableLanguages;
        private static readonly Dictionary<String, String> LOCALE_NORMALIZATION_MAP;

        static MappingFileProvider()
        {
            var normalizationMap = new Dictionary<String, String>();
            normalizationMap["zh_TW"] = "zh_Hant";
            normalizationMap["zh_HK"] = "zh_Hant";
            normalizationMap["zh_MO"] = "zh_Hant";
            LOCALE_NORMALIZATION_MAP = normalizationMap;
        }

        /**
        * Creates an empty {@link MappingFileProvider}. The default constructor is necessary for
        * implementing {@link Externalizable}. The empty provider could later be populated by
        * {@link #readFileConfigs(java.util.SortedMap)} or {@link #readExternal(java.io.ObjectInput)}.
        */
        public MappingFileProvider()
        {
        }

        /**
         * Initializes an {@link MappingFileProvider} with {@code availableDataFiles}.
         *
         * @param availableDataFiles  a map from country calling codes to sets of languages in which data
         *     files are available for the specific country calling code. The map is sorted in ascending
         *     order of the country calling codes as integers.
         */
        public void ReadFileConfigs(SortedDictionary<int, HashSet<String>> availableDataFiles)
        {
            numOfEntries = availableDataFiles.Count;
            countryCallingCodes = new int[numOfEntries];
            availableLanguages = new List<HashSet<String>>(numOfEntries);
            int index = 0;
            foreach (int countryCallingCode in availableDataFiles.Keys)
            {
                countryCallingCodes[index++] = countryCallingCode;
                availableLanguages.Add(new HashSet<String>(availableDataFiles[countryCallingCode]));
            }
        }

        /**
         * Returns a string representing the data in this class. The string contains one line for each
         * country calling code. The country calling code is followed by a '|' and then a list of
         * comma-separated languages sorted in ascending order.
         */
        public override String ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < numOfEntries; i++)
            {
                output.Append(countryCallingCodes[i]);
                output.Append('|');
                foreach (var lang in availableLanguages[i].OrderBy(a => a))
                {
                    output.Append(lang);
                    output.Append(',');
                }
                output.Append('\n');
            }
            return output.ToString();
        }

        /**
         * Gets the name of the file that contains the mapping data for the {@code countryCallingCode} in
         * the language specified.
         *
         * @param countryCallingCode  the country calling code of phone numbers which the data file
         *     contains
         * @param language  two-letter lowercase ISO language codes as defined by ISO 639-1
         * @param script  four-letter titlecase (the first letter is uppercase and the rest of the letters
         *     are lowercase) ISO script codes as defined in ISO 15924
         * @param region  two-letter uppercase ISO country codes as defined by ISO 3166-1
         * @return  the name of the file, or empty string if no such file can be found
         */
        public String GetFileName(int countryCallingCode, String language, String script, String region)
        {
            if (language.Length == 0)
            {
                return "";
            }
            int index = Array.BinarySearch(countryCallingCodes, countryCallingCode);
            if (index < 0)
            {
                return "";
            }
            var setOfLangs = availableLanguages[index];
            if (setOfLangs.Count > 0)
            {
                String languageCode = FindBestMatchingLanguageCode(setOfLangs, language, script, region);
                if (languageCode.Length > 0)
                {
                    StringBuilder fileName = new StringBuilder();
                    fileName.Append(countryCallingCode).Append('_').Append(languageCode);
                    return fileName.ToString();
                }
            }
            return "";
        }

        private String FindBestMatchingLanguageCode(
            HashSet<String> setOfLangs, String language, String script, String region)
        {
            StringBuilder fullLocale = ConstructFullLocale(language, script, region);
            String fullLocaleStr = fullLocale.ToString();
            String normalizedLocale;
            if (LOCALE_NORMALIZATION_MAP.TryGetValue(fullLocaleStr, out normalizedLocale))
            {
                if (setOfLangs.Contains(normalizedLocale))
                {
                    return normalizedLocale;
                }
            }
            if (setOfLangs.Contains(fullLocaleStr))
            {
                return fullLocaleStr;
            }

            if (OnlyOneOfScriptOrRegionIsEmpty(script, region))
            {
                if (setOfLangs.Contains(language))
                {
                    return language;
                }
            }
            else if (script.Length > 0 && region.Length > 0)
            {
                StringBuilder langWithScript = new StringBuilder(language).Append('_').Append(script);
                String langWithScriptStr = langWithScript.ToString();
                if (setOfLangs.Contains(langWithScriptStr))
                {
                    return langWithScriptStr;
                }

                StringBuilder langWithRegion = new StringBuilder(language).Append('_').Append(region);
                String langWithRegionStr = langWithRegion.ToString();
                if (setOfLangs.Contains(langWithRegionStr))
                {
                    return langWithRegionStr;
                }

                if (setOfLangs.Contains(language))
                {
                    return language;
                }
            }
            return "";
        }

        private bool OnlyOneOfScriptOrRegionIsEmpty(String script, String region)
        {
            return (script.Length == 0 && region.Length > 0) ||
                    (region.Length == 0 && script.Length > 0);
        }

        private StringBuilder ConstructFullLocale(String language, String script, String region)
        {
            StringBuilder fullLocale = new StringBuilder(language);
            AppendSubsequentLocalePart(script, fullLocale);
            AppendSubsequentLocalePart(region, fullLocale);
            return fullLocale;
        }

        private void AppendSubsequentLocalePart(String subsequentLocalePart, StringBuilder fullLocale)
        {
            if (subsequentLocalePart.Length > 0)
            {
                fullLocale.Append('_').Append(subsequentLocalePart);
            }
        }
    }
}