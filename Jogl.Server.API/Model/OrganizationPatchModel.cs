using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Jogl.Server.API.Model
{
    [ValidateNever]
    public class OrganizationPatchModel : OrganizationUpsertModel
    {

    }
}