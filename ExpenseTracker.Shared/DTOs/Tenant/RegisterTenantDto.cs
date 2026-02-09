using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Shared.DTOs.Tenant;
public class RegisterTenantDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerFirstName { get; set; } = string.Empty;
    public string OwnerLastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}