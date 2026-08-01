using FastEndpoints;
using HBP.Common;

namespace HBP.Hotel.API.Http;

internal static class ErrorResponseExtensions
{
    public static Task SendProblemAsync(
        this IResponseSender sender,
        Error error,
        CancellationToken cancellationToken = default
    )
    {
        var (statusCode, problem) = ErrorResponseMapper.Map(error);
        sender.HttpContext.Response.StatusCode = statusCode;
        return sender.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    }
}
