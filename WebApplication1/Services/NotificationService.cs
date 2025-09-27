using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface INotificationService
    {
        void CreateNotificationForUser(int userId, string title, string message, NotificationType type, NotificationPriority priority = NotificationPriority.Normal, string relatedLink = null);
        void CreateNotificationForAllUsers(string title, string message, NotificationType type, NotificationPriority priority = NotificationPriority.Normal, string relatedLink = null);
        void CreateNotificationForRole(string role, string title, string message, NotificationType type, NotificationPriority priority = NotificationPriority.Normal, string relatedLink = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly BookstoreDbContext _context;

        public NotificationService(BookstoreDbContext context)
        {
            _context = context;
        }

        // Synchronous wrapper methods for easier use in controllers
        public void CreateNotificationForUser(int userId, string title, string message, NotificationType type, NotificationPriority priority = NotificationPriority.Normal, string relatedLink = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    NotificationType = type.ToString(),
                    Priority = priority.ToString(),
                    RelatedLink = relatedLink,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the main flow
                System.Diagnostics.Debug.WriteLine($"Error creating notification: {ex.Message}");
            }
        }

        public void CreateNotificationForAllUsers(string title, string message, NotificationType type, NotificationPriority priority = NotificationPriority.Normal, string relatedLink = null)
        {
            try
            {
                var users = _context.Users.ToList();
                foreach (var user in users)
                {
                    var notification = new Notification
                    {
                        UserId = user.UserId,
                        Title = title,
                        Message = message,
                        NotificationType = type.ToString(),
                        Priority = priority.ToString(),
                        RelatedLink = relatedLink,
                        CreatedDate = DateTime.Now,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the main flow
                System.Diagnostics.Debug.WriteLine($"Error creating notifications for all users: {ex.Message}");
            }
        }

        public void CreateNotificationForRole(string role, string title, string message, NotificationType type, NotificationPriority priority = NotificationPriority.Normal, string relatedLink = null)
        {
            try
            {
                var users = _context.Users.Where(u => u.Role == role).ToList();
                foreach (var user in users)
                {
                    var notification = new Notification
                    {
                        UserId = user.UserId,
                        Title = title,
                        Message = message,
                        NotificationType = type.ToString(),
                        Priority = priority.ToString(),
                        RelatedLink = relatedLink,
                        CreatedDate = DateTime.Now,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the main flow
                System.Diagnostics.Debug.WriteLine($"Error creating notifications for role {role}: {ex.Message}");
            }
        }
    }
}