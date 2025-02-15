﻿using Amazon.Runtime.Internal;
using Jogl.Server.Configuration;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var userFeedRecordRepository = new UserFeedRecordRepository(config);
var userContentEntityRecordRepository = new UserContentEntityRecordRepository(config);
var mentionRepository = new MentionRepository(config);
var contentEntityRepository = new ContentEntityRepository(config);
var commentRepository = new CommentRepository(config);
var membershipRepo = new MembershipRepository(config);

var ufrs = userFeedRecordRepository.Query(ufr => true).ToList();
var memberships = membershipRepo.Query(m => m.CommunityEntityType == Jogl.Server.Data.CommunityEntityType.Channel).ToList();
foreach (var membership in memberships)
{
    if (ufrs.SingleOrDefault(ufr => ufr.FeedId == membership.CommunityEntityId && ufr.UserId == membership.UserId)?.FollowedUTC == null)
    {
        await userFeedRecordRepository.SetFeedFollowedAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);
    }
}

var followedUFRs = userFeedRecordRepository.Query(ufr => ufr.FollowedUTC.HasValue).ToList();
var ucers = userContentEntityRecordRepository.Query(ucer => true).ToList();
var contentEntities = contentEntityRepository.Query(ce => true).ToList();
var comments = commentRepository.Query(c => true).ToList();
var mentions = mentionRepository.Query(m => true).ToList();

foreach (var ufr in followedUFRs)
{
    var contentEntitiesInFeed = contentEntities.Where(ce => ce.FeedId == ufr.FeedId).ToList();
    var mentionsInFeed = mentions.Where(m => m.OriginFeedId == ufr.FeedId).ToList();

    if (contentEntitiesInFeed.Any(c => c.CreatedUTC > ufr.LastReadUTC))
    {
        ufr.Unread = true;
        await userFeedRecordRepository.UpdateAsync(ufr);
        continue;
    }

    if (mentionsInFeed.Any(m => m.CreatedUTC > ufr.LastReadUTC && m.OriginType == Jogl.Server.Data.MentionOrigin.ContentEntity))
    {
        ufr.Unread = true;
        await userFeedRecordRepository.UpdateAsync(ufr);
        continue;
    }

    var ucersInFeed = ucers.Where(ucer => ucer.FeedId == ufr.FeedId).ToList();
    foreach (var ucer in ucersInFeed)
    {
        var commentsInPost = comments.Where(c => c.ContentEntityId == ucer.ContentEntityId).ToList();
        if (commentsInPost.Any(c => c.CreatedUTC > ucer.LastReadUTC))
        {
            ufr.Unread = true;
            await userFeedRecordRepository.UpdateAsync(ufr);
            break;
        }

        var mentionsInPost = mentions.Where(m => commentsInPost.Any(c => c.Id.ToString() == m.OriginId)).ToList();
       if (mentionsInPost.Any(m => m.CreatedUTC > ucer.LastReadUTC))
        {
            ufr.Unread = true;
            await userFeedRecordRepository.UpdateAsync(ufr);
            break;
        }
    }
}

Console.WriteLine("Done");
Console.ReadLine();