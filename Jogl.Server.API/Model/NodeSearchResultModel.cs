using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class NodeSearchResultModel
    {
        public int Projects { get; set; }
        public int Communities { get; set; }
        public int Organizations { get; set; }
        public int Members { get; set; }
        public int Resources { get; set; }
        public int Papers { get; set; }
        public int Needs { get; set; }
        [JsonPropertyName("calls_for_proposals")]
        public int CallsForProposals { get; set; }
    }
}