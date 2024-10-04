namespace Application.Configuration.EnvironmentVariables;

public abstract class EnvironmentVariable
{
    public string Name { get; set; }

    public EnvironmentVariable(string name)
    {
        Name = name;
    }

    public static void PrintMissingEnvironmentVariablesAndExit(
        List<EnvironmentVariable> missingEnvironmentVariableNames)
    {
        Console.WriteLine("The following required environment variables are missing:");
        foreach (var missingEnvironmentVariableName in missingEnvironmentVariableNames)
            Console.WriteLine($"\t- {missingEnvironmentVariableName.Name}");

        Environment.Exit(1);
    }
}

public abstract class EnvironmentVariable<T> : EnvironmentVariable
{
    public bool IsRequired { get; set; }
    public T? Default { get; set; }
    public bool IsValid { get; set; } = false;
    public T? Value { get; set; } = default;

    public abstract void GetEnvironmentVariable();

    protected EnvironmentVariable(string name, bool isRequired, T? defaultValue = default) : base(name)
    {
        Name = name;
        IsRequired = isRequired;
        Default = defaultValue;
    }
}