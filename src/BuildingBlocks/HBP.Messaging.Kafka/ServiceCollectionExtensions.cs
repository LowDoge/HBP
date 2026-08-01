using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HBP.Messaging.Kafka;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHbpMessagePublishing(this IServiceCollection services)
    {
        services
            .AddOptions<MessagingConfig>()
            .BindConfiguration("Messaging")
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.Kafka.BootstrapServers),
                "Bootstrap servers must be set"
            )
            .Validate(o => !string.IsNullOrWhiteSpace(o.Kafka.ClientId), "Client ID must be set")
            .Validate(o => o.Outbox.BatchSize > 0, "Producer batch size must be greater than 0")
            .Validate(o => o.Outbox.MaxRetries > 0, "Max retries must be greater than 0")
            .ValidateOnStart();

        services.AddScoped<IMessagePublisher, OutboxMessagePublisher>();
        services.AddScoped<AddOnlyOutboxMessageRepository>();

        services.AddSingleton<KafkaProducer>(sp =>
        {
            var clock = sp.GetRequiredService<IClock>();
            var messagingOptions = sp.GetRequiredService<IOptions<MessagingConfig>>();
            var logger = sp.GetRequiredService<ILogger<KafkaProducer>>();
            return new KafkaProducer(messagingOptions.Value.Kafka, clock, logger);
        });

        services.AddSingleton<OutboxBatchProcessor>(sp =>
        {
            var outboxOptions = sp.GetRequiredService<IOptions<MessagingConfig>>().Value.Outbox;
            var producer = sp.GetRequiredService<KafkaProducer>();
            var clock = sp.GetRequiredService<IClock>();
            var logger = sp.GetRequiredService<ILogger<OutboxBatchProcessor>>();
            return new OutboxBatchProcessor(outboxOptions, producer, clock, logger);
        });

        services.AddHostedService<OutboxMessageBackgroundProducer>(sp =>
        {
            var outboxOptions = sp.GetRequiredService<IOptions<MessagingConfig>>().Value.Outbox;

            var connFactory = sp.GetRequiredService<IDbConnectionFactory>();
            var batchProcessor = sp.GetRequiredService<OutboxBatchProcessor>();
            var logger = sp.GetRequiredService<ILogger<OutboxMessageBackgroundProducer>>();
            return new OutboxMessageBackgroundProducer(
                outboxOptions,
                connFactory,
                batchProcessor,
                logger
            );
        });

        return services;
    }
}
