using Jogl.Server.Business.DTO;
using Swashbuckle.AspNetCore.Annotations;
namespace Jogl.Server.API.Model
{
    public class BaseSearchModel
    {
        public BaseSearchModel()
        {
          
        }

        [SwaggerParameter("The search query to filter projects by")]
        public virtual string? Search { get; set; }
    }
}