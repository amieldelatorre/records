namespace WebAPI.Controllers.ControllerExtensions;

public static class ValidGuid
{
    public static bool IsValidGuid(string input, out Guid guid)
    {
        return Guid.TryParse(input, out guid);
    }

    public static Dictionary<string, List<string>> CreateErrorMessage(string fieldName)
    {
        return new Dictionary<string, List<string>>
        {
            { $"{fieldName}", [$"'{fieldName}' must be a valid GUID."] },
        };
    }
}