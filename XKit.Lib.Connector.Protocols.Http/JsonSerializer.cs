using System.IO;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using XKit.Lib.Common.Utility.Extensions;
using System.Text.Json;

namespace XKit.Lib.Connector.Protocols.Http {
    public class JsonSerializer : ISerializer, IDeserializer {

        private static readonly JsonSerializer defaultInstance = new();

        public string ContentType {
            get { return "application/json"; } 
            set { }
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string Serialize(object obj) => Json.ToJson(obj);

        public T Deserialize<T>(RestSharp.IRestResponse response) {
            var content = response.Content;

            using var stringReader = new StringReader(content);
            return Json.FromJson<T>(stringReader.ReadToEnd());
        }

        public static JsonSerializer Default => defaultInstance;
    }
}
