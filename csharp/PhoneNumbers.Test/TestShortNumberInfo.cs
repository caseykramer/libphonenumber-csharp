/*
 * Copyright (C) 2013 The Libphonenumber Authors
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
     * Unit tests for ShortNumberUtil.java
     *
     * @author Shaopeng Jia
     */
    [TestFixture]
    class ShortNumberInfoTest: TestMetadataTestCase
    {
        private ShortNumberInfo shortInfo;

        [TestFixtureSetUp]
        public new void SetupFixture()
        {
            base.SetupFixture();
            shortInfo = new ShortNumberInfo(phoneUtil);
        }

        [Test]
        public void TestIsPossibleShortNumber()
        {
            PhoneNumber possibleNumber = new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(123456L).Build();
            Assert.True(shortInfo.IsPossibleShortNumber(possibleNumber));
            Assert.True(shortInfo.IsPossibleShortNumberForRegion("123456", RegionCode.FR));

            PhoneNumber impossibleNumber = new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(9L).Build();
            Assert.False(shortInfo.IsPossibleShortNumber(impossibleNumber));
            Assert.False(shortInfo.IsPossibleShortNumberForRegion("9", RegionCode.FR));

            // Note that GB and GG share the country calling code 44, and that this number is possible but
            // not valid.
            Assert.True(shortInfo.IsPossibleShortNumber(
                new PhoneNumber.Builder().SetCountryCode(44).SetNationalNumber(11001L).Build()));
        }

        public void testIsValidShortNumber()
        {
            Assert.True(shortInfo.IsValidShortNumber(
                new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(1010L).Build()));
            Assert.True(shortInfo.IsValidShortNumberForRegion("1010", RegionCode.FR));
            Assert.False(shortInfo.IsValidShortNumber(
                new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(123456L).Build()));
            Assert.False(shortInfo.IsValidShortNumberForRegion("123456", RegionCode.FR));

            // Note that GB and GG share the country calling code 44.
            Assert.True(shortInfo.IsValidShortNumber(
                new PhoneNumber.Builder().SetCountryCode(44).SetNationalNumber(18001L).Build()));
        }

        public void testGetExpectedCost()
        {
            String premiumRateExample = shortInfo.GetExampleShortNumberForCost(
                RegionCode.FR, ShortNumberInfo.ShortNumberCost.PREMIUM_RATE);
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.PREMIUM_RATE,
                shortInfo.GetExpectedCostForRegion(premiumRateExample, RegionCode.FR));
            PhoneNumber premiumRateNumber = new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(ulong.Parse(premiumRateExample)).Build();
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.PREMIUM_RATE,
                shortInfo.GetExpectedCost(premiumRateNumber));

            String standardRateExample = shortInfo.GetExampleShortNumberForCost(
                RegionCode.FR, ShortNumberInfo.ShortNumberCost.STANDARD_RATE);
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.STANDARD_RATE,
                shortInfo.GetExpectedCostForRegion(standardRateExample, RegionCode.FR));
            PhoneNumber standardRateNumber = new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(ulong.Parse(standardRateExample)).Build();
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.STANDARD_RATE,
                shortInfo.GetExpectedCost(standardRateNumber));

            String tollFreeExample = shortInfo.GetExampleShortNumberForCost(
                RegionCode.FR, ShortNumberInfo.ShortNumberCost.TOLL_FREE);
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCostForRegion(tollFreeExample, RegionCode.FR));
            PhoneNumber tollFreeNumber = new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(ulong.Parse(tollFreeExample)).Build();
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCost(tollFreeNumber));

            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCostForRegion("12345", RegionCode.FR));
            PhoneNumber unknownCostNumber = new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(12345L).Build();
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCost(unknownCostNumber));

            // Test that an invalid number may nevertheless have a cost other than UNKNOWN_COST.
            Assert.False(shortInfo.IsValidShortNumberForRegion("116123", RegionCode.FR));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCostForRegion("116123", RegionCode.FR));
            PhoneNumber invalidNumber = new PhoneNumber.Builder().SetCountryCode(33).SetNationalNumber(116123L).Build();
            Assert.False(shortInfo.IsValidShortNumber(invalidNumber));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCost(invalidNumber));

            // Test a nonexistent country code.
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCostForRegion("911", RegionCode.ZZ));
            unknownCostNumber = unknownCostNumber.ToBuilder().Clear().SetCountryCode(123).SetNationalNumber(911L).Build();
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCost(unknownCostNumber));
        }

        [Test]
        public void TestGetExpectedCostForSharedCountryCallingCode()
        {
            // Test some numbers which have different costs in countries sharing the same country calling
            // code. In Australia, 1234 is premium-rate, 1194 is standard-rate, and 733 is toll-free. These
            // are not known to be valid numbers in the Christmas Islands.
            String ambiguousPremiumRateString = "1234";
            PhoneNumber ambiguousPremiumRateNumber = new PhoneNumber.Builder().SetCountryCode(61)
                .SetNationalNumber(1234L).Build();
            String ambiguousStandardRateString = "1194";
            PhoneNumber ambiguousStandardRateNumber = new PhoneNumber.Builder().SetCountryCode(61)
                .SetNationalNumber(1194L).Build();
            String ambiguousTollFreeString = "733";
            PhoneNumber ambiguousTollFreeNumber = new PhoneNumber.Builder().SetCountryCode(61)
                .SetNationalNumber(733L).Build();

            Assert.True(shortInfo.IsValidShortNumber(ambiguousPremiumRateNumber));
            Assert.True(shortInfo.IsValidShortNumber(ambiguousStandardRateNumber));
            Assert.True(shortInfo.IsValidShortNumber(ambiguousTollFreeNumber));

            Assert.True(shortInfo.IsValidShortNumberForRegion(ambiguousPremiumRateString, RegionCode.AU));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.PREMIUM_RATE,
                shortInfo.GetExpectedCostForRegion(ambiguousPremiumRateString, RegionCode.AU));
            Assert.False(shortInfo.IsValidShortNumberForRegion(ambiguousPremiumRateString, RegionCode.CX));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCostForRegion(ambiguousPremiumRateString, RegionCode.CX));
            // PREMIUM_RATE takes precedence over UNKNOWN_COST.
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.PREMIUM_RATE,
                shortInfo.GetExpectedCost(ambiguousPremiumRateNumber));

            Assert.True(shortInfo.IsValidShortNumberForRegion(ambiguousStandardRateString, RegionCode.AU));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.STANDARD_RATE,
                shortInfo.GetExpectedCostForRegion(ambiguousStandardRateString, RegionCode.AU));
            Assert.False(shortInfo.IsValidShortNumberForRegion(ambiguousStandardRateString, RegionCode.CX));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCostForRegion(ambiguousStandardRateString, RegionCode.CX));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCost(ambiguousStandardRateNumber));

            Assert.True(shortInfo.IsValidShortNumberForRegion(ambiguousTollFreeString, RegionCode.AU));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCostForRegion(ambiguousTollFreeString, RegionCode.AU));
            Assert.False(shortInfo.IsValidShortNumberForRegion(ambiguousTollFreeString, RegionCode.CX));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCostForRegion(ambiguousTollFreeString, RegionCode.CX));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCost(ambiguousTollFreeNumber));
        }

        [Test]
        public void TestGetExampleShortNumber()
        {
            Assert.AreEqual("8711", shortInfo.GetExampleShortNumber(RegionCode.AM));
            Assert.AreEqual("1010", shortInfo.GetExampleShortNumber(RegionCode.FR));
            Assert.AreEqual("", shortInfo.GetExampleShortNumber(RegionCode.UN001));
            Assert.AreEqual("", shortInfo.GetExampleShortNumber(null));
        }

        [Test]
        public void testGetExampleShortNumberForCost()
        {
            Assert.AreEqual("3010", shortInfo.GetExampleShortNumberForCost(RegionCode.FR,
                ShortNumberInfo.ShortNumberCost.TOLL_FREE));
            Assert.AreEqual("1023", shortInfo.GetExampleShortNumberForCost(RegionCode.FR,
                ShortNumberInfo.ShortNumberCost.STANDARD_RATE));
            Assert.AreEqual("42000", shortInfo.GetExampleShortNumberForCost(RegionCode.FR,
                ShortNumberInfo.ShortNumberCost.PREMIUM_RATE));
            Assert.AreEqual("", shortInfo.GetExampleShortNumberForCost(RegionCode.FR,
                ShortNumberInfo.ShortNumberCost.UNKNOWN_COST));
        }

        [Test]
        public void TestConnectsToEmergencyNumber_US()
        {
            Assert.True(shortInfo.ConnectsToEmergencyNumber("911", RegionCode.US));
            Assert.True(shortInfo.ConnectsToEmergencyNumber("112", RegionCode.US));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("999", RegionCode.US));
        }

        [Test]
        public void testConnectsToEmergencyNumberLongNumber_US()
        {
            Assert.True(shortInfo.ConnectsToEmergencyNumber("9116666666", RegionCode.US));
            Assert.True(shortInfo.ConnectsToEmergencyNumber("1126666666", RegionCode.US));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("9996666666", RegionCode.US));
        }

        [Test]
        public void TestConnectsToEmergencyNumberWithFormatting_US()
        {
            Assert.True(shortInfo.ConnectsToEmergencyNumber("9-1-1", RegionCode.US));
            Assert.True(shortInfo.ConnectsToEmergencyNumber("1-1-2", RegionCode.US));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("9-9-9", RegionCode.US));
        }
        
        [Test]
        public void TestConnectsToEmergencyNumberWithPlusSign_US()
        {
            Assert.False(shortInfo.ConnectsToEmergencyNumber("+911", RegionCode.US));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("\uFF0B911", RegionCode.US));
            Assert.False(shortInfo.ConnectsToEmergencyNumber(" +911", RegionCode.US));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("+112", RegionCode.US));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("+999", RegionCode.US));
        }

        [Test]
        public void testConnectsToEmergencyNumber_BR()
        {
            Assert.True(shortInfo.ConnectsToEmergencyNumber("911", RegionCode.BR));
            Assert.True(shortInfo.ConnectsToEmergencyNumber("190", RegionCode.BR));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("999", RegionCode.BR));
        }

        [Test]
        public void TestConnectsToEmergencyNumberLongNumber_BR()
        {
            // Brazilian emergency numbers don't work when additional digits are appended.
            Assert.False(shortInfo.ConnectsToEmergencyNumber("9111", RegionCode.BR));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("1900", RegionCode.BR));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("9996", RegionCode.BR));
        }

        [Test]
        public void TestConnectsToEmergencyNumber_CL()
        {
            Assert.True(shortInfo.ConnectsToEmergencyNumber("131", RegionCode.CL));
            Assert.True(shortInfo.ConnectsToEmergencyNumber("133", RegionCode.CL));
        }

        [Test]
        public void TestConnectsToEmergencyNumberLongNumber_CL()
        {
            // Chilean emergency numbers don't work when additional digits are appended.
            Assert.False(shortInfo.ConnectsToEmergencyNumber("1313", RegionCode.CL));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("1330", RegionCode.CL));
        }

        [Test]
        public void TestConnectsToEmergencyNumber_AO()
        {
            // Angola doesn't have any metadata for emergency numbers in the test metadata.
            Assert.False(shortInfo.ConnectsToEmergencyNumber("911", RegionCode.AO));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("222123456", RegionCode.AO));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("923123456", RegionCode.AO));
        }

        [Test]
        public void TestConnectsToEmergencyNumber_ZW()
        {
            // Zimbabwe doesn't have any metadata in the test metadata.
            Assert.False(shortInfo.ConnectsToEmergencyNumber("911", RegionCode.ZW));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("01312345", RegionCode.ZW));
            Assert.False(shortInfo.ConnectsToEmergencyNumber("0711234567", RegionCode.ZW));
        }

        [Test]
        public void TestIsEmergencyNumber_US()
        {
            Assert.True(shortInfo.IsEmergencyNumber("911", RegionCode.US));
            Assert.True(shortInfo.IsEmergencyNumber("112", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("999", RegionCode.US));
        }

        [Test]
        public void TestIsEmergencyNumberLongNumber_US()
        {
            Assert.False(shortInfo.IsEmergencyNumber("9116666666", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("1126666666", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("9996666666", RegionCode.US));
        }

        [Test]
        public void TestIsEmergencyNumberWithFormatting_US()
        {
            Assert.True(shortInfo.IsEmergencyNumber("9-1-1", RegionCode.US));
            Assert.True(shortInfo.IsEmergencyNumber("*911", RegionCode.US));
            Assert.True(shortInfo.IsEmergencyNumber("1-1-2", RegionCode.US));
            Assert.True(shortInfo.IsEmergencyNumber("*112", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("9-9-9", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("*999", RegionCode.US));
        }

        [Test]
        public void TestIsEmergencyNumberWithPlusSign_US()
        {
            Assert.False(shortInfo.IsEmergencyNumber("+911", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("\uFF0B911", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber(" +911", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("+112", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("+999", RegionCode.US));
        }

        [Test]
        public void TestIsEmergencyNumber_BR()
        {
            Assert.True(shortInfo.IsEmergencyNumber("911", RegionCode.BR));
            Assert.True(shortInfo.IsEmergencyNumber("190", RegionCode.BR));
            Assert.False(shortInfo.IsEmergencyNumber("999", RegionCode.BR));
        }

        [Test]
        public void TestIsEmergencyNumberLongNumber_BR()
        {
            Assert.False(shortInfo.IsEmergencyNumber("9111", RegionCode.BR));
            Assert.False(shortInfo.IsEmergencyNumber("1900", RegionCode.BR));
            Assert.False(shortInfo.IsEmergencyNumber("9996", RegionCode.BR));
        }

        [Test]
        public void TestIsEmergencyNumber_AO()
        {
            // Angola doesn't have any metadata for emergency numbers in the test metadata.
            Assert.False(shortInfo.IsEmergencyNumber("911", RegionCode.AO));
            Assert.False(shortInfo.IsEmergencyNumber("222123456", RegionCode.AO));
            Assert.False(shortInfo.IsEmergencyNumber("923123456", RegionCode.AO));
        }

        [Test]
        public void TestIsEmergencyNumber_ZW()
        {
            // Zimbabwe doesn't have any metadata in the test metadata.
            Assert.False(shortInfo.IsEmergencyNumber("911", RegionCode.ZW));
            Assert.False(shortInfo.IsEmergencyNumber("01312345", RegionCode.ZW));
            Assert.False(shortInfo.IsEmergencyNumber("0711234567", RegionCode.ZW));
        }

        [Test]
        public void TestEmergencyNumberForSharedCountryCallingCode()
        {
            // Test the emergency number 112, which is valid in both Australia and the Christmas Islands.
            Assert.True(shortInfo.IsEmergencyNumber("112", RegionCode.AU));
            Assert.True(shortInfo.IsValidShortNumberForRegion("112", RegionCode.AU));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCostForRegion("112", RegionCode.AU));
            Assert.True(shortInfo.IsEmergencyNumber("112", RegionCode.CX));
            Assert.True(shortInfo.IsValidShortNumberForRegion("112", RegionCode.CX));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCostForRegion("112", RegionCode.CX));
            PhoneNumber sharedEmergencyNumber =
                new PhoneNumber.Builder().SetCountryCode(61).SetNationalNumber(112L).Build();
            Assert.True(shortInfo.IsValidShortNumber(sharedEmergencyNumber));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCost(sharedEmergencyNumber));
        }

        [Test]
        public void TestOverlappingNANPANumber()
        {
            // 211 is an emergency number in Barbados, while it is a toll-free information line in Canada
            // and the USA.
            Assert.True(shortInfo.IsEmergencyNumber("211", RegionCode.BB));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.TOLL_FREE,
                shortInfo.GetExpectedCostForRegion("211", RegionCode.BB));
            Assert.False(shortInfo.IsEmergencyNumber("211", RegionCode.US));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCostForRegion("211", RegionCode.US));
            Assert.False(shortInfo.IsEmergencyNumber("211", RegionCode.CA));
            Assert.AreEqual(ShortNumberInfo.ShortNumberCost.UNKNOWN_COST,
                shortInfo.GetExpectedCostForRegion("211", RegionCode.CA));
        }

    }
}
