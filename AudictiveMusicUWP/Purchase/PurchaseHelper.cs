using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Helpers;
using InAppNotificationLibrary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.UI.Xaml;

namespace AudictiveMusicUWP.Purchase
{
    public static class PurchaseHelper
    {
        public static bool? NeedToThankForDonation
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("NeedToThankForDonation");
                if (value == null)
                    return null;
                else
                    return (bool?)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("NeedToThankForDonation", value);
            }
        }

        public static bool? PreviousDonationFailed
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("PreviousDonationFailed");
                if (value == null)
                    return null;
                else
                    return (bool?)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("PreviousDonationFailed", value);
            }
        }

        public static async Task<ProductPurchaseStatus> PurchaseAsync(ProductListing product)
        {
            ProductPurchaseStatus status = ProductPurchaseStatus.NotPurchased;

            try
            {
                Guid productTransactionId;
                List<Guid> grantedConsumableTransactionIds = new List<Guid>();
                PurchaseResults purchaseResults = await CurrentApp.RequestProductPurchaseAsync(product.ProductId);
                status = purchaseResults.Status;

                switch (status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        productTransactionId = purchaseResults.TransactionId;

                        FulfillmentResult fulfillmentResult = await FulfillAsync(product.ProductId, productTransactionId);

                        break;

                    case ProductPurchaseStatus.NotFulfilled:

                        if (ApplicationInfo.Current.HasInternetConnection)
                        {
                            var unfulfilledPurchases = await CurrentApp.GetUnfulfilledConsumablesAsync();

                            foreach (UnfulfilledConsumable unfulfilledConsumable in unfulfilledPurchases)
                                await FulfillAsync(unfulfilledConsumable.ProductId, unfulfilledConsumable.TransactionId);
                        }

                        break;

                    case ProductPurchaseStatus.NotPurchased:

                        break;
                }

                //switch (purchaseResults.Status)
                //{
                //    case ProductPurchaseStatus.Succeeded:
                //        product1TempTransactionId = purchaseResults.TransactionId;
                //        // Grant the user their purchase here, and then pass the product ID and transaction ID to CurrentAppSimulator.reportConsumableFulfillment
                //        // To indicate local fulfillment to the Windows Store.
                //        break;

                //    case ProductPurchaseStatus.NotPurchased:

                //        break;

                //    case ProductPurchaseStatus.NotFulfilled:
                //        product1TempTransactionId = purchaseResults.TransactionId;

                //        // First check for unfulfilled purchases and grant any unfulfilled purchases from an earlier transaction.
                //        // Once products are fulfilled pass the product ID and transaction ID to CurrentAppSimulator.reportConsumableFulfillment
                //        // To indicate local fulfillment to the Windows Store.
                //        break;
                //}
            }
            catch
            {

            }

            return status;
        }

        private static async Task<FulfillmentResult> FulfillAsync(string productID, Guid transactionID)
        {
            if (ApplicationInfo.Current.HasInternetConnection == false)
                return FulfillmentResult.ServerError;

            FulfillmentResult result = FulfillmentResult.PurchasePending;

            try
            {
                result = await CurrentApp.ReportConsumableFulfillmentAsync(
        productID, transactionID);
            }
            catch
            {

            }

            return result;
        }

        public static async Task<List<ProductListing>> GetDonationsIAP()
        {
            List<ProductListing> list = new List<ProductListing>();

            ListingInformation listing = await CurrentApp.LoadListingInformationAsync();

            //foreach (var p in listing.ProductListings)
            //{
            //    list.Add(p.Value);
            //}

            list.Add(listing.ProductListings["SmallDonation"]);
            list.Add(listing.ProductListings["MediumDonation"]);
            list.Add(listing.ProductListings["LargeDonation"]);
            list.Add(listing.ProductListings["VeryLargeDonation"]);

            return list;
        }


        public static void ShowDonationErrorMessage()
        {
            InAppNotification inAppNotification = new InAppNotification()
            {
                Title = ApplicationInfo.Current.Resources.GetString("AskForDonationErrorTitle"),
                Message = ApplicationInfo.Current.Resources.GetString("AskForDonationErrorMessage"),
                Icon = "\uEB90",
            };

            InAppNotificationHelper.ShowNotification(inAppNotification);
        }

        public static void ShowDonationThanksMessage()
        {
            InAppNotification inAppNotification = new InAppNotification()
            {
                Title = ApplicationInfo.Current.Resources.GetString("ThankYouSoMuch"),
                Message = ApplicationInfo.Current.Resources.GetString("AskForDonationThankMessage"),
                Icon = "\uE899",
            };

            InAppNotificationHelper.ShowNotification(inAppNotification);
        }

    }
}
