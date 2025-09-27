using System;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class PrintingCheckoutViewModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int PageCount { get; set; }
        public decimal Cost { get; set; }
        public DeliveryOrPickup Option { get; set; }
    }
}