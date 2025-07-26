using Serilog;

namespace Application.Configurations.EnvironmentVariables;

public class IntEnvironmentVariable : AbstractEnvironmentVariable<int>
{
    public int MinValue { get; set; }

    public IntEnvironmentVariable(string name, bool isRequired, int defaultValue = 0) :
        base(name, isRequired, defaultValue: defaultValue) { }

    public IntEnvironmentVariable(string name, bool isRequired, int defaultValue = 0, int minValue = 0) :
        base(name, isRequired, defaultValue: defaultValue)
    {
        MinValue = minValue;
    }

    public override void GetEnvironmentVariable()
    {
        var stringValue = Environment.GetEnvironmentVariable(Name);

        if (string.IsNullOrWhiteSpace(stringValue))
        {
            if (IsRequired)
            {
                IsValid = false;
                return;
            }

            Value = Default;
            IsValid = true;
            return;
        }

        int value;
        var parseResult = int.TryParse(stringValue, out value);
        if (!parseResult)
        {
            Log.Logger.Error("unable to parse environment variable for {envVarName}: {envVarValue} ", Name, value);
            IsValid = false;
            return;
        }

        if (value < MinValue)
        {
            Log.Logger.Error("environment variable '{envVarName}' must be greater than {minValue}", Name, MinValue);
            IsValid = false;
            return;
        }

        Value = value;
        IsValid = true;
    }
}