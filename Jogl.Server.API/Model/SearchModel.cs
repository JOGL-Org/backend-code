using Jogl.Server.Data.Util;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Jogl.Server.API.Model
{
    public class SearchModel : BaseSearchModel
    {
        public SearchModel()
        {
            Page = 1;
            PageSize = 50;
            SortKey = SortKey.CreatedDate;
            SortAscending = false;
        }

        [SwaggerParameter("The page to fetch. Pages start at 1.")]
        [Range(1, 1000)]
        public virtual int Page { get; set; }

        [SwaggerParameter("Size of one page. The default is 50. Maximum value is 1000 ")]
        [Range(1, 1000)]
        public virtual int PageSize { get; set; }

        public SortKey SortKey { get; set; }
        public bool SortAscending { get; set; }
    }
}