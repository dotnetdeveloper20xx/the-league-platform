namespace TheLeague.Modules.Communications.Infrastructure.Services;

public class TemplateEngine
{
    public string ReplacePlaceholders(string template, TemplateContext context)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        var result = template;
        result = result.Replace("{{FirstName}}", context.FirstName ?? string.Empty);
        result = result.Replace("{{LastName}}", context.LastName ?? string.Empty);
        result = result.Replace("{{Email}}", context.Email ?? string.Empty);
        result = result.Replace("{{MemberNumber}}", context.MemberNumber ?? string.Empty);
        result = result.Replace("{{ClubName}}", context.ClubName ?? string.Empty);

        return result;
    }
}

public class TemplateContext
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? MemberNumber { get; set; }
    public string? ClubName { get; set; }
}
