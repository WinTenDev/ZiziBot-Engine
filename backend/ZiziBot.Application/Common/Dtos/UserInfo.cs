using ZiziBot.Application.Common.Enums;

namespace ZiziBot.Application.Common.Dtos;

public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public string? TransactionId { get; set; }
    public string? BearerToken { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserPhotoUrl { get; set; }
    public string? UserFirstName { get; set; }
    public string? UserLastName { get; set; }
    public string? UserFullName => (UserLastName + " " + UserLastName).Trim();
    public List<long> AdminChatId { get; set; } = [];
    public List<long> ListChatId { get; set; } = [];
    public List<RoleLevel> UserRoles { get; set; } = [];
}