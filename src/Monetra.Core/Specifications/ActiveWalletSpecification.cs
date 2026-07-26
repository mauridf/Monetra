using Monetra.Core.Entities;
using Monetra.Core.Enums;

namespace Monetra.Core.Specifications;

public class ActiveWalletSpecification
{
    public static bool IsSatisfiedBy(Wallet wallet)
    {
        return wallet != null
            && wallet.Status == WalletStatus.Active
            && !wallet.IsArchived;
    }
}
