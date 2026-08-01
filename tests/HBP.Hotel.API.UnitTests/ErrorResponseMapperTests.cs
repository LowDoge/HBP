using FluentAssertions;
using HBP.Common;
using HBP.Hotel.API.Http;
using Microsoft.AspNetCore.Http;

namespace HBP.Hotel.API.UnitTests;

public class ErrorResponseMapperTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.Internal, StatusCodes.Status500InternalServerError)]
    public void ToStatusCode_MapsEachErrorType(ErrorType type, int expected)
    {
        ErrorResponseMapper.ToStatusCode(type).Should().Be(expected);
    }

    [Fact]
    public void Map_BuildsProblemDetailsFromError()
    {
        var error = Error.Conflict("Cannot delete hotel with active bookings.");

        (var statusCode, var problem) = ErrorResponseMapper.Map(error);

        statusCode.Should().Be(StatusCodes.Status409Conflict);
        problem.Title.Should().Be(error.Code);
        problem.Detail.Should().Be(error.Message);
    }
}
