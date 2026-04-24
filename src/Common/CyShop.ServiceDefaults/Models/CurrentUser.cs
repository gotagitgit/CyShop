using System;
using System.Collections.Generic;
using System.Text;
using Cyshop.Common.Models;

namespace CyShop.ServiceDefaults.Models;

public class CurrentUser : ICurrentUser
{
    private Guid? _userId;

    public Guid UserId => _userId ?? throw new InvalidOperationException(
        "User identity has not been resolved. Ensure CurrentUserMiddleware runs before accessing this property.");

    public bool IsResolved => _userId.HasValue;

    internal void Set(Guid userId) => _userId = userId;
}
