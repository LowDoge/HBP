using FluentAssertions;

namespace HBP.Common.UnitTests;

public class ResultTests
{
    [Fact]
    public void ValidationErrorCreatesCorrectType()
    {
        var e = Error.Validation("Name is required");

        e.Type.Should().Be(ErrorType.Validation);
        e.Code.Should().Be("Validation");
        e.Message.Should().Be("Name is required");

    }

    [Fact]
    public void ConflictErrorCreatesCorrectType()
    {
        var e = Error.Conflict("Duplicate");

        e.Type.Should().Be(ErrorType.Conflict);
        e.Code.Should().Be("Conflict");
        e.Message.Should().Contain("Duplicate");
    }

    [Fact]
    public void ForbiddenErrorCreatesCorrectType()
    {
        var e = Error.Forbidden("Access denied");

        e.Type.Should().Be(ErrorType.Forbidden);
        e.Code.Should().Be("Forbidden");
        e.Message.Should().Contain("Access denied");
    }

    [Fact]
    public void InternalErrorCreatesCorrectType()
    {
        var e = Error.Internal("Unexpected");

        e.Type.Should().Be(ErrorType.Internal);
        e.Code.Should().Be("Internal");
        e.Message.Should().Contain("Unexpected");
    }

    [Fact]
    public void NotFoundErrorCreatesCorrectType()
    {
        var e = Error.NotFound("Hotel", "abc");

        e.Type.Should().Be(ErrorType.NotFound);
        e.Code.Should().Be("Hotel.NotFound");
        e.Message.Should().Contain("Hotel").And.Contain("abc");
    }

    [Fact]
    public void ErrorValuesEquals()
    {
        var e1 = Error.NotFound("Hotel", "abc");
        var e2 = Error.NotFound("Hotel", "abc");

        e1.Should().Be(e2);
    }

    [Fact]
    public void DifferentEntityErrorDifferentCode()
    {
        var e1 = Error.NotFound("Hotel", "x");
        var e2 = Error.NotFound("Room", "x");

        e1.Code.Should().NotBe(e2.Code);
        e1.Should().NotBe(e2);
    }

    [Fact]
    public void SuccessResultCreatesSuccessResult()
    {
        var v = new TestValue("x");
        var r = Result.Success(v);

        r.IsSuccess.Should().BeTrue();
        r.IsFailure.Should().BeFalse();
        r.Value.Should().BeSameAs(v);
        r.Error.Should().BeNull();
    }

    [Fact]
    public void FailureResultCreatesFailureResult()
    {
        var e = Error.NotFound("Test", "x");
        var r = Result.Failure<TestValue>(e);

        r.IsSuccess.Should().BeFalse();
        r.IsFailure.Should().BeTrue();
        r.Value.Should().BeNull();
        r.Error.Should().BeSameAs(e);
    }

    [Fact]
    public void ResultIsFailureInverseOfIsSuccess()
    {
        var ok = Result.Success(new TestValue("x"));
        var fail = Result.Failure<TestValue>(Error.Internal("x"));

        ok.IsFailure.Should().BeFalse();
        fail.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ImplicitFromValueResultCreatesSuccess()
    {
        Result<TestValue> result = new TestValue("x");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("x");
    }

    [Fact]
    public void ImplicitFromErrorResultCreatesFailure()
    {
        var error = Error.NotFound("Test", "x");
        Result<TestValue> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(error);
    }

    [Fact]
    public void SuccessResultsAreEquals()
    {
        var value = new TestValue("x");
        var a = Result.Success(value);
        var b = Result.Success(value);

        a.Should().Be(b);
    }

    [Fact]
    public void FailureResultsAreEquals()
    {
        var error = Error.NotFound("Test", "x");
        var a = Result.Failure<TestValue>(error);
        var b = Result.Failure<TestValue>(error);

        a.Should().Be(b);
    }

    private sealed record TestValue(string Name);
}
