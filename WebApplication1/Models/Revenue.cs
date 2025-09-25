using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public static class PeriodTypeConstants
    {
        public const string Daily = "Daily";
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Yearly = "Yearly";
    }

    public class Revenue
    {
        [Key]
        public int RevenueId { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Total Revenue (ZAR)")]
        [Column(TypeName = "money")]
        public decimal TotalRevenue { get; set; }

        [Required]
        [Display(Name = "Number of Orders")]
        public int NumberOfOrders { get; set; }

        [Required]
        [Display(Name = "Number of Books Sold")]
        public int NumberOfBooksSold { get; set; }

        [Display(Name = "Average Order Value (ZAR)")]
        [Column(TypeName = "money")]
        public decimal AverageOrderValue { get; set; }

        [StringLength(50)]
        [Display(Name = "Period Type")]
        public string PeriodType { get; set; } // Daily, Weekly, Monthly, Yearly

        public int? SellerId { get; set; } // Null for platform-wide revenue

        // Navigation properties
        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; }
    }

    public class RevenueReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalBooksSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenue> DailyRevenues { get; set; }
        public List<CategoryRevenue> CategoryRevenues { get; set; }
        public List<SellerRevenue> SellerRevenues { get; set; }
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class CategoryRevenue
    {
        public string Category { get; set; }
        public decimal Revenue { get; set; }
        public int BooksSold { get; set; }
    }

    public class SellerRevenue
    {
        public string SellerName { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}