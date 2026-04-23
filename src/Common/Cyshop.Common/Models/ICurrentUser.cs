namespace Cyshop.Common.Models;

public interface ICurrentUser
{
    Guid UserId { get; }

    bool IsResolved { get; }
}


