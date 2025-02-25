﻿using Amazon.Runtime.Internal;
using Jogl.Server.Configuration;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

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
var feedRepository = new FeedRepository(config);
var documentRepository = new DocumentRepository(config);
var workspaceRepository = new WorkspaceRepository(config);

var feeds = feedRepository.List(f => true);
var workspaces = workspaceRepository.List(u => true);

foreach (var feed in feeds)
{
    if (feed.Type != Jogl.Server.Data.FeedType.Workspace)
        continue;

    var workspace = workspaces.FirstOrDefault(f => f.Id == feed.Id);
    if (workspace == null)
    {
        await feedRepository.DeleteAsync(feed);
        continue;
    }

    if (workspace.Deleted)
    {
        await feedRepository.DeleteAsync(feed);
        continue;
    }
}

feeds = feedRepository.List(f => true);

foreach (var ufr in userFeedRecordRepository.List(u => !u.Deleted))
{
    var feed = feeds.FirstOrDefault(f => f.Id.ToString() == ufr.FeedId);
    if (feed == null)
    {
        await userFeedRecordRepository.DeleteAsync(ufr);
        continue;
    }

    if (feed.Deleted)
    {
        await userFeedRecordRepository.DeleteAsync(ufr);
        continue;
    }
}

foreach (var ucer in userContentEntityRecordRepository.List(u => !u.Deleted))
{
    var feed = feeds.FirstOrDefault(f => f.Id.ToString() == ucer.FeedId);
    if (feed == null)
    {
        await userContentEntityRecordRepository.DeleteAsync(ucer);
        continue;
    }

    if (feed.Deleted)
    {
        await userContentEntityRecordRepository.DeleteAsync(ucer);
        continue;
    }
}

Console.WriteLine("Done");
Console.ReadLine();