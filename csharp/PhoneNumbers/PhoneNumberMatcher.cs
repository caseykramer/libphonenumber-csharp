﻿/*
 * Copyright (C) 2009 Google Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PhoneNumbers
{
    public class PhoneNumberMatcher : IEnumerator<PhoneNumberMatch>
    {
        /**
        * The phone number pattern used by {@link #find}, similar to
        * {@code PhoneNumberUtil.VALID_PHONE_NUMBER}, but with the following differences:
        * <ul>
        *   <li>All captures are limited in order to place an upper bound to the text matched by the
        *       pattern.
        * <ul>
        *   <li>Leading punctuation / plus signs are limited.
        *   <li>Consecutive occurrences of punctuation are limited.
        *   <li>Number of digits is limited.
        * </ul>
        *   <li>No whitespace is allowed at the start or end.
        *   <li>No alpha digits (vanity numbers such as 1-800-SIX-FLAGS) are currently supported.
        * </ul>
        */
        private static readonly PhoneRegex PATTERN;

        /**
        * Matches strings that look like publication pages. Example:
        * <pre>Computing Complete Answers to Queries in the Presence of Limited Access Patterns.
        * Chen Li. VLDB J. 12(3): 211-227 (2003).</pre>
        *
        * The string "211-227 (2003)" is not a telephone number.
        */
        private static readonly PhoneRegex PUB_PAGES = new PhoneRegex("\\d{1,5}-+\\d{1,5}\\s{0,4}\\(\\d{1,4}", RegexOptions.Compiled);

        /**
        * Matches strings that look like dates using "/" as a separator. Examples: 3/10/2011, 31/10/96 or
        * 08/31/95.
        */
        private static readonly Regex SLASH_SEPARATED_DATES =
            new Regex("(?:(?:[0-3]?\\d/[01]?\\d)|(?:[01]?\\d/[0-3]?\\d))/(?:[12]\\d)?\\d{2}", RegexOptions.Compiled);

        /**
        * Matches timestamps. Examples: "2012-01-02 08:00". Note that the reg-ex does not include the
        * trailing ":\d\d" -- that is covered by TIME_STAMPS_SUFFIX.
        */
        private static readonly Regex TIME_STAMPS =
            new Regex("[12]\\d{3}[-/]?[01]\\d[-/]?[0-3]\\d +[0-2]\\d$", RegexOptions.Compiled);
        private static readonly PhoneRegex TIME_STAMPS_SUFFIX = new PhoneRegex(":[0-5]\\d", RegexOptions.Compiled);


        /**
        * Pattern to check that brackets match. Opening brackets should be closed within a phone number.
        * This also checks that there is something inside the brackets. Having no brackets at all is also
        * fine.
        */
        private static readonly PhoneRegex MATCHING_BRACKETS;        

        /**
         * Patterns used to extract phone numbers from a larger phone-number-like pattern. These are
         * ordered according to specificity. For example, white-space is last since that is frequently
         * used in numbers, not just to separate two numbers. We have separate patterns since we don't
         * want to break up the phone-number-like text on more than one different kind of symbol at one
         * time, although symbols of the same type (e.g. space) can be safely grouped together.
         *
         * Note that if there is a match, we will always check any text found up to the first match as
         * well.
         */
        private static readonly PhoneRegex[] INNER_MATCHES ={
            // Breaks on the slash - e.g. "651-234-2345/332-445-1234"
            new PhoneRegex("/+(.*)"),
            // Note that the bracket here is inside the capturing group, since we consider it part of the
            // phone number. Will match a pattern like "(650) 223 3345 (754) 223 3321".
            new PhoneRegex("(\\([^(]*)"),
            // Breaks on a hyphen - e.g. "12345 - 332-445-1234 is my number."
            // We require a space on either side of the hyphen for it to be considered a separator.
            new PhoneRegex("(?:\\p{Z}-|-\\p{Z})\\p{Z}*(.+)"),
            // Various types of wide hyphens. Note we have decided not to enforce a space here, since it's
            // possible that it's supposed to be used to break two numbers without spaces, and we haven't
            // seen many instances of it used within a number.
            new PhoneRegex("[\u2012-\u2015\uFF0D]\\p{Z}*(.+)"),
            // Breaks on a full stop - e.g. "12345. 332-445-1234 is my number."
            new PhoneRegex("\\.+\\p{Z}*([^.]+)"),
            // Breaks on space - e.g. "3324451234 8002341234"
            new PhoneRegex("\\p{Z}+(\\P{Z}+)")
        };

        /**
        * Punctuation that may be at the start of a phone number - brackets and plus signs.
        */
        private static readonly PhoneRegex LEAD_CLASS;

        static PhoneNumberMatcher()
        {
            /* Builds the MATCHING_BRACKETS and PATTERN regular expressions. The building blocks below exist
            * to make the pattern more easily understood. */

            String openingParens = "(\\[\uFF08\uFF3B";
            String closingParens = ")\\]\uFF09\uFF3D";
            String nonParens = "[^" + openingParens + closingParens + "]";

            /* Limit on the number of pairs of brackets in a phone number. */
            String bracketPairLimit = Limit(0, 3);
            /*
            * An opening bracket at the beginning may not be closed, but subsequent ones should be.  It's
            * also possible that the leading bracket was dropped, so we shouldn't be surprised if we see a
            * closing bracket first. We limit the sets of brackets in a phone number to four.
            */
            MATCHING_BRACKETS = new PhoneRegex(
                "(?:[" + openingParens + "])?" + "(?:" + nonParens + "+" + "[" + closingParens + "])?" +
                nonParens + "+" +
                "(?:[" + openingParens + "]" + nonParens + "+[" + closingParens + "])" + bracketPairLimit +
                nonParens + "*", RegexOptions.Compiled);

            /* Limit on the number of leading (plus) characters. */
            String leadLimit = Limit(0, 2);
            /* Limit on the number of consecutive punctuation characters. */
            String punctuationLimit = Limit(0, 4);
            /* The maximum number of digits allowed in a digit-separated block. As we allow all digits in a
            * single block, set high enough to accommodate the entire national number and the international
            * country code. */
            int digitBlockLimit =
                PhoneNumberUtil.MAX_LENGTH_FOR_NSN + PhoneNumberUtil.MAX_LENGTH_COUNTRY_CODE;
            /* Limit on the number of blocks separated by punctuation. Uses digitBlockLimit since some
            * formats use spaces to separate each digit. */
            String blockLimit = Limit(0, digitBlockLimit);

            /* A punctuation sequence allowing white space. */
            String punctuation = "[" + PhoneNumberUtil.VALID_PUNCTUATION + "]" + punctuationLimit;
            /* A digits block without punctuation. */
            String digitSequence = "\\p{Nd}" + Limit(1, digitBlockLimit);
            String leadClassChars = openingParens + PhoneNumberUtil.PLUS_CHARS;
            String leadClass = "[" + leadClassChars + "]";
            LEAD_CLASS = new PhoneRegex(leadClass, RegexOptions.Compiled);
            

            /* Phone number pattern allowing optional punctuation. */
            PATTERN = new PhoneRegex(
                "(?:" + leadClass + punctuation + ")" + leadLimit +
                digitSequence + "(?:" + punctuation + digitSequence + ")" + blockLimit +
                "(?:" + PhoneNumberUtil.EXTN_PATTERNS_FOR_MATCHING + ")?",
                PhoneNumberUtil.REGEX_FLAGS);
        }

        /** Returns a regular expression quantifier with an upper and lower limit. */
        private static String Limit(int lower, int upper)
        {
            if ((lower < 0) || (upper <= 0) || (upper < lower))
                throw new ArgumentOutOfRangeException();
            return "{" + lower + "," + upper + "}";
        }

        /** The phone number utility. */
        private readonly PhoneNumberUtil phoneUtil;
        /** The text searched for phone numbers. */
        private readonly String text;
        /**
        * The region (country) to assume for phone numbers without an international prefix, possibly
        * null.
        */
        private readonly String preferredRegion;
        /** The degree of validation requested. */
        private readonly PhoneNumberUtil.Leniency leniency;
        /** The maximum number of retries after matching an invalid number. */
        private long maxTries;

        /** The last successful match, null unless in {@link State#READY}. */
        private PhoneNumberMatch lastMatch = null;
        /** The next index to start searching at. Undefined in {@link State#DONE}. */
        private int searchIndex = 0;

        /**
        * Creates a new instance. See the factory methods in {@link PhoneNumberUtil} on how to obtain a
        * new instance.
        *
        * @param util      the phone number util to use
        * @param text      the character sequence that we will search, null for no text
        * @param country   the country to assume for phone numbers not written in international format
        *                  (with a leading plus, or with the international dialing prefix of the
        *                  specified region). May be null or "ZZ" if only numbers with a
        *                  leading plus should be considered.
        * @param leniency  the leniency to use when evaluating candidate phone numbers
        * @param maxTries  the maximum number of invalid numbers to try before giving up on the text.
        *                  This is to cover degenerate cases where the text has a lot of false positives
        *                  in it. Must be {@code >= 0}.
        */
        public PhoneNumberMatcher(PhoneNumberUtil util, String text, String country, PhoneNumberUtil.Leniency leniency,
            long maxTries)
        {
            if (util == null)
                throw new ArgumentNullException();

            if (maxTries < 0)
                throw new ArgumentOutOfRangeException();

            this.phoneUtil = util;
            this.text = (text != null) ? text : "";
            this.preferredRegion = country;
            this.leniency = leniency;
            this.maxTries = maxTries;
        }

        /**
        * Attempts to find the next subsequence in the searched sequence on or after {@code searchIndex}
        * that represents a phone number. Returns the next match, null if none was found.
        *
        * @param index  the search index to start searching at
        * @return  the phone number match found, null if none can be found
        */
        private PhoneNumberMatch Find(int index)
        {
            while (maxTries > 0)
            {
                var matcher = PATTERN.Match(text, index);
                if (!matcher.Success)
                    break;
                int start = matcher.Index;
                String candidate = text.Substring(start, matcher.Length);

                // Check for extra numbers at the end.
                // TODO: This is the place to start when trying to support extraction of multiple phone number
                // from split notations (+41 79 123 45 67 / 68).
                candidate = TrimAfterFirstMatch(PhoneNumberUtil.SECOND_NUMBER_START_PATTERN, candidate);

                PhoneNumberMatch match = ExtractMatch(candidate, start);
                if (match != null)
                {
                    return match;
                }

                index = start + candidate.Length;
                maxTries--;
            }

            return null;
        }

        /**
        * Trims away any characters after the first match of {@code pattern} in {@code candidate},
        * returning the trimmed version.
        */
        private static String TrimAfterFirstMatch(Regex pattern, String candidate)
        {
            var trailingCharsMatcher = pattern.Match(candidate);
            if (trailingCharsMatcher.Success)
            {
                candidate = candidate.Substring(0, trailingCharsMatcher.Index);
            }
            return candidate;
        }

        /**
        * Helper method to determine if a character is a Latin-script letter or not. For our purposes,
        * combining marks should also return true since we assume they have been added to a preceding
        * Latin character.
        */
        public static bool IsLatinLetter(char letter)
        {
            // Combining marks are a subset of non-spacing-mark.
            if (!char.IsLetter(letter) && char.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                return false;
            return
                letter >= 0x0000 && letter <= 0x007F        // BASIC_LATIN
                || letter >= 0x0080 && letter <= 0x00FF     // LATIN_1_SUPPLEMENT
                || letter >= 0x0100 && letter <= 0x017F     // LATIN_EXTENDED_A
                || letter >= 0x1E00 && letter <= 0x1EFF     // LATIN_EXTENDED_ADDITIONAL
                || letter >= 0x0180 && letter <= 0x024F     // LATIN_EXTENDED_B
                || letter >= 0x0300 && letter <= 0x036F     // COMBINING_DIACRITICAL_MARKS
                ;
        }

        private static bool IsInvalidPunctuationSymbol(char character)
        {
            return character == '%' || char.GetUnicodeCategory(character) == UnicodeCategory.CurrencySymbol;
        }

        public static String TrimAfterUnwantedChars(String s)
        {
            int found = -1;
            char c;
            UnicodeCategory uc;
            for (int i = 0; i != s.Length; ++i)
            {
                c = s[i];
                uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (c != '#' && (
                    uc != UnicodeCategory.UppercaseLetter &&
                    uc != UnicodeCategory.LowercaseLetter &&
                    uc != UnicodeCategory.TitlecaseLetter &&
                    uc != UnicodeCategory.ModifierLetter &&
                    uc != UnicodeCategory.OtherLetter &&
                    uc != UnicodeCategory.DecimalDigitNumber &&
                    uc != UnicodeCategory.LetterNumber &&
                    uc != UnicodeCategory.OtherNumber))
                {
                    if (found < 0)
                        found = i;
                }
                else
                {
                    found = -1;
                }
            }
            if (found >= 0)
                return s.Substring(0, found);
            return s;
        }

        /**
        * Attempts to extract a match from a {@code candidate} character sequence.
        *
        * @param candidate  the candidate text that might contain a phone number
        * @param offset  the offset of {@code candidate} within {@link #text}
        * @return  the match found, null if none can be found
        */
        private PhoneNumberMatch ExtractMatch(String candidate, int offset)
        {
            // Skip a match that is more likely to be a date.
            if (SLASH_SEPARATED_DATES.Match(candidate).Success)
                return null;
            // Skip potential time-stamps.
            if (TIME_STAMPS.Match(candidate).Success)
            {
                String followingText = text.Substring(offset + candidate.Length);
                if (TIME_STAMPS_SUFFIX.MatchBeginning(followingText).Success)
                    return null;
            }

            // Try to come up with a valid match given the entire candidate.
            String rawString = candidate;
            PhoneNumberMatch match = ParseAndVerify(rawString, offset);
            if (match != null)
                return match;

            // If that failed, try to find an "inner match" - there might be a phone number within this
            // candidate.
            return ExtractInnerMatch(rawString, offset);
        }

        /**
        * Attempts to extract a match from {@code candidate} if the whole candidate does not qualify as a
        * match.
        *
        * @param candidate  the candidate text that might contain a phone number
        * @param offset  the current offset of {@code candidate} within {@link #text}
        * @return  the match found, null if none can be found
        */
        private PhoneNumberMatch ExtractInnerMatch(String candidate, int offset)
        {
            foreach(var possibleInnterMatch in INNER_MATCHES)
            {
                int rangeStart = 0;
                var groupMatcher = possibleInnterMatch.Matches(candidate);
                var isFirstMatch = true;
                foreach(Match groupMatch in groupMatcher)
                {
                    if (!groupMatch.Success)
                    {
                        continue;
                    }
                    if(maxTries <= 0)
                    {
                        break;
                    }
                
                    if (isFirstMatch)
                    {
                        // We should handle any group before this one too
                        var group = TrimAfterFirstMatch(PhoneNumberUtil.UNWANTED_END_CHAR_PATTERN, candidate.Substring(0, groupMatch.Index));
                        var match = ParseAndVerify(group, offset);
                        if(match != null)
                        {
                            return match;
                        }
                        maxTries--;
                        isFirstMatch = false;
                    }
                    var nextGroup = TrimAfterFirstMatch(PhoneNumberUtil.UNWANTED_END_CHAR_PATTERN, groupMatch.Groups[1].Value);
                    var nextMatch = ParseAndVerify(nextGroup, offset + groupMatch.Groups[1].Index);
                    if (nextMatch != null)
                    {
                        return nextMatch;
                    }
                    maxTries--;
                }                
            }
            return null;
        }

        /**
        * Parses a phone number from the {@code candidate} using {@link PhoneNumberUtil#parse} and
        * verifies it matches the requested {@link #leniency}. If parsing and verification succeed, a
        * corresponding {@link PhoneNumberMatch} is returned, otherwise this method returns null.
        *
        * @param candidate  the candidate match
        * @param offset  the offset of {@code candidate} within {@link #text}
        * @return  the parsed and validated phone number match, or null
        */
        private PhoneNumberMatch ParseAndVerify(String candidate, int offset)
        {
            try
            {
                // Check the candidate doesn't contain any formatting which would indicate that it really
                // isn't a phone number.
                if (!MATCHING_BRACKETS.MatchAll(candidate).Success || PUB_PAGES.MatchBeginning(candidate).Success)
                    return null;

                // If leniency is set to VALID or stricter, we also want to skip numbers that are surrounded
                // by Latin alphabetic characters, to skip cases like abc8005001234 or 8005001234def.
                if (leniency >= PhoneNumberUtil.Leniency.VALID)
                {
                    // If the candidate is not at the start of the text, and does not start with phone-number
                    // punctuation, check the previous character.
                    if (offset > 0 && !LEAD_CLASS.MatchBeginning(candidate).Success)
                    {
                        char previousChar = text[offset - 1];
                        // We return null if it is a latin letter or an invalid punctuation symbol.
                        if (IsInvalidPunctuationSymbol(previousChar) || IsLatinLetter(previousChar))
                        {
                            return null;
                        }
                    }
                    int lastCharIndex = offset + candidate.Length;
                    if (lastCharIndex < text.Length)
                    {
                        char nextChar = text[lastCharIndex];
                        if (IsInvalidPunctuationSymbol(nextChar) || IsLatinLetter(nextChar))
                        {
                            return null;
                        }
                    }
                }

                PhoneNumber number = phoneUtil.ParseAndKeepRawInput(candidate, preferredRegion);
                // Check Israel * numbers: these are a special case in that they are four-digit numbers that
                // our library supports, but they can only be dialled with a leading *. Since we don't
                // actually store or detect the * in our phone number library, this means in practice we
                // detect most four digit numbers as being valid for Israel. We are considering moving these
                // numbers to ShortNumberInfo instead, in which case this problem would go away, but in the
                // meantime we want to restrict the false matches so we only allow these numbers if they are
                // preceded by a star. We enforce this for all leniency levels even though these numbers are
                // technically accepted by isPossibleNumber and isValidNumber since we consider it to be a
                // deficiency in those methods that they accept these numbers without the *.
                // TODO: Remove this or make it significantly less hacky once we've decided how to
                // handle these short codes going forward in ShortNumberInfo. We could use the formatting
                // rules for instance, but that would be slower.
                if (phoneUtil.GetRegionCodeForCountryCode(number.CountryCode).Equals("IL") &&
                    phoneUtil.GetNationalSignificantNumber(number).Length == 4 &&
                    (offset == 0 || (offset > 0 && text[offset - 1] != '*')))
                {
                    // No match.
                    return null;
                }

                if (phoneUtil.Verify(leniency, number, candidate, phoneUtil))
                {
                    // We used parseAndKeepRawInput to create this number, but for now we don't return the extra
                    // values parsed. TODO: stop clearing all values here and switch all users over
                    // to using rawInput() rather than the rawString() of PhoneNumberMatch.
                    var bnumber = number.ToBuilder();
                    bnumber.ClearCountryCodeSource();
                    bnumber.ClearRawInput();
                    bnumber.ClearPreferredDomesticCarrierCode();
                    return new PhoneNumberMatch(offset, candidate, bnumber.Build());
                }
            }
            catch (NumberParseException)
            {
                // ignore and continue
            }
            return null;
        }

        /**
        * Returns true if the groups of digits found in our candidate phone number match our
        * expectations.
        *
        * @param number  the original number we found when parsing
        * @param normalizedCandidate  the candidate number, normalized to only contain ASCII digits,
        *     but with non-digits (spaces etc) retained
        * @param expectedNumberGroups  the groups of digits that we would expect to see if we
        *     formatted this number
        */
        public delegate bool CheckGroups(PhoneNumberUtil util, PhoneNumber number,
                StringBuilder normalizedCandidate, String[] expectedNumberGroups);

        public static bool AllNumberGroupsRemainGrouped(PhoneNumberUtil util,
            PhoneNumber number,
            StringBuilder normalizedCandidate,
            String[] formattedNumberGroups)
        {
            int fromIndex = 0;
            if (number.CountryCodeSource != PhoneNumber.Types.CountryCodeSource.FROM_DEFAULT_COUNTRY)
            {
                // First skip the country code if the normalized candidate contained it.
                String countryCode = number.CountryCode.ToString();
                fromIndex = normalizedCandidate.ToString().IndexOf(countryCode) + countryCode.Length;
            }


            // Check each group of consecutive digits are not broken into separate groupings in the
            // {@code normalizedCandidate} string.
            for (int i = 0; i < formattedNumberGroups.Length; i++)
            {
                // Fails if the substring of {@code normalizedCandidate} starting from {@code fromIndex}
                // doesn't contain the consecutive digits in formattedNumberGroups[i].
                fromIndex = normalizedCandidate.ToString().IndexOf(formattedNumberGroups[i], fromIndex);
                if (fromIndex < 0)
                {
                    return false;
                }
                // Moves {@code fromIndex} forward.
                fromIndex += formattedNumberGroups[i].Length;
                if (i == 0 && fromIndex < normalizedCandidate.Length)
                {
                    // We are at the position right after the NDC. We get the region used for formatting
                    // information based on the country code in the phone number, rather than the number itself,
                    // as we do not need to distinguish between different countries with the same country
                    // calling code and this is faster.
                    String region = util.GetRegionCodeForCountryCode(number.CountryCode);
                    if (util.GetNddPrefixForRegion(region, true) != null &&
                        Char.IsDigit(normalizedCandidate.ToString()[fromIndex]))
                    {

                        // This means there is no formatting symbol after the NDC. In this case, we only
                        // accept the number if there is no formatting symbol at all in the number, except
                        // for extensions. This is only important for countries with national prefixes.
                        String nationalSignificantNumber = util.GetNationalSignificantNumber(number);
                        return normalizedCandidate.ToString().Substring(fromIndex - formattedNumberGroups[i].Length)
                            .StartsWith(nationalSignificantNumber);
                    }
                }
            }
            // The check here makes sure that we haven't mistakenly already used the extension to
            // match the last group of the subscriber number. Note the extension cannot have
            // formatting in-between digits.
            return normalizedCandidate.ToString().Substring(fromIndex).Contains(number.Extension);
        }

        public static bool AllNumberGroupsAreExactlyPresent(PhoneNumberUtil util,
            PhoneNumber number,
            StringBuilder normalizedCandidate,
            String[] formattedNumberGroups)
        {
            String[] candidateGroups =
                PhoneNumberUtil.NON_DIGITS_PATTERN.Split(normalizedCandidate.ToString());
            // Set this to the last group, skipping it if the number has an extension.
            int candidateNumberGroupIndex =
                number.HasExtension ? candidateGroups.Length - 2 : candidateGroups.Length - 1;
            // First we check if the national significant number is formatted as a block.
            // We use contains and not equals, since the national significant number may be present with
            // a prefix such as a national number prefix, or the country code itself.
            if (candidateGroups.Length == 1 ||
                candidateGroups[candidateNumberGroupIndex].Contains(
                    util.GetNationalSignificantNumber(number)))
            {
                return true;
            }
            // Starting from the end, go through in reverse, excluding the first group, and check the
            // candidate and number groups are the same.
            for (int formattedNumberGroupIndex = (formattedNumberGroups.Length - 1);
                formattedNumberGroupIndex > 0 && candidateNumberGroupIndex >= 0;
                formattedNumberGroupIndex--, candidateNumberGroupIndex--)
            {
                if (!candidateGroups[candidateNumberGroupIndex].Equals(
                    formattedNumberGroups[formattedNumberGroupIndex]))
                {
                    return false;
                }
            }
            // Now check the first group. There may be a national prefix at the start, so we only check
            // that the candidate group ends with the formatted number group.
            return (candidateNumberGroupIndex >= 0 &&
                candidateGroups[candidateNumberGroupIndex].EndsWith(formattedNumberGroups[0]));
        }

        /**
        * Helper method to get the national-number part of a number, formatted without any national
        * prefix, and return it as a set of digit blocks that would be formatted together.
        */
        private static String[] GetNationalNumberGroups(PhoneNumberUtil util, PhoneNumber number,
            NumberFormat formattingPattern)
        {
            if (formattingPattern == null)
            {
                // This will be in the format +CC-DG;ext=EXT where DG represents groups of digits.
                String rfc3966Format = util.Format(number, PhoneNumberFormat.RFC3966);
                // We remove the extension part from the formatted string before splitting it into different
                // groups.
                int endIndex = rfc3966Format.IndexOf(';');
                if (endIndex < 0)
                {
                    endIndex = rfc3966Format.Length;
                }
                // The country-code will have a '-' following it.
                int startIndex = rfc3966Format.IndexOf('-') + 1;
                return rfc3966Format.Substring(startIndex, endIndex - startIndex).Split(new []{'-'});
            }
            else
            {
                // We format the NSN only, and split that according to the separator.
                String nationalSignificantNumber = util.GetNationalSignificantNumber(number);
                return util.FormatNsnUsingPattern(nationalSignificantNumber,
                    formattingPattern, PhoneNumberFormat.RFC3966).Split(new []{'-'});
            }
        }

        public static bool CheckNumberGroupingIsValid(
            PhoneNumber number, String candidate, PhoneNumberUtil util, CheckGroups checker)
        {
            // TODO: Evaluate how this works for other locales (testing has been limited to NANPA regions)
            // and optimise if necessary.
            StringBuilder normalizedCandidate =
                PhoneNumberUtil.NormalizeDigits(candidate, true /* keep non-digits */);
            String[] formattedNumberGroups = PhoneNumberMatcher.GetNationalNumberGroups(util, number, null);
            if (checker(util, number, normalizedCandidate, formattedNumberGroups))
            {
                return true;
            }
            // If this didn't pass, see if there are any alternate formats, and try them instead.
            var alternateFormats =
                MetadataManager.GetAlternateFormatsForCountry(number.CountryCode);
            if (alternateFormats != null)
            {
                foreach (var alternateFormat in alternateFormats.NumberFormatList)
                {
                    formattedNumberGroups = GetNationalNumberGroups(util, number, alternateFormat);
                    if (checker(util, number, normalizedCandidate, formattedNumberGroups))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ContainsMoreThanOneSlashInNationalNumber(PhoneNumber number, String candidate)
        {
            int firstSlashInBodyIndex = candidate.IndexOf('/');
            if (firstSlashInBodyIndex < 0)
            {
                // No slashes, this is okay.
                return false;
            }
            // Now look for a second one.
            int secondSlashInBodyIndex = candidate.IndexOf('/', firstSlashInBodyIndex + 1);
            if (secondSlashInBodyIndex < 0)
            {
                // Only one slash, this is okay.
                return false;
            }

            // If the first slash is after the country calling code, this is permitted.
            bool candidateHasCountryCode =
                (number.CountryCodeSource == PhoneNumber.Types.CountryCodeSource.FROM_NUMBER_WITH_PLUS_SIGN ||
                 number.CountryCodeSource == PhoneNumber.Types.CountryCodeSource.FROM_NUMBER_WITHOUT_PLUS_SIGN);
            if (candidateHasCountryCode &&
                PhoneNumberUtil.NormalizeDigitsOnly(candidate.Substring(0, firstSlashInBodyIndex))
                    .Equals(number.CountryCode.ToString()))
            {
                // Any more slashes and this is illegal.
                return candidate.Substring(secondSlashInBodyIndex + 1).Contains("/");
            }
            return true;
        }


        public static bool ContainsOnlyValidXChars(
            PhoneNumber number, String candidate, PhoneNumberUtil util)
        {
            // The characters 'x' and 'X' can be (1) a carrier code, in which case they always precede the
            // national significant number or (2) an extension sign, in which case they always precede the
            // extension number. We assume a carrier code is more than 1 digit, so the first case has to
            // have more than 1 consecutive 'x' or 'X', whereas the second case can only have exactly 1 'x'
            // or 'X'. We ignore the character if it appears as the last character of the string.
            for (int index = 0; index < candidate.Length - 1; index++)
            {
                char charAtIndex = candidate[index];
                if (charAtIndex == 'x' || charAtIndex == 'X')
                {
                    char charAtNextIndex = candidate[index + 1];
                    if (charAtNextIndex == 'x' || charAtNextIndex == 'X')
                    {
                        // This is the carrier code case, in which the 'X's always precede the national
                        // significant number.
                        index++;
                        if (util.IsNumberMatch(number, candidate.Substring(index)) != PhoneNumberUtil.MatchType.NSN_MATCH)
                        {
                            return false;
                        }
                        // This is the extension sign case, in which the 'x' or 'X' should always precede the
                        // extension number.
                    }
                    else if (!PhoneNumberUtil.NormalizeDigitsOnly(candidate.Substring(index)).Equals(
                        number.Extension))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsNationalPrefixPresentIfRequired(PhoneNumber number, PhoneNumberUtil util)
        {
            // First, check how we deduced the country code. If it was written in international format, then
            // the national prefix is not required.
            if (number.CountryCodeSource != PhoneNumber.Types.CountryCodeSource.FROM_DEFAULT_COUNTRY)
            {
                return true;
            }
            String phoneNumberRegion =
                util.GetRegionCodeForCountryCode(number.CountryCode);
            PhoneMetadata metadata = util.GetMetadataForRegion(phoneNumberRegion);
            if (metadata == null)
            {
                return true;
            }
            // Check if a national prefix should be present when formatting this number.
            String nationalNumber = util.GetNationalSignificantNumber(number);
            NumberFormat formatRule =
                util.ChooseFormattingPatternForNumber(metadata.NumberFormatList, nationalNumber);
            // To do this, we check that a national prefix formatting rule was present and that it wasn't
            // just the first-group symbol ($1) with punctuation.
            if ((formatRule != null) && formatRule.NationalPrefixFormattingRule.Length > 0)
            {
                if (formatRule.NationalPrefixOptionalWhenFormatting)
                {
                    // The national-prefix is optional in these cases, so we don't need to check if it was
                    // present.
                    return true;
                }
                if (PhoneNumberUtil.FormattingRuleHasFirstGroupOnly(formatRule.NationalPrefixFormattingRule))
                {
                    // National Prefix not needed for this number.
                    return true;
                }
                // Normalize the remainder.
                String rawInputCopy = PhoneNumberUtil.NormalizeDigitsOnly(number.RawInput);
                StringBuilder rawInput = new StringBuilder(rawInputCopy);
                // Check if we found a national prefix and/or carrier code at the start of the raw input, and
                // return the result.
                return util.MaybeStripNationalPrefixAndCarrierCode(rawInput, metadata, null);
            }
            return true;
        }

        public PhoneNumberMatch Current
        {
            get { return lastMatch; }
        }

        Object IEnumerator.Current
        {
            get { return lastMatch; }
        }

        public bool MoveNext()
        {
            lastMatch = Find(searchIndex);
            if (lastMatch != null)
                searchIndex = lastMatch.Start + lastMatch.Length;
            return lastMatch != null;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}
