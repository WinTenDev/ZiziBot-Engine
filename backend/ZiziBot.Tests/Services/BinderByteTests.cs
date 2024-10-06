using FluentAssertions;
using Xunit;

namespace ZiziBot.Tests.Services;

public class BinderByteTests(BinderByteService binderByteService)
{
    [Theory]
    [InlineData("jne", "8825112045716759")]
    public async Task CheckAwbTest(string courier, string awb)
    {
        var awbInfo = await binderByteService.CekResiMergedAsync(courier, awb);

        awbInfo.Should().NotBeNull();
    }
}