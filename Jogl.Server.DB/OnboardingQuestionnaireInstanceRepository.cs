using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class OnboardingQuestionnaireInstanceRepository : BaseRepository<OnboardingQuestionnaireInstance>, IOnboardingQuestionnaireInstanceRepository
    {
        public OnboardingQuestionnaireInstanceRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "onboardingQuestionnaireInstances";

        protected override UpdateDefinition<OnboardingQuestionnaireInstance> GetDefaultUpdateDefinition(OnboardingQuestionnaireInstance updatedEntity)
        {
            return Builders<OnboardingQuestionnaireInstance>.Update.Set(e => e.Items, updatedEntity.Items)
                                                                   .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                                   .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}