namespace Monetra.Tests.E2E.Api;

public class TransactionFlowTests
{
    private readonly HttpClient _client;

    public TransactionFlowTests()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }

    [Fact(Skip = "E2E tests require a running API instance")]
    public async Task CreateTransaction_Then_Get_Then_Delete_ShouldSucceed()
    {
    }

    [Fact(Skip = "E2E tests require a running API instance")]
    public async Task CreateTransaction_WithInvalidData_ShouldReturn400()
    {
    }
}
