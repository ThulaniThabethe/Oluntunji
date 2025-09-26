using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class OrderListViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string CustomerName { get; set; }
        public int ItemCount { get; set; }
    }

    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderItemViewModel
    {
        public int OrderItemId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string BookImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string SellerName { get; set; }
    }

    public class OrderStatusUpdateViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public OrderStatus NewStatus { get; set; }

        public string Notes { get; set; }
    }

    public class OrderFilterViewModel
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }
    }

    public class OrderStatisticsViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalBooksSold { get; set; }
    }

    public class CustomerOrderHistoryViewModel
    {
        public List<OrderListViewModel> Orders { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
        public string MostPurchasedCategory { get; set; }
    }

    public class SellerOrderSummaryViewModel
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int BooksSold { get; set; }
        public List<OrderListViewModel> RecentOrders { get; set; }
    }

    public class DeliveryViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingProvince { get; set; }
        public string ShippingPostalCode { get; set; }
        public string TrackingNumber { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<DeliveryItemViewModel> Items { get; set; }
    }

    public class DeliveryItemViewModel
    {
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string SellerName { get; set; }
    }

    public class DeliveryUpdateViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string TrackingNumber { get; set; }

        [Required]
        public DateTime ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        [StringLength(1000)]
        public string DeliveryNotes { get; set; }

        public bool MarkAsDelivered { get; set; }
    }

    public class DeliveryListViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingProvince { get; set; }
        public string ShippingPostalCode { get; set; }
        public string OrderStatus { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DeliveryStatisticsViewModel
    {
        public int TotalDeliveries { get; set; }
        public int PendingShipments { get; set; }
        public int InTransit { get; set; }
        public int Delivered { get; set; }
        public int FailedDeliveries { get; set; }
        public decimal TotalDeliveryRevenue { get; set; }
        public double AverageDeliveryTime { get; set; } // in days
    }
}