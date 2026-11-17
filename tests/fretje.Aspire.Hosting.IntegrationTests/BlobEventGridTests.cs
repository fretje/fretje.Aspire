using fretje.Aspire.Hosting.IntegrationTests.Infrastructure;

namespace fretje.Aspire.Hosting.IntegrationTests;

public class BlobEventGridTests(BlobEventGridAppHostFixture fixture) : IClassFixture<BlobEventGridAppHostFixture>
{
    private static readonly byte[] _content = "Hello from integration test!"u8.ToArray();
    private readonly string _fileName = $"test-file-{Guid.NewGuid()}.txt";

    private readonly BlobEventGridAppHostFixture _fixture = fixture;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task UploadBlob_ShouldLogBlobTrigger()
    {
        await _fixture.BlobStorage.UploadFileAsync(_fileName, _content, Ct);

        await _fixture.App.ShouldHaveLoggedAsync(
            "functions",
            "Executed 'Functions.BlobTriggerFunction'",
            ct: Ct);
    }

    [Fact]
    public async Task UploadBlob_ShouldTriggerBlobProcessing()
    {
        await _fixture.BlobStorage.UploadFileAsync(_fileName, _content, Ct);

        var processedContent = await _fixture.BlobStorage
            .WaitForProcessedBlobAsync(_fileName, ct: Ct);

        processedContent
            .ShouldNotBeNull()
            .ShouldBe("Hello from integration test!");
    }
}
