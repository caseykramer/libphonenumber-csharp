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
