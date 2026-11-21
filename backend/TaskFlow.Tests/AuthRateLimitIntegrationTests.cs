using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TaskFlow.Tests;

public sealed class AuthRateLimitIntegrationTests(TaskFlowApiFixture fixture) : IClassFixture<TaskFlowApiFixture>
{
    [Fact]
    public async Task Login_endpoint_returns_429_after_ten_failed_attempts_from_the_same_client()
    {
        using var client = fixture.CreateClient();

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new
            {
                email = $"unknown-{attempt}@example.com",
                password = "incorrect-password"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var limited = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "last-attempt@example.com",
            password = "incorrect-password"
        });

        Assert.Equal((HttpStatusCode)429, limited.StatusCode);
    }
}
