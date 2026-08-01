using FluentAssertions;
using HBP.Messaging.Abstractions;

namespace HBP.Messaging.Kafka.UnitTests;

[Topic("custom.ordered")]
public sealed record CustomOrderedMessage : IMessage;

public sealed record PriceUpdated : IMessage;

public class KafkaTopicResolverTests
{
    [Fact]
    public void Resolve_WhenTopicAttributePresent_UsesIt()
    {
        KafkaTopicResolver.Resolve(new CustomOrderedMessage()).Should().Be("custom.ordered");
    }

    [Fact]
    public void Resolve_WithoutAttribute_InfersFromName()
    {
        KafkaTopicResolver.Resolve(new PriceUpdated()).Should().Be("price.updated");
    }
}
