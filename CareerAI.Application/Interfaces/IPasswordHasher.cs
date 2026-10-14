using System;
using System.Collections.Generic;
using System.Text;
namespace CareerAI.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);

    // Kullanıcı bulunamadığında da aynı maliyette bir doğrulama yapar 
    bool SimulateVerify(string password);
}