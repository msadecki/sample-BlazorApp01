namespace BlazorApp01.Messaging.Configuration;

public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;
    public int RetryLimit { get; set; } = 3;
    public int RetryInitialIntervalSeconds { get; set; } = 1;
    public int RetryIntervalIncrementSeconds { get; set; } = 2;
}