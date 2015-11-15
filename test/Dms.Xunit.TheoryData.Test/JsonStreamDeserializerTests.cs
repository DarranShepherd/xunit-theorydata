using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace Dms.Xunit.TheoryData.Test
{
    public class JsonStreamDeserializerTests
    {
        [Fact]
        public void ReadsAnArrayOfArraysOfSimpleTypes()
        {
            var deserializer = GetDeserializer("Dms.Xunit.TheoryData.Test.TestFiles.SingleRecord.json");
            var output = deserializer.Deserialize().Single();

            Assert.Equal(2, output.Length);
            Assert.IsType<long>(output[0]);
            Assert.IsType<string>(output[1]);
        }

        [Fact]
        public void ReadsAnArrayOfArraysOfObjectsAndRetainsTypes()
        {
            var deserializer = GetDeserializer("Dms.Xunit.TheoryData.Test.TestFiles.ObjectArray.json");
            var output = deserializer.Deserialize().Single();

            Assert.Equal(2, output.Length);
            Assert.IsType<TestParameter>(output[0]);
            Assert.IsType<Version>(output[1]);
        } 

        [Fact]
        public void ReadsAnArrayOfObjectsAndWrapsInObjectArrays()
        {
            var deserializer = GetDeserializer("Dms.Xunit.TheoryData.Test.TestFiles.ObjectRecord.json");
            var output = deserializer.Deserialize().Single();

            Assert.Equal(1, output.Length);
            Assert.IsType<TestParameter>(output[0]);
        }

        [Fact]
        public void ThrowsFileNotSupportedExceptionIfFileDoesntContainArray()
        {
            var deserializer = GetDeserializer("Dms.Xunit.TheoryData.Test.TestFiles.Object.json");

            Assert.Throws<FileNotSupportedException>(() => deserializer.Deserialize().ToArray());
        }

        [Fact]
        public void JArrayToObjectArray()
        {
            var json = @"[ 1, 'two', 3.456]";
            var jarray = JArray.Parse(json);
            var array = jarray.ToObject<object[]>();

            Assert.IsType<long>(array[0]);
            Assert.IsType<string>(array[1]);
            Assert.IsType<double>(array[2]);
        }

        [Fact]
        public void TypeHandledArrayToObjectArray()
        {
            var json = @"[
                            { '$type': 'System.Version, mscorlib', 'Major': 1, 'Minor': 2},
                            { '$type': 'System.Version, mscorlib', 'Major': 3, 'Minor': 4}
                        ]";
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var obj = JsonConvert.DeserializeObject<object[]>(json, settings);

            Assert.Equal("System.Object[]", obj.GetType().FullName);
            Assert.IsType<Version>(obj[0]);
            Assert.IsType<Version>(obj[1]);
        }

        [Fact]
        public void TypeHandledArrayToObject()
        {
            var json = @"[
                            { '$type': 'System.Version, mscorlib', 'Major': 1, 'Minor': 2},
                            { '$type': 'System.Version, mscorlib', 'Major': 3, 'Minor': 4}
                        ]";
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var obj = JsonConvert.DeserializeObject<object>(json, settings);
            var jarray = obj as JArray;
            var array = jarray.Children().Select(child => child.ToObject(Type.GetType(child["$type"].ToString()))).ToArray();

            Assert.IsType<Version>(array[0]);
            Assert.IsType<Version>(array[1]);
        }

        private JsonStreamDeserializer GetDeserializer(string resource)
        {
            return new JsonStreamDeserializer(GetStream(resource));
        }

        private Stream GetStream(string resource)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream(resource);
        }
    }
}
