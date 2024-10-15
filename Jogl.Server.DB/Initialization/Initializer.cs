namespace Jogl.Server.DB.Initialization
{
    public class Initializer : IInitializer
    {
        private readonly IEnumerable<IRepository> _repositories;

        public Initializer(IEnumerable<IRepository> repositories)
        {
            _repositories = repositories;
        }

        public async Task InitializeAsync()
        {
            foreach (var repo in _repositories)
            {
                await repo.InitializeAsync();
            }
        }
    }
}