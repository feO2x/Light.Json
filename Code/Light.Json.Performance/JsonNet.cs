using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Light.Json.Performance
{
    public static class JsonNet
    {
        public static JsonTextReader CreateJsonNetTextReader(string utf16Json) =>
            new JsonTextReader(new StringReader(utf16Json));

        public static JsonTextReader CreateJsonNetTextReader(byte[] utf8Json) =>
            new JsonTextReader(new StreamReader(new MemoryStream(utf8Json), Encoding.UTF8));
    }
}