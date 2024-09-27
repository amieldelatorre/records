namespace Persistence.Extensions;

public struct EnvironmentVariable<T>
{
    public string Name { get; }
    public bool IsRequired { get; }
    public T? Default { get; }

    public EnvironmentVariable(string name, bool isRequired, T? defaultValue)
    {
       Name = name;
       IsRequired = isRequired;
       Default = defaultValue;
    }

    public EnvironmentVariable(string name, bool isRequired)
    {
        Name = name;
        IsRequired = isRequired;
        Default = default;
    }
    public static string? GetEnvironmentVariable(EnvironmentVariable<string> environmentVariable)
    {
        var value = Environment.GetEnvironmentVariable(environmentVariable.Name);
        return value == null ? environmentVariable.Default : value.Trim();
    }

    public static bool GetEnvironmentVariable(EnvironmentVariable<bool> environmentVariable)
    {
        var value = Environment.GetEnvironmentVariable(environmentVariable.Name);
        if (string.IsNullOrWhiteSpace(value))
            return false;

        bool result;
        if (bool.TryParse(value, out result))
            return result;

        Console.WriteLine($"Unable to parse environment variable for {environmentVariable.Name}: {value}. Defaulting to 'false'");
        return false;
    }

    public static void PrintMissingEnvironmentVariables(List<string> missingEnvironmentVariableNames)
    {
        Console.WriteLine("The following required environment variables are missing:");
        foreach (var missingEnvironmentVariableName in missingEnvironmentVariableNames)
            Console.WriteLine($"\t- {missingEnvironmentVariableName}");
    }
}
