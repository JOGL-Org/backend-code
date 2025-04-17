using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using System;

namespace Jogl.Server.URL
{
    public class UrlService : IUrlService
    {
        private readonly IConfiguration _configuration;

        public UrlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public FeedType GetFeedType(string fragment)
        {
            switch (fragment)
            {
                case "user": return FeedType.User;
                case "project": return FeedType.Project;
                case "workspace": return FeedType.Workspace;
                case "hub": return FeedType.Node;
                case "organization": return FeedType.Organization;
                case "cfp": return FeedType.CallForProposal;
                default: throw new Exception($"Unknown url fragment for feed type: {fragment}");
            }
        }

        public string GetImageUrl(string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
                return $"{_configuration["App:URL"]}/images/default/default-project.png";

            return $"{_configuration["App:BackendURL"]}/images/{imageId}/tn";
        }

        public string GetUrl(FeedEntity entity)
        {
            var fragment = GetUrlFragment(entity.FeedType);

            switch (entity.FeedType)
            {
                case FeedType.Document:
                    var doc = (Document)entity;

                    switch (doc.Type)
                    {
                        case DocumentType.JoglDoc:
                            return $"{_configuration["App:URL"]}/{fragment}/{entity.Id}";
                        default:
                            return $"{_configuration["App:URL"]}/{GetUrlFragment(doc.FeedEntity.FeedType)}/{doc.FeedEntity.Id}?tab=documents";
                    }
                default:
                    return $"{_configuration["App:URL"]}/{fragment}/{entity.Id}";
            }
        }

        public string GetContentEntityUrl(string contentEntityId)
        {
            return $"{_configuration["App:URL"]}/post/{contentEntityId}";
        }

        public string GetUrl(FeedEntity entity, Channel channel)
        {
            var url = GetUrl(entity);
            if (channel == null)
                return url;

            return $"{url}?channelId={channel.Id}";
        }

        public string GetUrl(string path)
        {
            return $"{_configuration["App:URL"]}/{path}";
        }

        public string GetUrlFragment(CommunityEntityType type)
        {
            switch (type)
            {
                case CommunityEntityType.Project: return "project";
                case CommunityEntityType.Workspace: return "workspace";
                case CommunityEntityType.Node: return "hub";
                case CommunityEntityType.Organization: return "organization";
                case CommunityEntityType.CallForProposal: return "cfp";
                default: throw new Exception($"Unknown community entity type: {type}");
            }
        }

        public string GetUrlFragment(FeedType type)
        {
            switch (type)
            {
                case FeedType.Channel: return "channel";
                case FeedType.Project: return "project";
                case FeedType.Workspace: return "workspace";
                case FeedType.Node: return "hub";
                case FeedType.Organization: return "organization";
                case FeedType.CallForProposal: return "cfp";
                case FeedType.Need: return "need";
                case FeedType.Document: return "doc";
                case FeedType.Paper: return "paper";
                case FeedType.Event: return "event";
                case FeedType.User: return "user";
                default: throw new Exception($"Unknown feed type: {type}");
            }
        }

        public string GetOneTimeLoginLink(string email, string code)
        {
            return $"{_configuration["App:URL"]}/signin?email={email}&code={code}";
        }
    }
}