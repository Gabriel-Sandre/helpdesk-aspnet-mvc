using System.Security.Cryptography;

namespace HelpDesk.Services;

/// <summary>
/// Hash de senha com PBKDF2 (SHA-256, 100.000 iterações) + salt aleatório.
/// Formato armazenado: "iteracoes.saltBase64.hashBase64".
/// </summary>
public static class SenhaHasher
{
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    public static string Gerar(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));

        byte[] salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return string.Join('.', Iteracoes, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public static bool Conferir(string senha, string? senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash) || string.IsNullOrEmpty(senha))
            return false;

        string[] partes = senhaHash.Split('.', 3);
        if (partes.Length != 3 || !int.TryParse(partes[0], out int iteracoes))
            return false;

        try
        {
            byte[] salt = Convert.FromBase64String(partes[1]);
            byte[] esperado = Convert.FromBase64String(partes[2]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
