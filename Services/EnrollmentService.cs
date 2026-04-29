using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using SkillForge.Data;
using SkillForge.Models;
using System.Security.Cryptography;
using System.Text;

namespace SkillForge.Services
{
    public class EnrollmentService
    {
        private readonly SkillForgeDbContext _context;
        private readonly IConfiguration _config;

        // razorpay keys from appsettings
        private readonly string _keyId;
        private readonly string _keySecret;

        public EnrollmentService(SkillForgeDbContext context, IConfiguration config)
        {
            _context   = context;
            _config    = config;
            _keyId     = _config["Razorpay:KeyId"]     ?? throw new Exception("Razorpay KeyId missing");
            _keySecret = _config["Razorpay:KeySecret"] ?? throw new Exception("Razorpay KeySecret missing");
        }


        //  Create Razorpay Order   
        public EnrollResult CreateOrder(int studentId, int courseId)
        {
            try
            {
                // already enrolled 
                var existing = _context.Enrollments
                    .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);

                if (existing != null && existing.Status == EnrollmentStatus.Active)
                    return new EnrollResult { Success = false, Message = "You are already enrolled in this course." };

                // get course price
                var course = _context.Courses
                    .Include(c => c.CourseDetails)
                    .FirstOrDefault(c => c.Id == courseId);

                if (course == null)
                    return new EnrollResult { Success = false, Message = "Course not found." };

                var amount = course.CourseDetails?.Total_Price ?? 0;

                // razorpay amount is in paise 
                var amountInPaise = (int)(amount * 100);

                // create razorpay order
                var client = new RazorpayClient(_keyId, _keySecret);

                var options = new Dictionary<string, object>
                {
                    { "amount",   amountInPaise },
                    { "currency", "INR" },
                    { "receipt",  $"sf_{studentId}_{courseId}_{DateTime.UtcNow.Ticks}" },
                    { "payment_capture", 1 }   // auto-capture on success
                };

                var order = client.Order.Create(options);
                string razorpayOrderId = order["id"].ToString();

                // if pending enrollment exists
                Enrollment enrollment;
                if (existing != null)
                {
                    enrollment = existing;
                    enrollment.Status = EnrollmentStatus.Pending;
                }
                else
                {
                    enrollment = new Enrollment
                    {
                        StudentId = studentId,
                        CourseId  = courseId,
                        Status    = EnrollmentStatus.Pending
                    };
                    _context.Enrollments.Add(enrollment);
                }

                _context.SaveChanges();  // get enrollment Id

                // save payment record with razorpay order id
                var payment = _context.Payments
                    .FirstOrDefault(p => p.EnrollmentId == enrollment.Id);

                if (payment == null)
                {
                    payment = new Models.Payment
                    {
                        EnrollmentId     = enrollment.Id,
                        RazorpayOrderId  = razorpayOrderId,
                        Amount           = amount,
                        Status           = PaymentStatus.Pending
                    };
                    _context.Payments.Add(payment);
                }
                else
                {
                    // update order id for retry
                    payment.RazorpayOrderId = razorpayOrderId;
                    payment.Status = PaymentStatus.Pending;
                }

                _context.SaveChanges();

                return new EnrollResult
                {
                    Success        = true,
                    RazorpayOrderId = razorpayOrderId,
                    Amount         = amountInPaise,
                    CourseTitle    = course.Title,
                    EnrollmentId   = enrollment.Id
                };
            }
            catch (Exception ex)
            {
                return new EnrollResult { Success = false, Message = ex.Message };
            }
        }


        /// Verify Payment Signature
        public EnrollResult VerifyPayment(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            try
            {
                var expectedSignature = GenerateSignature(razorpayOrderId, razorpayPaymentId);

                if (expectedSignature != razorpaySignature)
                {
                    // signature mismatch
                    MarkPaymentFailed(razorpayOrderId);
                    return new EnrollResult { Success = false, Message = "Payment verification failed. Possible fraud." };
                }

                // signature matched — find payment in DB
                var payment = _context.Payments
                    .Include(p => p.Enrollment)
                    .FirstOrDefault(p => p.RazorpayOrderId == razorpayOrderId);

                if (payment == null)
                    return new EnrollResult { Success = false, Message = "Payment record not found." };

                // update payment
                payment.RazorpayPaymentId = razorpayPaymentId;
                payment.RazorpaySignature = razorpaySignature;
                payment.Status            = PaymentStatus.Success;
                payment.PaidAt            = DateTime.UtcNow;

                // activate enrollment
                payment.Enrollment.Status = EnrollmentStatus.Active;

                _context.SaveChanges();

                return new EnrollResult
                {
                    Success      = true,
                    Message      = "Payment verified. Enrollment active!",
                    EnrollmentId = payment.EnrollmentId
                };
            }
            catch (Exception ex)
            {
                return new EnrollResult { Success = false, Message = ex.Message };
            }
        }


        //signature generator
        private string GenerateSignature(string orderId, string paymentId)
        {
            var message = $"{orderId}|{paymentId}";
            var keyBytes = Encoding.UTF8.GetBytes(_keySecret);
            var msgBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(msgBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }


        // payment failed 
        private void MarkPaymentFailed(string razorpayOrderId)
        {
            var payment = _context.Payments
                .Include(p => p.Enrollment)
                .FirstOrDefault(p => p.RazorpayOrderId == razorpayOrderId);

            if (payment == null) return;

            payment.Status            = PaymentStatus.Failed;
            payment.Enrollment.Status = EnrollmentStatus.Failed;
            _context.SaveChanges();
        }


        // if student is already enrolled
        public bool IsEnrolled(int studentId, int courseId)
        {
            return _context.Enrollments
                .Any(e => e.StudentId == studentId &&
                          e.CourseId  == courseId  &&
                          e.Status    == EnrollmentStatus.Active);
        }
    }


    // result wrapper 
    public class EnrollResult
    {
        public bool    Success         { get; set; }
        public string? Message         { get; set; }
        public string? RazorpayOrderId { get; set; }
        public int     Amount          { get; set; }   // paise
        public string? CourseTitle     { get; set; }
        public int     EnrollmentId    { get; set; }
    }
}