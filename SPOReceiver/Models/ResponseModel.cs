using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPOReceiver.Models
{
    internal class ResponseModel<T>
    {
        [JsonProperty(PropertyName = "value")]
        public List<T> Value { get; set; }
    }
}
