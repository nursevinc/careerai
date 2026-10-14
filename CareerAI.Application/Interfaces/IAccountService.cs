using System;
using System.Collections.Generic;
using System.Text;

using CareerAI.Application.DTOs.Account;
using CareerAI.Application.DTOs.Auth;

namespace CareerAI.Application.Interfaces;

public interface IAccountService
{
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    // Başarılı olursa diğer cihazlardaki oturumlar kapanır, işlemi yapan cihaz için YENİ token döner
    Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);

    // Hesabı, CV dosyalarını ve tüm verileri kalıcı olarak siler
    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken ct = default);
}