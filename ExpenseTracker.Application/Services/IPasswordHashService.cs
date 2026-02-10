using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Services;
public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
