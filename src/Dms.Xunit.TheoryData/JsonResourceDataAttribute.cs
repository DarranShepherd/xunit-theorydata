using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace Dms.Xunit.TheoryData
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class JsonResourceDataAttribute : DataAttribute
    {
        private readonly string resourceName;
        private readonly Assembly assembly;

        public JsonResourceDataAttribute(string resourceName, Type typeInResourceAssembly = null)
        {
            this.resourceName = resourceName;
            if (typeInResourceAssembly != null) { 
                this.assembly = typeInResourceAssembly.GetTypeInfo().Assembly;
            }
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var stream = this.GetStream(testMethod);
            var deserializer = new JsonStreamDeserializer(stream);
            return deserializer.Deserialize();
        }

        private Stream GetStream(MethodInfo testMethod)
        {
            var resourceAssembly = this.GetAssembly(testMethod);
            var stream = resourceAssembly.GetManifestResourceStream(this.resourceName);

            if (stream == null)
            {
                var resourceNames = string.Join(Environment.NewLine, resourceAssembly.GetManifestResourceNames());
                throw new ResourceNotFoundException($"Could not read {this.resourceName} from {resourceAssembly.FullName}. Available resources are: {Environment.NewLine}{resourceNames}");
            }
            return stream;
        }

        private Assembly GetAssembly(MethodInfo testMethod)
        {
            var resourceAssembly = this.assembly ?? testMethod.DeclaringType?.GetTypeInfo().Assembly;
            if (resourceAssembly == null)
            {
                throw new Exception("Could not determine assembly from which to load resource ${this.resourceName}");
            }
            return resourceAssembly;
        }
    }
}
