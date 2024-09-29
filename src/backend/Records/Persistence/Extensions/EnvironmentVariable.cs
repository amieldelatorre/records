using Serilog;

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
        bool result;

        if (!string.IsNullOrWhiteSpace(value))
        {
            if (bool.TryParse(value, out result))
                return result;

            Log.Logger.Error("unable to parse environment variable for {envVarName}: {envVarValue}. Defaulting to '{envVarDefault}'", environmentVariable.Name, value, environmentVariable.Default);
            return false;
        }

        if (!environmentVariable.IsRequired)
            return environmentVariable.Default;

        PrintMissingEnvironmentVariablesAndExit([environmentVariable.Name]);
        return false;
    }

    public static void PrintMissingEnvironmentVariablesAndExit(List<string> missingEnvironmentVariableNames)
    {
        Console.WriteLine("The following required environment variables are missing:");
        foreach (var missingEnvironmentVariableName in missingEnvironmentVariableNames)
            Console.WriteLine($"\t- {missingEnvironmentVariableName}");

        Environment.Exit(1);
    }
}
