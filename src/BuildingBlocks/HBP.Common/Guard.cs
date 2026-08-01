using System.Runtime.CompilerServices;

namespace HBP.Common;

public static class Guard
{
    public static T AgainstNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string paramName = ""
    )
        where T : class
    {
        return value ?? throw new ArgumentNullException(paramName);
    }

    public static T AgainstNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string paramName = ""
    )
        where T : struct
    {
        return value ?? throw new ArgumentNullException(paramName);
    }

    public static string AgainstNullOrEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string paramName = ""
    )
    {
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Value cannot be null or empty", paramName);
    }

    public static string AgainstNullOrWhiteSpace(
        string? value,
        [CallerArgumentExpression(nameof(value))] string paramName = ""
    )
    {
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Value cannot be null or empty", paramName);
    }

    public static int AgainstNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string paramName = ""
    )
    {
        return value >= 0 ? value : throw new ArgumentOutOfRangeException(paramName);
    }

    public static int AgainstNonPositive(
        int value,
        [CallerArgumentExpression(nameof(value))] string paramName = ""
    )
    {
        return value > 0 ? value : throw new ArgumentOutOfRangeException(paramName);
    }

    public static void Against(bool condition, string message)
    {
        if (condition)
            throw new ArgumentException(message);
    }
}
