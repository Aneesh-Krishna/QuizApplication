using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizApp.Models
{
    public class Group
    {
        public Guid GroupId { get; set; }
        [Required]
        public required string GroupName { get; set; }
        public required string AdminId { get; set; }
        public ApplicationUser? Admin { get; set; }
        public ICollection<GroupMember>? GroupMembers { get; set; }
        public ICollection<Quiz>? Quizzes { get; set; }
    }
}
