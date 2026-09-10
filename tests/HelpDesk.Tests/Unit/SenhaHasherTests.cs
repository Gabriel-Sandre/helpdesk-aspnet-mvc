using HelpDesk.Services;

namespace HelpDesk.Tests.Unit;

public class SenhaHasherTests
{
    private const string SenhaCorreta = "senha-correta-123";

    [Fact]
    public void Gerar_SenhaValida_RetornaIteracoesSaltEHashSeparadosPorPonto()
    {
        string resultado = SenhaHasher.Gerar(SenhaCorreta);

        string[] partes = resultado.Split('.');
        Assert.Equal(3, partes.Length);
        // Piso de segurança: impede que alguém reduza as iterações sem perceber.
        Assert.True(int.Parse(partes[0]) >= 100_000);
        Assert.Equal(16, Convert.FromBase64String(partes[1]).Length);
        Assert.Equal(32, Convert.FromBase64String(partes[2]).Length);
    }

    [Fact]
    public void Gerar_MesmaSenhaDuasVezes_ProduzResultadosDiferentes()
    {
        string primeiro = SenhaHasher.Gerar(SenhaCorreta);
        string segundo = SenhaHasher.Gerar(SenhaCorreta);

        // Salt aleatório: senhas iguais não geram hashes iguais no banco.
        Assert.NotEqual(primeiro, segundo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Gerar_SenhaVaziaOuEmBranco_LancaArgumentException(string senha)
    {
        Assert.Throws<ArgumentException>(() => SenhaHasher.Gerar(senha));
    }

    [Fact]
    public void Conferir_SenhaCorreta_RetornaTrue()
    {
        string hash = SenhaHasher.Gerar(SenhaCorreta);

        Assert.True(SenhaHasher.Conferir(SenhaCorreta, hash));
    }

    [Theory]
    [InlineData("senha-errada")]
    [InlineData("SENHA-CORRETA-123")]
    [InlineData("senha-correta-123 ")]
    [InlineData("")]
    public void Conferir_SenhaDiferente_RetornaFalse(string senhaDigitada)
    {
        string hash = SenhaHasher.Gerar(SenhaCorreta);

        Assert.False(SenhaHasher.Conferir(senhaDigitada, hash));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("texto-sem-pontos")]
    [InlineData("100000.soDuasPartes")]
    [InlineData("abc.c2FsdA==.aGFzaA==")]
    [InlineData("100000.@@@.###")]
    public void Conferir_HashEmFormatoInvalido_RetornaFalse(string? hashInvalido)
    {
        Assert.False(SenhaHasher.Conferir(SenhaCorreta, hashInvalido));
    }
}
