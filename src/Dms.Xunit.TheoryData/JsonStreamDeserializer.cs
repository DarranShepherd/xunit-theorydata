using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Dms.Xunit.TheoryData
{
    public class JsonStreamDeserializer
    {
        private readonly Stream stream;

        public JsonStreamDeserializer(Stream stream)
        {
            this.stream = stream;
        }

        public IEnumerable<object[]> Deserialize()
        {
            var objects = this.DeserializeFromStream();
            foreach (var o in objects)
            {
                var array = o as object[];
                if (array != null)
                {
                    yield return array;
                    continue;
                }

                var jarray = o as JArray;
                if (jarray != null)
                {
                    yield return jarray.Children().Select(JTokenToType).ToArray();
                    continue;
                }

                var jtoken = o as JToken;
                if (jtoken != null)
                {
                    yield return new[] {JTokenToType(jtoken)};
                    continue;
                }

                yield return new[] {o};
            }
        }

        private object JTokenToType(JToken token)
        {
            var array = token as JArray;
            if (array != null)
            {
                return array.Children().Select(JTokenToType).ToArray();
            }

            var value = token as JValue;
            if (value != null)
            {
                return value.Value;
            }

            var obj = token as JObject;
            var type = Type.GetType(token["$type"]?.ToString() ?? string.Empty);
            if (obj != null && type != null)
            {
                return token.ToObject(type);
            }

            return token.ToObject<object>();
        }

        private object[] DeserializeFromStream()
        {
            try
            {
                var jsonSerializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.Auto};
                using (var reader = new JsonTextReader(new StreamReader(this.stream)))
                {
                    return jsonSerializer.Deserialize<object[]>(reader);
                }
            }
            catch (Exception ex)
            {
                throw new FileNotSupportedException("Could not deserialise resource to array. Json file should contain an array as the root entity.", ex);
            }
        }

        private IEnumerable<object[]> EnumerableOfArrays(JArray jarray)
        {
            foreach (var jtoken in jarray.Children())
            {
                if (jtoken.Type == JTokenType.Array)
                {
                    yield return jtoken.Children().Select(t => t.ToObject<object>()).ToArray();
                }
                else
                {
                    yield return new[] { jtoken.ToObject<object>() };
                }
            }
        }

        private JArray DeserializeStreamToJArray()
        {
            var jsonSerializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.Auto};
            using (var reader = new JsonTextReader(new StreamReader(this.stream)))
            {
                var json = jsonSerializer.Deserialize(reader) as JToken;

                if (json != null && json.Type == JTokenType.Array)
                {
                    return json as JArray;
                }

                throw new FileNotSupportedException("Could not deserialise resource to array. Json file should contain an array as the root entity.");
            }
        }
    }
}