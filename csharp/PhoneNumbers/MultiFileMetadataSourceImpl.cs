/*
 * Copyright (C) 2015 The Libphonenumber Authors
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
using System.Text;

namespace PhoneNumbers
{
    public sealed class MultiFileMetadataSourceImpl : IMetadataSource
    {
        public const string META_DATA_FILE_PREFIX = "PhoneNumberMetaData.xml";

        // A mapping from a region code to the PhoneMetadata for that region.

        // Note: Synchronization, though only needed for the Android version of the library, is used in
        // all versions for consistency.
        private readonly Dictionary<String, PhoneMetadata> _regionToMetadataMap = new Dictionary<string, PhoneMetadata>();

        // A mapping from a country calling code for a non-geographical entity to the PhoneMetadata for
        // that country calling code. Examples of the country calling codes include 800 (International
        // Toll Free Service) and 808 (International Shared Cost Service).
        // Note: Synchronization, though only needed for the Android version of the library, is used in
        // all versions for consistency.
        private readonly Dictionary<int, PhoneMetadata> _countryCodeToNonGeographicalMetadataMap = new Dictionary<int, PhoneMetadata>();

        // The prefix of the metadata files from which region data is loaded.
        private string _currentFilePrefix;

        // The metadata loader used to inject alternative metadata sources.
        private MetadataLoader _metadataLoader;

        // It is assumed that metadataLoader is not null.
        public MultiFileMetadataSourceImpl(string currentFilePrefix, MetadataLoader metadataLoader)
        {
            _currentFilePrefix = currentFilePrefix;
            _metadataLoader = metadataLoader;
        }

        // It is assumed that metadataLoader is not null.
        public MultiFileMetadataSourceImpl(MetadataLoader metadataLoader)
            : this(META_DATA_FILE_PREFIX, metadataLoader)
        { }

        public PhoneMetadata GetMetadataForRegion(string regionCode)
        {
            if (!_regionToMetadataMap.ContainsKey(regionCode))
            {
                // The regionCode here will be valid and won't be '001', so we don't need to worry about
                // what to pass in for the country calling code.
                LoadMetadataFromFile(_currentFilePrefix, regionCode, 0, _metadataLoader);
            }


            return _regionToMetadataMap[regionCode];
        }

        public PhoneMetadata GetMetadataForNonGeographicalRegion(int countryCallingCode)
        {
            if (!_countryCodeToNonGeographicalMetadataMap.ContainsKey(countryCallingCode))
            {
                LoadMetadataFromFile(_currentFilePrefix, PhoneNumberUtil.REGION_CODE_FOR_NON_GEO_ENTITY, countryCallingCode,_metadataLoader);
            }
            
            PhoneMetadata metadata = null;
            _countryCodeToNonGeographicalMetadataMap.TryGetValue(countryCallingCode, out metadata);
            return metadata;
        }

        internal void LoadMetadataFromFile(String filePrefix, String regionCode, int countryCallingCode, MetadataLoader metadataLoader)
        {
            var asm = Assembly.GetExecutingAssembly();
            bool isNonGeoRegion = PhoneNumberUtil.REGION_CODE_FOR_NON_GEO_ENTITY.Equals(regionCode);
            var name = asm.GetManifestResourceNames().Where(n => n.EndsWith(filePrefix)).FirstOrDefault() ?? "missing";
            using (var stream = metadataLoader.LoadMetadata(name))
            {
                var meta = BuildMetadataFromXml.BuildPhoneMetadataCollection(stream, false);
                foreach (var m in meta.MetadataList)
                {
                    if (isNonGeoRegion)
                        _countryCodeToNonGeographicalMetadataMap[m.CountryCode] = m;
                    else
                        _regionToMetadataMap[m.Id] = m;
                }
            }
        }

    }  
}
