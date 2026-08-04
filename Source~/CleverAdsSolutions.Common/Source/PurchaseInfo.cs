//  Copyright © 2026 CAS.AI. All rights reserved.

using System.Text;

namespace CAS
{
    /// <summary>
    /// Information about an in-app purchase. 
    /// 
    /// The total revenue generated will be the: quantity * price * currency_rate
    /// 
    /// <code>
    /// var unityReceipt = JsonUtility.FromJson&lt;UnifiedReceipt&gt;(order.Info.Receipt);
    /// var store = unityReceipt.Store;
    /// var payload = unityReceipt.Payload;
    /// var isSubscriptionType = product.definition.type == ProductType.Subscription;
    /// 
    /// var purchase = CAS.PurchaseInfo(
    ///     product.definition.id,
    ///     product.metadata.localizedPrice,
    ///     product.metadata.isoCurrencyCode
    /// );
    /// if (store.Equals("GooglePlay")) 
    /// {
    ///     purchase.SetGoogleValidation(payload, isSubscriptionType);
    /// }
    /// else if (store.Equals("AppleAppStore"))
    /// {
    ///     purchase.SetAppleValidation(
    ///         order.Info.transactionID,
    ///         order.Info.Apple?.jwsRepresentation,
    ///         order.Info.Apple?.AppReceipt,
    ///         isSubscriptionType
    ///     );
    /// }
    /// else if (store.Equals("AmazonAppStore"))
    /// {
    ///     purchase.SetAmazonValidation(payload);
    /// }
    /// CAS.MobileAds.ReportPurchase(purchase);
    /// </code>
    /// </summary>
    public class PurchaseInfo
    {
        /// <summary>
        /// Product identifier from the IAP provider.
        /// </summary>
        public readonly string productId;
        /// <summary>
        /// Price of one purchased item in the selected currency.
        /// </summary>
        public readonly double price;
        /// <summary>
        /// ISO 4217 currency code, for example USD or EUR.
        /// </summary>
        public readonly string currency;
        /// <summary>
        /// Number of items being purchased.
        /// </summary>
        public readonly int quantity;

        public string payload { get; private set; }
        public string amazonPayload { get; private set; }
        public bool isSubscriptionType { get; private set; }
        public string transactionID { get; private set; }
        public string jwsRepresentation { get; private set; }
        public string xsollaOrderId { get; private set; }
        public string xsollaUserId { get; private set; }

        /// <summary>
        /// Information about an in-app purchase.
        /// </summary>
        /// <param name="productId">Product identifier from the IAP provider: <c>product.definition.id</c></param>
        /// <param name="price">Price of one purchased item in the selected currency: <c>product.metadata.localizedPrice</c></param>
        /// <param name="currency">ISO 4217 currency code, for example USD or EUR: <c>product.metadata.isoCurrencyCode</c></param>
        /// <param name="quantity">Number of items being purchased: <c>payout.quantity</c></param>
        public PurchaseInfo(string productId, decimal price, string currency, int quantity = 1)
        {
            this.productId = productId;
            this.price = decimal.ToDouble(price);
            this.currency = currency;
            this.quantity = quantity;
        }

        /// <summary>
        /// Adds Google purchase data required to validate the purchase.
        /// </summary>
        /// <param name="payload">JSON string of the Google payload: <c>info.Receipt.Payload</c></param>
        /// <param name="isSubscriptionType">Is subscription product: <c>product.definition.type == ProductType.Subscription</c></param>
        public PurchaseInfo SetGoogleValidation(string payload, bool isSubscriptionType)
        {
            this.payload = payload;
            this.isSubscriptionType = isSubscriptionType;
            return this;
        }

        /// <summary>
        /// Adds iOS Store Kit data required to validate the purchase.
        /// </summary>
        /// <param name="transactionID">Transaction identifier from StoreKit: <c>order.Info.transactionID</c></param>
        /// <param name="jwsRepresentation">[CanBeNull] Cryptographically signed by Apple: <c>order.Info.Apple?.jwsRepresentation</c></param>
        /// <param name="receiptData">[CanBeNull] Binary receipt (ASN.1) from StoreKit legacy: <c>order.Info.Apple?.AppReceipt</c></param>
        /// <param name="isSubscriptionType">Is subscription product: <c>product.definition.type == ProductType.Subscription</c></param>
        public PurchaseInfo SetAppleValidation(string transactionID, string jwsRepresentation, string receiptData, bool isSubscriptionType)
        {
            this.transactionID = transactionID;
            this.jwsRepresentation = jwsRepresentation;
            payload = receiptData;
            this.isSubscriptionType = isSubscriptionType;
            return this;
        }

        /// <summary>
        /// Adds the required receipt ID and user ID data to validate an Amazon AppStore purchase,
        /// and marks this [PurchaseInfo] as coming from the Amazon Appstore.
        /// 
        /// The publisher is responsible for supplying the purchase price, currency, and quantity values (e.g. from their own
        /// product catalog / pricing configuration) to ensure revenue is reported accurately.
        /// </summary>
        /// <param name="payload">JSON string of the Amazon IAP Receipt: <c>info.Receipt.Payload</c></param>
        public PurchaseInfo SetAmazonValidation(string payload)
        {
            amazonPayload = payload;
            return this;
        }

        /// <summary>
        /// Adds the required Order ID data to validate an Xsolla purchase,
        /// and marks this PurchaseInfo as coming from the Xsolla.
        /// </summary>
        /// <param name="orderId">Xsolla order id.</param>
        /// <param name="userId">[CanBeNull] Xsolla user id.</param>
        public PurchaseInfo SetXsollaValidation(string orderId, string userId)
        {
            xsollaOrderId = orderId;
            xsollaUserId = userId;
            return this;
        }


        public override string ToString()
        {
            return new StringBuilder("Purchased ")
                .Append(productId)
                .Append(" for ")
                .Append(price)
                .Append(' ')
                .Append(currency)
                .ToString();
        }
    }
}
