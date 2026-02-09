using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Shared.DTOs.Tenant;
public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public int MaxUsers { get; set; }
    public bool IsActive { get; set; }
}