using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
