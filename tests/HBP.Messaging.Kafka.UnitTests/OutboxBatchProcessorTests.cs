using FluentAssertions;
using HBP.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace HBP.Messaging.Kafka.UnitTests;

public class OutboxBatchProcessorTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IOutboxProducer> _producer = new();
    private readonly Mock<IOutboxMessageRepository> _repository = new();
    private readonly Mock<IClock> _clock = new();
    private readonly OutboxProducerConfig _config = new() { MaxRetries = 5 };

    public OutboxBatchProcessorTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(FixedNow);
    }

    private OutboxBatchProcessor CreateProcessor() =>
        new(_config, _producer.Object, _clock.Object, Mock.Of<ILogger<OutboxBatchProcessor>>());

    private static OutboxMessage CreateMessage(int retryCount = 0) =>
        new(
            Guid.NewGuid(),
            "hotel.created",
            Key: null,
            "HotelCreatedEvent",
            "{}",
            DateTime.UtcNow,
            retryCount
        );

    [Fact]
    public async Task ProcessBatchAsync_WhenPublishingSucceeds_CompletesMessage()
    {
        var processor = CreateProcessor();
        var message = CreateMessage();

        await processor.ProcessBatchAsync(_repository.Object, new[] { message });

        _producer.Verify(p => p.ProduceAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(
            r => r.CompleteAsync(message.Id, FixedNow.UtcDateTime, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repository.Verify(
            r => r.FailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repository.Verify(
            r =>
                r.DeadLetterAsync(
                    It.IsAny<OutboxMessage>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenMessageFails_ContinuesWithNext()
    {
        var processor = CreateProcessor();
        var failing = CreateMessage();
        var succeeding = CreateMessage();

        _producer
            .SetupSequence(p =>
                p.ProduceAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new InvalidOperationException("kafka down"))
            .Returns(Task.CompletedTask);

        await processor.ProcessBatchAsync(_repository.Object, new[] { failing, succeeding });

        _repository.Verify(
            r => r.FailAsync(failing.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repository.Verify(
            r =>
                r.CompleteAsync(succeeding.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenRetryCountBelowMax_IncrementsRetry()
    {
        var processor = CreateProcessor();
        var message = CreateMessage(retryCount: 3);

        _producer
            .Setup(p => p.ProduceAsync(message, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("kafka down"));

        await processor.ProcessBatchAsync(_repository.Object, new[] { message });

        _repository.Verify(
            r => r.FailAsync(message.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repository.Verify(
            r =>
                r.DeadLetterAsync(
                    It.IsAny<OutboxMessage>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenRetryCountReachesMax_MovesToDlq()
    {
        var processor = CreateProcessor();
        var message = CreateMessage(retryCount: 4);

        _producer
            .Setup(p => p.ProduceAsync(message, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("kafka down"));

        await processor.ProcessBatchAsync(_repository.Object, new[] { message });

        _repository.Verify(
            r => r.FailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repository.Verify(
            r =>
                r.DeadLetterAsync(
                    It.Is<OutboxMessage>(m => m.Id == message.Id),
                    It.IsAny<string>(),
                    FixedNow.UtcDateTime,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenCancellationRequested_ThrowsAndDoesNotTouchRepository()
    {
        var processor = CreateProcessor();
        var message = CreateMessage();

        _producer
            .Setup(p => p.ProduceAsync(message, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => processor.ProcessBatchAsync(_repository.Object, new[] { message });

        await act.Should().ThrowAsync<OperationCanceledException>();
        _repository.Verify(
            r =>
                r.CompleteAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _repository.Verify(
            r => r.FailAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenNoMessages_DoesNothing()
    {
        var processor = CreateProcessor();

        await processor.ProcessBatchAsync(_repository.Object, Array.Empty<OutboxMessage>());

        _producer.Verify(
            p => p.ProduceAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _repository.Verify(
            r =>
                r.CompleteAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenMessageExceedsMaxRetries_MarksRetryCountInDeadLetter()
    {
        var processor = CreateProcessor();
        var message = CreateMessage(retryCount: 4);

        _producer
            .Setup(p => p.ProduceAsync(message, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("kafka down"));

        await processor.ProcessBatchAsync(_repository.Object, new[] { message });

        _repository.Verify(
            r =>
                r.DeadLetterAsync(
                    It.Is<OutboxMessage>(m =>
                        m.Id == message.Id && m.RetryCount + 1 == _config.MaxRetries
                    ),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
