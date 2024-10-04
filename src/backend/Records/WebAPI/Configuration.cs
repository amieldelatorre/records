using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebAPI;

public class Configuration
{
    public static readonly JsonSerializerSettings JsonSerializerConfig = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
}