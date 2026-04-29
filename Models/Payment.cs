using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Enrollment")]
        public int EnrollmentId { get; set; }

        // razorpay gives us these three IDs — all needed for verification
        public string RazorpayOrderId { get; set; } = string.Empty;  // created by us on backend
        public string? RazorpayPaymentId { get; set; }                  // filled after user pays
        public string? RazorpaySignature { get; set; }                  // filled after user pays

        public decimal Amount { get; set; }      // in INR
        public string Currency { get; set; } = "INR";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }  // set when verified

        // nav
        public Enrollment Enrollment { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }
}