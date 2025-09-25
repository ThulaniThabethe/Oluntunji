using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Author")]
        public string Author { get; set; }

        [StringLength(200)]
        [Display(Name = "Publisher")]
        public string Publisher { get; set; }

        [StringLength(50)]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }

        [Display(Name = "Publication Year")]
        public int? PublicationYear { get; set; }

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Price (ZAR)")]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [StringLength(100)]
        [Display(Name = "Genre")]
        public string Genre { get; set; }

        [StringLength(500)]
        [Display(Name = "Cover Image URL")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdatedDate { get; set; }

        // Foreign keys
        public int SellerId { get; set; }

        // Navigation properties
        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }

    public class BookCategory
    {
        public const string Fiction = "Fiction";
        public const string NonFiction = "Non-Fiction";
        public const string Children = "Children";
        public const string Educational = "Educational";
        public const string Business = "Business";
        public const string Technology = "Technology";
        public const string Health = "Health";
        public const string History = "History";
        public const string Science = "Science";
        public const string Art = "Art";
        public const string Religion = "Religion";
        public const string Biography = "Biography";
    }
}