using Microsoft.AspNetCore.Http;

namespace Business.Other
{
    public record MailRequest
    {
        public string To { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public List<IFormFile>? Attachments { get; init; }
    }
}
