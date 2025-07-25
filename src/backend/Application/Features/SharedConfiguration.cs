namespace Application.Features;

public static class SharedConfiguration
{
    public const int DefaultRequestTimeout = 2;
    public const int DefaultUsernameMinimumLength = 3;
    public const string DefaultAllowedUsernamePattern = "^[a-z0-9]*$";
}