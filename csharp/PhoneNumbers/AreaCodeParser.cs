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
using System.IO;
using System.Linq;
using System.Text;

/**
 * A utility that generates the binary serialization of the area code/location mappings from
 * human-readable text files. It also generates a configuration file which contains information on
 * data files available for use.
 *
 * <p> The text files must be located in sub-directories of the provided input path. For each input
 * file inputPath/lang/countryCallingCode.txt the corresponding binary file is generated as
 * outputPath/countryCallingCode_lang.
 *
 * @author Philippe Liard
 */
namespace PhoneNumbers
{
    class AreaCodeParser
    {
        public static AreaCodeMap ParseAreaCodeMap(Stream stream)
        {
            SortedDictionary<int, String> areaCodeMapTemp = new SortedDictionary<int, String>();
            using (var lines = new StreamReader(stream, Encoding.UTF8))
            {
                String line;
                while ((line = lines.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length <= 0 || line[0] == '#')
                        continue;
                    var indexOfPipe = line.IndexOf('|');
                    if (indexOfPipe == -1)
                    {
                        continue;
                    }
                    String areaCode = line.Substring(0, indexOfPipe);
                    String location = line.Substring(indexOfPipe + 1);
                    areaCodeMapTemp[int.Parse(areaCode)] = location;
                }
                // Build the corresponding area code map and serialize it to the binary format.
                AreaCodeMap areaCodeMap = new AreaCodeMap();
                areaCodeMap.readAreaCodeMap(areaCodeMapTemp);
                return areaCodeMap;
            }
        }
    }
}
