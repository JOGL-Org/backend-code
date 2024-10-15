using System.Text.Json.Serialization;

namespace Jogl.Server.Orcid.DTO
{
    public class Education
    {
        [JsonPropertyName("school")]
        public string OrganizationName { get; set; }

        [JsonPropertyName("department_name")]
        public string DepartmentName { get; set; }

        [JsonPropertyName("degree_name")]
        public string DegreeName { get; set; }

        [JsonPropertyName("dateFrom")]
        public string StartDate { get; set; }

        [JsonPropertyName("dateTo")]
        public string EndDate { get; set; }
    }
}
