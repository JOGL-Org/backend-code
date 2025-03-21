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
var membershipRepository = new MembershipRepository(config);
var invitationRepository = new InvitationRepository(config);
var feedRepository = new FeedRepository(config);
var documentRepository = new DocumentRepository(config);
var workspaceRepository = new WorkspaceRepository(config);
var userRepository = new UserRepository(config);

var feeds = feedRepository.List(f => true);
var workspaces = workspaceRepository.List(u => true);
var contentEntities = contentEntityRepository.List(u => true);
var comments = commentRepository.List(u => true);

foreach (var user in userRepository.List(u => true))
{
    user.Onboarding = true;

    if (user.Experience != null)
        foreach (var exp in user.Experience)
        {
            if (exp.DateTo == "Present")
            {
                exp.DateTo = null;
                exp.Current = true;
            }
        }

    if (user.Education != null)
        foreach (var edu in user.Education)
        {
            if (edu.DateTo == "Present")
            {
                edu.DateTo = null;
                edu.Current = true;
            }

            if (string.IsNullOrEmpty(edu.Program) && !string.IsNullOrEmpty(edu.Description))
            {
                edu.Program = edu.Description;
                edu.Description = null;
            }
        }

    await userRepository.UpdateAsync(user);
    await userRepository.SetOnboardingStatusAsync(user);
}

Console.WriteLine("Done");
Console.ReadLine();