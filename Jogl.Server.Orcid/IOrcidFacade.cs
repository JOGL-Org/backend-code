using Jogl.Server.Orcid.DTO;

namespace Jogl.Server.Orcid
{
    public interface IOrcidFacade
    {
        Task<Person> GetPersonalInfo(string orcidId, string accessToken);
        Task<(string, string)> GetOrcidIdAsync(string authorizationCode, string redirectUrlType);
        Task<List<Work>> GetWorksAsync(string orcidId, string accessToken);
        Task<List<Education>> GetEducationsAsync(string orcidId, string accessToken);

        Task<List<Employment>> GetEmploymentsAsync(string orcidId, string accessToken);
        Task<Work> GetWorkFromDOI(string orcidId);
        
        Task RevokeOrcidIdAsync(string accessToken);
    }
}