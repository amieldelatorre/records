namespace Application.Configurations.EnvironmentVariables;

public abstract class AbstractEnvironmentVariable
{
    public string Name { get; set; }

    public AbstractEnvironmentVariable(string name)
    {
        Name = name;
    }

    public static void PrintMissingEnvironmentVariablesAndExit(
        List<AbstractEnvironmentVariable> missingEnvironmentVariableNames)
    {
        Console.WriteLine("The following required environment variables are missing:");
        foreach (var missingEnvironmentVariableName in missingEnvironmentVariableNames)
            Console.WriteLine($"\t- {missingEnvironmentVariableName.Name}");

        Environment.Exit(1);
    }
}

public abstract class AbstractEnvironmentVariable<T> : AbstractEnvironmentVariable
{
    public bool IsRequired { get; set; }
    public T? Default { get; set; }
    public bool IsValid { get; set; } = false;
    public T? Value { get; set; } = default;

    public abstract void GetEnvironmentVariable();

    protected AbstractEnvironmentVariable(string name, bool isRequired, T? defaultValue = default) : base(name)
    {
        Name = name;
        IsRequired = isRequired;
        Default = defaultValue;
    }
}