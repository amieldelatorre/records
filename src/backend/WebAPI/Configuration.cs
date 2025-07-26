using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebAPI;

public static class Configuration
{
    public static readonly JsonSerializerSettings JsonSerializerConfig = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
}