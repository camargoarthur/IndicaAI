using IndicaAI.Web.Text;
namespace IndicaAI.Tests.Domain;

public sealed class NameKeyTests
{
    [Theory]
    [InlineData(" AÇÃO ", "ação")]
    [InlineData("Fuß", "fuss")]
    [InlineData("FUSS", "fuss")]
    [InlineData("e\u0301", "é")]
    [InlineData("acao", "acao")]
    public void UsesCanonicalFullCaseFolding(string input, string expected) => Assert.Equal(expected, UnicodeNameKey.Create(input));
}
