using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IEntityService
    {
        Task ProcessEmbeddedDataAsync(Entity entity);
    }
}
