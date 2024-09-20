
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SparkTech.Data.Models {
    public class Payment {
        [Key]
        public int Id { get; set; }
        public required string Reference { get; set; }
        public double Quantity { get; set; } = 0;
        public string? Sender { get; set; }
        public string? Receiver { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}