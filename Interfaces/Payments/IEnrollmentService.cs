using SkillForge.Models;

namespace SkillForge.Interfaces.Payments
{
    public interface IEnrollmentService
    {
        EnrollResult CreateOrder(int studentId, int courseId);
        EnrollResult CreateCartOrder(int studentId);
        EnrollResult VerifyPayment(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
        bool IsEnrolled(int studentId, int courseId);
    }
}
