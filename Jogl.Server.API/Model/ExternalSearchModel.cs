using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Jogl.Server.API.Model
{
    public class ExternalSearchModel : SearchModel
    {
        public ExternalSearchModel() : base()
        {

        }

        [SwaggerParameter("The search query to filter projects by")]
        [Required]
        public override string? Search { get; set; }

        [SwaggerParameter("The page to fetch. Pages start at 1.")]
        [Range(1, 100)]
        public override int Page { get; set; }

        [SwaggerParameter("Size of one page. The default is 50. Maximum value is 100")]
        [Range(1, 100)]
        public override int PageSize { get; set; }
    }
}