using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class MyBooksViewModel
    {
        [Display(Name = "Books")]
        public List<Book> Books { get; set; }

        [Display(Name = "Total Books")]
        public int TotalBooks { get; set; }

        [Display(Name = "Books In Stock")]
        public int BooksInStock { get; set; }

        [Display(Name = "Low Stock Books")]
        public int LowStockBooks { get; set; }

        [Display(Name = "Out Of Stock Books")]
        public int OutOfStockBooks { get; set; }

        [Display(Name = "Search Term")]
        public string SearchTerm { get; set; }

        [Display(Name = "Status Filter")]
        public string StatusFilter { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; }

        [Display(Name = "Current Page")]
        public int CurrentPage { get; set; }

        [Display(Name = "Total Pages")]
        public int TotalPages { get; set; }
    }
}