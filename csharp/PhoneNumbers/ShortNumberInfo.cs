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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        private static readonly ShortNumberInfo INSTANCE = new ShortNumberInfo(PhoneNumberUtil.GetInstance());

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
        public static ShortNumberInfo getInstance()
        {
            return INSTANCE;
        }

        private readonly PhoneNumberUtil phoneUtil;

        internal ShortNumberInfo(PhoneNumberUtil util)
        {
            phoneUtil = util;
        }

        /**
         * Check whether a short number is a possible number when dialled from a region, given the number
         * in the form of a string, and the region where the number is dialed from. This provides a more
         * lenient check than {@link #isValidShortNumberForRegion}.
         *
         * @param shortNumber the short number to check as a string
         * @param regionDialingFrom the region from which the number is dialed
         * @return whether the number is a possible short number
         */
        public bool IsPossibleShortNumberForRegion(string shortNumber, string regionDialingFrom)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
                return false;
            PhoneNumberDesc generalDesc = phoneMetadata.GeneralDesc;
            return phoneUtil.IsNumberPossibleForDesc(shortNumber, generalDesc);
        }

        /**
         * Check whether a short number is a possible number. If a country calling code is shared by
         * multiple regions, this returns true if it's possible in any of them. This provides a more
         * lenient check than {@link #isValidShortNumber}. See {@link
         * #isPossibleShortNumberForRegion(String, String)} for details.
         *
         * @param number the short number to check
         * @return whether the number is a possible short number
         */
        public bool IsPossibleShortNumber(PhoneNumber number)
        {
            List<String> regionCodes = phoneUtil.GetRegionCodesForCountryCode(number.CountryCode);
            String shortNumber = phoneUtil.GetNationalSignificantNumber(number);
            foreach (String region in regionCodes)
            {
                PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(region);
                if (phoneUtil.IsNumberPossibleForDesc(shortNumber, phoneMetadata.GeneralDesc))
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
         */
        public bool IsValidShortNumberForRegion(String shortNumber, String regionDialingFrom)
        {
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
                return false;

            PhoneNumberDesc generalDesc = phoneMetadata.GeneralDesc;
            if (!generalDesc.HasNationalNumberPattern ||
                !phoneUtil.IsNumberMatchingDesc(shortNumber, generalDesc))
                return false;

            PhoneNumberDesc shortNumberDesc = phoneMetadata.ShortCode;
            if (!shortNumberDesc.HasNationalNumberPattern)
                return false;

            return phoneUtil.IsNumberMatchingDesc(shortNumber, shortNumberDesc);
        }

        /**
         * Tests whether a short number matches a valid pattern. If a country calling code is shared by
         * multiple regions, this returns true if it's valid in any of them. Note that this doesn't verify
         * the number is actually in use, which is impossible to tell by just looking at the number
         * itself. See {@link #isValidShortNumberForRegion(String, String)} for details.
         *
         * @param number the short number for which we want to test the validity
         * @return whether the short number matches a valid pattern
         */
        public bool IsValidShortNumber(PhoneNumber number)
        {
            List<String> regionCodes = phoneUtil.GetRegionCodesForCountryCode(number.CountryCode);
            String shortNumber = phoneUtil.GetNationalSignificantNumber(number);
            String regionCode = GetRegionCodeForShortNumberFromRegionList(number, regionCodes);
            if (regionCodes.Count > 1 && regionCode != null)
            {
                // If a matching region had been found for the phone number from among two or more regions,
                // then we have already implicitly verified its validity for that region.
                return true;
            }
            return IsValidShortNumberForRegion(shortNumber, regionCode);
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
         */
        public ShortNumberCost GetExpectedCostForRegion(String shortNumber, String regionDialingFrom)
        {
            // Note that regionDialingFrom may be null, in which case phoneMetadata will also be null.
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionDialingFrom);
            if (phoneMetadata == null)
                return ShortNumberCost.UNKNOWN_COST;

            // The cost categories are tested in order of decreasing expense, since if for some reason the
            // patterns overlap the most expensive matching cost category should be returned.
            if (phoneUtil.IsNumberMatchingDesc(shortNumber, phoneMetadata.PremiumRate))
                return ShortNumberCost.PREMIUM_RATE;

            if (phoneUtil.IsNumberMatchingDesc(shortNumber, phoneMetadata.StandardRate))
                return ShortNumberCost.STANDARD_RATE;

            if (phoneUtil.IsNumberMatchingDesc(shortNumber, phoneMetadata.TollFree))
                return ShortNumberCost.TOLL_FREE;

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
         * same as {@link #getExpectedCostForRegion(String, String)}. However, if the country calling
         * code is shared by multiple regions, then it returns the highest cost in the sequence
         * PREMIUM_RATE, UNKNOWN_COST, STANDARD_RATE, TOLL_FREE. The reason for the position of
         * UNKNOWN_COST in this order is that if a number is UNKNOWN_COST in one region but STANDARD_RATE
         * or TOLL_FREE in another, its expected cost cannot be estimated as one of the latter since it
         * might be a PREMIUM_RATE number.
         *
         * For example, if a number is STANDARD_RATE in the US, but TOLL_FREE in Canada, the expected cost
         * returned by this method will be STANDARD_RATE, since the NANPA countries share the same country
         * calling code.
         *
         * Note: If the region from which the number is dialed is known, it is highly preferable to call
         * {@link #getExpectedCostForRegion(String, String)} instead.
         *
         * @param number the short number for which we want to know the expected cost category
         * @return the highest expected cost category of the short number in the region(s) with the given
         *     country calling code
         */
        public ShortNumberCost GetExpectedCost(PhoneNumber number)
        {
            List<String> regionCodes = phoneUtil.GetRegionCodesForCountryCode(number.CountryCode);
            if (regionCodes.Count == 0)
                return ShortNumberCost.UNKNOWN_COST;

            String shortNumber = phoneUtil.GetNationalSignificantNumber(number);
            if (regionCodes.Count == 1)
                return GetExpectedCostForRegion(shortNumber, regionCodes[0]);

            ShortNumberCost cost = ShortNumberCost.TOLL_FREE;
            foreach (string regionCode in regionCodes)
            {
                ShortNumberCost costForRegion = GetExpectedCostForRegion(shortNumber, regionCode);
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
        private String GetRegionCodeForShortNumberFromRegionList(PhoneNumber number, List<String> regionCodes)
        {
            if (regionCodes.Count == 0)
            {
                return null;
            }
            else if (regionCodes.Count == 1)
            {
                return regionCodes[0];
            }
            String nationalNumber = phoneUtil.GetNationalSignificantNumber(number);
            foreach (var regionCode in regionCodes)
            {
                PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionCode);
                if (phoneMetadata != null &&
                    phoneUtil.IsNumberMatchingDesc(nationalNumber, phoneMetadata.ShortCode))
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
         * Returns true if the number might be used to connect to an emergency service in the given
         * region.
         *
         * This method takes into account cases where the number might contain formatting, or might have
         * additional digits appended (when it is okay to do that in the region specified).
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
         * Returns true if the number exactly matches an emergency service number in the given region.
         *
         * This method takes into account cases where the number might contain formatting, but doesn't
         * allow additional digits to be appended.
         *
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
            PhoneRegex emergencyNumberPattern = new PhoneRegex(metadata.Emergency.NationalNumberPattern);
            return (!allowPrefixMatch || REGIONS_WHERE_EMERGENCY_NUMBERS_MUST_BE_EXACT.Contains(regionCode))
                ? emergencyNumberPattern.MatchAll(normalizedNumber).Success
                : emergencyNumberPattern.MatchBeginning(normalizedNumber).Success;
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
            List<String> regionCodes = phoneUtil.GetRegionCodesForCountryCode(number.CountryCode);
            String regionCode = GetRegionCodeForShortNumberFromRegionList(number, regionCodes);
            String nationalNumber = phoneUtil.GetNationalSignificantNumber(number);
            PhoneMetadata phoneMetadata = MetadataManager.GetShortNumberMetadataForRegion(regionCode);
            return (phoneMetadata != null) && (phoneUtil.IsNumberMatchingDesc(nationalNumber, phoneMetadata.CarrierSpecific));
        }
    }
}
