using MailKit;
using SkillForge.Areas.User.Models;
using SkillForge.Data;
using SkillForge.Models;

namespace SkillForge.Services.StudentService
{
    public class StudentService
    {
        private readonly SkillForgeDbContext _context;
   
    //constructor
    public StudentService(SkillForgeDbContext context)
        {
            _context = context;
        }


    //Method For Get COurse Page
    //public CoursePageVM GetCoursePage(int studentid)
    //    {
    //      //FetchRequest correct student that match from claim id
    //      var  student = _context.Students.FirstOrDefault(s => s.Id == studentid);

    //        if (student == null)
    //        {

    //        }
    //        return CoursePageVM();
    //    }
    }
}

