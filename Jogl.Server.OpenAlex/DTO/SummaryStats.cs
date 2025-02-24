using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class SummaryStats
    {
        [JsonPropertyName("2yr_mean_citedness")]
        public double _2yrMeanCitedness { get; set; }

        [JsonPropertyName("h_index")]
        public int HIndex { get; set; }

        [JsonPropertyName("i10_index")]
        public int I10Index { get; set; }
    }
}