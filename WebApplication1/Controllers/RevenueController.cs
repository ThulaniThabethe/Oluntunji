using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [AdminOrEmployee]
    public class RevenueController : BaseController
    {
        // GET: Revenue
        public ActionResult Index()
        {
            var revenueData = GetRevenueData();
            return View(revenueData);
        }

        // GET: Revenue/Dashboard
        public ActionResult Dashboard()
        {
            var dashboardData = new RevenueDashboardViewModel
            {
                DailyRevenue = GetDailyRevenue(),
                MonthlyRevenue = GetMonthlyRevenue(),
                YearlyRevenue = GetYearlyRevenue(),
                CategoryRevenue = GetCategoryRevenue(),
                SellerRevenue = GetSellerRevenue(),
                OrderStatistics = GetOrderStatistics()
            };

            return View(dashboardData);
        }

        // GET: Revenue/Reports
        public ActionResult Reports()
        {
            var model = new RevenueReportViewModel
            {
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now,
                PeriodType = PeriodTypeConstants.Monthly
            };
            return View(model);
        }

        // POST: Revenue/GenerateReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateReport(RevenueReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Reports", model);
            }

            var reportData = GenerateRevenueReport(model.StartDate, model.EndDate, model.PeriodType);
            model.ReportData = reportData;
            model.IsGenerated = true;

            return View("Reports", model);
        }

        // GET: Revenue/DailyReport
        public ActionResult DailyReport(DateTime? date)
        {
            var reportDate = date ?? DateTime.Now.Date;
            var dailyRevenue = Db.Revenues
                .Where(r => r.Date == reportDate && r.PeriodType == PeriodTypeConstants.Daily)
                .ToList();

            var model = new DailyRevenueReportViewModel
            {
                ReportDate = reportDate,
                RevenueData = dailyRevenue,
                TotalRevenue = dailyRevenue.Sum(r => r.TotalRevenue),
                TotalOrders = dailyRevenue.Sum(r => r.NumberOfOrders),
                TotalBooksSold = dailyRevenue.Sum(r => r.NumberOfBooksSold),
                AverageOrderValue = dailyRevenue.Any() ? dailyRevenue.Average(r => r.AverageOrderValue) : 0
            };

            return View(model);
        }

        // GET: Revenue/MonthlyReport
        public ActionResult MonthlyReport(int? year, int? month)
        {
            var reportYear = year ?? DateTime.Now.Year;
            var reportMonth = month ?? DateTime.Now.Month;
            var startDate = new DateTime(reportYear, reportMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var monthlyRevenue = Db.Revenues
                .Where(r => r.Date >= startDate && r.Date <= endDate && r.PeriodType == PeriodTypeConstants.Daily)
                .ToList();

            var model = new MonthlyRevenueReportViewModel
            {
                ReportYear = reportYear,
                ReportMonth = reportMonth,
                RevenueData = monthlyRevenue,
                TotalRevenue = monthlyRevenue.Sum(r => r.TotalRevenue),
                TotalOrders = monthlyRevenue.Sum(r => r.NumberOfOrders),
                TotalBooksSold = monthlyRevenue.Sum(r => r.NumberOfBooksSold),
                AverageOrderValue = monthlyRevenue.Any() ? monthlyRevenue.Average(r => r.AverageOrderValue) : 0,
                DailyBreakdown = GetDailyBreakdown(monthlyRevenue)
            };

            return View(model);
        }

        // GET: Revenue/CategoryReport
        public ActionResult CategoryReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var categoryRevenue = Db.OrderItems
                .Include(oi => oi.Book)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= start && oi.Order.OrderDate <= end && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                .GroupBy(oi => oi.Book.Category)
                .Select(g => new CategoryRevenueData
                {
                    Category = g.Key,
                    TotalRevenue = g.Sum(oi => oi.Subtotal),
                    NumberOfBooksSold = g.Sum(oi => oi.Quantity),
                    NumberOfOrders = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .ToList();

            var model = new CategoryReportViewModel
            {
                StartDate = start,
                EndDate = end,
                CategoryData = categoryRevenue,
                TotalRevenue = categoryRevenue.Sum(c => c.TotalRevenue),
                TotalBooksSold = categoryRevenue.Sum(c => c.NumberOfBooksSold)
            };

            return View(model);
        }

        // GET: Revenue/SellerReport
        [AdminOrEmployee]
        public ActionResult SellerReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var sellerRevenue = Db.OrderItems
                .Include(oi => oi.Book)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= start && oi.Order.OrderDate <= end && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                .GroupBy(oi => oi.Book.SellerId)
                .Select(g => new SellerRevenueData
                {
                    SellerId = g.Key,
                    SellerName = "Seller ID: " + g.Key, // Temporarily using ID instead of name
                    TotalRevenue = g.Sum(oi => oi.Subtotal),
                    NumberOfBooksSold = g.Sum(oi => oi.Quantity),
                    NumberOfOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                    AverageOrderValue = g.Average(oi => oi.Subtotal)
                })
                .ToList();

            var model = new SellerReportViewModel
            {
                StartDate = start,
                EndDate = end,
                SellerData = sellerRevenue,
                TotalRevenue = sellerRevenue.Sum(s => s.TotalRevenue),
                TotalBooksSold = sellerRevenue.Sum(s => s.NumberOfBooksSold)
            };

            return View(model);
        }

        // POST: Revenue/UpdateRevenueData
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRevenueData(DateTime? date)
        {
            try
            {
                var updateDate = date ?? DateTime.Now.Date;
                UpdateDailyRevenue(updateDate);
                UpdateMonthlyRevenue(updateDate);
                UpdateYearlyRevenue(updateDate);

                TempData["Success"] = "Revenue data updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating revenue data: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private RevenueDashboardViewModel GetRevenueData()
        {
            return new RevenueDashboardViewModel
            {
                DailyRevenue = GetDailyRevenue(),
                MonthlyRevenue = GetMonthlyRevenue(),
                YearlyRevenue = GetYearlyRevenue(),
                CategoryRevenue = GetCategoryRevenue(),
                SellerRevenue = GetSellerRevenue(),
                OrderStatistics = GetOrderStatistics()
            };
        }

        private List<Revenue> GetDailyRevenue()
        {
            var today = DateTime.Now.Date;
            return Db.Revenues
                .Where(r => r.Date >= today.AddDays(-30) && r.PeriodType == PeriodTypeConstants.Daily)
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        private List<Revenue> GetMonthlyRevenue()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            return Db.Revenues
                .Where(r => r.Date.Month == currentMonth && r.Date.Year == currentYear && r.PeriodType == PeriodTypeConstants.Monthly)
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        private List<Revenue> GetYearlyRevenue()
        {
            var currentYear = DateTime.Now.Year;
            return Db.Revenues
                .Where(r => r.Date.Year == currentYear && r.PeriodType == PeriodTypeConstants.Yearly)
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        private List<CategoryRevenueData> GetCategoryRevenue()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            return Db.OrderItems
                .Include(oi => oi.Book)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= thirtyDaysAgo && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                .GroupBy(oi => oi.Book.Category)
                .Select(g => new CategoryRevenueData
                {
                    Category = g.Key,
                    TotalRevenue = g.Sum(oi => oi.Subtotal),
                    NumberOfBooksSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(10)
                .ToList();
        }

        private List<SellerRevenueData> GetSellerRevenue()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            return Db.OrderItems
                .Include(oi => oi.Book)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= thirtyDaysAgo && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                .GroupBy(oi => oi.Book.SellerId)
                .Select(g => new SellerRevenueData
                {
                    SellerId = g.Key,
                    SellerName = "Seller ID: " + g.Key, // Temporarily using ID instead of name
                    TotalRevenue = g.Sum(oi => oi.Subtotal),
                    NumberOfBooksSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(s => s.TotalRevenue)
                .Take(10)
                .ToList();
        }

        private OrderStatisticsViewModel GetOrderStatistics()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var orders = Db.Orders
                .Where(o => o.OrderDate >= thirtyDaysAgo)
                .ToList();

            return new OrderStatisticsViewModel
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Pending.ToString()),
                ShippedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Shipped.ToString()),
                CompletedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered.ToString()),
                CancelledOrders = orders.Count(o => o.OrderStatus == OrderStatus.Cancelled.ToString()),
                TotalRevenue = orders.Where(o => o.OrderStatus == OrderStatus.Delivered.ToString()).Sum(o => o.TotalAmount),
                PendingRevenue = orders.Where(o => o.OrderStatus == OrderStatus.Pending.ToString()).Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                TotalBooksSold = Db.OrderItems.Where(oi => orders.Select(o => o.OrderId).Contains(oi.OrderId)).Sum(oi => oi.Quantity)
            };
        }

        private List<Revenue> GenerateRevenueReport(DateTime startDate, DateTime endDate, string periodType)
        {
            return Db.Revenues
                .Where(r => r.Date >= startDate && r.Date <= endDate && r.PeriodType == periodType)
                .OrderBy(r => r.Date)
                .ToList();
        }

        private List<DailyRevenueBreakdown> GetDailyBreakdown(List<Revenue> monthlyRevenue)
        {
            return monthlyRevenue
                .GroupBy(r => r.Date.Day)
                .Select(g => new DailyRevenueBreakdown
                {
                    Day = g.Key,
                    Revenue = g.Sum(r => r.TotalRevenue),
                    Orders = g.Sum(r => r.NumberOfOrders)
                })
                .ToList();
        }

        private void UpdateDailyRevenue(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var dailyOrders = Db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString())
                .ToList();

            var totalRevenue = dailyOrders.Sum(o => o.TotalAmount);
            var numberOfOrders = dailyOrders.Count;
            var numberOfBooksSold = dailyOrders.Sum(o => o.OrderItems.Sum(oi => oi.Quantity));
            var averageOrderValue = numberOfOrders > 0 ? totalRevenue / numberOfOrders : 0;

            var existingRevenue = Db.Revenues
                .FirstOrDefault(r => r.Date == startDate && r.PeriodType == PeriodTypeConstants.Daily);

            if (existingRevenue != null)
            {
                existingRevenue.TotalRevenue = totalRevenue;
                existingRevenue.NumberOfOrders = numberOfOrders;
                existingRevenue.NumberOfBooksSold = numberOfBooksSold;
                existingRevenue.AverageOrderValue = averageOrderValue;
            }
            else
            {
                var newRevenue = new Revenue
                {
                    Date = startDate,
                    PeriodType = PeriodTypeConstants.Daily,
                    TotalRevenue = totalRevenue,
                    NumberOfOrders = numberOfOrders,
                    NumberOfBooksSold = numberOfBooksSold,
                    AverageOrderValue = averageOrderValue
                };
                Db.Revenues.Add(newRevenue);
            }

            Db.SaveChanges();
        }

        private void UpdateMonthlyRevenue(DateTime date)
        {
            var startDate = new DateTime(date.Year, date.Month, 1);
            var endDate = startDate.AddMonths(1);

            var monthlyOrders = Db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString())
                .ToList();

            var totalRevenue = monthlyOrders.Sum(o => o.TotalAmount);
            var numberOfOrders = monthlyOrders.Count;
            var numberOfBooksSold = monthlyOrders.Sum(o => o.OrderItems.Sum(oi => oi.Quantity));
            var averageOrderValue = numberOfOrders > 0 ? totalRevenue / numberOfOrders : 0;

            var existingRevenue = Db.Revenues
                .FirstOrDefault(r => r.Date == startDate && r.PeriodType == PeriodTypeConstants.Monthly);

            if (existingRevenue != null)
            {
                existingRevenue.TotalRevenue = totalRevenue;
                existingRevenue.NumberOfOrders = numberOfOrders;
                existingRevenue.NumberOfBooksSold = numberOfBooksSold;
                existingRevenue.AverageOrderValue = averageOrderValue;
            }
            else
            {
                var newRevenue = new Revenue
                {
                    Date = startDate,
                    PeriodType = PeriodTypeConstants.Monthly,
                    TotalRevenue = totalRevenue,
                    NumberOfOrders = numberOfOrders,
                    NumberOfBooksSold = numberOfBooksSold,
                    AverageOrderValue = averageOrderValue
                };
                Db.Revenues.Add(newRevenue);
            }

            Db.SaveChanges();
        }

        private void UpdateYearlyRevenue(DateTime date)
        {
            var startDate = new DateTime(date.Year, 1, 1);
            var endDate = startDate.AddYears(1);

            var yearlyOrders = Db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString())
                .ToList();

            var totalRevenue = yearlyOrders.Sum(o => o.TotalAmount);
            var numberOfOrders = yearlyOrders.Count;
            var numberOfBooksSold = yearlyOrders.Sum(o => o.OrderItems.Sum(oi => oi.Quantity));
            var averageOrderValue = numberOfOrders > 0 ? totalRevenue / numberOfOrders : 0;

            var existingRevenue = Db.Revenues
                .FirstOrDefault(r => r.Date == startDate && r.PeriodType == PeriodTypeConstants.Yearly);

            if (existingRevenue != null)
            {
                existingRevenue.TotalRevenue = totalRevenue;
                existingRevenue.NumberOfOrders = numberOfOrders;
                existingRevenue.NumberOfBooksSold = numberOfBooksSold;
                existingRevenue.AverageOrderValue = averageOrderValue;
            }
            else
            {
                var newRevenue = new Revenue
                {
                    Date = startDate,
                    PeriodType = PeriodTypeConstants.Yearly,
                    TotalRevenue = totalRevenue,
                    NumberOfOrders = numberOfOrders,
                    NumberOfBooksSold = numberOfBooksSold,
                    AverageOrderValue = averageOrderValue
                };
                Db.Revenues.Add(newRevenue);
            }

            Db.SaveChanges();
        }
    }
}