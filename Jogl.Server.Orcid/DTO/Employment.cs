using System.Text.Json.Serialization;

namespace Jogl.Server.Orcid.DTO
{
    public class Employment
    {
        [JsonPropertyName("company")]
        public string OrganizationName { get; set; }
        
        [JsonPropertyName("department_name")] 
        public string DepartmentName { get; set; }

        [JsonPropertyName("position")]
        public string PositionName { get; set; }

        [JsonPropertyName("date_from")]
        public string StartDate { get; set; }

        [JsonPropertyName("date_to")]
        public string EndDate { get; set; }
    }
}
