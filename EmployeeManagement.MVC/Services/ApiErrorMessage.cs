using System.Text.Json;

namespace EmployeeManagement.MVC.Services;

internal static class ApiErrorMessage
{
    public static async Task<string> ReadAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"The API request failed ({(int)response.StatusCode} {response.StatusCode}).";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return content;
            }

            foreach (var property in new[] { "detail", "message", "title" })
            {
                if (root.TryGetProperty(property, out var value) &&
                    value.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(value.GetString()))
                {
                    return value.GetString()!;
                }
            }

            if (root.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Object)
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(property => property.Value.ValueKind == JsonValueKind.Array
                        ? property.Value.EnumerateArray()
                            .Where(message => message.ValueKind == JsonValueKind.String)
                            .Select(message => message.GetString()!)
                        : Enumerable.Empty<string>())
                    .Where(message => !string.IsNullOrWhiteSpace(message));

                var combined = string.Join(" ", messages);
                if (!string.IsNullOrWhiteSpace(combined))
                {
                    return combined;
                }
            }
        }
        catch (JsonException)
        {
            return content;
        }

        return $"The API request failed ({(int)response.StatusCode} {response.StatusCode}).";
    }
}
