using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        public ICollection<GroupMember>? GroupMemberships { get; set; }
    }
}
