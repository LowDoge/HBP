using HBP.Common;
using Microsoft.AspNetCore.Mvc;

namespace HBP.Hotel.API.Http;

internal static class ErrorResponseMapper
{
    public static (int StatusCode, ProblemDetails Problem) Map(Error error)
    {
        var problem = new ProblemDetails { Title = error.Code, Detail = error.Message };
        return (ToStatusCode(error.Type), problem);
    }

    public static int ToStatusCode(ErrorType type) =>
        type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Internal => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest,
        };
}
