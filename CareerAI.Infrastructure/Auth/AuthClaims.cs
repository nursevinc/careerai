using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Infrastructure.Auth;

public static class AuthClaims
{
    // Token'a yazılan güvenlik damgası claim'i
    public const string SecurityStamp = "stamp";
}