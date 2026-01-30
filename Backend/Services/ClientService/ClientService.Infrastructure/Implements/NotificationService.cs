using System.Collections.Concurrent;
using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Interfaces.NotificationServices;
using ClientService.Application.Notifications;
using ClientService.Application.Notifications.Queries.GetNotifications;
using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of INotificationService
/// </summary>
public class NotificationService : INotificationService
{
    private static readonly TimeSpan AggregateWindow = TimeSpan.FromSeconds(30);
    private const string SystemUser = "System";
    private static readonly ConcurrentDictionary<string, DebouncedNotificationState> PendingNotifications = new();

    private readonly ICommandRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHub _notificationHub;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    public NotificationService(
        ICommandRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork,
        INotificationHub notificationHub,
        IIdentityService identityService)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _notificationHub = notificationHub;
        _identityService = identityService;
    }

    /// <summary>
    /// Create or update submission notification (aggregate within a short window)
    /// </summary>
    public async Task CreateSubmissionNotificationAsync(Guid userId, Guid formId, string formTitle, Guid submissionId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var windowStart = now - AggregateWindow;

        var existing = await _notificationRepository
            .Find(n => n!.UserId == userId
                       && n.FormId == formId
                       && n.IsActive
                       && !n.IsRead
                       && n.LastSubmissionAt >= windowStart, cancellationToken: cancellationToken)
            .OrderByDescending(n => n!.LastSubmissionAt)
            .FirstOrDefaultAsync(cancellationToken);

        Notification notification;

        if (existing != null)
        {
            existing.Count += 1;
            existing.LastSubmissionAt = now;
            existing.LatestSubmissionId = submissionId;
            existing.Message = BuildMessage(formTitle, existing.Count);
            existing.UpdatedAt = now;
            existing.UpdatedBy = SystemUser;

            _notificationRepository.Update(existing, SystemUser);
            notification = existing;
        }
        else
        {
            notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FormId = formId,
                LatestSubmissionId = submissionId,
                Message = BuildMessage(formTitle, 1),
                Count = 1,
                FirstSubmissionAt = now,
                LastSubmissionAt = now,
                IsRead = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = SystemUser,
                UpdatedBy = SystemUser
            };

            await _notificationRepository.AddAsync(notification, SystemUser);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        SchedulePublish(userId, formId, new NotificationEvent
        {
            NotificationId = notification.Id,
            FormId = notification.FormId,
            LatestSubmissionId = notification.LatestSubmissionId,
            Message = notification.Message,
            Count = notification.Count,
            OccurredAt = notification.LastSubmissionAt ?? now
        });
    }

    /// <summary>
    /// Get notifications for current user
    /// </summary>
    public async Task<GetNotificationsQueryResponse> GetNotificationsAsync(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetNotificationsQueryResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var notifications = await _notificationRepository
                .Find(n => n!.UserId == currentUser.UserId && n.IsActive, cancellationToken: cancellationToken)
                .OrderByDescending(n => n!.LastSubmissionAt ?? n!.CreatedAt)
                .ToListAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Notifications retrieved successfully.");
            response.Response = notifications.Select(n => new NotificationResponseEntity
            {
                Id = n!.Id,
                FormId = n.FormId,
                LatestSubmissionId = n.LatestSubmissionId,
                Message = n.Message,
                Count = n.Count,
                FirstSubmissionAt = n.FirstSubmissionAt,
                LastSubmissionAt = n.LastSubmissionAt,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving notifications: {ex.Message}");
            return response;
        }
    }

    private void SchedulePublish(Guid userId, Guid formId, NotificationEvent notificationEvent)
    {
        var key = $"{userId:N}:{formId:N}";
        var state = PendingNotifications.GetOrAdd(key, _ => new DebouncedNotificationState());
        var shouldSchedule = false;

        lock (state.Sync)
        {
            state.Latest = notificationEvent;
            if (!state.IsScheduled)
            {
                state.IsScheduled = true;
                shouldSchedule = true;
            }
        }

        if (!shouldSchedule)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(AggregateWindow);

            NotificationEvent? payload;
            lock (state.Sync)
            {
                payload = state.Latest;
                state.IsScheduled = false;
            }

            if (payload != null)
            {
                _notificationHub.Publish(userId, payload);
            }

            PendingNotifications.TryRemove(key, out _);
        });
    }

    private static string BuildMessage(string formTitle, int count)
    {
        return count <= 1
            ? $"New submission for form '{formTitle}'"
            : $"{count} new submissions for form '{formTitle}'";
    }

    private sealed class DebouncedNotificationState
    {
        public object Sync { get; } = new();
        public bool IsScheduled { get; set; }
        public NotificationEvent? Latest { get; set; }
    }
}
