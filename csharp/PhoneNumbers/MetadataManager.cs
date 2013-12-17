/*
 * Copyright (C) 2012 The Libphonenumber Authors
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
using System.Reflection;

namespace PhoneNumbers
{
    /**
    * Class encapsulating loading of PhoneNumber Metadata information. Currently this is used only for
    * additional data files such as PhoneNumberAlternateFormats, but in the future it is envisaged it
    * would handle the main metadata file (PhoneNumberMetaData.xml) as well.
    *
    * @author Lara Rennie
    */

    public class MetadataManager
    {
        internal const String ALTERNATE_FORMATS_FILE_PREFIX = "PhoneNumberAlternateFormats.xml";
        internal const String SHORT_NUMBER_METADATA_FILE_PREFIX = "ShortNumberMetadata.xml";

        private static readonly Dictionary<int, PhoneMetadata> CallingCodeToAlternateFormatsMap =
            new Dictionary<int, PhoneMetadata>();

        // A set of which country calling codes there are alternate format data for. If the set has an
        // entry for a code, then there should be data for that code linked into the resources.
        private static readonly Dictionary<int, List<String>> CountryCodeSet =
            BuildMetadataFromXml.GetCountryCodeToRegionCodeMap(ALTERNATE_FORMATS_FILE_PREFIX);

        // A set of which region codes there are short number data for. If the set has an entry for a
        // code, then there should be data for that code linked into the resources.
        private static Dictionary<string, PhoneMetadata> RegionCodeToShortNumberMetadataMap = null;
            

        private static readonly object _regionCodeMapLock = new object();


        private MetadataManager()
        {
        }

        private static void LoadAlternateFormatsMedataFromFile(String filePrefix)
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(filePrefix)) ?? "missing";
            using (var stream = asm.GetManifestResourceStream(name))
            {
                var meta = BuildMetadataFromXml.BuildPhoneMetadataCollection(stream, false);
                foreach (var m in meta.MetadataList)
                {
                    CallingCodeToAlternateFormatsMap[m.CountryCode] = m;
                }
            }
        }

        public static PhoneMetadata GetAlternateFormatsForCountry(int countryCallingCode)
        {
            lock (CallingCodeToAlternateFormatsMap)
            {
                if (!CountryCodeSet.ContainsKey(countryCallingCode))
                    return null;
                if (!CallingCodeToAlternateFormatsMap.ContainsKey(countryCallingCode))
                    LoadAlternateFormatsMedataFromFile(ALTERNATE_FORMATS_FILE_PREFIX);
                return CallingCodeToAlternateFormatsMap.ContainsKey(countryCallingCode)
                           ? CallingCodeToAlternateFormatsMap[countryCallingCode]
                           : null;
            }
        }

        internal static HashSet<String> ShortNumberMetadataSupportedRegions
        {
            get
            {
                if(RegionCodeToShortNumberMetadataMap == null)
                    lock (_regionCodeMapLock)
                    {
                        RegionCodeToShortNumberMetadataMap =
                            BuildMetadataFromXml.GetRegionCodeToShortNumberMap(SHORT_NUMBER_METADATA_FILE_PREFIX, false);
                    }
                if (RegionCodeToShortNumberMetadataMap == null)
                    return new HashSet<string>();
                return new HashSet<string>(RegionCodeToShortNumberMetadataMap.Keys);
            }
        }

        internal static PhoneMetadata GetShortNumberMetadataForRegion(string regionCode)
        {
            if (RegionCodeToShortNumberMetadataMap == null)
            {
                lock (_regionCodeMapLock)
                {
                    RegionCodeToShortNumberMetadataMap =
                        BuildMetadataFromXml.GetRegionCodeToShortNumberMap(SHORT_NUMBER_METADATA_FILE_PREFIX, false);
                }
            }

            return regionCode != null && RegionCodeToShortNumberMetadataMap.ContainsKey(regionCode)
                       ? RegionCodeToShortNumberMetadataMap[regionCode]
                       : null;
        }
    }
}
