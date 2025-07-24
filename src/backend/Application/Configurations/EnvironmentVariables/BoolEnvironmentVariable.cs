using Serilog;

namespace Application.Configurations.EnvironmentVariables;

public class BoolEnvironmentVariable : AbstractEnvironmentVariable<bool>
{
    public BoolEnvironmentVariable(string name, bool isRequired, bool defaultValue) :
        base(name, isRequired, defaultValue) { }

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

        bool value;
        var parseResult = bool.TryParse(stringValue, out value);
        if (!parseResult)
        {
            Log.Logger.Error("unable to parse environment variable for {envVarName}: {envVarValue} ", Name, value);
            IsValid = false;
        }

        Value = value;
        IsValid = true;
    }
}