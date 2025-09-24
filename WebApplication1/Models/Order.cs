using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Total Amount (ZAR)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; } // Pending, Processing, Shipped, Delivered, Cancelled

        [StringLength(500)]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        [Display(Name = "Shipping City")]
        public string ShippingCity { get; set; }

        [StringLength(100)]
        [Display(Name = "Shipping Province")]
        public string ShippingProvince { get; set; }

        [StringLength(10)]
        [Display(Name = "Shipping Postal Code")]
        public string ShippingPostalCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } // Credit Card, EFT, Cash on Delivery

        [StringLength(50)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } // Pending, Paid, Failed, Refunded

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }

        [Display(Name = "Shipped Date")]
        public DateTime? ShippedDate { get; set; }

        [Display(Name = "Delivered Date")]
        public DateTime? DeliveredDate { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        // Foreign keys
        public int CustomerId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Unit Price (ZAR)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Display(Name = "Subtotal (ZAR)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }

        // Foreign keys
        public int OrderId { get; set; }
        public int BookId { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }
    }

    public class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
    }

    public class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Failed = "Failed";
        public const string Refunded = "Refunded";
    }
}