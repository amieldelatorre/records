namespace Application.Configuration.EnvironmentVariables;

public class StringEnvironmentVariable : EnvironmentVariable<string>
{
    public StringEnvironmentVariable(string name, bool isRequired, string? defaultValue = null) :
        base(name, isRequired, defaultValue) { }

    public override void GetEnvironmentVariable()
    {
        var value = Environment.GetEnvironmentVariable(Name);

        if (!string.IsNullOrWhiteSpace(value))
        {
            Value = value;
            IsValid = true;
            return;
        } 
        if (string.IsNullOrWhiteSpace(value) && IsRequired)
        {
            IsValid = false;
            return;
        }
        
        Value = Default;
        IsValid = true;
    }
}