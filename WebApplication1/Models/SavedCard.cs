using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class SavedCard
    {
        [Key]
        public int CardId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string CardholderName { get; set; }

        [Required]
        [StringLength(4)]
        public string LastFourDigits { get; set; }

        [Required]
        [StringLength(50)]
        public string CardType { get; set; } // Visa, MasterCard, etc.

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsDefault { get; set; } = false;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}