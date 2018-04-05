using Newtonsoft.Json;

namespace Nimator.Notifiers
{
    public sealed class OpsGenieHeartbeatRequest : OpsGenieRequest
    {
        public OpsGenieHeartbeatRequest(string apiKey, string name)
            : base(apiKey)
        {
            this.Name = name;
        }

        /// <summary>
        /// Name of the heartbeat.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
