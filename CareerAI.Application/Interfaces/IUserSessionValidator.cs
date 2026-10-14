using System;
using System.Collections.Generic;
using System.Text;
namespace CareerAI.Application.Interfaces;

public interface IUserSessionValidator
{
    // Token geçerli bir kullanıcıya ve güncel güvenlik damgasına mı ait?
    Task<bool> IsSessionValidAsync(Guid userId, Guid securityStamp, CancellationToken ct = default);
}