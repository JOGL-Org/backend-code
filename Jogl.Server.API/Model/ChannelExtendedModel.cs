using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ChannelExtendedModel : ChannelModel
    {
        [JsonIgnore]
        public override ChannelStatModel Stats { get => base.Stats; set => base.Stats = value; }
    }
}