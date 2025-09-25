using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } // info, warning, success, error

        [StringLength(20)]
        public string Priority { get; set; } // low, normal, high, urgent

        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string RelatedLink { get; set; } // Optional link to related content

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Success,
        Error,
        Order,
        Payment,
        System,
        Marketing
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }
}