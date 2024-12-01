using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class OnboardingQuestionnaireInstanceRepository : BaseRepository<OnboardingQuestionnaireInstance>, IOnboardingQuestionnaireInstanceRepository
    {
        public OnboardingQuestionnaireInstanceRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
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