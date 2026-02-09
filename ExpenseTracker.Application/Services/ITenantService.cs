using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Services;
public interface ITenantService
{
    Guid? GetCurrentTenantId();
    void SetTenantId(Guid tenantId);
}
