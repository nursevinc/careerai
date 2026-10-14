using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.Interfaces;

namespace CareerAI.Infrastructure.Auth;

public class BCryptPasswordHasher : IPasswordHasher
{
    // Work factor 12: her hash ~200-300 ms sürer. Brute-force'u yavaşlatır, kullanıcı fark etmez.
    private const int WorkFactor = 12;

    // Rastgele bir şifrenin hash'i: SimulateVerify için sahte hedef (uygulama ömrü boyunca bir kez üretilir)
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), WorkFactor);

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);

    public bool SimulateVerify(string password)
    {
        BCrypt.Net.BCrypt.Verify(password, DummyHash);
        return false;
    }
}