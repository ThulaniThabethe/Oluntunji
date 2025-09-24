using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Foreign keys
        public int CustomerId { get; set; }
        public int BookId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }
    }

    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; }
        public decimal TotalAmount { get; set; }

        public ShoppingCart()
        {
            Items = new List<CartItem>();
        }
    }
}