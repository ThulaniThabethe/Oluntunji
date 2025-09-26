using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class RevenueDashboardViewModel
    {
        public List<Revenue> DailyRevenue { get; set; }
        public List<Revenue> MonthlyRevenue { get; set; }
        public List<Revenue> YearlyRevenue { get; set; }
        public List<CategoryRevenueData> CategoryRevenue { get; set; }
        public List<SellerRevenueData> SellerRevenue { get; set; }
        public OrderStatisticsViewModel OrderStatistics { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyGrowth { get; set; }
        public decimal DailyGrowth { get; set; }
    }

    public class RevenueReportViewModel
    {
        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Period type is required")]
        [Display(Name = "Period Type")]
        public string PeriodType { get; set; }

        public List<Revenue> ReportData { get; set; }
        public bool IsGenerated { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalBooksSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class DailyRevenueReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public List<Revenue> RevenueData { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalBooksSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<OrderListViewModel> Orders { get; set; }
    }

    public class MonthlyRevenueReportViewModel
    {
        public int ReportYear { get; set; }
        public int ReportMonth { get; set; }
        public List<Revenue> RevenueData { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalBooksSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueBreakdown> DailyBreakdown { get; set; }
        public decimal PreviousMonthRevenue { get; set; }
        public decimal MonthOverMonthGrowth { get; set; }
    }

    public class CategoryReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CategoryRevenueData> CategoryData { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBooksSold { get; set; }
        public string TopCategory { get; set; }
        public decimal TopCategoryRevenue { get; set; }
    }

    public class SellerReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SellerRevenueData> SellerData { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBooksSold { get; set; }
        public string TopSeller { get; set; }
        public decimal TopSellerRevenue { get; set; }
    }

    public class CategoryRevenueData
    {
        public string Category { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NumberOfBooksSold { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class SellerRevenueData
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NumberOfBooksSold { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class DailyRevenueBreakdown
    {
        public int Day { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public int BooksSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class RevenueChartData
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public DateTime Date { get; set; }
    }

    public class RevenueFilterViewModel
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Seller")]
        public int? SellerId { get; set; }

        [Display(Name = "Period Type")]
        public string PeriodType { get; set; }

        public List<string> AvailableCategories { get; set; }
        public List<SelectListItem> AvailableSellers { get; set; }
    }

    public class RevenueSummaryViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal ThisYearRevenue { get; set; }
        public decimal YesterdayRevenue { get; set; }
        public decimal LastWeekRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public decimal LastYearRevenue { get; set; }
        public decimal TodayGrowth { get; set; }
        public decimal ThisWeekGrowth { get; set; }
        public decimal ThisMonthGrowth { get; set; }
        public decimal ThisYearGrowth { get; set; }
    }

    public class YearlyComparisonViewModel
    {
        public int CurrentYear { get; set; }
        public int PreviousYear { get; set; }
        public decimal CurrentYearTotal { get; set; }
        public decimal PreviousYearTotal { get; set; }
        public decimal GrowthRate { get; set; }
        public List<MonthlyRevenueData> CurrentYearMonthlyData { get; set; }
        public List<MonthlyRevenueData> PreviousYearMonthlyData { get; set; }
    }

    public class MonthlyRevenueData
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public int BooksSold { get; set; }
    }

    public class SeasonalRevenueData
    {
        public string Season { get; set; }
        public string Months { get; set; }
        public decimal Revenue { get; set; }
        public int StartMonth { get; set; }
        public int EndMonth { get; set; }
        public decimal PercentageOfTotal { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class SeasonalAnalysisViewModel
    {
        public int SelectedYear { get; set; }
        public List<SeasonalRevenueData> SeasonalData { get; set; }
        public List<SeasonalRevenueData> PreviousYearData { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PreviousYearTotal { get; set; }
        public decimal GrowthRate { get; set; }
        public SeasonalRevenueData BestPerformingSeason { get; set; }
        public SeasonalRevenueData WorstPerformingSeason { get; set; }
        public string BestSeasonName { get; set; }
        public string WorstSeasonName { get; set; }
        public decimal BestSeasonRevenue { get; set; }
        public decimal WorstSeasonRevenue { get; set; }
    }
}