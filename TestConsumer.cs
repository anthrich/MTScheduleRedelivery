using MassTransit;

namespace MTScheduleRedelivery;

public class TestConsumer : IConsumer<TestMessage>
{
    private readonly ILogger<TestConsumer> _logger;

    public TestConsumer(ILogger<TestConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<TestMessage> context)
    {
        var redeliveryCount = context.GetRedeliveryCount();
        _logger.LogInformation("Redelivered {RedeliveryCount} times", redeliveryCount);
        throw new Exception("Redeliver this message");
    }
}