namespace HBP.Messaging.Abstractions;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TopicAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
