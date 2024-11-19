using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeQuizApp.Models
{
    public class GroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string GroupMemberId { get; set; }
        public Guid GroupId { get; set; }
        public Group? Group { get; set; }
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
