using System.ComponentModel.DataAnnotations;

namespace SkillForge.Areas.User.Models
{
    public class DashboardVM
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Mobile { get; set; }

        public string Bio { get; set; }
        public string City { get; set; }
        public string Profession { get; set; }

        public string PhotoPath { get; set; }
  
    }
}
