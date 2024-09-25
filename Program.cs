using Hangfire;
using MassTransit;
using MTScheduleRedelivery;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(h => 
    {
        h.UseRecommendedSerializerSettings();
        h.UseInMemoryStorage();
    }
);
builder.Services.AddHangfireServer();

builder.Services.AddMassTransit(mt =>
{
    mt.AddPublishMessageScheduler();
    mt.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            "amqps://localhost:5672",
            h =>
            {
                h.Username("guest");
                h.Password("guest");
            }
        );
        cfg.Message<TestMessage>(c => c.SetEntityName("TestMessage"));
        cfg.UseHangfireScheduler();
        cfg.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5)));
        cfg.ConfigureEndpoints(ctx);
    });
    mt.AddConsumer<TestConsumer>();
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/", () =>
    {
        return "MT Schedule Redelivery";
    })
    .WithOpenApi();

app.MapPost("/test", (IPublishEndpoint publishEndpoint) =>
{
    var random = new Random();
    publishEndpoint.Publish(new TestMessage(random.Next(1000)));
});

app.UseHangfireDashboard();

app.Run();