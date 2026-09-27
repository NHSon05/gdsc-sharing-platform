using GdscSharingPlatform.Application.Features.Sharing;

namespace GdscSharingPlatform.UnitTests.Features.Sharing;

public sealed class SocialValidationTests
{
    [Theory]
    [InlineData(" **hello** ", true)]
    [InlineData("[docs](https://example.com)", true)]
    [InlineData("  ", false)]
    [InlineData("<script>alert(1)</script>", false)]
    [InlineData("[x](javascript:alert(1))", false)]
    [InlineData("[x](java&#x73;cript:alert(1))", false)]
    [InlineData("[x](%6aavascript:alert(1))", false)]
    [InlineData("<img src=x onerror=alert(1)>", false)]
    public void LimitedMarkdownRejectsRawHtmlAndDangerousSchemes(string body, bool valid)
    {
        Assert.Equal(valid, new CreateCommentRequestValidator().Validate(new CreateCommentRequest(body)).IsValid);
        Assert.Equal(valid, new UpdateCommentRequestValidator().Validate(new UpdateCommentRequest(body, 0)).IsValid);
    }
    [Fact]
    public void CommentLengthIsMeasuredAfterTrim()
    {
        Assert.True(new CreateCommentRequestValidator().Validate(new CreateCommentRequest(" " + new string('x', 2000) + " ")).IsValid);
        Assert.False(new CreateCommentRequestValidator().Validate(new CreateCommentRequest(new string('x', 2001))).IsValid);
    }
}
