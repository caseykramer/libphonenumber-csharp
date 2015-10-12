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
using System.Text;
using NUnit.Framework;

namespace PhoneNumbers.Test
{
    public class MultiFileMetadataSourceImplTest:TestMetadataTestCase  
    {

        private MultiFileMetadataSourceImpl _multiFileMetadataSource;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _multiFileMetadataSource = new MultiFileMetadataSourceImpl("/no/such/file",PhoneNumberUtil.DEFAULT_METADATA_LOADER);
        }

        
        [Test]
        public void TestMissingMetadataFileThrowsRuntimeException()
        {
            // In normal usage we should never get a state where we are asking to load metadata that doesn't
            // exist. However if the library is packaged incorrectly in the jar, this could happen and the
            // best we can do is make sure the exception has the file name in it.
            Assert.Throws<NullReferenceException>(() => _multiFileMetadataSource.LoadMetadataFromFile("XX", -1));
            Assert.Throws<NullReferenceException>(() => _multiFileMetadataSource.LoadMetadataFromFile(PhoneNumberUtil.REGION_CODE_FOR_NON_GEO_ENTITY, 123));
        }
}

}
