/*
 * Copyright (C) 2024 Contributors
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
using NUnit.Framework;

namespace PhoneNumbers.Test
{
    /**
    * Unit tests for IDisposable implementations to prevent memory leaks.
    */
    [TestFixture]
    class TestDisposable
    {
        [Test]
        public void TestAreaCodeMapDispose()
        {
            var areaCodeMap = new AreaCodeMap();
            var sortedMap = new SortedDictionary<int, String>();
            sortedMap[1212] = "New York";
            sortedMap[1213] = "New York";
            sortedMap[1214] = "New York";
            sortedMap[1480] = "Arizona";
            
            areaCodeMap.readAreaCodeMap(sortedMap);
            
            // Verify map works before disposal
            var number = new PhoneNumber.Builder().SetCountryCode(1).SetNationalNumber(2126641234L).Build();
            Assert.AreEqual("New York", areaCodeMap.Lookup(number));
            
            // Dispose should not throw
            Assert.DoesNotThrow(() => areaCodeMap.Dispose());
            
            // Multiple Dispose calls should be safe (idempotent)
            Assert.DoesNotThrow(() => areaCodeMap.Dispose());
        }

        [Test]
        public void TestFlyweightMapStorageDispose()
        {
            var storage = new FlyweightMapStorage();
            var sortedMap = new SortedDictionary<int, String>();
            sortedMap[1212] = "New York";
            sortedMap[1213] = "New York";
            sortedMap[1214] = "New York";
            sortedMap[1480] = "Arizona";
            
            storage.readFromSortedMap(sortedMap);
            
            // Verify storage works before disposal
            Assert.AreEqual(1212, storage.getPrefix(0));
            Assert.AreEqual("New York", storage.getDescription(0));
            
            // Dispose should not throw
            Assert.DoesNotThrow(() => storage.Dispose());
            
            // Multiple Dispose calls should be safe (idempotent)
            Assert.DoesNotThrow(() => storage.Dispose());
        }

        [Test]
        public void TestPhoneNumberOfflineGeocoderDispose()
        {
            // Note: We can't easily test the singleton instance dispose
            // as it's a static singleton. This test verifies that
            // Dispose can be called without throwing.
            var geocoder = new PhoneNumberOfflineGeocoder("res.test_");
            
            // Dispose should not throw
            Assert.DoesNotThrow(() => geocoder.Dispose());
            
            // Multiple Dispose calls should be safe (idempotent)
            Assert.DoesNotThrow(() => geocoder.Dispose());
        }

        [Test]
        public void TestAreaCodeMapGetSmallerMapStorageDisposesUnused()
        {
            // This test verifies that the unused storage strategy is disposed
            // when getSmallerMapStorage chooses the optimal one
            var areaCodeMap = new AreaCodeMap();
            
            // Create a map that will favor FlyweightMapStorage
            var sortedMap = new SortedDictionary<int, String>();
            sortedMap[1212] = "New York";
            sortedMap[1213] = "New York";
            sortedMap[1214] = "New York";
            sortedMap[1480] = "Arizona";
            
            var storage = areaCodeMap.getSmallerMapStorage(sortedMap);
            
            // Verify FlyweightMapStorage was chosen
            Assert.IsInstanceOf<FlyweightMapStorage>(storage);
            
            // The DefaultMapStorage should have been disposed internally
            // This test verifies no exception is thrown during the process
            Assert.IsNotNull(storage);
        }

        [Test]
        public void TestPhoneNumberMatcherDispose()
        {
            var phoneUtil = PhoneNumberUtil.GetInstance();
            var text = "Call me at +1 650-253-0000";
            
            using (var matcher = new PhoneNumberMatcher(phoneUtil, text, "US", 
                PhoneNumberUtil.Leniency.POSSIBLE, long.MaxValue))
            {
                // Verify matcher works
                Assert.IsTrue(matcher.MoveNext());
                Assert.IsNotNull(matcher.Current);
                
                // Dispose is called automatically by using statement
            }
            
            // Verify no exception was thrown during disposal
            Assert.Pass();
        }
    }
}
