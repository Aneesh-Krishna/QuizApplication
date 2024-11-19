using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealTimeQuizApp.Models;
using RealTimeQuizApp.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
namespace RealTimeQuizApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public GroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllGroups")]
        public IActionResult GetAllGroups()
        {
            var groups = _context.Groups.Select(g => new
            {
                g.GroupId,
                g.GroupName,
                g.AdminId
            });

            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(Guid id)
        {
            var group = await _context.Groups.FindAsync(id);
            if(group == null)
                return NotFound();

            return Ok(group);
        }

        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromForm] string groupName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("UserID not found");

            var user = await _context.Users.FindAsync(userId);

            var group = new Group
            {
                GroupName = groupName,
                AdminId = userId
            };

            var groupMember = new GroupMember
            {
                UserId = userId,
                GroupId = group.GroupId
            };

            _context.Groups.Add(group);
            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();  

            return Ok(group);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
                return Unauthorized("UserID not found");

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                return NotFound();

            if (group.AdminId != userId)
                return Unauthorized("You are not the group admin!");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        [HttpGet("GetGroupMembers/{groupId}")]
        public IActionResult GetAllGroupMembers(Guid groupId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound();

            var IsMember = _context.GroupMembers
                .Where(gm => (gm.GroupId == groupId && gm.UserId == userId))
                .FirstOrDefault() != null;
            if (!IsMember)
                return Unauthorized("You're not a member of the group!");

            var groupMembers = _context.GroupMembers
                .Include(gm => gm.Group)
                .Include(gm => gm.User)
                .Where(gm => gm.GroupId == groupId)
                .Select(gm => new
                {
                    gm.GroupId,
                    GroupName = gm.Group.GroupName,
                    UserID = gm.UserId,
                    Name = gm.User.FullName,
                });

            return Ok(groupMembers);
        }

        [HttpGet("GetGroupMember/{id}")]
        public IActionResult GetGroupMemberById(string id)
        {
            var groupMember = _context.GroupMembers
                .Include(gm => gm.Group)
                .Include(gm => gm.User)
                .FirstOrDefault(gm => gm.GroupMemberId == id);
            if (groupMember == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            if (groupMember?.Group?.AdminId != userId && groupMember?.UserId != userId)
                return Unauthorized("You're not authorized");

            return Ok(new
            {
                groupMember.GroupId,
                GroupName = groupMember?.Group?.GroupName,
                groupMember?.UserId,
                Name = groupMember?.User?.FullName
            });
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddMember([FromForm] Guid groupId, [FromForm] string newUserId) 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) 
                return NotFound();
            
            var IsMember = _context.GroupMembers
                .Where(gm => (gm.GroupId == groupId && gm.UserId == userId))
                .FirstOrDefault() != null;
            if (!IsMember) 
                return Unauthorized("You're not a member of the group.");

            var newMemberIsMember = _context.GroupMembers
                .Where(gm => (gm.GroupId == groupId && gm.UserId == newUserId))
                .FirstOrDefault() != null;
            if (newMemberIsMember)
                return Unauthorized("The user is already a member of the group");

            var groupMember = new GroupMember
            {
                GroupId = groupId,
                UserId = newUserId
            };

            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();

            return Ok(groupMember);
        }

        [HttpPost("RemoveUser")]
        public async Task<IActionResult> RemoveMember([FromForm] string groupMemberId)
        {
            var userId = User.FindFirst (ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) 
                return NotFound("UserID not found");

            var groupMember = _context.GroupMembers.Find(groupMemberId);
            if (groupMember == null)
                return NotFound();

            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.GroupId == groupMember.GroupId);
            if (group == null)
                return NotFound();

            if (group.AdminId != userId)
                return Unauthorized("You're not the admin");

            if (groupMember.UserId == userId)
                return BadRequest("You're the admin. You can't remove yourself before transfering your rights");

            _context.GroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();
            return Ok(groupMember);
        }

        [HttpPost("TransferAdminRights")]
        public async Task<IActionResult> TransferAdminRights([FromForm] Guid groupId, [FromForm] string groupMemberId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("UserID not found");

            var groupMember = await _context.GroupMembers.FindAsync(groupMemberId);
            if (groupMember == null)
                return NotFound();

            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync (g => g.GroupId == groupMember.GroupId);
            if (group == null)
                return NotFound("Group not found");

            if (group.AdminId != userId)
                return Unauthorized("You're not the admin");

            var newAdminIsMember = await _context.GroupMembers
                .Include(gm => gm.Group)
                .Where(gm => (gm.GroupId == groupId && gm.GroupMemberId == groupMemberId))
                .FirstOrDefaultAsync() != null;

            if (!newAdminIsMember) 
                return NotFound("New Admin's ID not found!");

            group.AdminId = groupMemberId;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            return Ok(new { message = "New admin is: ", newAdminId = groupMemberId});
        }

    }
}
