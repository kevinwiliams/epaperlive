﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

using ePaperLive.Models;

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

        
        // Merchant Response URL
        public async Task<ActionResult> CompleteTransaction()
        {
            try
            {
                var websiteHost = ConfigurationManager.AppSettings["ecomm_Prod"];
                //var host = Utilities.SetAppEnvironment(websiteHost);

                //var sessionRepository = new SessionRepository();
                //JNGI_UserSession existing = new JNGI_UserSession();
                //JsonResponse customerData = new JsonResponse();
                // Retrieve data that was saved before 3DS processing.

                await Task.FromResult(0);
                var httpCtx = HttpContext;
                var threedsparams = new ThreeDSParams();
                var cardProcessor = new CardProcessor();

                threedsparams.MerID = httpCtx.Request.Form["MerID"];
                threedsparams.AcqID = httpCtx.Request.Form["AcqID"];
                threedsparams.OrderID = httpCtx.Request.Form["OrderID"];
                threedsparams.ResponseCode = httpCtx.Request.Form["ResponseCode"];
                threedsparams.ReasonCode = httpCtx.Request.Form["ReasonCode"];
                threedsparams.ReasonCodeDesc = httpCtx.Request.Form["ReasonCodeDesc"];
                threedsparams.ReferenceNo = httpCtx.Request.Form["ReferenceNo"];
                threedsparams.PaddedCardNo = httpCtx.Request.Form["PaddedCardNo"];
                threedsparams.AuthCode = httpCtx.Request.Form["AuthCode"];
                threedsparams.CVV2Result = httpCtx.Request.Form["CVV2Result"];
                threedsparams.AuthenticationResult = httpCtx.Request.Form["AuthenticationResult"];
                threedsparams.CAVVValue = httpCtx.Request.Form["CAVVValue"];
                threedsparams.ECIIndicator = httpCtx.Request.Form["ECIIndicator"];
                threedsparams.TransactionStain = httpCtx.Request.Form["TransactionStain"];
                threedsparams.OriginalResponseCode = httpCtx.Request.Form["OriginalResponseCode"];
                threedsparams.Signature = httpCtx.Request.Form["Signature"];
                threedsparams.SignatureMethod = httpCtx.Request.Form["SignatureMethod"];

                //var logModel = new JNGIOnlineAPILogModel()
                //{
                //    CallingMethod = $"{nameof(CompleteTransaction)}. Class: {nameof(AccountsController)}",
                //};
                //var logDetails = new Dictionary<string, object>()
                //{
                //    { "OrderNumber", threedsparams.OrderID }
                //};


                //_logger.CreateLog("Attempting to read response from gateway for 3DS Flow", logModel, LogType.Information, additionalFields: logDetails);
                //var savedKeys = tempData.Cache.Get(threedsparams.OrderID);
                //if (savedKeys == null)
                //{
                //    _logger.CreateLog("Failed to get data from cache", logModel, LogType.Warning, additionalFields: logDetails);
                //    return Redirect($"{host}/payments?status=cacheerror");
                //}
                //string[] keys = savedKeys.ToString().Split('|');
                //clientKey = int.Parse(keys[0]);
                //policyKey = int.Parse(keys[1]);

                var originalAmount = CardUtils.GetAmountFromString("");
                threedsparams.Amount = originalAmount;

                //var email = keys[3];

                //logModel.ClientKey = clientKey;
                //logModel.Email = email;
                //logModel.PolicyKey = policyKey;

                // Update details
                //if (!logDetails.ContainsKey(nameof(logModel.Email)))
                //{
                //    logDetails.Add(nameof(logModel.Email), logModel.Email);
                //}
                //else
                //{
                //    logDetails[nameof(logModel.Email)] = logModel.Email;
                //}
                //if (!logDetails.ContainsKey(nameof(logModel.ClientKey)))
                //{
                //    logDetails.Add(nameof(logModel.ClientKey), logModel.ClientKey);
                //}
                //else
                //{
                //    logDetails[nameof(logModel.ClientKey)] = logModel.ClientKey;
                //}

                //if (!logDetails.ContainsKey(nameof(logModel.PolicyKey)))
                //{
                //    logDetails.Add(nameof(logModel.PolicyKey), logModel.PolicyKey);
                //}
                //else
                //{
                //    logDetails[nameof(logModel.PolicyKey)] = logModel.PolicyKey;
                //}

                //_logger.CreateLog("Retrieved data from cache", logModel, LogType.Information, additionalFields: logDetails);

                //sessionRepository = new SessionRepository();
                //existing = await sessionRepository.Get(clientKey, email);
                //customerData = JsonConvert.DeserializeObject<JsonResponse>(existing.RootObject);
                //var previousTransactions = customerData.Transactions.Where(p => p.PolicyKey == policyKey);
                //Transaction transaction = null;
                //if (previousTransactions.Any())
                //{
                //    var mostRecent = previousTransactions.Max(tr => tr.TransactionDate);
                //    transaction = previousTransactions.FirstOrDefault(t => t.TransactionDate == mostRecent);
                //}
                //else
                //{
                //    transaction = new Transaction();
                //}

                // Free up cache
                //tempData.Cache.Remove(threedsparams.OrderID);
                var summary = CardProcessor.GetGateway3DSecureResponse(threedsparams);
                //var fullName = $"{customerData.Client.Forename} {customerData.Client.LastName}";
                //var polNumber = customerData.Policies.FirstOrDefault(p => p.PolicyKey == policyKey)?.PolicyNumber;

                //logModel.FullName = fullName;
                //logModel.PolicyNumber = polNumber;

                //if (!logDetails.ContainsKey(nameof(logModel.FullName)))
                //{
                //    logDetails.Add(nameof(logModel.FullName), logModel.FullName);
                //}
                //else
                //{
                //    logDetails[nameof(logModel.FullName)] = logModel.FullName;
                //}

                //if (!logDetails.ContainsKey(nameof(logModel.PolicyNumber)))
                //{
                //    logDetails.Add(nameof(logModel.PolicyNumber), logModel.PolicyNumber);
                //}
                //_logger.CreateLog("Retrieved 3DS response from gateway", logModel, LogType.Information, additionalFields: logDetails);

                // Send Payment Gateway notification
                //var notificationDetails = new EmailNotificationDetails()
                //{
                //    FullName = fullName,
                //    AddressLine1 = transaction.CardAddress1,
                //    AddressLine2 = transaction.CardAddress2,
                //    AddressLine3 = transaction.CardParish,
                //    Email = email,
                //    Contact = customerData?.Contacts?.FirstOrDefault(c => c.ItemName.ToLower() == "number")?.Item ?? ""
                //};

                //_logger.CreateLog("Attempting to send payment notification", logModel, LogType.Information, additionalFields: logDetails);

                //var authIsSent = await CardUtils
                //    .SendPaymentNotification(summary,
                //    notificationDetails, new string[] { email },
                //    policyNumber: polNumber);

                //if (authIsSent)
                //{
                //    _logger.CreateLog("Sent payment notification", logModel, LogType.Information, additionalFields: logDetails);
                //}
                //else
                //{
                //    _logger.CreateLog("Failed to send payment notification", logModel, LogType.Warning, additionalFields: logDetails);
                //}

                //var server = HttpContext.Current.Server;
                //await Utilities.FileHelper(server.MapPath("~/App_Data"), $"testhost{DateTime.Now.ToFileTime()}.txt", Encoding.UTF8.GetBytes($@"{host} {Request.RequestUri.Host}"));
                //var summaryString = JsonConvert.SerializeObject(summary);
                //await Utilities.FileHelper(server.MapPath("~/App_Data"), $"summaryString{DateTime.Now.ToFileTime()}.txt", Encoding.UTF8.GetBytes(summaryString));

                // Redirect
                // Setup messages for failure and passes.
                switch (summary.TransactionStatus.Status)
                {
                    case PaymentStatus.Successful:

                        //var processedClientData = new JsonResponse();

                        //_logger.CreateLog("Payment successfully authorized & captured", logModel, LogType.Information, additionalFields: logDetails);

                        //// This is to be done as the 3D-Secure trans is Authorize Only.
                        //var captureSummary = await cardProcessor.CaptureAmount(originalAmount, threedsparams.OrderID);
                        //_logger.CreateLog("Captured payment", logModel, LogType.Information, additionalFields: logDetails);

                        //// Route to order confirmation.
                        //// Send second email to notify capture
                        //var captureIsSent = await CardUtils
                        //.SendPaymentNotification(captureSummary,
                        //notificationDetails, new string[] { email },
                        //policyNumber: customerData.Policies.FirstOrDefault(p => p.PolicyKey == policyKey)?.PolicyNumber);

                        //if (captureIsSent)
                        //{
                        //    _logger.CreateLog("Sent payment notification", logModel, LogType.Information, additionalFields: logDetails);
                        //}
                        //else
                        //{
                        //    _logger.CreateLog("Failed to send payment notification", logModel, LogType.Warning, additionalFields: logDetails);
                        //}

                        //var currentPolicy = customerData.Policies.FirstOrDefault(p => p.PolicyKey == policyKey);
                        //var matchedTransactions = customerData.Transactions.Where(tr => tr.PolicyKey == policyKey);
                        //DateTime mostRecent = matchedTransactions.Any() ? matchedTransactions.Max(t => t.TransactionDate) : default(DateTime);
                        //var currentTransaction = customerData.Transactions.FirstOrDefault(tr => tr.PolicyKey == policyKey && tr.TransactionDate == mostRecent);
                        //var currentRisk = customerData.Risks.FirstOrDefault(r => r.PolicyKey == policyKey && r.Selected);

                        //// This came up null in a one off case
                        //// Reset if null
                        //if (currentTransaction == null)
                        //{
                        //    currentTransaction = transaction;
                        //}
                        //currentTransaction.AuthorizationCode = summary.AuthCode;
                        //currentTransaction.OrderID = long.Parse(summary.OrderId);
                        //currentTransaction.ConfirmationNumber = summary.OrderId;
                        // Save successful order
                        //var updatedID = await Account.SaveOrderConfirmation_New(clientKey, policyKey, customerData, currentTransaction, currentPolicy, currentRisk);
                        //var session = sessionRepository.CreateObject(customerData);
                        //await sessionRepository.AddOrUpdate(policyKey, session, long.Parse(summary.OrderId), customerData);

                        // Set to 15 minutes by default if not found
                        int cacheExpiryDuration = int.Parse(ConfigurationManager.AppSettings["cacheExpiryDuration"] ?? "15");
                        // Repopulate cache for new flow.
                        //tempData.Cache.Add(summary.OrderId, $"{email}|{policyKey}", new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheExpiryDuration) });
                        return Ok();
                    case PaymentStatus.Failed:
                        //_logger.CreateLog("Authorization failed", logModel, LogType.Warning, additionalFields: logDetails);
                        //return Redirect($"{host}/payments?status=failed");
                    case PaymentStatus.InternalError:
                    case PaymentStatus.GatewayError:
                        //_logger.CreateLog("Gateway/Internal failure", logModel, LogType.Warning, additionalFields: logDetails);
                        //return Redirect($"{host}/payments?status=error");
                        break;
                    default:
                        //_logger.CreateLog("Generic failure", logModel, LogType.Warning, additionalFields: logDetails);
                        //return Redirect($"{host}/payments?status=failed");
                        break;
                }
                // return Ok();
            }
            catch (Exception ex)
            {
                //Utilities.LogError(ex);
                ////_logger.CreateLog("Something went wrong on the server with this request", logModel, LogType.Error, ex, additionalFields: logDetails);
                //return InternalServerError(ex);
                throw ex;
            }

            return Ok();

        }
    }
}