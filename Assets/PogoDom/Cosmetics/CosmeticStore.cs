using System;

namespace PogoDom.Cosmetics
{
    public sealed class CosmeticOffer
    {
        public string OfferId { get; }
        public CosmeticId CosmeticId { get; }
        public AcquisitionKind Acquisition { get; }
        public int Price { get; }

        public CosmeticOffer(string offerId, CosmeticId cosmeticId, AcquisitionKind acquisition, int price)
        {
            if (string.IsNullOrWhiteSpace(offerId)) throw new ArgumentException("Offer id cannot be empty.", nameof(offerId));
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
            if ((acquisition == AcquisitionKind.Coins || acquisition == AcquisitionKind.Gems) && price == 0)
                throw new ArgumentException("Currency offers must have a positive price.", nameof(price));
            if (acquisition != AcquisitionKind.Coins && acquisition != AcquisitionKind.Gems && price != 0)
                throw new ArgumentException("Non-currency unlock sources must not carry a price.", nameof(price));

            OfferId = offerId.Trim().ToLowerInvariant();
            CosmeticId = cosmeticId;
            Acquisition = acquisition;
            Price = price;
        }

        public void Validate(CosmeticCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            catalog.Get(CosmeticId);
        }
    }
}
