using System.Text.Json.Serialization;
using FluentValidation;

namespace ZiziBot.Application.Common.Dtos;

public class TelegramSessionDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("photo_url")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("auth_date")]
    public long AuthDate { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; }
}

public class TelegramSessionDtoValidator : AbstractValidator<TelegramSessionDto>
{
    public TelegramSessionDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AuthDate).NotEmpty();
        RuleFor(x => x.Hash).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}