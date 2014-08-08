/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PhoneNumbers.Test
{
    /**
    * Unit tests for AsYouTypeFormatter.java
    *
    * Note that these tests use the test metadata, not the normal metadata file, so should not be used
    * for regression test purposes - these tests are illustrative only and test functionality.
    *
    * @author Shaopeng Jia
    */
    [TestFixture]
    class TestAsYouTypeFormatter: TestMetadataTestCase
    {
        [Test]
        public void TestInvalidRegion()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("ZZ");
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+4", formatter.InputDigit('4'));
            Assert.AreEqual("+48 ", formatter.InputDigit('8'));
            Assert.AreEqual("+48 8", formatter.InputDigit('8'));
            Assert.AreEqual("+48 88", formatter.InputDigit('8'));
            Assert.AreEqual("+48 88 1", formatter.InputDigit('1'));
            Assert.AreEqual("+48 88 12", formatter.InputDigit('2'));
            Assert.AreEqual("+48 88 123", formatter.InputDigit('3'));
            Assert.AreEqual("+48 88 123 1", formatter.InputDigit('1'));
            Assert.AreEqual("+48 88 123 12", formatter.InputDigit('2'));

            formatter.Clear();
            Assert.AreEqual("6", formatter.InputDigit('6'));
            Assert.AreEqual("65", formatter.InputDigit('5'));
            Assert.AreEqual("650", formatter.InputDigit('0'));
            Assert.AreEqual("6502", formatter.InputDigit('2'));
            Assert.AreEqual("65025", formatter.InputDigit('5'));
            Assert.AreEqual("650253", formatter.InputDigit('3'));
        }

        [Test]
        public void TestInvalidPlusSign()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("ZZ");
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+4", formatter.InputDigit('4'));
            Assert.AreEqual("+48 ", formatter.InputDigit('8'));
            Assert.AreEqual("+48 8", formatter.InputDigit('8'));
            Assert.AreEqual("+48 88", formatter.InputDigit('8'));
            Assert.AreEqual("+48 88 1", formatter.InputDigit('1'));
            Assert.AreEqual("+48 88 12", formatter.InputDigit('2'));
            Assert.AreEqual("+48 88 123", formatter.InputDigit('3'));
            Assert.AreEqual("+48 88 123 1", formatter.InputDigit('1'));
            // A plus sign can only appear at the beginning of the number; otherwise, no formatting is
            // applied. 
            Assert.AreEqual("+48881231+", formatter.InputDigit('+'));
            Assert.AreEqual("+48881231+2", formatter.InputDigit('2'));
        }

        [Test]
        public void TestTooLongNumberMatchingMultipleLeadingDigits()
        {
            // See http://code.google.com/p/libphonenumber/issues/detail?id=36
            // The bug occurred last time for countries which have two formatting rules with exactly the
            // same leading digits pattern but differ in length.
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("ZZ");
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+81 ", formatter.InputDigit('1'));
            Assert.AreEqual("+81 9", formatter.InputDigit('9'));
            Assert.AreEqual("+81 90", formatter.InputDigit('0'));
            Assert.AreEqual("+81 90 1", formatter.InputDigit('1'));
            Assert.AreEqual("+81 90 12", formatter.InputDigit('2'));
            Assert.AreEqual("+81 90 123", formatter.InputDigit('3'));
            Assert.AreEqual("+81 90 1234", formatter.InputDigit('4'));
            Assert.AreEqual("+81 90 1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("+81 90 1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("+81 90 1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("+81 90 1234 5678", formatter.InputDigit('8'));
            Assert.AreEqual("+81 90 12 345 6789", formatter.InputDigit('9'));
            Assert.AreEqual("+81901234567890", formatter.InputDigit('0'));
            Assert.AreEqual("+819012345678901", formatter.InputDigit('1'));
        }

        [Test]
        public void TestCountryWithSpaceInNationalPrefixFormattingRule()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("BY");
            Assert.AreEqual("8", formatter.InputDigit('8'));
            Assert.AreEqual("88", formatter.InputDigit('8'));
            Assert.AreEqual("881", formatter.InputDigit('1'));
            Assert.AreEqual("8 819", formatter.InputDigit('9'));
            Assert.AreEqual("8 8190", formatter.InputDigit('0'));
            // The formatting rule for 5 digit numbers states that no space should be present after the
            // national prefix.
            Assert.AreEqual("881 901", formatter.InputDigit('1'));
            Assert.AreEqual("8 819 012", formatter.InputDigit('2'));
            // Too long, no formatting rule applies.
            Assert.AreEqual("88190123", formatter.InputDigit('3'));
        }

        [Test]
        public void TestCountryWithSpaceInNationalPrefixFormattingRuleAndLongNdd()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("BY");
            Assert.AreEqual("9", formatter.InputDigit('9'));
            Assert.AreEqual("99", formatter.InputDigit('9'));
            Assert.AreEqual("999", formatter.InputDigit('9'));
            Assert.AreEqual("9999", formatter.InputDigit('9'));
            Assert.AreEqual("99999 ", formatter.InputDigit('9'));
            Assert.AreEqual("99999 1", formatter.InputDigit('1'));
            Assert.AreEqual("99999 12", formatter.InputDigit('2'));
            Assert.AreEqual("99999 123", formatter.InputDigit('3'));
            Assert.AreEqual("99999 1234", formatter.InputDigit('4'));
            Assert.AreEqual("99999 12 345", formatter.InputDigit('5'));
        }


        [Test]
        public void TestAYTFUS()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("US");
            Assert.AreEqual("6", formatter.InputDigit('6'));
            Assert.AreEqual("65", formatter.InputDigit('5'));
            Assert.AreEqual("650", formatter.InputDigit('0'));
            Assert.AreEqual("650 2", formatter.InputDigit('2'));
            Assert.AreEqual("650 25", formatter.InputDigit('5'));
            Assert.AreEqual("650 253", formatter.InputDigit('3'));
            // Note this is how a US local number (without area code) should be formatted.
            Assert.AreEqual("650 2532", formatter.InputDigit('2'));
            Assert.AreEqual("650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual("650 253 2222", formatter.InputDigit('2'));

            formatter.Clear();
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("16", formatter.InputDigit('6'));
            Assert.AreEqual("1 65", formatter.InputDigit('5'));
            Assert.AreEqual("1 650", formatter.InputDigit('0'));
            Assert.AreEqual("1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 25", formatter.InputDigit('5'));
            Assert.AreEqual("1 650 253", formatter.InputDigit('3'));
            Assert.AreEqual("1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 253 2222", formatter.InputDigit('2'));

            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011 ", formatter.InputDigit('1'));
            Assert.AreEqual("011 4", formatter.InputDigit('4'));
            Assert.AreEqual("011 44 ", formatter.InputDigit('4'));
            Assert.AreEqual("011 44 6", formatter.InputDigit('6'));
            Assert.AreEqual("011 44 61", formatter.InputDigit('1'));
            Assert.AreEqual("011 44 6 12", formatter.InputDigit('2'));
            Assert.AreEqual("011 44 6 123", formatter.InputDigit('3'));
            Assert.AreEqual("011 44 6 123 1", formatter.InputDigit('1'));
            Assert.AreEqual("011 44 6 123 12", formatter.InputDigit('2'));
            Assert.AreEqual("011 44 6 123 123", formatter.InputDigit('3'));
            Assert.AreEqual("011 44 6 123 123 1", formatter.InputDigit('1'));
            Assert.AreEqual("011 44 6 123 123 12", formatter.InputDigit('2'));
            Assert.AreEqual("011 44 6 123 123 123", formatter.InputDigit('3'));

            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011 ", formatter.InputDigit('1'));
            Assert.AreEqual("011 5", formatter.InputDigit('5'));
            Assert.AreEqual("011 54 ", formatter.InputDigit('4'));
            Assert.AreEqual("011 54 9", formatter.InputDigit('9'));
            Assert.AreEqual("011 54 91", formatter.InputDigit('1'));
            Assert.AreEqual("011 54 9 11", formatter.InputDigit('1'));
            Assert.AreEqual("011 54 9 11 2", formatter.InputDigit('2'));
            Assert.AreEqual("011 54 9 11 23", formatter.InputDigit('3'));
            Assert.AreEqual("011 54 9 11 231", formatter.InputDigit('1'));
            Assert.AreEqual("011 54 9 11 2312", formatter.InputDigit('2'));
            Assert.AreEqual("011 54 9 11 2312 1", formatter.InputDigit('1'));
            Assert.AreEqual("011 54 9 11 2312 12", formatter.InputDigit('2'));
            Assert.AreEqual("011 54 9 11 2312 123", formatter.InputDigit('3'));
            Assert.AreEqual("011 54 9 11 2312 1234", formatter.InputDigit('4'));

            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011 ", formatter.InputDigit('1'));
            Assert.AreEqual("011 2", formatter.InputDigit('2'));
            Assert.AreEqual("011 24", formatter.InputDigit('4'));
            Assert.AreEqual("011 244 ", formatter.InputDigit('4'));
            Assert.AreEqual("011 244 2", formatter.InputDigit('2'));
            Assert.AreEqual("011 244 28", formatter.InputDigit('8'));
            Assert.AreEqual("011 244 280", formatter.InputDigit('0'));
            Assert.AreEqual("011 244 280 0", formatter.InputDigit('0'));
            Assert.AreEqual("011 244 280 00", formatter.InputDigit('0'));
            Assert.AreEqual("011 244 280 000", formatter.InputDigit('0'));
            Assert.AreEqual("011 244 280 000 0", formatter.InputDigit('0'));
            Assert.AreEqual("011 244 280 000 00", formatter.InputDigit('0'));
            Assert.AreEqual("011 244 280 000 000", formatter.InputDigit('0'));

            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+4", formatter.InputDigit('4'));
            Assert.AreEqual("+48 ", formatter.InputDigit('8'));
            Assert.AreEqual("+48 8", formatter.InputDigit('8'));
            Assert.AreEqual("+48 88", formatter.InputDigit('8'));
            Assert.AreEqual("+48 88 1", formatter.InputDigit('1'));
            Assert.AreEqual("+48 88 12", formatter.InputDigit('2'));
            Assert.AreEqual("+48 88 123", formatter.InputDigit('3'));
            Assert.AreEqual("+48 88 123 1", formatter.InputDigit('1'));
            Assert.AreEqual("+48 88 123 12", formatter.InputDigit('2'));
            Assert.AreEqual("+48 88 123 12 1", formatter.InputDigit('1'));
            Assert.AreEqual("+48 88 123 12 12", formatter.InputDigit('2'));
        }

        [Test]
        public void TestAYTFUSFullWidthCharacters()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("US");
            Assert.AreEqual("\uFF16", formatter.InputDigit('\uFF16'));
            Assert.AreEqual("\uFF16\uFF15", formatter.InputDigit('\uFF15'));
            Assert.AreEqual("650", formatter.InputDigit('\uFF10'));
            Assert.AreEqual("650 2", formatter.InputDigit('\uFF12'));
            Assert.AreEqual("650 25", formatter.InputDigit('\uFF15'));
            Assert.AreEqual("650 253", formatter.InputDigit('\uFF13'));
            Assert.AreEqual("650 2532", formatter.InputDigit('\uFF12'));
            Assert.AreEqual("650 253 22", formatter.InputDigit('\uFF12'));
            Assert.AreEqual("650 253 222", formatter.InputDigit('\uFF12'));
            Assert.AreEqual("650 253 2222", formatter.InputDigit('\uFF12'));
        }

        [Test]
        public void TestAYTFUSMobileShortCode()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("US");
            Assert.AreEqual("*", formatter.InputDigit('*'));
            Assert.AreEqual("*1", formatter.InputDigit('1'));
            Assert.AreEqual("*12", formatter.InputDigit('2'));
            Assert.AreEqual("*121", formatter.InputDigit('1'));
            Assert.AreEqual("*121#", formatter.InputDigit('#'));
        }

        [Test]
        public void TestAYTFUSVanityNumber()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("US");
            Assert.AreEqual("8", formatter.InputDigit('8'));
            Assert.AreEqual("80", formatter.InputDigit('0'));
            Assert.AreEqual("800", formatter.InputDigit('0'));
            Assert.AreEqual("800 ", formatter.InputDigit(' '));
            Assert.AreEqual("800 M", formatter.InputDigit('M'));
            Assert.AreEqual("800 MY", formatter.InputDigit('Y'));
            Assert.AreEqual("800 MY ", formatter.InputDigit(' '));
            Assert.AreEqual("800 MY A", formatter.InputDigit('A'));
            Assert.AreEqual("800 MY AP", formatter.InputDigit('P'));
            Assert.AreEqual("800 MY APP", formatter.InputDigit('P'));
            Assert.AreEqual("800 MY APPL", formatter.InputDigit('L'));
            Assert.AreEqual("800 MY APPLE", formatter.InputDigit('E'));
        }

        [Test]
        public void TestAYTFAndRememberPositionUS()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("US");
            Assert.AreEqual("1", formatter.InputDigitAndRememberPosition('1'));
            Assert.AreEqual(1, formatter.GetRememberedPosition());
            Assert.AreEqual("16", formatter.InputDigit('6'));
            Assert.AreEqual("1 65", formatter.InputDigit('5'));
            Assert.AreEqual(1, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650", formatter.InputDigitAndRememberPosition('0'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 25", formatter.InputDigit('5'));
            // Note the remembered position for digit "0" changes from 4 to 5, because a space is now
            // inserted in the front.
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650 253", formatter.InputDigit('3'));
            Assert.AreEqual("1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650 253 222", formatter.InputDigitAndRememberPosition('2'));
            Assert.AreEqual(13, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650 253 2222", formatter.InputDigit('2'));
            Assert.AreEqual(13, formatter.GetRememberedPosition());
            Assert.AreEqual("165025322222", formatter.InputDigit('2'));
            Assert.AreEqual(10, formatter.GetRememberedPosition());
            Assert.AreEqual("1650253222222", formatter.InputDigit('2'));
            Assert.AreEqual(10, formatter.GetRememberedPosition());

            formatter.Clear();
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("16", formatter.InputDigitAndRememberPosition('6'));
            Assert.AreEqual(2, formatter.GetRememberedPosition());
            Assert.AreEqual("1 65", formatter.InputDigit('5'));
            Assert.AreEqual("1 650", formatter.InputDigit('0'));
            Assert.AreEqual(3, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 25", formatter.InputDigit('5'));
            Assert.AreEqual(3, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650 253", formatter.InputDigit('3'));
            Assert.AreEqual("1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual(3, formatter.GetRememberedPosition());
            Assert.AreEqual("1 650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual("1 650 253 2222", formatter.InputDigit('2'));
            Assert.AreEqual("165025322222", formatter.InputDigit('2'));
            Assert.AreEqual(2, formatter.GetRememberedPosition());
            Assert.AreEqual("1650253222222", formatter.InputDigit('2'));
            Assert.AreEqual(2, formatter.GetRememberedPosition());

            formatter.Clear();
            Assert.AreEqual("6", formatter.InputDigit('6'));
            Assert.AreEqual("65", formatter.InputDigit('5'));
            Assert.AreEqual("650", formatter.InputDigit('0'));
            Assert.AreEqual("650 2", formatter.InputDigit('2'));
            Assert.AreEqual("650 25", formatter.InputDigit('5'));
            Assert.AreEqual("650 253", formatter.InputDigit('3'));
            Assert.AreEqual("650 2532", formatter.InputDigitAndRememberPosition('2'));
            Assert.AreEqual(8, formatter.GetRememberedPosition());
            Assert.AreEqual("650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual(9, formatter.GetRememberedPosition());
            Assert.AreEqual("650 253 222", formatter.InputDigit('2'));
            // No more formatting when semicolon is entered.
            Assert.AreEqual("650253222;", formatter.InputDigit(';'));
            Assert.AreEqual(7, formatter.GetRememberedPosition());
            Assert.AreEqual("650253222;2", formatter.InputDigit('2'));

            formatter.Clear();
            Assert.AreEqual("6", formatter.InputDigit('6'));
            Assert.AreEqual("65", formatter.InputDigit('5'));
            Assert.AreEqual("650", formatter.InputDigit('0'));
            // No more formatting when users choose to do their own formatting.
            Assert.AreEqual("650-", formatter.InputDigit('-'));
            Assert.AreEqual("650-2", formatter.InputDigitAndRememberPosition('2'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("650-25", formatter.InputDigit('5'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("650-253", formatter.InputDigit('3'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("650-253-", formatter.InputDigit('-'));
            Assert.AreEqual("650-253-2", formatter.InputDigit('2'));
            Assert.AreEqual("650-253-22", formatter.InputDigit('2'));
            Assert.AreEqual("650-253-222", formatter.InputDigit('2'));
            Assert.AreEqual("650-253-2222", formatter.InputDigit('2'));

            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011 ", formatter.InputDigit('1'));
            Assert.AreEqual("011 4", formatter.InputDigitAndRememberPosition('4'));
            Assert.AreEqual("011 48 ", formatter.InputDigit('8'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("011 48 8", formatter.InputDigit('8'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("011 48 88", formatter.InputDigit('8'));
            Assert.AreEqual("011 48 88 1", formatter.InputDigit('1'));
            Assert.AreEqual("011 48 88 12", formatter.InputDigit('2'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("011 48 88 123", formatter.InputDigit('3'));
            Assert.AreEqual("011 48 88 123 1", formatter.InputDigit('1'));
            Assert.AreEqual("011 48 88 123 12", formatter.InputDigit('2'));
            Assert.AreEqual("011 48 88 123 12 1", formatter.InputDigit('1'));
            Assert.AreEqual("011 48 88 123 12 12", formatter.InputDigit('2'));

            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+1", formatter.InputDigit('1'));
            Assert.AreEqual("+1 6", formatter.InputDigitAndRememberPosition('6'));
            Assert.AreEqual("+1 65", formatter.InputDigit('5'));
            Assert.AreEqual("+1 650", formatter.InputDigit('0'));
            Assert.AreEqual(4, formatter.GetRememberedPosition());
            Assert.AreEqual("+1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual(4, formatter.GetRememberedPosition());
            Assert.AreEqual("+1 650 25", formatter.InputDigit('5'));
            Assert.AreEqual("+1 650 253", formatter.InputDigitAndRememberPosition('3'));
            Assert.AreEqual("+1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("+1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("+1 650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual(10, formatter.GetRememberedPosition());

            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+1", formatter.InputDigit('1'));
            Assert.AreEqual("+1 6", formatter.InputDigitAndRememberPosition('6'));
            Assert.AreEqual("+1 65", formatter.InputDigit('5'));
            Assert.AreEqual("+1 650", formatter.InputDigit('0'));
            Assert.AreEqual(4, formatter.GetRememberedPosition());
            Assert.AreEqual("+1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual(4, formatter.GetRememberedPosition());
            Assert.AreEqual("+1 650 25", formatter.InputDigit('5'));
            Assert.AreEqual("+1 650 253", formatter.InputDigit('3'));
            Assert.AreEqual("+1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("+1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("+1 650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual("+1650253222;", formatter.InputDigit(';'));
            Assert.AreEqual(3, formatter.GetRememberedPosition());
        }

        [Test]
        public void TestAYTFGBFixedLine()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("GB");
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("02", formatter.InputDigit('2'));
            Assert.AreEqual("020", formatter.InputDigit('0'));
            Assert.AreEqual("020 7", formatter.InputDigitAndRememberPosition('7'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("020 70", formatter.InputDigit('0'));
            Assert.AreEqual("020 703", formatter.InputDigit('3'));
            Assert.AreEqual(5, formatter.GetRememberedPosition());
            Assert.AreEqual("020 7031", formatter.InputDigit('1'));
            Assert.AreEqual("020 7031 3", formatter.InputDigit('3'));
            Assert.AreEqual("020 7031 30", formatter.InputDigit('0'));
            Assert.AreEqual("020 7031 300", formatter.InputDigit('0'));
            Assert.AreEqual("020 7031 3000", formatter.InputDigit('0'));
        }

        [Test]
        public void TestAYTFGBTollFree()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("GB");
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("08", formatter.InputDigit('8'));
            Assert.AreEqual("080", formatter.InputDigit('0'));
            Assert.AreEqual("080 7", formatter.InputDigit('7'));
            Assert.AreEqual("080 70", formatter.InputDigit('0'));
            Assert.AreEqual("080 703", formatter.InputDigit('3'));
            Assert.AreEqual("080 7031", formatter.InputDigit('1'));
            Assert.AreEqual("080 7031 3", formatter.InputDigit('3'));
            Assert.AreEqual("080 7031 30", formatter.InputDigit('0'));
            Assert.AreEqual("080 7031 300", formatter.InputDigit('0'));
            Assert.AreEqual("080 7031 3000", formatter.InputDigit('0'));
        }

        [Test]
        public void TestAYTFGBPremiumRate()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("GB");
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("09", formatter.InputDigit('9'));
            Assert.AreEqual("090", formatter.InputDigit('0'));
            Assert.AreEqual("090 7", formatter.InputDigit('7'));
            Assert.AreEqual("090 70", formatter.InputDigit('0'));
            Assert.AreEqual("090 703", formatter.InputDigit('3'));
            Assert.AreEqual("090 7031", formatter.InputDigit('1'));
            Assert.AreEqual("090 7031 3", formatter.InputDigit('3'));
            Assert.AreEqual("090 7031 30", formatter.InputDigit('0'));
            Assert.AreEqual("090 7031 300", formatter.InputDigit('0'));
            Assert.AreEqual("090 7031 3000", formatter.InputDigit('0'));
        }

        [Test]
        public void TestAYTFNZMobile()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("NZ");
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("02", formatter.InputDigit('2'));
            Assert.AreEqual("021", formatter.InputDigit('1'));
            Assert.AreEqual("02-11", formatter.InputDigit('1'));
            Assert.AreEqual("02-112", formatter.InputDigit('2'));
            // Note the unittest is using fake metadata which might produce non-ideal results.
            Assert.AreEqual("02-112 3", formatter.InputDigit('3'));
            Assert.AreEqual("02-112 34", formatter.InputDigit('4'));
            Assert.AreEqual("02-112 345", formatter.InputDigit('5'));
            Assert.AreEqual("02-112 3456", formatter.InputDigit('6'));
        }

        [Test]
        public void TestAYTFDE()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("DE");
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("03", formatter.InputDigit('3'));
            Assert.AreEqual("030", formatter.InputDigit('0'));
            Assert.AreEqual("030/1", formatter.InputDigit('1'));
            Assert.AreEqual("030/12", formatter.InputDigit('2'));
            Assert.AreEqual("030/123", formatter.InputDigit('3'));
            Assert.AreEqual("030/1234", formatter.InputDigit('4'));

            // 04134 1234
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("04", formatter.InputDigit('4'));
            Assert.AreEqual("041", formatter.InputDigit('1'));
            Assert.AreEqual("041 3", formatter.InputDigit('3'));
            Assert.AreEqual("041 34", formatter.InputDigit('4'));
            Assert.AreEqual("04134 1", formatter.InputDigit('1'));
            Assert.AreEqual("04134 12", formatter.InputDigit('2'));
            Assert.AreEqual("04134 123", formatter.InputDigit('3'));
            Assert.AreEqual("04134 1234", formatter.InputDigit('4'));

            // 08021 2345
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("08", formatter.InputDigit('8'));
            Assert.AreEqual("080", formatter.InputDigit('0'));
            Assert.AreEqual("080 2", formatter.InputDigit('2'));
            Assert.AreEqual("080 21", formatter.InputDigit('1'));
            Assert.AreEqual("08021 2", formatter.InputDigit('2'));
            Assert.AreEqual("08021 23", formatter.InputDigit('3'));
            Assert.AreEqual("08021 234", formatter.InputDigit('4'));
            Assert.AreEqual("08021 2345", formatter.InputDigit('5'));

            // 00 1 650 253 2250
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("00", formatter.InputDigit('0'));
            Assert.AreEqual("00 1 ", formatter.InputDigit('1'));
            Assert.AreEqual("00 1 6", formatter.InputDigit('6'));
            Assert.AreEqual("00 1 65", formatter.InputDigit('5'));
            Assert.AreEqual("00 1 650", formatter.InputDigit('0'));
            Assert.AreEqual("00 1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual("00 1 650 25", formatter.InputDigit('5'));
            Assert.AreEqual("00 1 650 253", formatter.InputDigit('3'));
            Assert.AreEqual("00 1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("00 1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("00 1 650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual("00 1 650 253 2222", formatter.InputDigit('2'));
        }

        [Test]
        public void TestAYTFAR()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("AR");
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011", formatter.InputDigit('1'));
            Assert.AreEqual("011 7", formatter.InputDigit('7'));
            Assert.AreEqual("011 70", formatter.InputDigit('0'));
            Assert.AreEqual("011 703", formatter.InputDigit('3'));
            Assert.AreEqual("011 7031", formatter.InputDigit('1'));
            Assert.AreEqual("011 7031-3", formatter.InputDigit('3'));
            Assert.AreEqual("011 7031-30", formatter.InputDigit('0'));
            Assert.AreEqual("011 7031-300", formatter.InputDigit('0'));
            Assert.AreEqual("011 7031-3000", formatter.InputDigit('0'));
        }

        [Test]
        public void TestAYTFARMobile()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("AR");
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+5", formatter.InputDigit('5'));
            Assert.AreEqual("+54 ", formatter.InputDigit('4'));
            Assert.AreEqual("+54 9", formatter.InputDigit('9'));
            Assert.AreEqual("+54 91", formatter.InputDigit('1'));
            Assert.AreEqual("+54 9 11", formatter.InputDigit('1'));
            Assert.AreEqual("+54 9 11 2", formatter.InputDigit('2'));
            Assert.AreEqual("+54 9 11 23", formatter.InputDigit('3'));
            Assert.AreEqual("+54 9 11 231", formatter.InputDigit('1'));
            Assert.AreEqual("+54 9 11 2312", formatter.InputDigit('2'));
            Assert.AreEqual("+54 9 11 2312 1", formatter.InputDigit('1'));
            Assert.AreEqual("+54 9 11 2312 12", formatter.InputDigit('2'));
            Assert.AreEqual("+54 9 11 2312 123", formatter.InputDigit('3'));
            Assert.AreEqual("+54 9 11 2312 1234", formatter.InputDigit('4'));
        }

        [Test]
        public void TestAYTFKR()
        {
            // +82 51 234 5678
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("KR");
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+82 ", formatter.InputDigit('2'));
            Assert.AreEqual("+82 5", formatter.InputDigit('5'));
            Assert.AreEqual("+82 51", formatter.InputDigit('1'));
            Assert.AreEqual("+82 51-2", formatter.InputDigit('2'));
            Assert.AreEqual("+82 51-23", formatter.InputDigit('3'));
            Assert.AreEqual("+82 51-234", formatter.InputDigit('4'));
            Assert.AreEqual("+82 51-234-5", formatter.InputDigit('5'));
            Assert.AreEqual("+82 51-234-56", formatter.InputDigit('6'));
            Assert.AreEqual("+82 51-234-567", formatter.InputDigit('7'));
            Assert.AreEqual("+82 51-234-5678", formatter.InputDigit('8'));

            // +82 2 531 5678
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+82 ", formatter.InputDigit('2'));
            Assert.AreEqual("+82 2", formatter.InputDigit('2'));
            Assert.AreEqual("+82 25", formatter.InputDigit('5'));
            Assert.AreEqual("+82 2-53", formatter.InputDigit('3'));
            Assert.AreEqual("+82 2-531", formatter.InputDigit('1'));
            Assert.AreEqual("+82 2-531-5", formatter.InputDigit('5'));
            Assert.AreEqual("+82 2-531-56", formatter.InputDigit('6'));
            Assert.AreEqual("+82 2-531-567", formatter.InputDigit('7'));
            Assert.AreEqual("+82 2-531-5678", formatter.InputDigit('8'));

            // +82 2 3665 5678
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+82 ", formatter.InputDigit('2'));
            Assert.AreEqual("+82 2", formatter.InputDigit('2'));
            Assert.AreEqual("+82 23", formatter.InputDigit('3'));
            Assert.AreEqual("+82 2-36", formatter.InputDigit('6'));
            Assert.AreEqual("+82 2-366", formatter.InputDigit('6'));
            Assert.AreEqual("+82 2-3665", formatter.InputDigit('5'));
            Assert.AreEqual("+82 2-3665-5", formatter.InputDigit('5'));
            Assert.AreEqual("+82 2-3665-56", formatter.InputDigit('6'));
            Assert.AreEqual("+82 2-3665-567", formatter.InputDigit('7'));
            Assert.AreEqual("+82 2-3665-5678", formatter.InputDigit('8'));

            // 02-114
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("02", formatter.InputDigit('2'));
            Assert.AreEqual("021", formatter.InputDigit('1'));
            Assert.AreEqual("02-11", formatter.InputDigit('1'));
            Assert.AreEqual("02-114", formatter.InputDigit('4'));

            // 02-1300
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("02", formatter.InputDigit('2'));
            Assert.AreEqual("021", formatter.InputDigit('1'));
            Assert.AreEqual("02-13", formatter.InputDigit('3'));
            Assert.AreEqual("02-130", formatter.InputDigit('0'));
            Assert.AreEqual("02-1300", formatter.InputDigit('0'));

            // 011-456-7890
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011", formatter.InputDigit('1'));
            Assert.AreEqual("011-4", formatter.InputDigit('4'));
            Assert.AreEqual("011-45", formatter.InputDigit('5'));
            Assert.AreEqual("011-456", formatter.InputDigit('6'));
            Assert.AreEqual("011-456-7", formatter.InputDigit('7'));
            Assert.AreEqual("011-456-78", formatter.InputDigit('8'));
            Assert.AreEqual("011-456-789", formatter.InputDigit('9'));
            Assert.AreEqual("011-456-7890", formatter.InputDigit('0'));

            // 011-9876-7890
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011", formatter.InputDigit('1'));
            Assert.AreEqual("011-9", formatter.InputDigit('9'));
            Assert.AreEqual("011-98", formatter.InputDigit('8'));
            Assert.AreEqual("011-987", formatter.InputDigit('7'));
            Assert.AreEqual("011-9876", formatter.InputDigit('6'));
            Assert.AreEqual("011-9876-7", formatter.InputDigit('7'));
            Assert.AreEqual("011-9876-78", formatter.InputDigit('8'));
            Assert.AreEqual("011-9876-789", formatter.InputDigit('9'));
            Assert.AreEqual("011-9876-7890", formatter.InputDigit('0'));
        }

        [Test]
        public void TestAYTF_MX()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("MX");

            // +52 800 123 4567
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 ", formatter.InputDigit('2'));
            Assert.AreEqual("+52 8", formatter.InputDigit('8'));
            Assert.AreEqual("+52 80", formatter.InputDigit('0'));
            Assert.AreEqual("+52 800", formatter.InputDigit('0'));
            Assert.AreEqual("+52 800 1", formatter.InputDigit('1'));
            Assert.AreEqual("+52 800 12", formatter.InputDigit('2'));
            Assert.AreEqual("+52 800 123", formatter.InputDigit('3'));
            Assert.AreEqual("+52 800 123 4", formatter.InputDigit('4'));
            Assert.AreEqual("+52 800 123 45", formatter.InputDigit('5'));
            Assert.AreEqual("+52 800 123 456", formatter.InputDigit('6'));
            Assert.AreEqual("+52 800 123 4567", formatter.InputDigit('7'));

            // +52 55 1234 5678
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 ", formatter.InputDigit('2'));
            Assert.AreEqual("+52 5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 55", formatter.InputDigit('5'));
            Assert.AreEqual("+52 55 1", formatter.InputDigit('1'));
            Assert.AreEqual("+52 55 12", formatter.InputDigit('2'));
            Assert.AreEqual("+52 55 123", formatter.InputDigit('3'));
            Assert.AreEqual("+52 55 1234", formatter.InputDigit('4'));
            Assert.AreEqual("+52 55 1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 55 1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("+52 55 1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("+52 55 1234 5678", formatter.InputDigit('8'));

            // +52 212 345 6789
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 ", formatter.InputDigit('2'));
            Assert.AreEqual("+52 2", formatter.InputDigit('2'));
            Assert.AreEqual("+52 21", formatter.InputDigit('1'));
            Assert.AreEqual("+52 212", formatter.InputDigit('2'));
            Assert.AreEqual("+52 212 3", formatter.InputDigit('3'));
            Assert.AreEqual("+52 212 34", formatter.InputDigit('4'));
            Assert.AreEqual("+52 212 345", formatter.InputDigit('5'));
            Assert.AreEqual("+52 212 345 6", formatter.InputDigit('6'));
            Assert.AreEqual("+52 212 345 67", formatter.InputDigit('7'));
            Assert.AreEqual("+52 212 345 678", formatter.InputDigit('8'));
            Assert.AreEqual("+52 212 345 6789", formatter.InputDigit('9'));

            // +52 1 55 1234 5678
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 ", formatter.InputDigit('2'));
            Assert.AreEqual("+52 1", formatter.InputDigit('1'));
            Assert.AreEqual("+52 15", formatter.InputDigit('5'));
            Assert.AreEqual("+52 1 55", formatter.InputDigit('5'));
            Assert.AreEqual("+52 1 55 1", formatter.InputDigit('1'));
            Assert.AreEqual("+52 1 55 12", formatter.InputDigit('2'));
            Assert.AreEqual("+52 1 55 123", formatter.InputDigit('3'));
            Assert.AreEqual("+52 1 55 1234", formatter.InputDigit('4'));
            Assert.AreEqual("+52 1 55 1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 1 55 1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("+52 1 55 1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("+52 1 55 1234 5678", formatter.InputDigit('8'));

            // +52 1 541 234 5678
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 ", formatter.InputDigit('2'));
            Assert.AreEqual("+52 1", formatter.InputDigit('1'));
            Assert.AreEqual("+52 15", formatter.InputDigit('5'));
            Assert.AreEqual("+52 1 54", formatter.InputDigit('4'));
            Assert.AreEqual("+52 1 541", formatter.InputDigit('1'));
            Assert.AreEqual("+52 1 541 2", formatter.InputDigit('2'));
            Assert.AreEqual("+52 1 541 23", formatter.InputDigit('3'));
            Assert.AreEqual("+52 1 541 234", formatter.InputDigit('4'));
            Assert.AreEqual("+52 1 541 234 5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 1 541 234 56", formatter.InputDigit('6'));
            Assert.AreEqual("+52 1 541 234 567", formatter.InputDigit('7'));
            Assert.AreEqual("+52 1 541 234 5678", formatter.InputDigit('8'));
        }

        [Test]
        public void TestAYTF_International_Toll_Free()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.US);
            // +800 1234 5678
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+80", formatter.InputDigit('0'));
            Assert.AreEqual("+800 ", formatter.InputDigit('0'));
            Assert.AreEqual("+800 1", formatter.InputDigit('1'));
            Assert.AreEqual("+800 12", formatter.InputDigit('2'));
            Assert.AreEqual("+800 123", formatter.InputDigit('3'));
            Assert.AreEqual("+800 1234", formatter.InputDigit('4'));
            Assert.AreEqual("+800 1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("+800 1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("+800 1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("+800 1234 5678", formatter.InputDigit('8'));
            Assert.AreEqual("+800123456789", formatter.InputDigit('9'));
        }

        [Test]
        public void TestAYTFMultipleLeadingDigitPatterns()
        {
            // +81 50 2345 6789
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("JP");
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+81 ", formatter.InputDigit('1'));
            Assert.AreEqual("+81 5", formatter.InputDigit('5'));
            Assert.AreEqual("+81 50", formatter.InputDigit('0'));
            Assert.AreEqual("+81 50 2", formatter.InputDigit('2'));
            Assert.AreEqual("+81 50 23", formatter.InputDigit('3'));
            Assert.AreEqual("+81 50 234", formatter.InputDigit('4'));
            Assert.AreEqual("+81 50 2345", formatter.InputDigit('5'));
            Assert.AreEqual("+81 50 2345 6", formatter.InputDigit('6'));
            Assert.AreEqual("+81 50 2345 67", formatter.InputDigit('7'));
            Assert.AreEqual("+81 50 2345 678", formatter.InputDigit('8'));
            Assert.AreEqual("+81 50 2345 6789", formatter.InputDigit('9'));

            // +81 222 12 5678
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+81 ", formatter.InputDigit('1'));
            Assert.AreEqual("+81 2", formatter.InputDigit('2'));
            Assert.AreEqual("+81 22", formatter.InputDigit('2'));
            Assert.AreEqual("+81 22 2", formatter.InputDigit('2'));
            Assert.AreEqual("+81 22 21", formatter.InputDigit('1'));
            Assert.AreEqual("+81 2221 2", formatter.InputDigit('2'));
            Assert.AreEqual("+81 222 12 5", formatter.InputDigit('5'));
            Assert.AreEqual("+81 222 12 56", formatter.InputDigit('6'));
            Assert.AreEqual("+81 222 12 567", formatter.InputDigit('7'));
            Assert.AreEqual("+81 222 12 5678", formatter.InputDigit('8'));

            // 011113
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("01", formatter.InputDigit('1'));
            Assert.AreEqual("011", formatter.InputDigit('1'));
            Assert.AreEqual("011 1", formatter.InputDigit('1'));
            Assert.AreEqual("011 11", formatter.InputDigit('1'));
            Assert.AreEqual("011113", formatter.InputDigit('3'));

            // +81 3332 2 5678
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+81 ", formatter.InputDigit('1'));
            Assert.AreEqual("+81 3", formatter.InputDigit('3'));
            Assert.AreEqual("+81 33", formatter.InputDigit('3'));
            Assert.AreEqual("+81 33 3", formatter.InputDigit('3'));
            Assert.AreEqual("+81 3332", formatter.InputDigit('2'));
            Assert.AreEqual("+81 3332 2", formatter.InputDigit('2'));
            Assert.AreEqual("+81 3332 2 5", formatter.InputDigit('5'));
            Assert.AreEqual("+81 3332 2 56", formatter.InputDigit('6'));
            Assert.AreEqual("+81 3332 2 567", formatter.InputDigit('7'));
            Assert.AreEqual("+81 3332 2 5678", formatter.InputDigit('8'));
        }

        [Test]
        public void TestAYTFLongIDD_AU()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("AU");
            // 0011 1 650 253 2250
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("00", formatter.InputDigit('0'));
            Assert.AreEqual("001", formatter.InputDigit('1'));
            Assert.AreEqual("0011", formatter.InputDigit('1'));
            Assert.AreEqual("0011 1 ", formatter.InputDigit('1'));
            Assert.AreEqual("0011 1 6", formatter.InputDigit('6'));
            Assert.AreEqual("0011 1 65", formatter.InputDigit('5'));
            Assert.AreEqual("0011 1 650", formatter.InputDigit('0'));
            Assert.AreEqual("0011 1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual("0011 1 650 25", formatter.InputDigit('5'));
            Assert.AreEqual("0011 1 650 253", formatter.InputDigit('3'));
            Assert.AreEqual("0011 1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("0011 1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("0011 1 650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual("0011 1 650 253 2222", formatter.InputDigit('2'));

            // 0011 81 3332 2 5678
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("00", formatter.InputDigit('0'));
            Assert.AreEqual("001", formatter.InputDigit('1'));
            Assert.AreEqual("0011", formatter.InputDigit('1'));
            Assert.AreEqual("00118", formatter.InputDigit('8'));
            Assert.AreEqual("0011 81 ", formatter.InputDigit('1'));
            Assert.AreEqual("0011 81 3", formatter.InputDigit('3'));
            Assert.AreEqual("0011 81 33", formatter.InputDigit('3'));
            Assert.AreEqual("0011 81 33 3", formatter.InputDigit('3'));
            Assert.AreEqual("0011 81 3332", formatter.InputDigit('2'));
            Assert.AreEqual("0011 81 3332 2", formatter.InputDigit('2'));
            Assert.AreEqual("0011 81 3332 2 5", formatter.InputDigit('5'));
            Assert.AreEqual("0011 81 3332 2 56", formatter.InputDigit('6'));
            Assert.AreEqual("0011 81 3332 2 567", formatter.InputDigit('7'));
            Assert.AreEqual("0011 81 3332 2 5678", formatter.InputDigit('8'));

            // 0011 244 250 253 222
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("00", formatter.InputDigit('0'));
            Assert.AreEqual("001", formatter.InputDigit('1'));
            Assert.AreEqual("0011", formatter.InputDigit('1'));
            Assert.AreEqual("00112", formatter.InputDigit('2'));
            Assert.AreEqual("001124", formatter.InputDigit('4'));
            Assert.AreEqual("0011 244 ", formatter.InputDigit('4'));
            Assert.AreEqual("0011 244 2", formatter.InputDigit('2'));
            Assert.AreEqual("0011 244 25", formatter.InputDigit('5'));
            Assert.AreEqual("0011 244 250", formatter.InputDigit('0'));
            Assert.AreEqual("0011 244 250 2", formatter.InputDigit('2'));
            Assert.AreEqual("0011 244 250 25", formatter.InputDigit('5'));
            Assert.AreEqual("0011 244 250 253", formatter.InputDigit('3'));
            Assert.AreEqual("0011 244 250 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("0011 244 250 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("0011 244 250 253 222", formatter.InputDigit('2'));
        }

        [Test]
        public void testAYTFLongIDD_KR()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("KR");
            // 00300 1 650 253 2222
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("00", formatter.InputDigit('0'));
            Assert.AreEqual("003", formatter.InputDigit('3'));
            Assert.AreEqual("0030", formatter.InputDigit('0'));
            Assert.AreEqual("00300", formatter.InputDigit('0'));
            Assert.AreEqual("00300 1 ", formatter.InputDigit('1'));
            Assert.AreEqual("00300 1 6", formatter.InputDigit('6'));
            Assert.AreEqual("00300 1 65", formatter.InputDigit('5'));
            Assert.AreEqual("00300 1 650", formatter.InputDigit('0'));
            Assert.AreEqual("00300 1 650 2", formatter.InputDigit('2'));
            Assert.AreEqual("00300 1 650 25", formatter.InputDigit('5'));
            Assert.AreEqual("00300 1 650 253", formatter.InputDigit('3'));
            Assert.AreEqual("00300 1 650 253 2", formatter.InputDigit('2'));
            Assert.AreEqual("00300 1 650 253 22", formatter.InputDigit('2'));
            Assert.AreEqual("00300 1 650 253 222", formatter.InputDigit('2'));
            Assert.AreEqual("00300 1 650 253 2222", formatter.InputDigit('2'));
        }

        [Test]
        public void testAYTFLongNDD_KR()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("KR");
            // 08811-9876-7890
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("08", formatter.InputDigit('8'));
            Assert.AreEqual("088", formatter.InputDigit('8'));
            Assert.AreEqual("0881", formatter.InputDigit('1'));
            Assert.AreEqual("08811", formatter.InputDigit('1'));
            Assert.AreEqual("08811-9", formatter.InputDigit('9'));
            Assert.AreEqual("08811-98", formatter.InputDigit('8'));
            Assert.AreEqual("08811-987", formatter.InputDigit('7'));
            Assert.AreEqual("08811-9876", formatter.InputDigit('6'));
            Assert.AreEqual("08811-9876-7", formatter.InputDigit('7'));
            Assert.AreEqual("08811-9876-78", formatter.InputDigit('8'));
            Assert.AreEqual("08811-9876-789", formatter.InputDigit('9'));
            Assert.AreEqual("08811-9876-7890", formatter.InputDigit('0'));

            // 08500 11-9876-7890
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("08", formatter.InputDigit('8'));
            Assert.AreEqual("085", formatter.InputDigit('5'));
            Assert.AreEqual("0850", formatter.InputDigit('0'));
            Assert.AreEqual("08500 ", formatter.InputDigit('0'));
            Assert.AreEqual("08500 1", formatter.InputDigit('1'));
            Assert.AreEqual("08500 11", formatter.InputDigit('1'));
            Assert.AreEqual("08500 11-9", formatter.InputDigit('9'));
            Assert.AreEqual("08500 11-98", formatter.InputDigit('8'));
            Assert.AreEqual("08500 11-987", formatter.InputDigit('7'));
            Assert.AreEqual("08500 11-9876", formatter.InputDigit('6'));
            Assert.AreEqual("08500 11-9876-7", formatter.InputDigit('7'));
            Assert.AreEqual("08500 11-9876-78", formatter.InputDigit('8'));
            Assert.AreEqual("08500 11-9876-789", formatter.InputDigit('9'));
            Assert.AreEqual("08500 11-9876-7890", formatter.InputDigit('0'));
        }

        [Test]
        public void TestAYTFLongNDD_SG()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter("SG");
            // 777777 9876 7890
            Assert.AreEqual("7", formatter.InputDigit('7'));
            Assert.AreEqual("77", formatter.InputDigit('7'));
            Assert.AreEqual("777", formatter.InputDigit('7'));
            Assert.AreEqual("7777", formatter.InputDigit('7'));
            Assert.AreEqual("77777", formatter.InputDigit('7'));
            Assert.AreEqual("777777 ", formatter.InputDigit('7'));
            Assert.AreEqual("777777 9", formatter.InputDigit('9'));
            Assert.AreEqual("777777 98", formatter.InputDigit('8'));
            Assert.AreEqual("777777 987", formatter.InputDigit('7'));
            Assert.AreEqual("777777 9876", formatter.InputDigit('6'));
            Assert.AreEqual("777777 9876 7", formatter.InputDigit('7'));
            Assert.AreEqual("777777 9876 78", formatter.InputDigit('8'));
            Assert.AreEqual("777777 9876 789", formatter.InputDigit('9'));
            Assert.AreEqual("777777 9876 7890", formatter.InputDigit('0'));
        }

        [Test]
        public void TestAYTFShortNumberFormattingFix_AU()
        {
            // For Australia, the national prefix is not optional when formatting.
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.AU);

            // 1234567890 - For leading digit 1, the national prefix formatting rule has first group only.
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("12", formatter.InputDigit('2'));
            Assert.AreEqual("123", formatter.InputDigit('3'));
            Assert.AreEqual("1234", formatter.InputDigit('4'));
            Assert.AreEqual("1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("1234 567 8", formatter.InputDigit('8'));
            Assert.AreEqual("1234 567 89", formatter.InputDigit('9'));
            Assert.AreEqual("1234 567 890", formatter.InputDigit('0'));
            
            // +61 1234 567 890 - Test the same number, but with the country code.
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+6", formatter.InputDigit('6'));
            Assert.AreEqual("+61 ", formatter.InputDigit('1'));
            Assert.AreEqual("+61 1", formatter.InputDigit('1'));
            Assert.AreEqual("+61 12", formatter.InputDigit('2'));
            Assert.AreEqual("+61 123", formatter.InputDigit('3'));
            Assert.AreEqual("+61 1234", formatter.InputDigit('4'));
            Assert.AreEqual("+61 1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("+61 1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("+61 1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("+61 1234 567 8", formatter.InputDigit('8'));
            Assert.AreEqual("+61 1234 567 89", formatter.InputDigit('9'));
            Assert.AreEqual("+61 1234 567 890", formatter.InputDigit('0'));

            // 212345678 - For leading digit 2, the national prefix formatting rule puts the national prefix
            // before the first group.
            formatter.Clear();
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("02", formatter.InputDigit('2'));
            Assert.AreEqual("021", formatter.InputDigit('1'));
            Assert.AreEqual("02 12", formatter.InputDigit('2'));
            Assert.AreEqual("02 123", formatter.InputDigit('3'));
            Assert.AreEqual("02 1234", formatter.InputDigit('4'));
            Assert.AreEqual("02 1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("02 1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("02 1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("02 1234 5678", formatter.InputDigit('8'));

            // 212345678 - Test the same number, but without the leading 0.
            formatter.Clear();
            Assert.AreEqual("2", formatter.InputDigit('2'));
            Assert.AreEqual("21", formatter.InputDigit('1'));
            Assert.AreEqual("212", formatter.InputDigit('2'));
            Assert.AreEqual("2123", formatter.InputDigit('3'));
            Assert.AreEqual("21234", formatter.InputDigit('4'));
            Assert.AreEqual("212345", formatter.InputDigit('5'));
            Assert.AreEqual("2123456", formatter.InputDigit('6'));
            Assert.AreEqual("21234567", formatter.InputDigit('7'));
            Assert.AreEqual("212345678", formatter.InputDigit('8'));

            // +61 2 1234 5678 - Test the same number, but with the country code.
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+6", formatter.InputDigit('6'));
            Assert.AreEqual("+61 ", formatter.InputDigit('1'));
            Assert.AreEqual("+61 2", formatter.InputDigit('2'));
            Assert.AreEqual("+61 21", formatter.InputDigit('1'));
            Assert.AreEqual("+61 2 12", formatter.InputDigit('2'));
            Assert.AreEqual("+61 2 123", formatter.InputDigit('3'));
            Assert.AreEqual("+61 2 1234", formatter.InputDigit('4'));
            Assert.AreEqual("+61 2 1234 5", formatter.InputDigit('5'));
            Assert.AreEqual("+61 2 1234 56", formatter.InputDigit('6'));
            Assert.AreEqual("+61 2 1234 567", formatter.InputDigit('7'));
            Assert.AreEqual("+61 2 1234 5678", formatter.InputDigit('8'));
        }

        [Test]
        public void TestAYTFShortNumberFormattingFix_KR()
        {
            // For Korea, the national prefix is not optional when formatting, and the national prefix
            // formatting rule doesn't consist of only the first group.
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.KR);

            // 111
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("11", formatter.InputDigit('1'));
            Assert.AreEqual("111", formatter.InputDigit('1'));

            // 114
            formatter.Clear();
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("11", formatter.InputDigit('1'));
            Assert.AreEqual("114", formatter.InputDigit('4'));

            // 13121234 - Test a mobile number without the national prefix. Even though it is not an
            // emergency number, it should be formatted as a block.
            formatter.Clear();
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("13", formatter.InputDigit('3'));
            Assert.AreEqual("131", formatter.InputDigit('1'));
            Assert.AreEqual("1312", formatter.InputDigit('2'));
            Assert.AreEqual("13121", formatter.InputDigit('1'));
            Assert.AreEqual("131212", formatter.InputDigit('2'));
            Assert.AreEqual("1312123", formatter.InputDigit('3'));
            Assert.AreEqual("13121234", formatter.InputDigit('4'));

            // +82 131-2-1234 - Test the same number, but with the country code.
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+82 ", formatter.InputDigit('2'));
            Assert.AreEqual("+82 1", formatter.InputDigit('1'));
            Assert.AreEqual("+82 13", formatter.InputDigit('3'));
            Assert.AreEqual("+82 131", formatter.InputDigit('1'));
            Assert.AreEqual("+82 131-2", formatter.InputDigit('2'));
            Assert.AreEqual("+82 131-2-1", formatter.InputDigit('1'));
            Assert.AreEqual("+82 131-2-12", formatter.InputDigit('2'));
            Assert.AreEqual("+82 131-2-123", formatter.InputDigit('3'));
            Assert.AreEqual("+82 131-2-1234", formatter.InputDigit('4'));
        }

        public void testAYTFShortNumberFormattingFix_MX()
        {
            // For Mexico, the national prefix is optional when formatting.
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.MX);

            // 911
            Assert.AreEqual("9", formatter.InputDigit('9'));
            Assert.AreEqual("91", formatter.InputDigit('1'));
            Assert.AreEqual("911", formatter.InputDigit('1'));

            // 800 123 4567 - Test a toll-free number, which should have a formatting rule applied to it
            // even though it doesn't begin with the national prefix.
            formatter.Clear();
            Assert.AreEqual("8", formatter.InputDigit('8'));
            Assert.AreEqual("80", formatter.InputDigit('0'));
            Assert.AreEqual("800", formatter.InputDigit('0'));
            Assert.AreEqual("800 1", formatter.InputDigit('1'));
            Assert.AreEqual("800 12", formatter.InputDigit('2'));
            Assert.AreEqual("800 123", formatter.InputDigit('3'));
            Assert.AreEqual("800 123 4", formatter.InputDigit('4'));
            Assert.AreEqual("800 123 45", formatter.InputDigit('5'));
            Assert.AreEqual("800 123 456", formatter.InputDigit('6'));
            Assert.AreEqual("800 123 4567", formatter.InputDigit('7'));

            // +52 800 123 4567 - Test the same number, but with the country code.
            formatter.Clear();
            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+5", formatter.InputDigit('5'));
            Assert.AreEqual("+52 ", formatter.InputDigit('2'));
            Assert.AreEqual("+52 8", formatter.InputDigit('8'));
            Assert.AreEqual("+52 80", formatter.InputDigit('0'));
            Assert.AreEqual("+52 800", formatter.InputDigit('0'));
            Assert.AreEqual("+52 800 1", formatter.InputDigit('1'));
            Assert.AreEqual("+52 800 12", formatter.InputDigit('2'));
            Assert.AreEqual("+52 800 123", formatter.InputDigit('3'));
            Assert.AreEqual("+52 800 123 4", formatter.InputDigit('4'));
            Assert.AreEqual("+52 800 123 45", formatter.InputDigit('5'));
            Assert.AreEqual("+52 800 123 456", formatter.InputDigit('6'));
            Assert.AreEqual("+52 800 123 4567", formatter.InputDigit('7'));
        }

        [Test]
        public void TestAYTFNoNationalPrefix()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.IT);

            Assert.AreEqual("3", formatter.InputDigit('3'));
            Assert.AreEqual("33", formatter.InputDigit('3'));
            Assert.AreEqual("333", formatter.InputDigit('3'));
            Assert.AreEqual("333 3", formatter.InputDigit('3'));
            Assert.AreEqual("333 33", formatter.InputDigit('3'));
            Assert.AreEqual("333 333", formatter.InputDigit('3'));
        }

        [Test]
        public void TestAYTFNoNationalPrefixFormattingRule()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.AO);

            Assert.AreEqual("3", formatter.InputDigit('3'));
            Assert.AreEqual("33", formatter.InputDigit('3'));
            Assert.AreEqual("333", formatter.InputDigit('3'));
            Assert.AreEqual("333 3", formatter.InputDigit('3'));
            Assert.AreEqual("333 33", formatter.InputDigit('3'));
            Assert.AreEqual("333 333", formatter.InputDigit('3'));
        }

        [Test]
        public void TestAYTFShortNumberFormattingFix_US()
        {
            // For the US, an initial 1 is treated specially.
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.US);

            // 101 - Test that the initial 1 is not treated as a national prefix.
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("10", formatter.InputDigit('0'));
            Assert.AreEqual("101", formatter.InputDigit('1'));

            // 112 - Test that the initial 1 is not treated as a national prefix.
            formatter.Clear();
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("11", formatter.InputDigit('1'));
            Assert.AreEqual("112", formatter.InputDigit('2'));

            // 122 - Test that the initial 1 is treated as a national prefix.
            formatter.Clear();
            Assert.AreEqual("1", formatter.InputDigit('1'));
            Assert.AreEqual("12", formatter.InputDigit('2'));
            Assert.AreEqual("1 22", formatter.InputDigit('2'));
        }

        [Test]
        public void TestAYTFClearNDDAfterIDDExtraction()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.KR);

            // Check that when we have successfully extracted an IDD, the previously extracted NDD is
            // cleared since it is no longer valid.
            Assert.AreEqual("0", formatter.InputDigit('0'));
            Assert.AreEqual("00", formatter.InputDigit('0'));
            Assert.AreEqual("007", formatter.InputDigit('7'));
            Assert.AreEqual("0070", formatter.InputDigit('0'));
            Assert.AreEqual("00700", formatter.InputDigit('0'));
            Assert.AreEqual("0", formatter.getExtractedNationalPrefix());

            // Once the IDD "00700" has been extracted, it no longer makes sense for the initial "0" to be
            // treated as an NDD.
            Assert.AreEqual("00700 1 ", formatter.InputDigit('1'));
            Assert.AreEqual("", formatter.getExtractedNationalPrefix());

            Assert.AreEqual("00700 1 2", formatter.InputDigit('2'));
            Assert.AreEqual("00700 1 23", formatter.InputDigit('3'));
            Assert.AreEqual("00700 1 234", formatter.InputDigit('4'));
            Assert.AreEqual("00700 1 234 5", formatter.InputDigit('5'));
            Assert.AreEqual("00700 1 234 56", formatter.InputDigit('6'));
            Assert.AreEqual("00700 1 234 567", formatter.InputDigit('7'));
            Assert.AreEqual("00700 1 234 567 8", formatter.InputDigit('8'));
            Assert.AreEqual("00700 1 234 567 89", formatter.InputDigit('9'));
            Assert.AreEqual("00700 1 234 567 890", formatter.InputDigit('0'));
            Assert.AreEqual("00700 1 234 567 8901", formatter.InputDigit('1'));
            Assert.AreEqual("00700123456789012", formatter.InputDigit('2'));
            Assert.AreEqual("007001234567890123", formatter.InputDigit('3'));
            Assert.AreEqual("0070012345678901234", formatter.InputDigit('4'));
            Assert.AreEqual("00700123456789012345", formatter.InputDigit('5'));
            Assert.AreEqual("007001234567890123456", formatter.InputDigit('6'));
            Assert.AreEqual("0070012345678901234567", formatter.InputDigit('7'));
        }

        [Test]
        public void TestAYTFNumberPatternsBecomingInvalidShouldNotResultInDigitLoss()
        {
            AsYouTypeFormatter formatter = phoneUtil.GetAsYouTypeFormatter(RegionCode.CN);

            Assert.AreEqual("+", formatter.InputDigit('+'));
            Assert.AreEqual("+8", formatter.InputDigit('8'));
            Assert.AreEqual("+86 ", formatter.InputDigit('6'));
            Assert.AreEqual("+86 9", formatter.InputDigit('9'));
            Assert.AreEqual("+86 98", formatter.InputDigit('8'));
            Assert.AreEqual("+86 988", formatter.InputDigit('8'));
            Assert.AreEqual("+86 988 1", formatter.InputDigit('1'));
            // Now the number pattern is no longer valid because there are multiple leading digit patterns;
            // when we try again to extract a country code we should ensure we use the last leading digit
            // pattern, rather than the first one such that it *thinks* it's found a valid formatting rule
            // again.
            // https://code.google.com/p/libphonenumber/issues/detail?id=437
            Assert.AreEqual("+8698812", formatter.InputDigit('2'));
            Assert.AreEqual("+86988123", formatter.InputDigit('3'));
            Assert.AreEqual("+869881234", formatter.InputDigit('4'));
            Assert.AreEqual("+8698812345", formatter.InputDigit('5'));
        }

    }
}
