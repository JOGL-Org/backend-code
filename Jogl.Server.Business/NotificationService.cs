using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Notifications;

namespace Jogl.Server.Business
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IOnboardingQuestionnaireInstanceRepository _onboardingQuestionnaireInstanceRepository;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly INotificationFacade _notificationFacade;

        public NotificationService(INotificationRepository notificationRepository, IUserRepository userRepository, IMembershipRepository membershipRepository, IOnboardingQuestionnaireInstanceRepository onboardingQuestionnaireInstanceRepository, ICommunityEntityService communityEntityService, INotificationFacade notificationFacade)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _onboardingQuestionnaireInstanceRepository = onboardingQuestionnaireInstanceRepository;
            _communityEntityService = communityEntityService;
            _notificationFacade = notificationFacade;
        }

        private async Task NotifyAsync(Notification notification)
        {
            await _notificationRepository.CreateAsync(notification);
        }

        private async Task NotifyAsync(List<Notification> notifications)
        {
            if (!notifications.Any())
                return;

            await _notificationRepository.CreateAsync(notifications);
        }

        public ListPage<Notification> ListSince(string userId, DateTime? dateTimeUTC, int page, int pageSize)
        {
            var total = _notificationRepository.Count(n => n.UserId == userId && n.CreatedUTC >= dateTimeUTC && !n.Deleted);
            var notifications = _notificationRepository.List(n => n.UserId == userId && n.CreatedUTC >= dateTimeUTC && !n.Deleted, page, pageSize, SortKey.CreatedDate);
            foreach (var notification in notifications)
            {
                switch (notification.Type)
                {
                    case NotificationType.AdminRequest:
                        {
                            var userDataItem = notification.Data.SingleOrDefault(d => d.Key == NotificationDataKey.User);
                            var communityEntityDataItem = notification.Data.SingleOrDefault(d => d.Key == NotificationDataKey.CommunityEntity);
                            if (communityEntityDataItem == null || userDataItem == null)
                                break;

                            var answers = _onboardingQuestionnaireInstanceRepository.Get(r => r.CommunityEntityId == communityEntityDataItem.EntityId && r.UserId == userDataItem.EntityId && !r.Deleted);
                            communityEntityDataItem.EntityOnboardingAnswersAvailable = answers != null;
                            break;
                        }
                    case NotificationType.Invite:
                        {
                            var communityEntityDataItem = notification.Data.SingleOrDefault(d => d.Key == NotificationDataKey.CommunityEntity);
                            if (communityEntityDataItem == null)
                                break;

                            communityEntityDataItem.CommunityEntityOnboarding = _communityEntityService.Get(communityEntityDataItem.EntityId)?.Onboarding;
                            break;
                        }
                    default:
                        break;
                }
            }

            return new ListPage<Notification>(notifications, (int)total);
        }

        public Notification Get(string notificationId)
        {
            return _notificationRepository.Get(notificationId);
        }

        public async Task UpdateAsync(Notification notification)
        {
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task NotifyAccessLevelChangedAsync(Membership membership)
        {
            var communityEntity = _communityEntityService.Get(membership.CommunityEntityId, membership.CommunityEntityType);

            await NotifyAsync(new Notification
            {
                CreatedUTC = DateTime.UtcNow,
                CreatedByUserId = membership.UpdatedByUserId,
                Type = NotificationType.AdminAccessLevel,
                UserId = membership.UserId,
                OriginFeedId = membership.CommunityEntityId,
                Data = new List<NotificationData>
                {
                    new NotificationData{ Key = NotificationDataKey.Role, EntityTitle = membership.AccessLevel.ToString()   },
                    new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = membership.CommunityEntityId, EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityBannerId = communityEntity.BannerId, EntityLogoId = communityEntity.LogoId}
                },
                Text = GetNotificationText(NotificationType.AdminAccessLevel, membership, communityEntity)
            });
        }

        public async Task NotifyCommunityEntityInviteCreatedAsync(CommunityEntityInvitation invitation)
        {
            var adminMemberships = _membershipRepository.List(m => m.CommunityEntityId == invitation.TargetCommunityEntityId && (m.AccessLevel == AccessLevel.Admin || m.AccessLevel == AccessLevel.Owner) && !m.Deleted);
            var sourceCommunityEntity = _communityEntityService.Get(invitation.SourceCommunityEntityId, invitation.SourceCommunityEntityType);
            var targetCommunityEntity = _communityEntityService.Get(invitation.TargetCommunityEntityId, invitation.TargetCommunityEntityType);

            foreach (var adminMembership in adminMemberships)
            {
                await NotifyAsync(new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = invitation.CreatedByUserId,
                    Type = NotificationType.AdminCommunityEntityInvite,
                    UserId = adminMembership.UserId,
                    OriginFeedId = invitation.TargetCommunityEntityId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.Invitation, EntityId = invitation.Id.ToString()},
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = sourceCommunityEntity.Id.ToString(), EntityTitle = sourceCommunityEntity.Title, CommunityEntityType = sourceCommunityEntity.Type, EntityLogoId = sourceCommunityEntity.LogoId, EntityBannerId = sourceCommunityEntity.BannerId},
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = targetCommunityEntity.Id.ToString(), EntityTitle = targetCommunityEntity.Title, CommunityEntityType = targetCommunityEntity.Type, EntityLogoId = targetCommunityEntity.LogoId, EntityBannerId = targetCommunityEntity.BannerId}
                    },
                    Text = GetNotificationText(NotificationType.AdminCommunityEntityInvite, invitation, sourceCommunityEntity, targetCommunityEntity)
                });
            }
        }

        public async Task NotifyCommunityEntityInviteCreatedWithdrawAsync(CommunityEntityInvitation invitation)
        {
            var notifications = _notificationRepository.List(n => !n.Deleted && !n.Actioned && n.Type == NotificationType.AdminCommunityEntityInvite && n.Data.Any(d => d.Key == NotificationDataKey.Invitation && d.EntityId == invitation.Id.ToString()));

            foreach (var notification in notifications)
            {
                notification.Actioned = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task NotifyInviteCreatedAsync(Invitation invitation, User user)
        {
            var communityEntity = _communityEntityService.Get(invitation.CommunityEntityId, invitation.CommunityEntityType);

            await NotifyAsync(new Notification
            {
                CreatedUTC = DateTime.UtcNow,
                CreatedByUserId = invitation.CreatedByUserId,
                Type = NotificationType.Invite,
                UserId = invitation.InviteeUserId,
                OriginFeedId = invitation.CommunityEntityId,
                Data = new List<NotificationData>
                {
                    new NotificationData{ Key = NotificationDataKey.Invitation, EntityId = invitation.Id.ToString(), EntitySubtype= invitation.AccessLevel.ToString().ToLower()},
                    new NotificationData{ Key = NotificationDataKey.User, EntityId = user.Id.ToString(), EntityTitle = user.FeedTitle },
                    new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = invitation.CommunityEntityId, EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityLogoId = communityEntity.LogoId, EntityBannerId = communityEntity.BannerId, EntityHomeChannelId = communityEntity.HomeChannelId}
                },
                Text = GetNotificationText(NotificationType.Invite, invitation, user, communityEntity)
            });
        }

        public async Task NotifyInvitesCreatedAsync(IEnumerable<Invitation> invitations)
        {
            foreach (var grp in invitations.GroupBy(i => i.Entity))
            {
                var communityEntity = grp.Key;

                await NotifyAsync(invitations.Select(i => new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = i.CreatedByUserId,
                    Type = NotificationType.Invite,
                    UserId = i.InviteeUserId,
                    OriginFeedId = i.CommunityEntityId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.Invitation, EntityId = i.Id.ToString(), EntitySubtype= i.AccessLevel.ToString().ToLower()},
                        new NotificationData{ Key = NotificationDataKey.User, EntityId = i.CreatedBy.Id.ToString(), EntityTitle = i.CreatedBy.FeedTitle },
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntity.Id.ToString(), EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityLogoId = communityEntity.LogoId, EntityBannerId = communityEntity.BannerId, EntityHomeChannelId = communityEntity.HomeChannelId}
                    },
                    Text = GetNotificationText(NotificationType.Invite, i, i.CreatedBy, communityEntity)
                }).ToList());
            }
        }

        public async Task NotifyEventInviteCreatedAsync(Event ev, CommunityEntity communityEntity, User user, IEnumerable<EventAttendance> invitations)
        {
            await NotifyAsync(invitations.Select(i => new Notification
            {
                CreatedUTC = DateTime.UtcNow,
                CreatedByUserId = i.CreatedByUserId,
                Type = NotificationType.EventInvite,
                UserId = i.UserId,
                OriginFeedId = ev.Id.ToString(),
                Data = new List<NotificationData>
                {
                    new NotificationData{ Key = NotificationDataKey.Invitation, EntityId = i.Id.ToString(), EntitySubtype= i.AccessLevel.ToString().ToLower()},
                    new NotificationData{ Key = NotificationDataKey.FeedEntity, EntityId = ev.Id.ToString(),EntityTitle = ev.Title},
                    new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntity.Id.ToString(), EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityLogoId = communityEntity.LogoId, EntityBannerId = communityEntity.BannerId, EntityHomeChannelId = communityEntity.HomeChannelId},
                    new NotificationData{ Key = NotificationDataKey.User, EntityId = user.Id.ToString(), EntityTitle = user.FeedTitle}
                },
                Text = GetNotificationText(NotificationType.EventInvite, i, ev, communityEntity)
            }).ToList());
        }

        public async Task NotifyEventInviteUpdatedAsync(Event ev, CommunityEntity communityEntity, User user, IEnumerable<EventAttendance> invitations)
        {
            await NotifyAsync(invitations.Select(i => new Notification
            {
                CreatedUTC = DateTime.UtcNow,
                CreatedByUserId = i.UpdatedByUserId,
                Type = NotificationType.EventInviteUpdate,
                UserId = i.UserId,
                OriginFeedId = ev.Id.ToString(),
                Data = new List<NotificationData>
                {
                    new NotificationData{ Key = NotificationDataKey.Invitation, EntityId = i.Id.ToString(), EntitySubtype= i.AccessLevel.ToString().ToLower()},
                    new NotificationData{ Key = NotificationDataKey.FeedEntity, EntityId = ev.Id.ToString(),EntityTitle = ev.Title},
                    new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntity.Id.ToString(), EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityLogoId = communityEntity.LogoId, EntityBannerId = communityEntity.BannerId},
                    new NotificationData{ Key = NotificationDataKey.User, EntityId = user.Id.ToString(), EntityTitle = user.FeedTitle}
                },
                Text = GetNotificationText(NotificationType.EventInviteUpdate, i, ev, communityEntity)
            }).ToList());
        }

        public async Task NotifyInviteWithdrawAsync(Invitation invitation)
        {
            var notifications = _notificationRepository.List(n => !n.Deleted && !n.Actioned && n.Type == NotificationType.Invite && n.Data.Any(d => d.Key == NotificationDataKey.Invitation && d.EntityId == invitation.Id.ToString()));

            foreach (var notification in notifications)
            {
                notification.Actioned = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task NotifyInvitesWithdrawAsync(IEnumerable<Invitation> invitations)
        {
            var invitationIds = invitations.Select(i => i.Id.ToString());
            var notifications = _notificationRepository.List(n => !n.Deleted && !n.Actioned && n.Type == NotificationType.Invite && n.Data.Any(d => d.Key == NotificationDataKey.Invitation && invitationIds.Contains(d.EntityId)));

            foreach (var notification in notifications)
            {
                notification.Actioned = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task NotifyEventInviteWithdrawAsync(EventAttendance invitation)
        {
            var notifications = _notificationRepository.List(n => !n.Deleted && !n.Actioned && (n.Type == NotificationType.EventInviteUpdate || n.Type == NotificationType.EventInvite) && n.Data.Any(d => d.Key == NotificationDataKey.Invitation && d.EntityId == invitation.Id.ToString()));

            foreach (var notification in notifications)
            {
                notification.Actioned = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task NotifyEventInvitesWithdrawAsync(IEnumerable<EventAttendance> invitations)
        {
            var invitationIds = invitations.Select(i => i.Id.ToString());
            var notifications = _notificationRepository.List(n => !n.Deleted && !n.Actioned && (n.Type == NotificationType.EventInviteUpdate || n.Type == NotificationType.EventInvite) && n.Data.Any(d => d.Key == NotificationDataKey.Invitation && invitationIds.Contains(d.EntityId)));

            foreach (var notification in notifications)
            {
                notification.Actioned = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task NotifyMemberJoinedAsync(Membership membership)
        {
            var user = _userRepository.Get(membership.UserId);
            var memberships = _membershipRepository.List(m => m.CommunityEntityId == membership.CommunityEntityId && !m.Deleted);
            var communityEntity = _communityEntityService.Get(membership.CommunityEntityId, membership.CommunityEntityType);

            foreach (var member in memberships)
            {
                if (member.UserId == membership.UserId)
                    continue;

                await NotifyAsync(new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = membership.UserId,
                    Type = member.AccessLevel == AccessLevel.Member ? NotificationType.Member : NotificationType.AdminMember,
                    UserId = member.UserId,
                    OriginFeedId = membership.CommunityEntityId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.User, EntityId = membership.UserId, EntityTitle = user.FirstName + " " + user.LastName},
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = membership.CommunityEntityId, EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityBannerId = communityEntity.BannerId, EntityLogoId = communityEntity.LogoId}
                    },
                    Text = GetNotificationText(member.AccessLevel == AccessLevel.Member ? NotificationType.Member : NotificationType.AdminMember, user, communityEntity)
                });
            }
        }

        public async Task NotifyCommunityEntityJoinedAsync(Relation relation)
        {
            var communityEntitySource = _communityEntityService.Get(relation.SourceCommunityEntityId, relation.SourceCommunityEntityType);
            var communityEntityTarget = _communityEntityService.Get(relation.TargetCommunityEntityId, relation.TargetCommunityEntityType);
            var sourceMemberships = _membershipRepository.List(m => m.CommunityEntityId == relation.SourceCommunityEntityId && !m.Deleted);
            var targetMemberships = _membershipRepository.List(m => m.CommunityEntityId == relation.TargetCommunityEntityId && !m.Deleted);

            foreach (var member in sourceMemberships)
            {
                await NotifyAsync(new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = relation.CreatedByUserId,
                    Type = member.AccessLevel == AccessLevel.Member ? NotificationType.Relation : NotificationType.AdminRelation,
                    UserId = member.UserId,
                    OriginFeedId = relation.TargetCommunityEntityId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntitySource.Id.ToString(), EntityTitle = communityEntitySource.Title, CommunityEntityType = communityEntitySource.Type, EntityBannerId = communityEntitySource.BannerId, EntityLogoId = communityEntitySource.LogoId},
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntityTarget.Id.ToString(), EntityTitle = communityEntityTarget.Title, CommunityEntityType = communityEntityTarget.Type, EntityBannerId = communityEntityTarget.BannerId, EntityLogoId = communityEntityTarget.LogoId}
                    },
                    Text = GetNotificationText(member.AccessLevel == AccessLevel.Member ? NotificationType.Relation : NotificationType.AdminRelation, communityEntitySource, communityEntityTarget)
                });
            }

            foreach (var member in targetMemberships)
            {
                //skip members we've already notified via source memberships 
                if (sourceMemberships.Any(m => m.UserId == member.UserId))
                    continue;

                await NotifyAsync(new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = relation.CreatedByUserId,
                    Type = member.AccessLevel == AccessLevel.Member ? NotificationType.Relation : NotificationType.AdminRelation,
                    UserId = member.UserId,
                    OriginFeedId = relation.SourceCommunityEntityId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntitySource.Id.ToString(), EntityTitle = communityEntitySource.Title, CommunityEntityType = communityEntitySource.Type},
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntityTarget.Id.ToString(), EntityTitle = communityEntityTarget.Title, CommunityEntityType = communityEntityTarget.Type}
                    },
                    Text = GetNotificationText(member.AccessLevel == AccessLevel.Member ? NotificationType.Relation : NotificationType.AdminRelation, communityEntitySource, communityEntityTarget)
                });
            }
        }

        public async Task NotifyNeedCreatedAsync(Need need)
        {
            var memberships = _membershipRepository.List(m => m.CommunityEntityId == need.EntityId && !m.Deleted);
            var communityEntity = _communityEntityService.Get(need.EntityId);
            var user = _userRepository.Get(need.CreatedByUserId);

            foreach (var member in memberships)
            {
                if (member.UserId == need.CreatedByUserId)
                    continue;

                await NotifyAsync(new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = need.CreatedByUserId,
                    Type = NotificationType.Need,
                    UserId = member.UserId,
                    OriginFeedId = need.EntityId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.User, EntityId = user.Id.ToString(), EntityTitle = user.FirstName + " " + user.LastName },
                        new NotificationData{ Key = NotificationDataKey.ContentEntity, EntityId = need.Id.ToString(), EntityTitle = need.Title },
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntity.Id.ToString(), EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityBannerId = communityEntity.BannerId, EntityLogoId = communityEntity.LogoId }
                    },
                    Text = GetNotificationText(NotificationType.Need, user, communityEntity)
                });
            }
        }

        public async Task NotifyResourceCreatedAsync(Resource resource)
        {
            var memberships = _membershipRepository.List(m => m.CommunityEntityId == resource.FeedId && !m.Deleted);
            var communityEntity = _communityEntityService.Get(resource.FeedId);
            var user = _userRepository.Get(resource.CreatedByUserId);

            foreach (var member in memberships)
            {
                if (member.UserId == resource.CreatedByUserId)
                    continue;

                await NotifyAsync(new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = resource.CreatedByUserId,
                    Type = NotificationType.Resource,
                    UserId = member.UserId,
                    OriginFeedId = resource.FeedId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.ContentEntity, EntityId = resource.Id.ToString(), EntityTitle = resource.Title },
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntity.Id.ToString(), EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type , EntityBannerId = communityEntity.BannerId, EntityLogoId = communityEntity.LogoId}
                    },
                    Text = GetNotificationText(NotificationType.Resource, user, communityEntity),
                });
            }
        }

        public async Task NotifyRequestAcceptedAsync(Invitation invitation)
        {
            var communityEntity = _communityEntityService.Get(invitation.CommunityEntityId, invitation.CommunityEntityType);

            await NotifyAsync(new Notification
            {
                CreatedUTC = DateTime.UtcNow,
                CreatedByUserId = invitation.UpdatedByUserId,
                Type = NotificationType.Acceptation,
                UserId = invitation.InviteeUserId,
                OriginFeedId = invitation.CommunityEntityId,
                Data = new List<NotificationData>
                {
                    new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = invitation.CommunityEntityId, EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityBannerId = communityEntity.BannerId, EntityLogoId = communityEntity.LogoId}
                },
                Text = GetNotificationText(NotificationType.Acceptation, communityEntity),
            });
        }

        public async Task NotifyRequestCreatedAsync(Invitation invitation)
        {
            var adminMemberships = _membershipRepository.List(m => m.CommunityEntityId == invitation.CommunityEntityId && (m.AccessLevel == AccessLevel.Admin || m.AccessLevel == AccessLevel.Owner) && !m.Deleted);
            var user = _userRepository.Get(invitation.InviteeUserId);
            var communityEntity = _communityEntityService.Get(invitation.CommunityEntityId, invitation.CommunityEntityType);

            foreach (var adminMembership in adminMemberships)
            {
                await NotifyAsync(new Notification
                {
                    CreatedUTC = DateTime.UtcNow,
                    CreatedByUserId = invitation.CreatedByUserId,
                    Type = NotificationType.AdminRequest,
                    UserId = adminMembership.UserId,
                    OriginFeedId = invitation.CommunityEntityId,
                    Data = new List<NotificationData>
                    {
                        new NotificationData{ Key = NotificationDataKey.Invitation, EntityId = invitation.Id.ToString()},
                        new NotificationData{ Key = NotificationDataKey.User, EntityId = user.Id.ToString(), EntityTitle = user.FirstName + " " +user.LastName},
                        new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = invitation.CommunityEntityId, EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityBannerId = communityEntity.BannerId, EntityLogoId = communityEntity.LogoId}
                    },
                    Text = GetNotificationText(NotificationType.AdminRequest, invitation, user, communityEntity),
                });
            }
        }

        public async Task NotifyRequestsCreatedAsync(IEnumerable<Invitation> invitations)
        {
            foreach (var grp in invitations.GroupBy(i => i.Entity))
            {
                var communityEntity = grp.Key;
                var adminMemberships = _membershipRepository.List(m => m.CommunityEntityId == communityEntity.Id.ToString() && (m.AccessLevel == AccessLevel.Admin || m.AccessLevel == AccessLevel.Owner) && !m.Deleted);

                foreach (var adminMembership in adminMemberships)
                {
                    await NotifyAsync(invitations.Select(i => new Notification
                    {
                        CreatedUTC = DateTime.UtcNow,
                        CreatedByUserId = i.CreatedByUserId,
                        Type = NotificationType.AdminRequest,
                        UserId = adminMembership.UserId,
                        OriginFeedId = i.CommunityEntityId,
                        Data = new List<NotificationData>
                        {
                            new NotificationData{ Key = NotificationDataKey.Invitation, EntityId = i.Id.ToString()},
                            new NotificationData{ Key = NotificationDataKey.User, EntityId = i.InviteeUserId, EntityTitle = i.User.FirstName + " " + i.User.LastName},
                            new NotificationData{ Key = NotificationDataKey.CommunityEntity, EntityId = communityEntity.Id.ToString(), EntityTitle = communityEntity.Title, CommunityEntityType = communityEntity.Type, EntityBannerId = communityEntity.BannerId, EntityLogoId = communityEntity.LogoId}
                        },
                        Text = GetNotificationText(NotificationType.AdminRequest, i, i.User, communityEntity),
                    }).ToList());
                }
            }
        }

        public async Task NotifyRequestCreatedWithdrawAsync(Invitation invitation)
        {
            var notifications = _notificationRepository.List(n => !n.Deleted && !n.Actioned && n.Type == NotificationType.AdminRequest && n.Data.Any(d => d.Key == NotificationDataKey.Invitation && d.EntityId == invitation.Id.ToString()));

            foreach (var notification in notifications)
            {
                notification.Actioned = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task NotifyUserFollowedAsync(UserFollowing following)
        {
            var user = _userRepository.Get(following.UserIdFrom);

            await NotifyAsync(new Notification
            {
                CreatedUTC = DateTime.UtcNow,
                CreatedByUserId = following.CreatedByUserId,
                Type = NotificationType.Follower,
                UserId = following.UserIdTo,
                OriginFeedId = following.UserIdFrom,
                Data = new List<NotificationData>
                {
                    new NotificationData{ Key = NotificationDataKey.User, EntityId = following.UserIdFrom, EntityTitle = user.FirstName + " " +user.LastName}
                },
                Text = GetNotificationText(NotificationType.Follower, user),
            });
        }

        public string GetNotificationText(NotificationType type, params Entity[] data)
        {
            Invitation invitation;
            User user;
            Paper paper;
            Membership membership;
            FeedEntity feedEntity;
            CommunityEntity communityEntity;
            CommunityEntity communityEntitySource;
            CommunityEntity communityEntityTarget;
            CommunityEntityInvitation communityEntityInvitation;

            EventAttendance eventAttendance;
            Event ev;

            switch (type)
            {
                case NotificationType.Follower:
                    user = (User)data[0];
                    return $"{GetEntityTitle(user)} now follows you";
                case NotificationType.Need:
                    user = (User)data[0];
                    communityEntity = (CommunityEntity)data[1];
                    return $"{GetEntityTitle(user)} created a need in {GetEntityTitle(communityEntity)}";
                case NotificationType.Resource:
                    user = (User)data[0];
                    communityEntity = (CommunityEntity)data[1];
                    return $"{GetEntityTitle(user)} created a resource in {GetEntityTitle(communityEntity)}";
                case NotificationType.Paper:
                    user = (User)data[0];
                    paper = (Paper)data[1];
                    feedEntity = (FeedEntity)data[2];
                    return $"A paper {GetEntityTitle(paper)} was added in {GetEntityTitle(feedEntity)} by {GetEntityTitle(user)}";

                //membership
                case NotificationType.AdminAccessLevel:
                    membership = (Membership)data[0];
                    communityEntity = (CommunityEntity)data[1];
                    return $"You are now an {GetAccessLevelString(membership.AccessLevel)} of {GetEntityTitle(communityEntity)}";
                case NotificationType.AdminMember:
                case NotificationType.Member:
                    user = (User)data[0];
                    communityEntity = (CommunityEntity)data[1];
                    return $"{GetEntityTitle(user)} is now a member of {GetEntityType(communityEntity)} {GetEntityTitle(communityEntity)}";
                case NotificationType.Invite:
                    invitation = (Invitation)data[0];
                    user = (User)data[1];
                    communityEntity = (CommunityEntity)data[2];
                    return $"{GetEntityTitle(user)} is inviting you to be a {GetAccessLevelString(invitation.AccessLevel)}";
                case NotificationType.AdminRequest:
                    invitation = (Invitation)data[0];
                    user = (User)data[1];
                    communityEntity = (CommunityEntity)data[2];
                    return $"{GetEntityTitle(user)} is sending a membership request";
                case NotificationType.Acceptation:
                    communityEntity = (CommunityEntity)data[0];
                    return $"Your request to join {GetEntityTitle(communityEntity)} has been accepted";

                //events
                case NotificationType.EventInvite:
                    eventAttendance = (EventAttendance)data[0];
                    ev = (Event)data[1];
                    communityEntity = (CommunityEntity)data[2];
                    return $"You have been invited to an event: {GetEntityTitle(ev)}";
                case NotificationType.EventInviteUpdate:
                    eventAttendance = (EventAttendance)data[0];
                    ev = (Event)data[1];
                    communityEntity = (CommunityEntity)data[2];
                    return $"Your event invitation has been updated: {GetEntityTitle(ev)}";

                //linking
                case NotificationType.AdminCommunityEntityInvite:
                    communityEntityInvitation = (CommunityEntityInvitation)data[0];
                    communityEntitySource = (CommunityEntity)data[1];
                    communityEntityTarget = (CommunityEntity)data[2];
                    return $"{GetEntityType(communityEntitySource)} {GetEntityTitle(communityEntitySource)} is requesting to link with {GetEntityType(communityEntityTarget)} {GetEntityTitle(communityEntityTarget)}";
                case NotificationType.AdminRelation:
                case NotificationType.Relation:
                    communityEntitySource = (CommunityEntity)data[0];
                    communityEntityTarget = (CommunityEntity)data[1];
                    return $"{GetEntityType(communityEntitySource)} {GetEntityTitle(communityEntitySource)} is now linked with {GetEntityType(communityEntityTarget)} {GetEntityTitle(communityEntityTarget)}";

                default:
                    throw new Exception();
            }
        }

        private string GetAccessLevelString(AccessLevel level)
        {
            switch (level)
            {
                case AccessLevel.Member:
                    return "a <b>member</b>";
                default:
                    return $"an <b>{level.ToString().ToLower()}</b>";
            }
        }

        private string GetEntityTitle(FeedEntity feedEntity)
        {
            if (string.IsNullOrEmpty(feedEntity.FeedTitle))
                return "<b>Untitled</b>";

            return $"<b>{feedEntity.FeedTitle}</b>";
        }

        private string GetEntityType(FeedEntity feedEntity)
        {
            return $"<b>{feedEntity.FeedType.ToString().ToLower()}</b>";
        }

        //public async Task NotifyCommentPostedAsync(Comment comment)
        //{
        //    var contentEntity = _contentEntityRepository.Get(comment.ContentEntityId);

        //    await NotifyAsync(new Notification
        //    {
        //        CreatedUTC = DateTime.UtcNow,
        //        Type = NotificationType.Comment,
        //        UserId = contentEntity.CreatedByUserId,
        //        Data = new List<NotificationData>
        //        {
        //            new NotificationData { Key = NotificationDataKey.ContentEntity, EntityId = contentEntity.Id.ToString(), EntityTitle = contentEntity.Title, ContentEntityType = contentEntity.Type },
        //            GetFeedEntityData(contentEntity.FeedId),
        //        }
        //    });
        //}

        //public async Task NotifyMentionAsync(string userId, Comment comment)
        //{
        //    var contentEntity = _contentEntityRepository.Get(comment.ContentEntityId);
        //    var mentionerUser = _userRepository.Get(contentEntity.CreatedByUserId);

        //    await NotifyAsync(new Notification
        //    {
        //        CreatedUTC = DateTime.UtcNow,
        //        Type = NotificationType.Mention,
        //        UserId = userId,
        //        Data = new List<NotificationData>
        //        {
        //            new NotificationData { Key = NotificationDataKey.ContentEntity, EntityId = contentEntity.Id.ToString(), EntityTitle = contentEntity.Title, ContentEntityType = contentEntity.Type },
        //            new NotificationData { Key = NotificationDataKey.User, EntityId = mentionerUser.Id.ToString(), EntityTitle = mentionerUser.FirstName + " " + mentionerUser.LastName},
        //            GetFeedEntityData(contentEntity.FeedId),
        //        }
        //    });
        //}

        //public async Task NotifyMentionAsync(string userId, ContentEntity contentEntity)
        //{
        //    var mentionerUser = _userRepository.Get(contentEntity.CreatedByUserId);

        //    await NotifyAsync(new Notification
        //    {
        //        CreatedUTC = DateTime.UtcNow,
        //        Type = NotificationType.Mention,
        //        UserId = userId,
        //        Data = new List<NotificationData>
        //        {
        //            new NotificationData { Key = NotificationDataKey.ContentEntity, EntityId = contentEntity.Id.ToString(), EntityTitle = contentEntity.Title, ContentEntityType = contentEntity.Type },
        //            new NotificationData { Key = NotificationDataKey.User, EntityId = mentionerUser.Id.ToString(), EntityTitle = mentionerUser.FirstName + " " + mentionerUser.LastName},
        //            GetFeedEntityData(contentEntity.FeedId),
        //        }
        //    });
        //}

        private NotificationData GetFeedEntityData(Entity feedEntity)
        {
            var communityEntity = feedEntity as CommunityEntity;
            var user = feedEntity as User;
            var contentEntity = feedEntity as ContentEntity;

            var key = NotificationDataKey.User;
            if (communityEntity != null)
                key = NotificationDataKey.CommunityEntity;
            if (contentEntity != null)
                key = NotificationDataKey.ContentEntity;

            return new NotificationData
            {
                Key = key,
                CommunityEntityType = communityEntity?.Type,
                ContentEntityType = contentEntity?.Type,
                EntityLogoId = communityEntity?.LogoId ?? user?.AvatarId,
                EntityId = feedEntity.Id.ToString(),
                EntityTitle = communityEntity?.Title ?? contentEntity.Text ?? user?.FirstName + " " + user?.LastName,
                //EntityOnboardingEnabled = communityEntity?.Onboarding?.Enabled ?? false,
            };
        }
    }
}