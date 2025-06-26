namespace Jogl.Server.DB.Context
{
    public class OperationContext : IOperationContext
    {
        public string? UserId { get; set; }
        public string? NodeId { get; set; }
    }
}