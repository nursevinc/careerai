using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Interfaces;
using CareerAI.Infrastructure.Data;
using CareerAI.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CareerAI.Tests.Integration;

[Collection(ApiCollection.Name)]
public class DatabaseFileStorageTests(CareerApiFactory factory)
{
    [Fact]
    public async Task SaveOpenDelete_RoundTrip_Works()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IFileStorage storage = new DatabaseFileStorage(db);

        var name = $"{Guid.NewGuid():N}.pdf";
        var bytes = TestPdf.Create(ApiTestHelpers.SampleCvText);

        await storage.SaveAsync(name, new MemoryStream(bytes));

        await using (var stream = await storage.OpenReadAsync(name))
        {
            Assert.NotNull(stream);
            using var copy = new MemoryStream();
            await stream.CopyToAsync(copy);
            Assert.Equal(bytes, copy.ToArray());
        }

        await storage.DeleteAsync(name);
        Assert.Null(await storage.OpenReadAsync(name));
    }

    [Fact]
    public async Task OpenRead_UnknownFile_ReturnsNull()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Null(await new DatabaseFileStorage(db).OpenReadAsync("does-not-exist.pdf"));
    }
}