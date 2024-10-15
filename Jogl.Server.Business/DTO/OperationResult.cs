namespace Jogl.Server.Business.DTO
{
    public enum Status { OK, Failed, Conflict }
    public class OperationResult<T>
    {
        public Status Status { get; set; }
        public string Id { get; set; }
        public T OriginalPayload { get; set; }
    }
}
