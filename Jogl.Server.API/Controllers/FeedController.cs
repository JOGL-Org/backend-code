using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.Data.Util;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("feed")]
    public class FeedController : BaseController
    {
        private readonly IContentService _contentService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IDocumentService _documentService;
        private readonly IUserService _userService;

        public FeedController(IContentService contentService, ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IDocumentService documentService, IUserService userService, IMapper mapper, ILogger<FeedController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _contentService = contentService;
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _documentService = documentService;
            _userService = userService;
        }

        [HttpPost]
        [Route("{feedId}")]
        public async Task<IActionResult> AddContentEntity([FromRoute] string feedId, [FromBody] ContentEntityUpsertModel model)
        {
            var permissions = _communityEntityService.ListPermissions(feedId, CurrentUserId);
            if (!permissions.Contains(Permission.PostContentEntity))
                return Forbid();

            if (_contentService.MentionEveryone(model.Text) && !permissions.Contains(Permission.MentionEveryone))
                return Forbid();

            var contentEntity = _mapper.Map<ContentEntity>(model);
            contentEntity.FeedId = feedId;
            await InitCreationAsync(contentEntity);
            var contentEntityId = await _contentService.CreateAsync(contentEntity);
            return Ok(contentEntityId);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{feedId}")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Discussion data, including stats for the currently logged in user", typeof(DiscussionModel))]
        public async Task<IActionResult> GetFeedData([FromRoute] string feedId, [FromQuery] ContentEntityType? type, [FromQuery] ContentEntityFilter filter, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Forbid();

            var data = _contentService.GetDiscussion(CurrentUserId, feedId, type, filter, model.Search, model.Page, model.PageSize);
            var dataModel = _mapper.Map<DiscussionModel>(data);
            dataModel.Permissions = _communityEntityService.ListPermissions(feedId, CurrentUserId);

            return Ok(dataModel);
        }

        [HttpGet]
        [Route("{feedId}/permissions")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Discussion permissions for the currently logged in user", typeof(List<Permission>))]
        public async Task<IActionResult> GetDiscussionPermissions([FromRoute] string feedId)
        {
            var permissions = _communityEntityService.ListPermissions(feedId, CurrentUserId);
            if (!permissions.Contains(Permission.Read))
                return Forbid();

            return Ok(permissions);
        }

        [Obsolete]
        [HttpGet]
        [Route("{feedId}/mentions")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Discussion mentions for the currently logged in user", typeof(List<ContentEntityModel>))]
        public async Task<IActionResult> GetDiscussionMentions([FromRoute] string feedId, [FromQuery] ContentEntityType? type, [FromQuery] int offset, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Forbid();

            var contentEntities = _contentService.ListMentionContentEntities(CurrentUserId, feedId, type, model.Search, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Items.Select(_mapper.Map<ContentEntityModel>);
            return Ok(contentEntityModels);
        }

        [Obsolete]
        [HttpGet]
        [Route("{feedId}/threads")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Discussion threads for the currently logged in user", typeof(List<ContentEntityModel>))]
        public async Task<IActionResult> GetDiscussionThreads([FromRoute] string feedId, [FromQuery] ContentEntityType? type, [FromQuery] int offset, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Forbid();

            var contentEntities = _contentService.ListThreadContentEntities(CurrentUserId, feedId, type, model.Search, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Items.Select(_mapper.Map<ContentEntityModel>);
            return Ok(contentEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{feedId}/posts/list")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Posts", typeof(ListPage<ContentEntityModel>))]
        public async Task<IActionResult> GetFeedPosts([FromRoute] string feedId, [FromQuery] ContentEntityType? type, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Forbid();

            var contentEntities = _contentService.ListPostContentEntities(CurrentUserId, feedId, type, model.Search, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Items.Select(_mapper.Map<ContentEntityModel>);
            return Ok(new ListPage<ContentEntityModel>(contentEntityModels, contentEntities.Total));
        }

        [HttpGet]
        [Route("{feedId}/mentions/list")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Posts with mentions", typeof(ListPage<ContentEntityModel>))]
        public async Task<IActionResult> GetFeedMentions([FromRoute] string feedId, [FromQuery] ContentEntityType? type, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Forbid();

            var contentEntities = _contentService.ListMentionContentEntities(CurrentUserId, feedId, type, model.Search, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Items.Select(_mapper.Map<ContentEntityModel>);
            return Ok(new ListPage<ContentEntityModel>(contentEntityModels, contentEntities.Total));
        }

        [HttpGet]
        [Route("{feedId}/threads/list")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Posts with unread threads", typeof(ListPage<ContentEntityModel>))]
        public async Task<IActionResult> GetFeedThreads([FromRoute] string feedId, [FromQuery] ContentEntityType? type, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Forbid();

            var contentEntities = _contentService.ListThreadContentEntities(CurrentUserId, feedId, type, model.Search, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Items.Select(_mapper.Map<ContentEntityModel>);
            return Ok(new ListPage<ContentEntityModel>(contentEntityModels, contentEntities.Total));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("node/{nodeId}/posts/list")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Posts", typeof(List<ContentEntityModel>))]
        public async Task<IActionResult> GetNodePosts([FromRoute] string nodeId, [FromQuery] SearchModel model)
        {
            var contentEntities = _contentService.ListContentEntitiesForNode(CurrentUserId, nodeId, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Select(_mapper.Map<ContentEntityModel>);
            return Ok(contentEntityModels);
        }

        [HttpGet]
        [Route("node/{nodeId}/mentions/list")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Posts with mentions", typeof(List<ContentEntityModel>))]
        public async Task<IActionResult> GetNodeMentions([FromRoute] string nodeId, [FromQuery] SearchModel model)
        {
            var contentEntities = _contentService.ListMentionsForNode(CurrentUserId, nodeId, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Select(_mapper.Map<ContentEntityModel>);
            return Ok(contentEntityModels);
        }

        [HttpGet]
        [Route("node/{nodeId}/threads/list")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Posts with unread threads", typeof(List<ContentEntityModel>))]
        public async Task<IActionResult> GetNodeThreads([FromRoute] string nodeId, [FromQuery] SearchModel model)
        {
            var contentEntities = _contentService.ListThreadsForNode(CurrentUserId, nodeId, model.Page, model.PageSize);
            var contentEntityModels = contentEntities.Select(_mapper.Map<ContentEntityModel>);
            return Ok(contentEntityModels);
        }

        [HttpPost]
        [Route("{feedId}/opened")]
        [SwaggerOperation($"Records the user having opened the specified feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The feed was marked as opened")]
        public async Task<IActionResult> MarkFeedOpened([FromRoute] string feedId)
        {
            await _contentService.SetFeedOpenedAsync(feedId, CurrentUserId);
            return Ok();
        }

        [HttpPost]
        [Route("{feedId}/seen")]
        // [Route("{feedId}/mentions/seen")]
        [SwaggerOperation($"Records the user having read the specified feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"All new posts and mentions were marked as seen")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, $"There was nothing to mark as seen")]
        public async Task<IActionResult> MarkFeedSeen([FromRoute] string feedId)
        {
            var res = await _contentService.SetFeedReadAsync(feedId, CurrentUserId);
            if (!res)
                return NoContent();

            return Ok();
        }

        [HttpPost]
        [Route("{feedId}/star")]
        [SwaggerOperation($"Makes the feed starred for a user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The feed has not yet been read by the user or is not accessible to them")]
        public async Task<IActionResult> StarFeed([FromRoute] string feedId)
        {
            var record = _contentService.GetFeedRecord(CurrentUserId, feedId);
            if (record == null)
                return NotFound();

            record.Starred = true;
            await _contentService.UpdateFeedRecordAsync(record);
            return Ok();
        }

        [HttpDelete]
        [Route("{feedId}/star")]
        [SwaggerOperation($"Makes the feed unstarred for a user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The feed has not yet been read by the user or is not accessible to them")]
        public async Task<IActionResult> UnstarFeed([FromRoute] string feedId)
        {
            var record = _contentService.GetFeedRecord(CurrentUserId, feedId);
            if (record == null)
                return NotFound();

            record.Starred = false;
            await _contentService.UpdateFeedRecordAsync(record);
            return Ok();
        }

        [HttpPost]
        [Route("{feedId}/active")]
        [SwaggerOperation($"Makes the feed unarchived for a user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The feed has not yet been read by the user or is not accessible to them")]
        public async Task<IActionResult> UnarchiveFeed([FromRoute] string feedId)
        {
            var record = _contentService.GetFeedRecord(CurrentUserId, feedId);
            if (record == null)
                return NotFound();

            record.Muted = false;
            await _contentService.UpdateFeedRecordAsync(record);
            return Ok();
        }

        [HttpDelete]
        [Route("{feedId}/active")]
        [SwaggerOperation($"Makes the feed archived for a user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The feed has not yet been read by the user or is not accessible to them")]
        public async Task<IActionResult> ArchiveFeed([FromRoute] string feedId)
        {
            var record = _contentService.GetFeedRecord(CurrentUserId, feedId);
            if (record == null)
                return NotFound();

            record.Muted = true;
            await _contentService.UpdateFeedRecordAsync(record);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("contentEntities/{contentEntityId}/feed")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to view the content entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The feed data", typeof(FeedModel))]
        public async Task<IActionResult> GetFeedForContentEntity([FromRoute] string contentEntityId)
        {
            var contentEntity = _contentService.GetDetail(contentEntityId, CurrentUserId);
            if (contentEntity == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(contentEntity.FeedId, Permission.Read, CurrentUserId))
                return Forbid();

            var feed = _contentService.GetFeed(contentEntity.FeedId);
            var model = _mapper.Map<FeedModel>(feed);
            return Ok(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("contentEntities/{contentEntityId}")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to view the content entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The content entity data", typeof(ContentEntityModel))]
        public async Task<IActionResult> GetContentEntity([FromRoute] string contentEntityId)
        {
            var contentEntity = _contentService.GetDetail(contentEntityId, CurrentUserId);
            if (contentEntity == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(contentEntity.FeedId, Permission.Read, CurrentUserId))
                return Forbid();

            var model = _mapper.Map<ContentEntityModel>(contentEntity);
            return Ok(model);
        }

        [HttpPut]
        [Route("contentEntities/{contentEntityId}")]
        public async Task<IActionResult> UpdateContentEntity([FromRoute] string contentEntityId, [FromBody] ContentEntityUpsertModel model)
        {
            var existingContentEntity = _contentService.GetDetail(contentEntityId, CurrentUserId);
            if (existingContentEntity == null)
                return NotFound();

            if (existingContentEntity.CreatedByUserId != CurrentUserId)
                return Forbid();

            var contentEntity = _mapper.Map<ContentEntity>(model);
            contentEntity.Id = ObjectId.Parse(contentEntityId);
            contentEntity.FeedId = existingContentEntity.FeedId;
            await InitUpdateAsync(contentEntity);

            await _contentService.UpdateAsync(contentEntity);
            return Ok();
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpPatch]
        [Route("contentEntities/{contentEntityId}")]
        public async Task<IActionResult> PatchContentEntity([FromRoute] string contentEntityId, [FromBody] ContentEntityPatchModel model)
        {
            var contentObject = _contentService.GetDetail(contentEntityId, CurrentUserId);
            if (contentObject == null)
                return NotFound();

            if (contentObject.CreatedByUserId != CurrentUserId)
                return Forbid();

            var upsertModel = _mapper.Map<ContentEntityUpsertModel>(contentObject);
            ApplyPatchModel(model, upsertModel);

            var updatedContentEntity = _mapper.Map<ContentEntity>(upsertModel);
            updatedContentEntity.FeedId = contentObject.FeedId;
            await InitUpdateAsync(updatedContentEntity);

            await _contentService.UpdateAsync(updatedContentEntity);
            return Ok();
        }

        [HttpDelete]
        [Route("contentEntities/{contentEntityId}")]
        public async Task<IActionResult> DeleteContentEntity([FromRoute] string contentEntityId)
        {
            var contentEntity = _contentService.Get(contentEntityId);
            if (contentEntity == null)
                return NotFound();

            if (contentEntity.CreatedByUserId != CurrentUserId && !_communityEntityService.HasPermission(contentEntity.FeedId, Permission.DeleteContentEntity, CurrentUserId))
                return Forbid();

            await _contentService.DeleteAsync(contentEntity.Id.ToString());
            return Ok();
        }

        [HttpPost]
        [Route("contentEntities/{contentEntityId}/seen")]
        [SwaggerOperation($"Records the user having read a post")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"All new replies and mentions were marked as seen")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, $"There was nothing to mark as seen")]
        public async Task<IActionResult> MarkPostSeen([FromRoute] string contentEntityId)
        {
            var contentEntity = _contentService.Get(contentEntityId);
            if (contentEntity == null)
                return NotFound();

            var res = await _contentService.SetContentEntityReadAsync(contentEntityId, contentEntity.FeedId, CurrentUserId);
            if (!res)
                return NoContent();
            return Ok();
        }

        [Obsolete]
        [HttpPost]
        [Route("contentEntities/seen")]
        [SwaggerOperation($"Records the user having read the specific content entities")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for one of the ids")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The content entities were marked as seen")]
        public async Task<IActionResult> MarkPostsSeen([SwaggerParameter("An array of content entity ids to be marked as seen by the user")][FromBody] List<string> contentEntityIds)
        {
            if (!contentEntityIds.Any())
                return Ok();

            var contentEntity = _contentService.Get(contentEntityIds.First());
            if (contentEntity == null)
                return NotFound();

            await _contentService.SetContentEntitiesReadAsync(contentEntityIds, contentEntity.FeedId, CurrentUserId);
            return Ok();
        }

        [HttpPost]
        [Route("contentEntities/{contentEntityId}/reactions")]
        [SwaggerOperation($"Creates or replaces a user's reaction to a content entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Content entity not found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user does not have the rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Reaction created or replaced", typeof(string))]
        public async Task<IActionResult> UpsertReaction([FromRoute] string contentEntityId, [FromBody] ReactionUpsertModel model)
        {
            var contentObject = _contentService.Get(contentEntityId);
            if (contentObject == null)
                return NotFound();

            var permissions = _communityEntityService.ListPermissions(contentObject.FeedId, CurrentUserId);
            if (!permissions.Contains(Permission.Read))
                return Forbid();

            var reaction = _contentService.GetReaction(contentEntityId, CurrentUserId);
            if (reaction != null)
            {
                reaction.Key = model.Key;
                await InitUpdateAsync(reaction);
                await _contentService.UpdateReactionAsync(reaction);
                return Ok(reaction.Id.ToString());
            }
            else
            {
                reaction = new Reaction
                {
                    FeedId = contentObject.FeedId,
                    OriginId = contentEntityId,
                    UserId = CurrentUserId,
                    Key = model.Key
                };

                await InitCreationAsync(reaction);
                var id = await _contentService.CreateReactionAsync(reaction);
                return Ok(id);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("contentEntities/{contentEntityId}/reactions")]
        [SwaggerOperation($"Lists all reactions to a content entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Content entity not found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user does not have the rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Reaction data", typeof(List<ReactionModel>))]
        public async Task<IActionResult> GetReactions([FromRoute] string contentEntityId)
        {
            var contentObject = _contentService.Get(contentEntityId);
            if (contentObject == null)
                return NotFound();

            var permissions = _communityEntityService.ListPermissions(contentObject.FeedId, CurrentUserId);
            if (!permissions.Contains(Permission.Read))
                return Forbid();

            var reactions = _contentService.ListReactions(contentEntityId, CurrentUserId);
            var reactionModels = reactions.Select(_mapper.Map<ReactionModel>);
            return Ok(reactionModels);
        }

        [HttpDelete]
        [Route("contentEntities/{contentEntityId}/reactions/{id}")]
        [SwaggerOperation($"Removes a user's reaction to a content entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Reaction not found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user does not have the rights to delete the reaction")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The reaction was deleted")]
        public async Task<IActionResult> DeleteReaction([FromRoute] string contentEntityId, [FromRoute] string id)
        {
            var reaction = _contentService.GetReaction(id);
            if (reaction == null || reaction.OriginId != contentEntityId)
                return NotFound();

            if (reaction.CreatedByUserId != CurrentUserId)
                return Forbid();

            await _contentService.DeleteReactionAsync(reaction.Id.ToString());
            return Ok();
        }

        [HttpPost]
        [Route("contentEntities/{contentEntityId}/comments")]
        public async Task<IActionResult> AddComment([FromRoute] string contentEntityId, [FromBody] CommentUpsertModel model)
        {
            var contentEntity = _contentService.Get(contentEntityId);
            if (contentEntity == null)
                return NotFound();

            var permissions = _communityEntityService.ListPermissions(contentEntity.FeedId, CurrentUserId);
            if (!permissions.Contains(Permission.PostComment))
                return Forbid();

            if (_contentService.MentionEveryone(model.Text) && !permissions.Contains(Permission.MentionEveryone))
                return Forbid();

            var comment = _mapper.Map<Comment>(model);
            comment.FeedId = contentEntity.FeedId;
            comment.ContentEntityId = contentEntityId;
            comment.UserId = CurrentUserId;

            await InitCreationAsync(comment);
            var id = await _contentService.CreateCommentAsync(comment);

            return Ok(id);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("contentEntities/{contentEntityId}/comments")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The feed, entity or content entity does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view discussions for the parent entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The comment data", typeof(List<CommentModel>))]
        public async Task<IActionResult> GetComments([FromRoute] string contentEntityId, [FromQuery] SearchModel model)
        {
            var contentObject = _contentService.Get(contentEntityId);
            if (contentObject == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(contentObject.FeedId, Permission.Read, CurrentUserId))
                return Forbid();

            var comments = _contentService.ListComments(contentEntityId, CurrentUserId, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var commentModels = comments.Items.Select(_mapper.Map<CommentModel>);
            return Ok(commentModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("contentEntities/{contentEntityId}/comments/list")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The content entity does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view discussions for the parent entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The comment data", typeof(ListPage<CommentModel>))]
        public async Task<IActionResult> GetFeedComments([FromRoute] string contentEntityId, [FromQuery] SearchModel model)
        {
            var contentObject = _contentService.Get(contentEntityId);
            if (contentObject == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(contentObject.FeedId, Permission.Read, CurrentUserId))
                return Forbid();

            var comments = _contentService.ListComments(contentEntityId, CurrentUserId, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var contentEntityModels = comments.Items.Select(_mapper.Map<CommentModel>);
            return Ok(new ListPage<CommentModel>(contentEntityModels, comments.Total));
        }

        [HttpPut]
        [Route("contentEntities/{contentEntityId}/comments/{id}")]
        public async Task<IActionResult> UpdateComment([FromRoute] string contentEntityId, [FromRoute] string id, [FromBody] CommentUpsertModel model)
        {
            var comment = _contentService.GetComment(id);
            if (comment == null || comment.ContentEntityId != contentEntityId)
                return NotFound();

            if (comment.CreatedByUserId != CurrentUserId)
                return Forbid();

            var updatedComment = _mapper.Map<Comment>(model);
            updatedComment.Id = ObjectId.Parse(id);
            updatedComment.ContentEntityId = comment.ContentEntityId;
            updatedComment.FeedId = comment.FeedId;
            await InitUpdateAsync(updatedComment);

            await _contentService.UpdateCommentAsync(updatedComment);
            return Ok();
        }

        [Obsolete]
        [HttpPatch]
        [Route("contentEntities/{contentEntityId}/comments/{id}")]
        public async Task<IActionResult> PatchComment([FromRoute] string contentEntityId, [FromRoute] string id, [FromBody] CommentPatchModel model)
        {
            var comment = _contentService.GetComment(id);
            if (comment == null || comment.ContentEntityId != contentEntityId)
                return NotFound();

            if (comment.CreatedByUserId != CurrentUserId)
                return Forbid();

            var upsertModel = _mapper.Map<CommentUpsertModel>(comment);
            ApplyPatchModel(model, upsertModel);

            var updatedComment = _mapper.Map<Comment>(upsertModel);
            updatedComment.ContentEntityId = comment.ContentEntityId;
            updatedComment.FeedId = comment.FeedId;
            await InitUpdateAsync(updatedComment);

            await _contentService.UpdateCommentAsync(updatedComment);
            return Ok();
        }

        [HttpDelete]
        [Route("contentEntities/{contentEntityId}/comments/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] string contentEntityId, [FromRoute] string id)
        {
            var comment = _contentService.GetComment(id);
            if (comment == null || comment.ContentEntityId != contentEntityId)
                return NotFound();

            var contentEntity = _contentService.Get(contentEntityId);
            if (contentEntity == null)
                return NotFound();

            if (comment.CreatedByUserId != CurrentUserId && !_communityEntityService.HasPermission(contentEntity.FeedId, Permission.DeleteComment, CurrentUserId))
                return Forbid();

            await _contentService.DeleteCommentAsync(comment.Id.ToString());
            return Ok();
        }

        [Obsolete]
        [HttpPost]
        [Route("contentEntities/comments/seen")]
        [Route("comments/seen")]
        [SwaggerOperation($"Records the user having read the specific comments under a post")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"All comments were marked as seen")]
        public async Task<IActionResult> MarkCommentsSeen([SwaggerParameter("An array of comment ids to be marked as seen by the user")][FromBody] List<string> commentIds)
        {
            await _contentService.SetCommentsReadAsync(commentIds, CurrentUserId);
            return Ok();
        }

        [HttpPost]
        [Route("comments/{commentId}/reactions")]
        [SwaggerOperation($"Creates or replaces a user's reaction to a comment")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Comment not found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user does not have the rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Reaction created or replaced", typeof(string))]
        public async Task<IActionResult> UpsertReactionForComment([FromRoute] string commentId, [FromBody] ReactionUpsertModel model)
        {
            var comment = _contentService.GetComment(commentId);
            if (comment == null)
                return NotFound();

            var contentObject = _contentService.Get(comment.ContentEntityId);
            if (contentObject == null)
                return NotFound();

            var permissions = _communityEntityService.ListPermissions(contentObject.FeedId, CurrentUserId);
            if (!permissions.Contains(Permission.Read))
                return Forbid();

            var reaction = _contentService.GetReaction(commentId, CurrentUserId);
            if (reaction != null)
            {
                reaction.Key = model.Key;
                await InitUpdateAsync(reaction);
                await _contentService.UpdateReactionAsync(reaction);
                return Ok(reaction.Id.ToString());
            }
            else
            {
                reaction = new Reaction
                {
                    FeedId = contentObject.FeedId,
                    OriginId = commentId,
                    UserId = CurrentUserId,
                    Key = model.Key
                };

                await InitCreationAsync(reaction);
                var id = await _contentService.CreateReactionAsync(reaction);
                return Ok(id);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("comments/{commentId}/reactions")]
        [SwaggerOperation($"Lists all reactions to a comment")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Comment not found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user does not have the rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Reaction data", typeof(List<ReactionModel>))]
        public async Task<IActionResult> GetReactionsForComment([FromRoute] string commentId)
        {
            var comment = _contentService.GetComment(commentId);
            if (comment == null)
                return NotFound();

            var contentObject = _contentService.Get(comment.ContentEntityId);
            if (contentObject == null)
                return NotFound();

            var permissions = _communityEntityService.ListPermissions(contentObject.FeedId, CurrentUserId);
            if (!permissions.Contains(Permission.Read))
                return Forbid();

            var reactions = _contentService.ListReactions(commentId, CurrentUserId);
            var reactionModels = reactions.Select(_mapper.Map<ReactionModel>);
            return Ok(reactionModels);
        }

        [HttpDelete]
        [Route("comments/{commentId}/reactions/{id}")]
        [SwaggerOperation($"Removes a user's reaction to a comment")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Reaction not found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user does not have the rights to delete the reaction")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The reaction was deleted")]
        public async Task<IActionResult> DeleteReactionForComment([FromRoute] string commentId, [FromRoute] string id)
        {
            var reaction = _contentService.GetReaction(id);
            if (reaction == null || reaction.OriginId != commentId)
                return NotFound();

            if (reaction.CreatedByUserId != CurrentUserId)
                return Forbid();

            await _contentService.DeleteReactionAsync(reaction.Id.ToString());
            return Ok();
        }

        [HttpPost]
        [Route("contentEntities/{contentEntityId}/documents")]
        [SwaggerOperation($"Adds a new document for the specified content entity.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for that id or the feed id is incorrect")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add documents for the content entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was created", typeof(string))]
        public async Task<IActionResult> AddDocument([FromRoute] string contentEntityId, [FromBody] DocumentInsertModel model)
        {
            var contentObject = _contentService.Get(contentEntityId);
            if (contentObject == null)
                return NotFound();

            if (contentObject.CreatedByUserId != CurrentUserId)
                return Forbid();

            var document = _mapper.Map<Document>(model);
            document.ContentEntityId = contentEntityId;
            document.FeedId = contentObject.FeedId;
            await InitCreationAsync(document);
            var documentId = await _documentService.CreateAsync(document);
            return Ok(documentId);
        }

        [Obsolete]
        [HttpPut]
        [Route("contentEntities/{contentEntityId}/documents/{documentId}")]
        [SwaggerOperation($"Updates the title and description for the document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for that id, the feed id is incorrect or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was updated")]
        public async Task<IActionResult> UpdateDocument([FromRoute] string contentEntityId, [FromRoute] string documentId, [FromBody] DocumentUpdateModel model)
        {
            var contentObject = _contentService.Get(contentEntityId);
            if (contentObject == null)
                return NotFound();

            var existingDocument = await _documentService.GetAsync(documentId, CurrentUserId, false);
            if (existingDocument == null)
                return NotFound();

            if (existingDocument.ContentEntityId != contentEntityId)
                return NotFound();

            //TODO check access rights to project and return 403 or 404 if needed
            var document = _mapper.Map<Document>(model);
            document.Id = ObjectId.Parse(documentId);
            await InitUpdateAsync(document);
            await _documentService.UpdateAsync(document);
            return Ok();
        }

        [HttpDelete]
        [Route("contentEntities/{contentEntityId}/documents/{documentId}")]
        [SwaggerOperation($"Deletes the specified document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete the document")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was deleted")]
        public async Task<IActionResult> DeleteDocument([FromRoute] string contentEntityId, [FromRoute] string documentId)
        {
            var contentObject = _contentService.Get(contentEntityId);
            if (contentObject == null)
                return NotFound();

            var existingDocument = _documentService.Get(documentId);
            if (existingDocument == null || existingDocument.ContentEntityId != contentEntityId)
                return NotFound();

            if (contentObject.CreatedByUserId != CurrentUserId)
                return Forbid();

            await _documentService.DeleteAsync(documentId);
            return Ok();
        }

        [Obsolete]
        [HttpDelete]
        [Route("contentEntities/{contentEntityId}/comments/{commentId}/documents/{documentId}")]
        [SwaggerOperation($"Deletes the specified document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No content entity was found for that id, no comment was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete the document")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was deleted")]
        public async Task<IActionResult> DeleteDocument([FromRoute] string contentEntityId, [FromRoute] string commentId, [FromRoute] string documentId)
        {
            var comment = _contentService.GetComment(commentId);
            if (comment == null)
                return NotFound();

            var existingDocument = _documentService.Get(documentId);
            if (existingDocument == null || existingDocument.ContentEntityId != contentEntityId || existingDocument.CommentId != commentId)
                return NotFound();

            if (comment.CreatedByUserId != CurrentUserId)
                return Forbid();

            await _documentService.DeleteAsync(documentId);
            return Ok();
        }

        [HttpGet]
        [Route("draft/{entityId}")]
        [SwaggerOperation($"Fetches a draft for a channel, feed or post id")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The draft text", typeof(string))]
        public async Task<IActionResult> GetDraft([FromRoute] string entityId)
        {
            var draft = _contentService.GetDraft(entityId, CurrentUserId);
            return Ok(draft?.Text);
        }

        [HttpPost]
        [Route("draft/{entityId}")]
        [SwaggerOperation($"Creates or updates a draft for a channel, feed or post id")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The draft was created or updated")]
        public async Task<IActionResult> UpsertDraft([FromRoute] string entityId, [FromBody] string text)
        {
            await _contentService.SetDraftAsync(entityId, CurrentUserId, text);
            return Ok();
        }

        [HttpGet]
        [Route("nodes")]
        [SwaggerOperation("Returns a list of nodes with metadata")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Node metadata", typeof(List<NodeFeedDataModel>))]
        public async Task<IActionResult> GetNodeMetadataNew()
        {
            var nodeData = _contentService.ListNodeMetadata(CurrentUserId);
            var nodeDataModels = nodeData.Select(_mapper.Map<NodeFeedDataModel>);

            return Ok(nodeDataModels);
        }

        [HttpGet]
        [Route("nodes/{id}")]
        [SwaggerOperation("Returns a node with metadata")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Node metadata", typeof(NodeFeedDataModel))]
        public async Task<IActionResult> GetNodeMetadata(string id)
        {
            var nodeData = _contentService.GetNodeMetadata(id, CurrentUserId);
            var nodeDataModel = _mapper.Map<NodeFeedDataModel>(nodeData);
            
            return Ok(nodeDataModel);
        }

        [HttpGet]
        [Route("{feedId}/users")]
        [Route("{feedId}/users/autocomplete")]
        [SwaggerOperation("List all users for a given feed")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to list the feed's users")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of user models", typeof(List<UserMiniModel>))]
        public async Task<IActionResult> AutocompleteUsers([SwaggerParameter("the id of the community entity, event, need, paper, document")][FromRoute] string feedId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Ok(new List<UserMiniModel>()); //TODO return 403 when front end is fixed

            var users = _userService.AutocompleteForEntity(feedId, model.Search, model.Page, model.PageSize);
            var userModels = users.Select(_mapper.Map<UserMiniModel>).ToList();

            var feedIntegrations = _contentService.ListFeedIntegrations(feedId, model.Search);
            foreach (var feedIntegration in feedIntegrations)
            {
                if (feedIntegration.Type != FeedIntegrationType.JOGLAgentPublication)
                    continue;

                userModels.Add(new UserMiniModel
                {
                    FirstName = feedIntegration.SourceId,
                    LastName = "(AI)",
                    Id = feedIntegration.Id.ToString(),
                    Username = feedIntegration.SourceId.ToLower()
                });
            }

            return Ok(userModels);
        }

        [HttpPost]
        [Route("{feedId}/integrations")]
        [SwaggerOperation($"Adds a feed integration to the specified feed")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add integrations to the feed")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"The operation failed validation; this may be because the source id does not exist, or it's private and the access token needs to be supplied")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"The integration already exists for the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The feed integration id", typeof(string))]
        public async Task<IActionResult> AddIntegration([FromRoute] string feedId, [FromBody] FeedIntegrationUpsertModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Manage, CurrentUserId))
                return Forbid();

            var existingIntegration = _contentService.GetFeedIntegration(feedId, model.Type, model.SourceId);
            if (existingIntegration != null)
                return Conflict();

            var integration = _mapper.Map<FeedIntegration>(model);
            integration.FeedId = feedId;
            await InitCreationAsync(integration);
            var res = await _contentService.ValidateFeedIntegrationAsync(integration);
            if (!res)
                return NotFound();

            var id = await _contentService.CreateFeedIntegrationAsync(integration);
            return Ok(id);
        }

        [HttpGet]
        [Route("{feedId}/integrations")]
        [SwaggerOperation($"Lists feed integrations for the specified feed")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The feed integration data", typeof(List<FeedIntegrationModel>))]
        public async Task<IActionResult> ListIntegrations([FromRoute] string feedId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.Read, CurrentUserId))
                return Forbid();

            var data = _contentService.ListFeedIntegrations(feedId, model.Search);
            var models = data.Select(f => _mapper.Map<FeedIntegrationModel>(f));
            return Ok(models);
        }

        [HttpPost]
        [Route("integrations/token")]
        [SwaggerOperation($"Exchanges an authorization code for an access token")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The operation failed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The access token", typeof(string))]
        public async Task<IActionResult> ExchangeFeedIntegrationToken([FromBody] FeedIntegrationTokenModel model)
        {
            var token = await _contentService.ExchangeFeedIntegrationTokenAsync(model.Type, model.AuthorizationCode);
            if (string.IsNullOrEmpty(token))
                return NotFound();

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("integrations/options/{feedIntegrationType}")]
        [SwaggerOperation($"Lists feed integration options for a specific feed integration type")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The options", typeof(List<string>))]
        public async Task<IActionResult> ListFeedIntegrationOptions([FromRoute] FeedIntegrationType feedIntegrationType)
        {
            var data = _contentService.ListFeedIntegrationOptions(feedIntegrationType);
            return Ok(data);
        }

        [HttpDelete]
        [Route("integrations/{id}")]
        [SwaggerOperation($"Removes a feed integration")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to remove integrations from the feed")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"No integration was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The feed integration was removed")]
        public async Task<IActionResult> DeleteIntegration([FromRoute] string id)
        {
            var integration = _contentService.GetFeedIntegration(id);
            if (integration == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(integration.FeedId, Permission.Manage, CurrentUserId))
                return Forbid();

            await _contentService.DeleteIntegrationAsync(integration);
            return Ok();
        }

        [HttpPost]
        [Route("report/contentEntity/{id}")]
        [SwaggerOperation("Reports a content entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Successfully reported a post")]
        public async Task<IActionResult> ReportContentEntity([SwaggerParameter("The id of the content entity")][FromRoute] string id)
        {
            return Ok();
        }

        [HttpPost]
        [Route("report/comment/{id}")]
        [SwaggerOperation("Reports a content entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Successfully reported a comment")]
        public async Task<IActionResult> ReportComment([SwaggerParameter("The id of the comment")][FromRoute] string id)
        {
            return Ok();
        }
    }
}