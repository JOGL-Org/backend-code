namespace Jogl.Server.DB.Context
{
    public interface IOperationContext
    {
        string UserId { get; set; }
        string NodeId { get; set; }
    }
}