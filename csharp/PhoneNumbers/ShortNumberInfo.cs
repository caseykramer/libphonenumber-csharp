/*
* Copyright (C) 2011 The Libphonenumber Authors
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PhoneNumbers.Internal;

namespace PhoneNumbers
{
    /**
     * Methods for getting information about short phone numbers, such as short codes and emergency
     * numbers. Note that most commercial short numbers are not handled here, but by the
     * {@link PhoneNumberUtil}.
     *
     * @author Shaopeng Jia
     * @author David Yonge-Mallo
     */
    public class ShortNumberInfo
    {
        private static readonly ShortNumberInfo INSTANCE = new ShortNumberInfo(RegexBasedMatcher.Create());

        // In these countries, if extra digits are added to an emergency number, it no longer connects
        // to the emergency service.
        private static readonly HashSet<string> REGIONS_WHERE_EMERGENCY_NUMBERS_MUST_BE_EXACT = new HashSet<String>();

        static ShortNumberInfo()
        {
            REGIONS_WHERE_EMERGENCY_NUMBERS_MUST_BE_EXACT.Add("BR");
            REGIONS_WHERE_EMERGENCY_NUMBERS_MUST_BE_EXACT.Add("CL");
            REGIONS_WHERE_EMERGENCY_NUMBERS_MUST_BE_EXACT.Add("NI");
        }

        /** Cost categories of short numbers. */
        public enum ShortNumberCost
        {
            TOLL_FREE,
            STANDARD_RATE,
            PREMIUM_RATE,
            UNKNOWN_COST
        }

        /** Returns the singleton instance of the ShortNumberInfo. */
        public static ShortNumberInfo GetInstance()
        {
            return INSTANCE;
        }

        // MatcherApi supports the basic matching method for checking if a given national number matches
        // a national number patten or a possible number patten defined in the given
        // {@code PhoneNumberDesc}.
        private readonly IMatcherApi _matcherApi;

        // A mapping from a country calling code to the region codes which denote the region represented
        // by that country calling code. In the case of multiple regions sharing a calling code, such as
        // the NANPA regions, the one indicated with "isMainCountryForCode" in the metadata should be
        // first.
        private readonly Dictionary<int, List<string>> _countryCallingCodeToRegionCodeMap;



        internal ShortNumberInfo(IMatcherApi util)
        {
            _matcherApi = util;
            // TODO: Create ShortNumberInfo for a given map
            _countryCallingCodeToRegionCodeMap = CountryCodeToRegionCodeMap.GetCountryCodeToRegionCodeMap();

        }

        /**
         * Returns a list with the region codes that match the specific country calling code. For
         * non-geographical country calling codes, the region code 001 is returned. Also, in the case
         * of no region code being found, an empty list is returned.
         */
        private ReadOnlyCollection<String> GetRegionCodesForCountryCode(int countryCallingCode)
        {
            List<String> regionCodes = _countryCallingCodeToRegionCodeMap[countryCallingCode];
            return new List<string>(regionCodes == null ? new List<string>(0): regionCodes).AsReadOnly();
        }


        /**
         * Check whether a short number is a possible number when dialled from a region, given the number
         * in the form of a string, and the region where the number is dialed from. This provides a more
         * lenient check than {@link #isValidShortNumberForRegion}.
         *
         * @param shortNumber the short number to check as a string
         * @param regionDialingFrom the region from which the number is dialed
         * @return whether the number is a possible short number
         * @deprecated Anyone who was using it and passing in a string with whitespace (or other
         *             formatting characters) would have been getting the wrong result. You should parse
         *             the string to PhoneNumber and use the method
         *             {@code #isPossibleShortNumberForRegion(PhoneNumber, String)}. This method will be
         *             removed in the next release.
         */
        [Obsolete]
        public bool IsPossibleShortNumberForRegion(string shortNumber, string regionDialingFrom)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
                return false;
            PhoneNumberDesc generalDesc = phoneMetadata.GeneralDesc;
            return _matcherApi.MatchesPossibleNumber(shortNumber, generalDesc);
        }

        /**
         * Check whether a short number is a possible number when dialed from the given region. This
         * provides a more lenient check than {@link #isValidShortNumberForRegion}.
         *
         * @param number the short number to check
         * @param regionDialingFrom the region from which the number is dialed
         * @return whether the number is a possible short number
         */
        public bool IsPossibleShortNumberForRegion(PhoneNumber number, String regionDialingFrom)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
            {
                return false;
            }
            return _matcherApi.MatchesPossibleNumber(GetNationalSignificantNumber(number),phoneMetadata.GeneralDesc);
        }


        /**
         * Check whether a short number is a possible number. If a country calling code is shared by
         * multiple regions, this returns true if it's possible in any of them. This provides a more
         * lenient check than {@link #isValidShortNumber}. See {@link
         * #isPossibleShortNumberForRegion(PhoneNumber, String)} for details.
         *
         * @param number the short number to check
         * @return whether the number is a possible short number
         */
        public bool IsPossibleShortNumber(PhoneNumber number)
        {
            IEnumerable<String> regionCodes = GetRegionCodesForCountryCode(number.CountryCode);
            String shortNumber = GetNationalSignificantNumber(number);
            foreach (String region in regionCodes)
            {
                PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(region);
                if (phoneMetadata == null)
                    continue;
                if (_matcherApi.MatchesPossibleNumber(shortNumber, phoneMetadata.GeneralDesc))
                    return true;                
                
            }
            return false;
        }

        /**
         * Tests whether a short number matches a valid pattern in a region. Note that this doesn't verify
         * the number is actually in use, which is impossible to tell by just looking at the number
         * itself.
         *
         * @param shortNumber the short number to check as a string
         * @param regionDialingFrom the region from which the number is dialed
         * @return whether the short number matches a valid pattern
         * @deprecated Anyone who was using it and passing in a string with whitespace (or other
         *             formatting characters) would have been getting the wrong result. You should parse
         *             the string to PhoneNumber and use the method
         *             {@code #isValidShortNumberForRegion(PhoneNumber, String)}. This method will be
         *             removed in the next release.
         */
        [Obsolete]
        public bool IsValidShortNumberForRegion(String shortNumber, String regionDialingFrom)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
                return false;

            PhoneNumberDesc generalDesc = phoneMetadata.GeneralDesc;
            if (!MatchesPossibleNumberAndNationalNumber(shortNumber,generalDesc))
            {
                return false;
            }

            PhoneNumberDesc shortNumberDesc = phoneMetadata.ShortCode;
            if (!shortNumberDesc.HasNationalNumberPattern)
                return false;

            return MatchesPossibleNumberAndNationalNumber(shortNumber, shortNumberDesc);
        }

        /**
         * Tests whether a short number matches a valid pattern in a region. Note that this doesn't verify
         * the number is actually in use, which is impossible to tell by just looking at the number
         * itself.
         *
         * @param number the short number for which we want to test the validity
         * @param regionDialingFrom the region from which the number is dialed
         * @return whether the short number matches a valid pattern
         */
        public bool IsValidShortNumberForRegion(PhoneNumber number, string regionDialingFrom)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
            {
                return false;
            }
            String shortNumber = GetNationalSignificantNumber(number);
            PhoneNumberDesc generalDesc = phoneMetadata.GeneralDesc;
            if (!MatchesPossibleNumberAndNationalNumber(shortNumber, generalDesc))
            {
                return false;
            }
            PhoneNumberDesc shortNumberDesc = phoneMetadata.ShortCode;
            return MatchesPossibleNumberAndNationalNumber(shortNumber, shortNumberDesc);
        }


        /**
         * Tests whether a short number matches a valid pattern. If a country calling code is shared by
         * multiple regions, this returns true if it's valid in any of them. Note that this doesn't verify
         * the number is actually in use, which is impossible to tell by just looking at the number
         * itself. See {@link #isValidShortNumberForRegion(PhoneNumber, String)} for details.
         *
         * @param number the short number for which we want to test the validity
         * @return whether the short number matches a valid pattern
         */
        public bool IsValidShortNumber(PhoneNumber number)
        {
            var regionCodes = GetRegionCodesForCountryCode(number.CountryCode);
            String regionCode = GetRegionCodeForShortNumberFromRegionList(number, regionCodes);
            if (regionCodes.Count > 1 && regionCode != null)
            {
                // If a matching region had been found for the phone number from among two or more regions,
                // then we have already implicitly verified its validity for that region.
                return true;
            }
            return IsValidShortNumberForRegion(number, regionCode);
        }

        /**
         * Gets the expected cost category of a short number when dialled from a region (however, nothing
         * is implied about its validity). If it is important that the number is valid, then its validity
         * must first be checked using {@link isValidShortNumberForRegion}. Note that emergency numbers
         * are always considered toll-free. Example usage:
         * <pre>{@code
         * ShortNumberInfo shortInfo = ShortNumberInfo.getInstance();
         * String shortNumber = "110";
         * String regionCode = "FR";
         * if (shortInfo.isValidShortNumberForRegion(shortNumber, regionCode)) {
         *   ShortNumberInfo.ShortNumberCost cost = shortInfo.getExpectedCostForRegion(shortNumber,
         *       regionCode);
         *   // Do something with the cost information here.
         * }}</pre>
         *
         * @param shortNumber the short number for which we want to know the expected cost category,
         *     as a string
         * @param regionDialingFrom the region from which the number is dialed
         * @return the expected cost category for that region of the short number. Returns UNKNOWN_COST if
         *     the number does not match a cost category. Note that an invalid number may match any cost
         *     category.
         * @deprecated Anyone who was using it and passing in a string with whitespace (or other
         *             formatting characters) would have been getting the wrong result. You should parse
         *             the string to PhoneNumber and use the method
         *             {@code #getExpectedCostForRegion(PhoneNumber, String)}. This method will be
         *             removed in the next release.
         */
        [Obsolete] 
        public ShortNumberCost GetExpectedCostForRegion(String shortNumber, String regionDialingFrom)
        {
            // Note that regionDialingFrom may be null, in which case phoneMetadata will also be null.
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
                return ShortNumberCost.UNKNOWN_COST;

            // The cost categories are tested in order of decreasing expense, since if for some reason the
            // patterns overlap the most expensive matching cost category should be returned.
            if (MatchesPossibleNumberAndNationalNumber(shortNumber, phoneMetadata.PremiumRate))
                return ShortNumberCost.PREMIUM_RATE;

            if (MatchesPossibleNumberAndNationalNumber(shortNumber, phoneMetadata.StandardRate))
                return ShortNumberCost.STANDARD_RATE;

            if (MatchesPossibleNumberAndNationalNumber(shortNumber, phoneMetadata.TollFree))
                return ShortNumberCost.TOLL_FREE;

            if (IsEmergencyNumber(shortNumber, regionDialingFrom)) 
            {
                // Emergency numbers are implicitly toll-free.
                return ShortNumberCost.TOLL_FREE;
            }
            return ShortNumberCost.UNKNOWN_COST;

        }

        /**
         * Gets the expected cost category of a short number when dialed from a region (however, nothing
         * is implied about its validity). If it is important that the number is valid, then its validity
         * must first be checked using {@link #isValidShortNumberForRegion}. Note that emergency numbers
         * are always considered toll-free. Example usage:
         * <pre>{@code
         * // The region for which the number was parsed and the region we subsequently check against
         * // need not be the same. Here we parse the number in the US and check it for Canada.
         * PhoneNumber number = phoneUtil.parse("110", "US");
         * ...
         * String regionCode = "CA";
         * ShortNumberInfo shortInfo = ShortNumberInfo.getInstance();
         * if (shortInfo.isValidShortNumberForRegion(shortNumber, regionCode)) {
         *   ShortNumberCost cost = shortInfo.getExpectedCostForRegion(number, regionCode);
         *   // Do something with the cost information here.
         * }}</pre>
         *
         * @param number the short number for which we want to know the expected cost category
         * @param regionDialingFrom the region from which the number is dialed
         * @return the expected cost category for that region of the short number. Returns UNKNOWN_COST if
         *     the number does not match a cost category. Note that an invalid number may match any cost
         *     category.
         */
        public ShortNumberCost GetExpectedCostForRegion(PhoneNumber number, string regionDialingFrom)
        {
            // Note that regionDialingFrom may be null, in which case phoneMetadata will also be null.
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
            {
                return ShortNumberCost.UNKNOWN_COST;
            }

            String shortNumber = GetNationalSignificantNumber(number);

            // The cost categories are tested in order of decreasing expense, since if for some reason the
            // patterns overlap the most expensive matching cost category should be returned.
            if (MatchesPossibleNumberAndNationalNumber(shortNumber, phoneMetadata.PremiumRate))
            {
                return ShortNumberCost.PREMIUM_RATE;
            }
            if (MatchesPossibleNumberAndNationalNumber(shortNumber, phoneMetadata.StandardRate))
            {
                return ShortNumberCost.STANDARD_RATE;
            }
            if (MatchesPossibleNumberAndNationalNumber(shortNumber, phoneMetadata.TollFree))
            {
                return ShortNumberCost.TOLL_FREE;
            }
            if (IsEmergencyNumber(shortNumber, regionDialingFrom))
            {
                // Emergency numbers are implicitly toll-free.
                return ShortNumberCost.TOLL_FREE;
            }
            return ShortNumberCost.UNKNOWN_COST;
        }


        /**
         * Gets the expected cost category of a short number (however, nothing is implied about its
         * validity). If the country calling code is unique to a region, this method behaves exactly the
         * same as {@link #getExpectedCostForRegion(PhoneNumber, String)}. However, if the country 
         * calling code is shared by multiple regions, then it returns the highest cost in the sequence
         * PREMIUM_RATE, UNKNOWN_COST, STANDARD_RATE, TOLL_FREE. The reason for the position of
         * UNKNOWN_COST in this order is that if a number is UNKNOWN_COST in one region but STANDARD_RATE
         * or TOLL_FREE in another, its expected cost cannot be estimated as one of the latter since it
         * might be a PREMIUM_RATE number.
         * <p>
         * For example, if a number is STANDARD_RATE in the US, but TOLL_FREE in Canada, the expected 
         * cost returned by this method will be STANDARD_RATE, since the NANPA countries share the same 
         * country calling code.
         * </p>
         * Note: If the region from which the number is dialed is known, it is highly preferable to call
         * {@link #getExpectedCostForRegion(PhoneNumber, String)} instead.
         *
         * @param number the short number for which we want to know the expected cost category
         * @return the highest expected cost category of the short number in the region(s) with the given
         *     country calling code
         */
        public ShortNumberCost GetExpectedCost(PhoneNumber number)
        {
            var regionCodes = GetRegionCodesForCountryCode(number.CountryCode);
            if (regionCodes.Count == 0)
                return ShortNumberCost.UNKNOWN_COST;

            if (regionCodes.Count == 1)
                return GetExpectedCostForRegion(number, regionCodes[0]);

            ShortNumberCost cost = ShortNumberCost.TOLL_FREE;
            foreach (string regionCode in regionCodes)
            {
                ShortNumberCost costForRegion = GetExpectedCostForRegion(number, regionCode);
                switch (costForRegion)
                {
                    case ShortNumberCost.PREMIUM_RATE:
                        return ShortNumberCost.PREMIUM_RATE;
                    case ShortNumberCost.UNKNOWN_COST:
                        cost = ShortNumberCost.UNKNOWN_COST;
                        break;
                    case ShortNumberCost.STANDARD_RATE:
                        if (cost != ShortNumberCost.UNKNOWN_COST)
                        {
                            cost = ShortNumberCost.STANDARD_RATE;
                        }
                        break;
                    case ShortNumberCost.TOLL_FREE:
                        // Do nothing.
                        break;
                    default:
                        break;
                    // logger.log(Level.SEVERE, "Unrecognised cost for region: " + costForRegion);
                }
            }
            return cost;
        }

        // Helper method to get the region code for a given phone number, from a list of possible region
        // codes. If the list contains more than one region, the first region for which the number is
        // valid is returned.
        private String GetRegionCodeForShortNumberFromRegionList(PhoneNumber number, IEnumerable<String> regionCodes)
        {
            var numberOfCodes = regionCodes.Count();
            if (numberOfCodes == 0)
            {
                return null;
            }
            else if (numberOfCodes == 1)
            {
                return regionCodes.First();
            }
            String nationalNumber = GetNationalSignificantNumber(number);
            foreach (var regionCode in regionCodes)
            {
                PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionCode);
                if (phoneMetadata != null
                    && MatchesPossibleNumberAndNationalNumber(nationalNumber, phoneMetadata.ShortCode))
                {
                    // The number is valid for this region.
                    return regionCode;
                }
            }
            return null;
        }

        /**
         * Convenience method to get a list of what regions the library has metadata for.
         */
        internal HashSet<String> SupportedRegions
        {
            get { return MetadataManager.ShortNumberMetadataSupportedRegions; }
        }

        /**
         * Gets a valid short number for the specified region.
         *
         * @param regionCode the region for which an example short number is needed
         * @return a valid short number for the specified region. Returns an empty string when the
         *     metadata does not contain such information.
         */
        // @VisibleForTesting
        internal String GetExampleShortNumber(String regionCode)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionCode);
            if (phoneMetadata == null)
            {
                return "";
            }
            PhoneNumberDesc desc = phoneMetadata.ShortCode;
            if (desc.HasExampleNumber)
            {
                return desc.ExampleNumber;
            }
            return "";
        }

        /**
         * Gets a valid short number for the specified cost category.
         *
         * @param regionCode the region for which an example short number is needed
         * @param cost the cost category of number that is needed
         * @return a valid short number for the specified region and cost category. Returns an empty
         *     string when the metadata does not contain such information, or the cost is UNKNOWN_COST.
         */
        // @VisibleForTesting
        internal string GetExampleShortNumberForCost(string regionCode, ShortNumberCost cost)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionCode);
            if (phoneMetadata == null)
                return "";

            PhoneNumberDesc desc = null;
            switch (cost)
            {
                case ShortNumberCost.TOLL_FREE:
                    desc = phoneMetadata.TollFree;
                    break;
                case ShortNumberCost.STANDARD_RATE:
                    desc = phoneMetadata.StandardRate;
                    break;
                case ShortNumberCost.PREMIUM_RATE:
                    desc = phoneMetadata.PremiumRate;
                    break;
                default:
                    break;
                // UNKNOWN_COST numbers are computed by the process of elimination from the other cost
                // categories.
            }
            if (desc != null && desc.HasExampleNumber)
            {
                return desc.ExampleNumber;
            }
            return "";
        }

        /**
         * Returns true if the given number, exactly as dialed, might be used to connect to an emergency
         * service in the given region.
         * <p>
         * This method accepts a string, rather than a PhoneNumber, because it needs to distinguish
         * cases such as "+1 911" and "911", where the former may not connect to an emergency service in
         * all cases but the latter would. This method takes into account cases where the number might
         * contain formatting, or might have additional digits appended (when it is okay to do that in
         * the specified region).
         *
         * @param number the phone number to test
         * @param regionCode the region where the phone number is being dialed
         * @return whether the number might be used to connect to an emergency service in the given region
         */
        public bool ConnectsToEmergencyNumber(string number, string regionCode)
        {
            return MatchesEmergencyNumberHelper(number, regionCode, true /* allows prefix match */);
        }

        /**
         * Returns true if the given number exactly matches an emergency service number in the given 
         * region.
         * <p>
         * This method takes into account cases where the number might contain formatting, but doesn't
         * allow additional digits to be appended. Note that {@code isEmergencyNumber(number, region)}
         * implies {@code connectsToEmergencyNumber(number, region)}.
         * </p>
         * @param number the phone number to test
         * @param regionCode the region where the phone number is being dialed
         * @return whether the number exactly matches an emergency services number in the given region
         */
        public bool IsEmergencyNumber(string number, string regionCode)
        {
            return MatchesEmergencyNumberHelper(number, regionCode, false /* doesn't allow prefix match */);
        }

        private bool MatchesEmergencyNumberHelper(string number, string regionCode, bool allowPrefixMatch)
        {
            number = PhoneNumberUtil.ExtractPossibleNumber(number);
            if (PhoneNumberUtil.PLUS_CHARS_PATTERN.MatchBeginning(number).Success)
            {
                // Returns false if the number starts with a plus sign. We don't believe dialing the country
                // code before emergency numbers (e.g. +1911) works, but later, if that proves to work, we can
                // add additional logic here to handle it.
                return false;
            }
            PhoneMetadata metadata = MetadataManager.GetShortNumberMetadataForRegion(regionCode);
            if (metadata == null || !metadata.HasEmergency)
            {
                return false;
            }
            String normalizedNumber = PhoneNumberUtil.NormalizeDigitsOnly(number);
            PhoneNumberDesc emergencyDesc = metadata.Emergency;
            bool allowPrefixMatchingForRegion = allowPrefixMatch && !REGIONS_WHERE_EMERGENCY_NUMBERS_MUST_BE_EXACT.Contains(regionCode);
            return _matcherApi.MatchesNationalNumber(normalizedNumber, emergencyDesc, allowPrefixMatchingForRegion);
        }

        /**
         * Given a valid short number, determines whether it is carrier-specific (however, nothing is
         * implied about its validity). If it is important that the number is valid, then its validity
         * must first be checked using {@link #isValidShortNumber} or
         * {@link #isValidShortNumberForRegion}.
         *
         * @param number the valid short number to check
         * @return whether the short number is carrier-specific (assuming the input was a valid short
         *     number).
         */
        public bool IsCarrierSpecific(PhoneNumber number)
        {
            var regionCodes = GetRegionCodesForCountryCode(number.CountryCode);
            String regionCode = GetRegionCodeForShortNumberFromRegionList(number, regionCodes);
            String nationalNumber = GetNationalSignificantNumber(number);
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionCode);
            return (phoneMetadata != null) 
                && (MatchesPossibleNumberAndNationalNumber(nationalNumber, phoneMetadata.CarrierSpecific));
        }

        /**
         * Gets the national significant number of the a phone number. Note a national significant number
         * doesn't contain a national prefix or any formatting.
         * <p>
         * This is a temporary duplicate of the {@code getNationalSignificantNumber} method from
         * {@code PhoneNumberUtil}. Ultimately a canonical static version should exist in a separate
         * utility class (to prevent {@code ShortNumberInfo} needing to depend on PhoneNumberUtil).
         *
         * @param number  the phone number for which the national significant number is needed
         * @return  the national significant number of the PhoneNumber object passed in
         */
        private static string GetNationalSignificantNumber(PhoneNumber number)
        {
            // If leading zero(s) have been set, we prefix this now. Note this is not a national prefix.
            StringBuilder nationalNumber = new StringBuilder();
            if (number.ItalianLeadingZero)
            {
                char[] zeros = new char[number.NumberOfLeadingZeros].Select(_ => '0').ToArray();
                
                nationalNumber.Append(new string(zeros));
            }
            nationalNumber.Append(number.NationalNumber);
            return nationalNumber.ToString();
        }

        // TODO: Once we have benchmarked ShortNumberInfo, consider if it is worth keeping
        // this performance optimization, and if so move this into the matcher implementation.
        private Boolean MatchesPossibleNumberAndNationalNumber(String number,PhoneNumberDesc numberDesc)
        {
            return _matcherApi.MatchesPossibleNumber(number, numberDesc)
                && _matcherApi.MatchesNationalNumber(number, numberDesc, false);
        }

    }
}
