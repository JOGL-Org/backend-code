namespace Jogl.Server.Search
{
    public interface IBusinessSearchService
    {
        Task<List<User>> SearchUsersAsync(string query, string nodeId);
    }
}
