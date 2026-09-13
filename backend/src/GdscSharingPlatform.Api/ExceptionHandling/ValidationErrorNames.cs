using System.Text.Json;

namespace GdscSharingPlatform.Api.ExceptionHandling;

public static class ValidationErrorNames
{
    public static IReadOnlyDictionary<string, string[]> ForJson(IEnumerable<KeyValuePair<string, string[]>> errors) =>
        errors.GroupBy(x => string.Join('.', x.Key.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName)))
            .ToDictionary(group => group.Key, group => group.SelectMany(x => x.Value).Distinct().ToArray());
}
