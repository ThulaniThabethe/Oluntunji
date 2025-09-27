using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class PrintRequest
    {
        [Key]
        public int PrintRequestId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public string FilePath { get; set; }

        public int PageCount { get; set; }

        public decimal Cost { get; set; }

        public DateTime RequestDate { get; set; }

        public PrintRequestStatus Status { get; set; }

        public DeliveryOrPickup Option { get; set; }
    }

    public enum PrintRequestStatus
    {
        Pending,
        Approved,
        Printed,
        Completed,
        Cancelled
    }

    public enum DeliveryOrPickup
    {
        Delivery,
        Pickup
    }
}