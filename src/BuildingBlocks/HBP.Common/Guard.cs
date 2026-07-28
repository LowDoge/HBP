namespace HBP.Common;

public static class Guard
{
    public static T AgainstNull<T>(T? value, string paramName) where T : class =>
        value ?? throw new ArgumentNullException(paramName);

    public static T AgainstNull<T>(T? value, string paramName) where T : struct =>
        value ?? throw new ArgumentNullException(paramName);

    public static string AgainstNullOrEmpty(string? value, string paramName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Value cannot be null or empty", paramName);

    public static int AgainstNegative(int value, string paramName) =>
        value >= 0 ? value : throw new ArgumentOutOfRangeException(paramName);

    public static int AgainstNonPositive(int value, string paramName) =>
        value > 0
            ? value
            : throw new ArgumentOutOfRangeException(paramName);

    public static void Against(bool condition, string message)
    {
        if (condition)
            throw new ArgumentException(message);
    }
}
