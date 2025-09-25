using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        // GET: Notification/Notifications
        public ActionResult Notifications()
        {
            var currentUser = CurrentUser;
            var notifications = Db.Notifications
                .Where(n => n.UserId == currentUser.UserId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();

            var model = new NotificationsListViewModel
            {
                Notifications = notifications.Select(n => new NotificationViewModel
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    Priority = n.Priority,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate,
                    RelatedLink = n.RelatedLink
                }).ToList(),
                TotalNotifications = notifications.Count,
                UnreadCount = notifications.Count(n => !n.IsRead),
                NotificationsByType = notifications
                    .GroupBy(n => n.NotificationType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return View(model);
        }

        // GET: Notification/NotificationSettings
        public ActionResult NotificationSettings()
        {
            var currentUser = CurrentUser;
            var model = new NotificationSettingsViewModel
            {
                EmailNotifications = currentUser.EmailNotifications,
                SmsNotifications = currentUser.SmsNotifications,
                PushNotifications = currentUser.PushNotifications
            };

            return View(model);
        }

        // POST: Notification/NotificationSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotificationSettings(NotificationSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = CurrentUser;
            currentUser.EmailNotifications = model.EmailNotifications;
            currentUser.SmsNotifications = model.SmsNotifications;
            currentUser.PushNotifications = model.PushNotifications;

            Db.SaveChanges();

            TempData["Success"] = "Notification settings updated successfully!";
            return RedirectToAction("Notifications");
        }

        // POST: Notification/MarkAsRead
        [HttpPost]
        public ActionResult MarkAsRead(int notificationId)
        {
            try
            {
                var currentUser = CurrentUser;
                var notification = Db.Notifications
                    .FirstOrDefault(n => n.NotificationId == notificationId && n.UserId == currentUser.UserId);

                if (notification == null)
                {
                    return Json(new { success = false, message = "Notification not found." });
                }

                notification.IsRead = true;
                Db.SaveChanges();

                return Json(new { success = true, message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error marking notification as read: " + ex.Message });
            }
        }

        // POST: Notification/Delete
        [HttpPost]
        public ActionResult Delete(int notificationId)
        {
            try
            {
                var currentUser = CurrentUser;
                var notification = Db.Notifications
                    .FirstOrDefault(n => n.NotificationId == notificationId && n.UserId == currentUser.UserId);

                if (notification == null)
                {
                    return Json(new { success = false, message = "Notification not found." });
                }

                Db.Notifications.Remove(notification);
                Db.SaveChanges();

                return Json(new { success = true, message = "Notification deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting notification: " + ex.Message });
            }
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        public ActionResult MarkAllAsRead()
        {
            try
            {
                var currentUser = CurrentUser;
                var unreadNotifications = Db.Notifications
                    .Where(n => n.UserId == currentUser.UserId && !n.IsRead)
                    .ToList();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                Db.SaveChanges();

                return Json(new { success = true, message = "All notifications marked as read." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error marking all notifications as read: " + ex.Message });
            }
        }

        // POST: Notification/CreateNotification (for internal use by other controllers)
        [HttpPost]
        public ActionResult CreateNotification(int userId, string title, string message, string notificationType, string priority = "normal", string relatedLink = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    NotificationType = notificationType,
                    Priority = priority,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    RelatedLink = relatedLink
                };

                Db.Notifications.Add(notification);
                Db.SaveChanges();

                return Json(new { success = true, notificationId = notification.NotificationId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error creating notification: " + ex.Message });
            }
        }
    }
}