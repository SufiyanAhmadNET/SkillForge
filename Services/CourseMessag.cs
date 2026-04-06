namespace SkillForge.Services
{
    public enum CourseMessag
    {
        None,

        //For Instructor
        CoursesAdded,
        CourseNotAdded,
        SavedToDraft,
        SentForApproval,
        CoursePublished,
        CourseActive,
        CourseDelete,
        CourseUpdate,

        //For Student
        PurchaseSUccess,
        AddWishlist,
        CancellBuy,
        EnrollSucces,
        PurchaseFailed,
        CouponApplied,
        InvalidCoupon
    }
}
