using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Withdrawal
    {
        [Key]
        public int WithdrawalId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime WithdrawalDate { get; set; }

        [Required]
        public string Status { get; set; }
    }
}