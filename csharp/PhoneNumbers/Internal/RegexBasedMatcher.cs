/*
 * Copyright (C) 2014 The Libphonenumber Authors
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

namespace PhoneNumbers.Internal
{
    /// <summary>
    /// Implementation of the matcher API using the regular expressions in the PhoneNumberDesc
    /// proto message to match numbers.
    /// </summary> 
    public class RegexBasedMatcher : IMatcherApi
    {
        public static IMatcherApi Create()
        {
            return new RegexBasedMatcher();
        }

        private readonly RegexCache _regexCache = new RegexCache(100);

        private RegexBasedMatcher()
        {}

        public bool MatchesNationalNumber(string nationalNumber, PhoneNumberDesc numberDesc, bool allowPrefixMatch)
        {
            var nationalNumberPatternMatcher = _regexCache.GetPatternForRegex(numberDesc.NationalNumberPattern);
            return nationalNumberPatternMatcher.matches(nationalNumber)
                || (allowPrefixMatch && nationalNumberPatternMatcher.lookingAt(nationalNumber));
        }

        public bool MatchesPossibleNumber(string nationalNumber, PhoneNumberDesc numberDesc)
        {
            var possibleNumberPatternMatcher = _regexCache.GetPatternForRegex(numberDesc.PossibleNumberPattern);
            return possibleNumberPatternMatcher.matches(nationalNumber);
        }
    }
}
