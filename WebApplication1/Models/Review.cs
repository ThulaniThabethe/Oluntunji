using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [StringLength(1000)]
        [Display(Name = "Review Text")]
        public string ReviewText { get; set; }

        [Required]
        [Display(Name = "Review Date")]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; } = false;

        // Foreign keys
        public int CustomerId { get; set; }
        public int BookId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }
    }
}