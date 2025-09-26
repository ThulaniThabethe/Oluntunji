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
            var revenueData = Db.Revenues
                .OrderByDescending(r => r.Date)
                .Take(100)
                .ToList();
            
            // Set ViewBag data for the summary cards
            var today = DateTime.Now.Date;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            ViewBag.DailyRevenue = Db.Revenues
                .Where(r => r.Date == today && r.PeriodType == PeriodTypeConstants.Daily)
                .Sum(r => (decimal?)r.TotalRevenue) ?? 0;
                
            ViewBag.MonthlyRevenue = Db.Revenues
                .Where(r => r.Date.Month == currentMonth && r.Date.Year == currentYear && r.PeriodType == PeriodTypeConstants.Monthly)
                .Sum(r => (decimal?)r.TotalRevenue) ?? 0;
                
            ViewBag.YearlyRevenue = Db.Revenues
                .Where(r => r.Date.Year == currentYear && r.PeriodType == PeriodTypeConstants.Yearly)
                .Sum(r => (decimal?)r.TotalRevenue) ?? 0;
                
            ViewBag.TotalOrders = Db.Orders.Count();
            
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

            // Get current month revenue data
            var monthlyRevenue = Db.Revenues
                .Where(r => r.Date >= startDate && r.Date <= endDate && r.PeriodType == PeriodTypeConstants.Daily)
                .ToList();

            // Get previous month data for comparison
            var previousMonthStart = startDate.AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
            var previousMonthRevenue = Db.Revenues
                .Where(r => r.Date >= previousMonthStart && r.Date <= previousMonthEnd && r.PeriodType == PeriodTypeConstants.Daily)
                .Sum(r => (decimal?)r.TotalRevenue) ?? 0;

            var currentMonthRevenue = monthlyRevenue.Sum(r => r.TotalRevenue);
            var monthOverMonthGrowth = previousMonthRevenue > 0 ? 
                ((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100 : 0;

            var model = new MonthlyRevenueReportViewModel
            {
                ReportYear = reportYear,
                ReportMonth = reportMonth,
                RevenueData = monthlyRevenue,
                TotalRevenue = currentMonthRevenue,
                TotalOrders = monthlyRevenue.Sum(r => r.NumberOfOrders),
                TotalBooksSold = monthlyRevenue.Sum(r => r.NumberOfBooksSold),
                AverageOrderValue = monthlyRevenue.Any() ? monthlyRevenue.Average(r => r.AverageOrderValue) : 0,
                DailyBreakdown = GetDailyBreakdown(monthlyRevenue),
                PreviousMonthRevenue = previousMonthRevenue,
                MonthOverMonthGrowth = monthOverMonthGrowth
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
                .GroupBy(oi => new { oi.Book.Category, oi.OrderId })
                .Select(g => new { g.Key.Category, g.Key.OrderId })
                .GroupBy(x => x.Category)
                .Select(g => new CategoryRevenueData
                {
                    Category = g.Key,
                    TotalRevenue = Db.OrderItems
                        .Where(oi => oi.Book.Category == g.Key && oi.Order.OrderDate >= start && oi.Order.OrderDate <= end && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                        .Sum(oi => (decimal?)oi.Subtotal) ?? 0,
                    NumberOfBooksSold = Db.OrderItems
                        .Where(oi => oi.Book.Category == g.Key && oi.Order.OrderDate >= start && oi.Order.OrderDate <= end && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                        .Sum(oi => oi.Quantity),
                    NumberOfOrders = g.Count()
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

        // GET: Revenue/YearlyComparison
        public ActionResult YearlyComparison(int? year)
        {
            var comparisonYear = year ?? DateTime.Now.Year;
            var previousYear = comparisonYear - 1;
            
            // Get current year data
            var currentYearStart = new DateTime(comparisonYear, 1, 1);
            var currentYearEnd = currentYearStart.AddYears(1);
            
            var currentYearRevenue = Db.Revenues
                .Where(r => r.Date >= currentYearStart && r.Date < currentYearEnd && r.PeriodType == PeriodTypeConstants.Daily)
                .ToList();
            
            // Get previous year data
            var previousYearStart = new DateTime(previousYear, 1, 1);
            var previousYearEnd = previousYearStart.AddYears(1);
            
            var previousYearRevenue = Db.Revenues
                .Where(r => r.Date >= previousYearStart && r.Date < previousYearEnd && r.PeriodType == PeriodTypeConstants.Daily)
                .ToList();
            
            // Calculate monthly breakdowns
            var currentYearMonthly = GetMonthlyBreakdown(currentYearRevenue, comparisonYear);
            var previousYearMonthly = GetMonthlyBreakdown(previousYearRevenue, previousYear);
            
            var model = new YearlyComparisonViewModel
            {
                CurrentYear = comparisonYear,
                PreviousYear = previousYear,
                CurrentYearTotal = currentYearRevenue.Sum(r => r.TotalRevenue),
                PreviousYearTotal = previousYearRevenue.Sum(r => r.TotalRevenue),
                CurrentYearMonthlyData = currentYearMonthly,
                PreviousYearMonthlyData = previousYearMonthly,
                GrowthRate = previousYearRevenue.Sum(r => r.TotalRevenue) > 0 ? 
                    ((currentYearRevenue.Sum(r => r.TotalRevenue) - previousYearRevenue.Sum(r => r.TotalRevenue)) / previousYearRevenue.Sum(r => r.TotalRevenue)) * 100 : 0
            };
            
            return View(model);
        }

        // GET: Revenue/SeasonalAnalysis
        [AdminOrEmployee]
        public ActionResult SeasonalAnalysis(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;
            
            // Get seasonal data for the selected year
            var seasonalData = GetSeasonalBreakdown(selectedYear);
            
            // Get previous year data for comparison
            var previousYearData = GetSeasonalBreakdown(selectedYear - 1);
            
            var model = new SeasonalAnalysisViewModel
            {
                SelectedYear = selectedYear,
                SeasonalData = seasonalData,
                PreviousYearData = previousYearData,
                TotalRevenue = seasonalData.Sum(s => s.Revenue),
                PreviousYearTotal = previousYearData.Sum(s => s.Revenue),
                BestPerformingSeason = seasonalData.OrderByDescending(s => s.Revenue).FirstOrDefault(),
                WorstPerformingSeason = seasonalData.OrderBy(s => s.Revenue).FirstOrDefault()
            };
            
            // Calculate growth rate
            if (model.PreviousYearTotal > 0)
            {
                model.GrowthRate = ((model.TotalRevenue - model.PreviousYearTotal) / model.PreviousYearTotal) * 100;
            }
            else
            {
                model.GrowthRate = 0;
            }
            
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
                .GroupBy(oi => new { oi.Book.SellerId, oi.OrderId })
                .Select(g => new { g.Key.SellerId, g.Key.OrderId })
                .GroupBy(x => x.SellerId)
                .Select(g => new SellerRevenueData
                {
                    SellerId = g.Key,
                    SellerName = "Seller ID: " + g.Key, // Temporarily using ID instead of name
                    TotalRevenue = Db.OrderItems
                        .Where(oi => oi.Book.SellerId == g.Key && oi.Order.OrderDate >= start && oi.Order.OrderDate <= end && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                        .Sum(oi => (decimal?)oi.Subtotal) ?? 0,
                    NumberOfBooksSold = Db.OrderItems
                        .Where(oi => oi.Book.SellerId == g.Key && oi.Order.OrderDate >= start && oi.Order.OrderDate <= end && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                        .Sum(oi => oi.Quantity),
                    NumberOfOrders = g.Count(),
                    AverageOrderValue = Db.OrderItems
                        .Where(oi => oi.Book.SellerId == g.Key && oi.Order.OrderDate >= start && oi.Order.OrderDate <= end && oi.Order.OrderStatus == OrderStatus.Delivered.ToString())
                        .Average(oi => (decimal?)oi.Subtotal) ?? 0
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
            var thirtyDaysAgo = today.AddDays(-30);
            return Db.Revenues
                .Where(r => r.Date >= thirtyDaysAgo && r.PeriodType == PeriodTypeConstants.Daily)
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
            
            // Use a join to avoid issues with Include and grouping
            var categoryRevenue = from oi in Db.OrderItems
                                  join o in Db.Orders on oi.OrderId equals o.OrderId
                                  join b in Db.Books on oi.BookId equals b.BookId
                                  where o.OrderDate >= thirtyDaysAgo && o.OrderStatus == OrderStatus.Delivered.ToString()
                                  group oi by b.Category into g
                                  select new CategoryRevenueData
                                  {
                                      Category = g.Key,
                                      TotalRevenue = g.Sum(oi => oi.Subtotal),
                                      NumberOfBooksSold = g.Sum(oi => oi.Quantity)
                                  };

            return categoryRevenue
                .OrderByDescending(c => c.TotalRevenue)
                .Take(10)
                .ToList();
        }

        private List<SellerRevenueData> GetSellerRevenue()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            
            // Use a join to avoid issues with Include and grouping
            var sellerRevenue = from oi in Db.OrderItems
                               join o in Db.Orders on oi.OrderId equals o.OrderId
                               join b in Db.Books on oi.BookId equals b.BookId
                               where o.OrderDate >= thirtyDaysAgo && o.OrderStatus == OrderStatus.Delivered.ToString()
                               group oi by b.SellerId into g
                               select new SellerRevenueData
                               {
                                   SellerId = g.Key,
                                   SellerName = "Seller ID: " + g.Key, // Temporarily using ID instead of name
                                   TotalRevenue = g.Sum(oi => oi.Subtotal),
                                   NumberOfBooksSold = g.Sum(oi => oi.Quantity)
                               };

            return sellerRevenue
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

            // Calculate total books sold using a separate query that doesn't require Contains
            var totalBooksSold = Db.OrderItems
                .Join(Db.Orders.Where(o => o.OrderDate >= thirtyDaysAgo),
                    oi => oi.OrderId,
                    o => o.OrderId,
                    (oi, o) => oi.Quantity)
                .Sum();

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
                TotalBooksSold = totalBooksSold
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

            // Get orders without Include to avoid EF issues
            var dailyOrders = Db.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString())
                .ToList();

            var totalRevenue = dailyOrders.Sum(o => o.TotalAmount);
            var numberOfOrders = dailyOrders.Count;
            
            // Get books sold using a separate join query
            var numberOfBooksSold = Db.OrderItems
                .Join(Db.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString()),
                    oi => oi.OrderId,
                    o => o.OrderId,
                    (oi, o) => oi.Quantity)
                .Sum();
                
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

            // Get orders without Include to avoid EF issues
            var monthlyOrders = Db.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString())
                .ToList();

            var totalRevenue = monthlyOrders.Sum(o => o.TotalAmount);
            var numberOfOrders = monthlyOrders.Count;
            
            // Get books sold using a separate join query
            var numberOfBooksSold = Db.OrderItems
                .Join(Db.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString()),
                    oi => oi.OrderId,
                    o => o.OrderId,
                    (oi, o) => oi.Quantity)
                .Sum();
                
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

            // Get orders without Include to avoid EF issues
            var yearlyOrders = Db.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString())
                .ToList();

            var totalRevenue = yearlyOrders.Sum(o => o.TotalAmount);
            var numberOfOrders = yearlyOrders.Count;
            
            // Get books sold using a separate join query
            var numberOfBooksSold = Db.OrderItems
                .Join(Db.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.OrderStatus == OrderStatus.Delivered.ToString()),
                    oi => oi.OrderId,
                    o => o.OrderId,
                    (oi, o) => oi.Quantity)
                .Sum();
                
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

        private List<MonthlyRevenueData> GetMonthlyBreakdown(List<Revenue> yearlyRevenue, int year)
        {
            var monthlyData = new List<MonthlyRevenueData>();
            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            
            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1);
                
                var monthRevenue = yearlyRevenue
                    .Where(r => r.Date >= monthStart && r.Date < monthEnd)
                    .ToList();
                
                monthlyData.Add(new MonthlyRevenueData
                {
                    Month = month,
                    MonthName = monthNames[month - 1],
                    Revenue = monthRevenue.Sum(r => r.TotalRevenue),
                    Orders = monthRevenue.Sum(r => r.NumberOfOrders),
                    BooksSold = monthRevenue.Sum(r => r.NumberOfBooksSold)
                });
            }
            
            return monthlyData;
        }

        private List<SeasonalRevenueData> GetSeasonalBreakdown(int year)
        {
            var monthlyData = Db.Revenues
                .Where(r => r.Date.Year == year && r.PeriodType == PeriodTypeConstants.Monthly)
                .ToList();

            var seasonalData = new List<SeasonalRevenueData>();

            // Spring: March, April, May
            var springRevenue = monthlyData
                .Where(m => m.Date.Month >= 3 && m.Date.Month <= 5)
                .Sum(m => m.TotalRevenue);
            seasonalData.Add(new SeasonalRevenueData
            {
                Season = "Spring",
                Months = "Mar-May",
                Revenue = springRevenue,
                StartMonth = 3,
                EndMonth = 5
            });

            // Summer: June, July, August
            var summerRevenue = monthlyData
                .Where(m => m.Date.Month >= 6 && m.Date.Month <= 8)
                .Sum(m => m.TotalRevenue);
            seasonalData.Add(new SeasonalRevenueData
            {
                Season = "Summer",
                Months = "Jun-Aug",
                Revenue = summerRevenue,
                StartMonth = 6,
                EndMonth = 8
            });

            // Fall: September, October, November
            var fallRevenue = monthlyData
                .Where(m => m.Date.Month >= 9 && m.Date.Month <= 11)
                .Sum(m => m.TotalRevenue);
            seasonalData.Add(new SeasonalRevenueData
            {
                Season = "Fall",
                Months = "Sep-Nov",
                Revenue = fallRevenue,
                StartMonth = 9,
                EndMonth = 11
            });

            // Winter: December, January, February
            var winterRevenue = monthlyData
                .Where(m => m.Date.Month == 12 || m.Date.Month <= 2)
                .Sum(m => m.TotalRevenue);
            seasonalData.Add(new SeasonalRevenueData
            {
                Season = "Winter",
                Months = "Dec-Feb",
                Revenue = winterRevenue,
                StartMonth = 12,
                EndMonth = 2
            });

            return seasonalData;
        }
    }
}