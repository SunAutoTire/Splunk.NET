using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SunAuto.Logging.Client.Test;

/// <summary>
/// Reproduces the host's registration order: AddSunAutoLogging binds the Logging:SunAuto section,
/// and the app then layers a UserIdResolver on top via a separate Configure call.
/// </summary>
public class OptionsCompositionTests
{
    private static IServiceCollection HostLike(Action<LoggerOptions> configure)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:SunAuto:MinimumLevel"] = "Information",
                ["Logging:SunAuto:IncludeScopes"] = "true",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(logging => logging.AddSunAutoLogging());
        services.AddOptions<LoggerOptions>().Configure(configure);
        return services;
    }

    [Fact]
    public void Resolver_survives_the_config_binding()
    {
        var expected = Guid.NewGuid();
        using var provider = HostLike(o => o.UserIdResolver = () => expected).BuildServiceProvider();

        var options = provider.GetRequiredService<IOptionsMonitor<LoggerOptions>>().CurrentValue;

        Assert.NotNull(options.UserIdResolver);
        Assert.Equal(expected, options.UserIdResolver!.Invoke());
    }

    /// <summary>
    /// The host wires the resolver through the dependency-taking overload so it can reach
    /// IHttpContextAccessor; this covers that registration shape rather than the plain one.
    /// </summary>
    [Fact]
    public void Resolver_survives_when_registered_through_a_dependency()
    {
        var expected = Guid.NewGuid();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:SunAuto:MinimumLevel"] = "Information",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(logging => logging.AddSunAutoLogging());
        services.AddSingleton(new UserHolder(expected));
        services.AddOptions<LoggerOptions>()
            .Configure<UserHolder>((o, holder) => o.UserIdResolver = () => holder.Current);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<LoggerOptions>>().CurrentValue;

        Assert.NotNull(options.UserIdResolver);
        Assert.Equal(expected, options.UserIdResolver!.Invoke());
    }

    private sealed record UserHolder(Guid? Current);

    [Fact]
    public void Resolver_reaches_the_entry_the_logger_creates()
    {
        var expected = Guid.NewGuid();
        var captured = new List<QueueEntry>();

        var services = HostLike(o =>
        {
            o.UserIdResolver = () => expected;
            o.Sink = captured.Add;
        });

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ILogger<OptionsCompositionTests>>().LogInformation("hello");

        Assert.Equal(expected, Assert.Single(captured).UserId);
    }
}
