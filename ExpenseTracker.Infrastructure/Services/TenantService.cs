using System;
using System.Collections.Generic;
using System.Text;
using ExpenseTracker.Application.Services;

namespace ExpenseTracker.Infrastructure.Services;
public class TenantService : ITenantService
{
    private Guid? _tenantId;

    public Guid? GetCurrentTenantId() => _tenantId;

    public void SetTenantId(Guid tenantId)
    {
        if (_tenantId.HasValue && _tenantId.Value != tenantId)
        {
            throw new InvalidOperationException("Tenant ID already set for this request");
        }
        _tenantId = tenantId;
    }
}