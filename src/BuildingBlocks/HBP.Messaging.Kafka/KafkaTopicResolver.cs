using System.Reflection;
using System.Text;
using HBP.Common;
using HBP.Messaging.Abstractions;

namespace HBP.Messaging.Kafka;

internal static class KafkaTopicResolver
{
    public static string Resolve(IMessage message)
    {
        var type = Guard.AgainstNull(message).GetType();

        var attribute = type.GetCustomAttribute<TopicAttribute>();
        if (attribute is not null)
        {
            return attribute.Name;
        }

        var name = type.Name;
        if (name.EndsWith("Event"))
        {
            name = name[..^5];
        }

        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var currentChar = name[i];
            if (i > 0 && char.IsUpper(currentChar) && char.IsLower(name[i - 1]))
            {
                sb.Append('.');
            }

            sb.Append(char.ToLowerInvariant(currentChar));
        }

        return sb.ToString();
    }
}
