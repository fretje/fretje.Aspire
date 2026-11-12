namespace fretje.Aspire.Hosting.LocalEventing.Tests;

public class ProxiedBlobConnectionStringTests
{
    [Fact]
    public void Rewrite_SwapsHttpsBlobEndpointAuthority_KeepingPathAndCredentials()
    {
        const string input = "BlobEndpoint=https://127.0.0.1:10000/devstoreaccount1;AccountKey=secret;Other=foo";
        var result = ProxiedBlobConnectionString.Rewrite(input, "8080");

        result.ShouldContain("BlobEndpoint=http://127.0.0.1:8080/devstoreaccount1");
        result.ShouldContain("AccountKey=secret");
        result.ShouldContain("Other=foo");
    }

    [Fact]
    public void Rewrite_SwapsHttpBlobEndpointAuthority_KeepingPathAndCredentials()
    {
        const string input = "BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;AccountKey=secret";
        var result = ProxiedBlobConnectionString.Rewrite(input, "8080");

        result.ShouldContain("BlobEndpoint=http://127.0.0.1:8080/devstoreaccount1");
        result.ShouldContain("AccountKey=secret");
    }

    [Fact]
    public void Rewrite_PreservesPath_ForNonDefaultAccountPaths()
    {
        const string input = "BlobEndpoint=https://storage.blob.core.windows.net/mycontainer/myaccount;AccountKey=secret";
        var result = ProxiedBlobConnectionString.Rewrite(input, "8080");

        result.ShouldContain("BlobEndpoint=http://127.0.0.1:8080/mycontainer/myaccount");
    }

    [Fact]
    public void Rewrite_PreservesCredentials()
    {
        const string input = "BlobEndpoint=https://127.0.0.1:10000/devstoreaccount1;AccountName=devstoreaccount1;AccountKey=secret";
        var result = ProxiedBlobConnectionString.Rewrite(input, "8080");

        result.ShouldContain("AccountName=devstoreaccount1");
        result.ShouldContain("AccountKey=secret");
    }

    [Fact]
    public void Rewrite_MissingBlobEndpoint_ThrowsInvalidOperationException()
    {
        const string input = "AccountKey=secret;Other=foo";
        var action = () => ProxiedBlobConnectionString.Rewrite(input, "8080");

        var ex = action.ShouldThrow<InvalidOperationException>();
        ex.Message.ShouldContain("BlobEndpoint");
    }

    [Fact]
    public void Rewrite_NullBlobsConnectionString_ThrowsInvalidOperationException()
    {
        var action = () => ProxiedBlobConnectionString.Rewrite(blobsConnectionString: null, "8080");

        var ex = action.ShouldThrow<InvalidOperationException>();
        ex.Message.ShouldContain("blobs connection string");
    }

    [Fact]
    public void Rewrite_NullProxyPort_ThrowsInvalidOperationException()
    {
        const string input = "BlobEndpoint=https://127.0.0.1:10000/devstoreaccount1;AccountKey=secret";
        var action = () => ProxiedBlobConnectionString.Rewrite(input, proxyPort: null);

        var ex = action.ShouldThrow<InvalidOperationException>();
        ex.Message.ShouldContain("proxy port");
    }
}
