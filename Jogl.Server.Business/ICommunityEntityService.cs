﻿using Jogl.Server.Data;
using Jogl.Server.Data.Enum;

namespace Jogl.Server.Business
{
    public interface ICommunityEntityService
    {
        List<CommunityEntity> List(IEnumerable<string> ids);
        List<CommunityEntity> List(string id, string currentUserId, Permission? permission, string search, int page, int pageSize);
        CommunityEntity Get(string id);
        CommunityEntity GetEnriched(string id, string userId);
        CommunityEntity Get(string id, CommunityEntityType type);
        CommunityEntity GetEnriched(string id, CommunityEntityType type, string userId);
        CommunityEntity Get(string id, FeedType types);
        CommunityEntity GetEnriched(string id, FeedType type, string userId);
        FeedEntity GetEntity(string id, FeedType type);
        FeedEntity GetFeedEntity(string feedId);

        List<Permission> ListPermissions(string id, string userId);
        bool HasPermission(string id, Permission p, string userId);

        Task UpdateAsync(CommunityEntity entity);

        string GetPrintName(CommunityEntityType communityEntityType);
    }
}