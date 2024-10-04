namespace Application.Configuration.EnvironmentVariables;

public abstract class IEnvironmentVariable
{
    public string Name { get; set; }

    public IEnvironmentVariable(string name)
    {
        Name = name;
    }
}

public abstract class IEnvironmentVariable<T> : IEnvironmentVariable
{
    public bool IsRequired { get; set; }
    public T? Default { get; set; }
    public bool IsValid { get; set; } = false;
    public T? Value { get; set; } = default;

    public abstract void GetEnvironmentVariable();

    protected IEnvironmentVariable(string name, bool isRequired, T? defaultValue = default) : base(name)
    {
        Name = name;
        IsRequired = isRequired;
        Default = defaultValue;
    }

    public static void PrintMissingEnvironmentVariablesAndExit(
        List<IEnvironmentVariable> missingEnvironmentVariableNames)
    {
        Console.WriteLine("The following required environment variables are missing:");
        foreach (var missingEnvironmentVariableName in missingEnvironmentVariableNames)
            Console.WriteLine($"\t- {missingEnvironmentVariableName.Name}");

        Environment.Exit(1);
    }
}