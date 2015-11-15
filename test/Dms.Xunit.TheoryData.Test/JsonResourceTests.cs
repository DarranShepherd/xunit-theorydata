using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Dms.Xunit.TheoryData.Test
{
    public class JsonResourceTests
    {
        [Theory]
        [JsonResourceData("Dms.Xunit.TheoryData.Test.TestFiles.SingleRecord.json")]
        public void ReadsSimpleArrayFromJsonResource(int i, string s)
        {
            Assert.Equal(1, i);
            Assert.Equal("two", s);
        }

        [Theory]
        [JsonResourceData("Dms.Xunit.TheoryData.Test.TestFiles.MultipleRecords.json")]
        public void ReadsMultipleSimpleArraysFromJsonResource(int i, string s)
        {
            Assert.Equal(i.ToString(), s);
        }

        [Theory]
        [JsonResourceData("Dms.Xunit.TheoryData.Test.TestFiles.ObjectRecord.json")]
        public void ReadsObjectFromJsonResource(TestParameter param)
        {
            Assert.Equal("value", param.Property);
        }

        [Theory]
        [JsonResourceData("Dms.Xunit.TheoryData.Test.TestFiles.MultipleObjectRecords.json")]
        public void ReadsMultipleObjectsFromJsonResource(TestParameter param)
        {
            Assert.StartsWith("value", param.Property);
        }

        [Theory]
        [JsonResourceData("Dms.Xunit.TheoryData.Test.TestFiles.ObjectArray.json")]
        public void ReadsArrayOfObjectsFromJsonResource(TestParameter param, Version v)
        {
            Assert.Equal("value", param.Property);
            Assert.Equal(3, v.Major);
            Assert.Equal(4, v.Minor);
        }

        [Theory]
        [JsonResourceData("Dms.Xunit.TheoryData.Test.TestFiles.MultipleObjectArrays.json")]
        public void ReadsMultipleArraysOfObjectsFromJsonResource(TestParameter param, Version v)
        {
            Assert.Equal(v.ToString(), param.Property);
        }

        [Fact]
        public void ThrowsExceptionIfResourceNotFound()
        {
            var attribute = new JsonResourceDataAttribute("Foo");
            var methodInfo = this.GetType().GetMethod(nameof(ThrowsExceptionIfResourceNotFound));


            Assert.Throws<ResourceNotFoundException>(() => attribute.GetData(methodInfo));
        }

        [Fact]
        public void ExceptionSuggestsFoundResources()
        {
            var attribute = new JsonResourceDataAttribute("Foo");
            var methodInfo = this.GetType().GetMethod(nameof(ExceptionSuggestsFoundResources));

            var exception = Assert.Throws<ResourceNotFoundException>(() => attribute.GetData(methodInfo));

            Assert.Contains("Dms.Xunit.TheoryData.Test.TestFiles.ObjectRecord.json", exception.Message);
            Assert.Contains("Dms.Xunit.TheoryData.Test.TestFiles.SingleRecord.json", exception.Message);
        }

        [Fact]
        public void ThrowsFileNotSupportedWithObjectJson()
        {
            var attribute = new JsonResourceDataAttribute("Dms.Xunit.TheoryData.Test.TestFiles.Object.json");
            var methodInfo = this.GetType().GetMethod(nameof(ThrowsFileNotSupportedWithObjectJson));

            Assert.Throws<FileNotSupportedException>(() => attribute.GetData(methodInfo).ToArray());
        }
    }

    public class TestParameter : IXunitSerializable
    {
        public string Property { get; set; }

        public override string ToString()
        {
            return $"{{Property = {this.Property}}}";
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            this.Property = info.GetValue<string>("Property");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Property", this.Property);
        }
    }
}
