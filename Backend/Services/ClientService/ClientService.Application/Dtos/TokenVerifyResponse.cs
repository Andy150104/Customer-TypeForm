using BaseService.Common.ApiEntities;

namespace ClientService.Application.Dtos;

public record TokenVerifyResponse : AbstractApiResponse<TokenVerifyResponseEntity>
{
    public override TokenVerifyResponseEntity Response { get; set; } = null!;
}

public class TokenVerifyResponseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}
