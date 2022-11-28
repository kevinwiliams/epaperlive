using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FACGatewayService;
using FACGatewayService.FACPG;
using Newtonsoft.Json;

namespace ePaperLive.Controllers
{
    public class PaymentsController : Controller
    {
        // GET: Payments
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> ChargeCard(int policyKey, dynamic purchaseDetailsData)
        {
            var websiteHost = ConfigurationManager.AppSettings["epaper_Prod"];
            //var host = Util.SetAppEnvironment(websiteHost);
            TransactionSummary summary = new TransactionSummary();
            dynamic encrypted;
           
            try
            {
               // var processedClientData = new JsonResponse();
                //var host = Util.SetAppEnvironment(Request.RequestUri.Host);

                Dictionary<string, object> responseData = null;
                // Setup card processor.
                var cardProcessor = new CardProcessor();
                var transactionDetails = new FACGatewayService.FACPG.TransactionDetails();
                var cardDetails = new FACGatewayService.FACPG.CardDetails();
                var billingDetails = new BillingDetails();

                var decrypted = Util.EncryptRijndaelManaged(purchaseDetailsData.ToString(), "D");
                var clientData = JsonConvert.DeserializeObject(decrypted);

                var currentPolicy = clientData.Policies.FirstOrDefault();
                var currentRisk = clientData.Risks.FirstOrDefault();
                var clientKey = clientData.Client.ClientKey;

                if (currentRisk == null)
                {
                    var errMsg = ConfigurationManager.AppSettings["SystemError"];
                    summary.FriendlyErrorMessages.Add(errMsg);
                    var iresponseData = new Dictionary<string, object>()
                    {
                        ["summary"] = summary,
                        ["processedClientData"] = clientData
                    };
                    encrypted = Util.EncryptRijndaelManaged(JsonConvert.SerializeObject(iresponseData), "E");
                    return (encrypted);
                }

                var currentTransaction = clientData.Transactions.FirstOrDefault();
                currentTransaction.TransactionDate = DateTime.Now;

                //clientData = await Util.SetEmailAddress(clientData);

                // Save the stripped card numbers
                string sCardType = Util.GetCreditCardType(currentTransaction.CardNumber);
                Dictionary<string, string> result = Util.StripCardNumber(currentTransaction.CardNumber);
                if (result != null)
                {
                    currentTransaction.CardNumberLastFour = result["lastFour"];
                    currentTransaction.CardNumberFirstSix = result["firstSix"];
                }

                

                if (currentTransaction.PaymentMethod == "Credit Card")
                {
                    cardDetails.CardCVV2 = currentTransaction.CardCVV;
                    cardDetails.CardNumber = currentTransaction.CardNumber;
                    cardDetails.CardExpiryDate = CardUtils.FormatExpiryDate(currentTransaction.CardExpiryMonth, currentTransaction.CardExpiryYear);

                    
                    transactionDetails.Amount = CardUtils.ZeroPadAmount(currentTransaction.CardAmount);

                    var spParams = new Dictionary<string, object>()
                    {
                        { "PolicyKey", currentPolicy.PolicyKey }
                    };

                    // Format: UWYEAR|TransactionType|ProductID|PolicyKey
                    transactionDetails.OrderNumber = $"{DateTime.Now.Year}";

                    //For Testing Purposes Only
                    string xxx = (DateTime.Now.Millisecond).ToString();
                    xxx = xxx.Substring(xxx.Length - 2, 2);
                    transactionDetails.OrderNumber = $"{DateTime.Now.Year}{xxx}";

                    // Update Billing Details
                    billingDetails.BillToAddress = currentTransaction.CardAddress1;
                    billingDetails.BillToAddress2 = currentTransaction.CardAddress2;
                    billingDetails.BillToCity = currentTransaction.CardParish;

                    // Update transaction with order id
                    // Front-end is sending the clientkey
                    currentTransaction.OrderID = long.Parse(transactionDetails.OrderNumber);
                    currentTransaction.ConfirmationNumber = transactionDetails.OrderNumber;
                    currentTransaction.CardExpiryDate = cardDetails.CardExpiryDate;
                    currentTransaction.CardType = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(sCardType ?? currentTransaction?.CardType);
                    
                    if (!CardUtils.IsCardCharged(transactionDetails.OrderNumber))
                    {
                        // Clear sensitive data and save for later retrieval.
                        currentTransaction.CardCVV = "";
                        currentTransaction.CardNumber = "";

                       summary = await cardProcessor.ChargeCard(cardDetails, transactionDetails, billingDetails);

                        // 3D Secure (Visa/MasterCard) Flow
                        if (!string.IsNullOrWhiteSpace(summary.Merchant3DSResponseHtml))
                        {
                         

                            // Set to 15 minutes by default if not found
                            int cacheExpiryDuration = int.Parse(ConfigurationManager.AppSettings["cacheExpiryDuration"] ?? "15");
                            //tempData.Cache.Add(transactionDetails.OrderNumber, $"{clientKey}|{policyKey}|{transactionDetails.Amount}|{clientData.Client.EmailAddress}", new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheExpiryDuration) });

                            // Return the payment object.
                            encrypted = Util.EncryptRijndaelManaged(JsonConvert.SerializeObject(summary), "E");
                            return encrypted;
                        }
                        else
                        {
                            // KeyCard Flow

                        
                            // Send Payment Gateway notification
                            var notificationDetails = new EmailNotificationDetails()
                            {
                                FullName = $"{clientData.Client.Forename} {clientData.Client.LastName}",
                                AddressLine1 = billingDetails.BillToAddress,
                                AddressLine2 = billingDetails.BillToAddress2,
                                AddressLine3 = billingDetails.BillToCity,
                                Email = clientData.Client.EmailAddress,
                               // Contact = clientData.Contacts.FirstOrDefault(c => c.ItemName.ToLower() == "number")?.Item
                            };
                            await CardUtils.SendPaymentNotification(summary, notificationDetails, new string[] { clientData.Client.EmailAddress }, policyNumber: currentPolicy.PolicyNumber);

                            var transStatus = summary.TransactionStatus;
                            switch (transStatus.Status)
                            {
                                case PaymentStatus.Successful:
                                  
                                    currentTransaction.AuthorizationCode = summary.AuthCode;
                                    currentTransaction.OrderID = long.Parse(summary.OrderId);
                                    currentTransaction.ConfirmationNumber = summary.OrderId;
                                    // Save successful order
                                  

                                    // Make Policy Live
                                    //processedClientData = await Policy.MakePolicyLive_New(clientData, policyKey, currentTransaction, long.Parse(summary.OrderId));

                                    //processedClientData.PaymentHistory = await Policy.GetPolicyBalances(clientKey);

                                    //if (processedClientData.IsMadeLiveSuccessful)
                                    //{
                                        // Update Order
                                        //session = sessionRepository.CreateObject(processedClientData);
                                        //await sessionRepository.AddOrUpdate(policyKey, session, long.Parse(summary.OrderId), processedClientData);
                                        //var confirmedResponse = await Account.UpdateOrderConfirmation(updatedID, policyKey, processedClientData, processedClientData.IsMadeLiveSuccessful, long.Parse(summary.OrderId));
                                        //await Document.CreateEcomDipFile(policyKey, processedClientData, long.Parse(summary.OrderId));
                                        //await Util.SendEmailOrderConfirmation(policyKey, processedClientData, long.Parse(summary.OrderId));
                                    //}
                                    responseData = new Dictionary<string, object>()
                                    {
                                        ["summary"] = summary,
                                        //["processedClientData"] = processedClientData
                                    };
                                    encrypted = Util.EncryptRijndaelManaged(JsonConvert.SerializeObject(responseData), "E");
                                    return encrypted;
                                case PaymentStatus.Failed:
                                // TODO: Add Friendly Message property to Card Processor to display to user.
                                case PaymentStatus.GatewayError:
                                case PaymentStatus.InternalError:
                                    //_logger.CreateLog("KeyCard payment failed", logModel, LogType.Warning, additionalFields: logDetails);

                                    var errMsg = ConfigurationManager.AppSettings["CreditCardError"];
                                    summary.FriendlyErrorMessages.Add(errMsg);
                                    encrypted = Util.EncryptRijndaelManaged(JsonConvert.SerializeObject(summary), "E");
                                    return encrypted;
                            }
                        }
                    }
                    else
                    {
                        // Clear sensitive data and save for later retrieval.
                        // Save data of previous charge to our database for later processing.
                        currentTransaction.CardCVV = "";
                        currentTransaction.CardNumber = "";
                        var transactionResponse = cardProcessor.GetGatewayTransactionStatus(transactionDetails.OrderNumber);
                        var transSummary = cardProcessor.GetTransactionSummary(transactionResponse);
                        currentTransaction.AuthorizationCode = transSummary.AuthCode;
                        //await Account.SaveOrderConfirmation_New(clientKey, policyKey, currentTransaction, currentPolicy, currentRisk);

                    }

                    // DOING: Implement case for already being bought.
                   /* var appDb = new EFContext();
                    JNGI_ECOMTransactionHistory existingTransaction = null;
                    var existingTransactions = appDb.JNGI_ECOMTransactionsHistory
                    .Where(e => e.PolicyKey == policyKey && e.TransactionType.ToUpper() == currentTransaction.TransactionType.ToUpper());

                    if (await existingTransactions.CountAsync() > 0)
                    {
                        var mostRecent = await existingTransactions.MaxAsync(tr => tr.TransactionDate);
                        existingTransaction = await existingTransactions.FirstOrDefaultAsync(tr => (!tr.IsMadeLiveSuccessful) && tr.TransactionDate == mostRecent);
                    }

                    summary.TransactionStatus = new TransactionStatus();
                    summary.TransactionStatus.Status = PaymentStatus.Successful;
                    // Redirect to processing
                    // Return the payment object.
                    dynamic processingPageInfo = new
                    {
                        //tid = existingTransaction?.ConfirmationNo ?? transactionDetails.OrderNumber,
                        //oid = existingTransaction?.ID ?? 0,
                        ck = clientData.Client.ClientKey,
                        cc = clientData.Client.ShortName
                    };*/
                    responseData = new Dictionary<string, object>()
                    {
                        ["summary"] = summary,
                        ["processedClientData"] = clientData,
                        //[nameof(processingPageInfo)] = processingPageInfo
                    };
                    encrypted = Util.EncryptRijndaelManaged(JsonConvert.SerializeObject(responseData), "E");
                    return encrypted;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // Something went wrong to get here.
            return Ok();
        }

        private ActionResult Ok()
        {
            throw new NotImplementedException();
        }
    }
}