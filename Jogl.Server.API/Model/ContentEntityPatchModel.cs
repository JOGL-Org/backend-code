using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Jogl.Server.API.Model
{
    [Obsolete]
    [ValidateNever]
    public class ContentEntityPatchModel : ContentEntityUpsertModel
    {
    }
}