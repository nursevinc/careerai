using System;
using System.Collections.Generic;
using System.Text;

namespace CareerAI.Application.DTOs.Account;

public record UpdateProfileRequest(string Name);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record DeleteAccountRequest(string Password);