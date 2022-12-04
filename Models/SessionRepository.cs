﻿using ePaperLive.DBModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ePaperLive.Models
{
    public class SessionRepository
    {
        private ApplicationDbContext appUsersDB = new ApplicationDbContext();
        public async Task<bool> AddOrUpdate(string policyKey, JOL_UserSession newSession, long orderID = 0, AuthSubcriber data = null)
        {
            bool success = false;
            try
            {
                //data = await Util.SetEmailAddress(data);

                var iUser = (await appUsersDB.subscribers.FirstOrDefaultAsync(a => a.EmailAddress == newSession.Email));
                string email = iUser != null ? iUser.EmailAddress : (data != null ? data.EmailAddress : "");
                newSession.Email = email;
                using (var db = new ApplicationDbContext())
                {
                    // Add if it doesn't exist. Otherwise update existing.
                    var lastSession = await db.JOL_UserSession.FirstOrDefaultAsync(c => c.Email == email);
                    if (lastSession == null)
                    {
                        db.JOL_UserSession.Add(newSession);
                        await db.SaveChangesAsync();
                        return success = true;
                    }
                    else
                    {
                        // Compare the original with existing.
                        // Append changes.
                        var currentTransactionData = JsonConvert.DeserializeObject<AuthSubcriber>(newSession.RootObject);
                        var existingTransactionData = JsonConvert.DeserializeObject<AuthSubcriber>(lastSession.RootObject);


                        lastSession.TimeStamp = newSession.TimeStamp;

                        var newTransactionData = new AuthSubcriber();
                        // Check if flexi and if 3D secure
                        // Flexi 2 transactions
                        if (orderID > 0)
                        {

                            // One transaction at a time
                            var currentTransaction = currentTransactionData.PaymentDetails.FirstOrDefault(t => t.OrderID == orderID);

                            if (currentTransaction.EnrolledIn3DSecure)
                            {
                                if (!existingTransactionData.PaymentDetails.Any(c => c.OrderID == orderID))
                                {
                                    existingTransactionData.PaymentDetails.Add(currentTransaction);
                                }
                                else
                                {
                                    // Remove and re add
                                    var index = existingTransactionData.PaymentDetails.FindIndex(t => t.OrderID == orderID);
                                    existingTransactionData.PaymentDetails.RemoveAt(index);
                                    existingTransactionData.PaymentDetails.Add(currentTransaction);
                                }
                            }
                        }
                        else
                        {
                            // Add new transaction for non-3D secure.

                            var transaction = currentTransactionData.PaymentDetails.FirstOrDefault(t => t.OrderNumber == policyKey);
                            if (transaction != null)
                            {
                                existingTransactionData.PaymentDetails.Add(transaction);
                            }

                        }


                        /*

                        // Overwrite only if new.
                        if (!string.IsNullOrWhiteSpace(currentTransactionData.Client.ProofOfId))
                        {
                            if (!existingTransactionData.Client.ProofOfId.Equals(currentTransactionData.Client.ProofOfId))
                            {
                                newTransactionData = currentTransactionData;
                                newTransactionData.PaymentDetails = existingTransactionData.PaymentDetails;
                                var updatedClientData = (CreateObject(newTransactionData)).RootObject;
                                lastSession.RootObject = updatedClientData;
                            }
                        }
                        else
                        {
                            newTransactionData = currentTransactionData;
                            newTransactionData.Client.ProofOfId = existingTransactionData.Client.ProofOfId;
                            newTransactionData.PaymentDetails = existingTransactionData.PaymentDetails;
                            var updatedClientData = (CreateObject(newTransactionData)).RootObject;
                            lastSession.RootObject = updatedClientData;
                        }

                        // Overwrite only if new.
                        if (!string.IsNullOrWhiteSpace(currentTransactionData.Client.ProofOfAddress))
                        {
                            if (!existingTransactionData.Client.ProofOfAddress.Equals(currentTransactionData.Client.ProofOfAddress))
                            {
                                newTransactionData = currentTransactionData;
                                newTransactionData.PaymentDetails = existingTransactionData.PaymentDetails;
                                var updatedClientData = (CreateObject(newTransactionData)).RootObject;
                                lastSession.RootObject = updatedClientData;
                            }
                        }
                        else
                        {
                            newTransactionData = currentTransactionData;
                            newTransactionData.Client.ProofOfAddress = existingTransactionData.Client.ProofOfAddress;
                            newTransactionData.PaymentDetails = existingTransactionData.PaymentDetails;
                            var updatedClientData = (CreateObject(newTransactionData)).RootObject;
                            lastSession.RootObject = updatedClientData;
                        }

                        if (data != null)
                        {
                            lastSession.ClientKey = data.Client.ClientKey;
                            lastSession.ClientCode = data.Client.ShortName;
                        }
                        */
                    }

                    await db.SaveChangesAsync();
                    success = true;
                }
            }
            catch (DbEntityValidationException dbEvex)
            {
                foreach (var eve in dbEvex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Util.LogError($"{ve.ErrorMessage}: {ve.PropertyName}", dbEvex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }

            return success;
        }

        public JOL_UserSession CreateObject(AuthSubcriber rootObject)
        {
            var session = new JOL_UserSession()
            {
                Email = rootObject.EmailAddress,
                TimeStamp = DateTime.Now,
                RootObject = JsonConvert.SerializeObject(rootObject),
                LastPageVisited = rootObject.LastPageVisited
            };

            return session;
        }

        public async Task SaveClientSnapshot(ClientSnapShot snapshot)
        {
            var db = new ApplicationDbContext();

            try
            {
                db.ClientSnapShot.Add(snapshot);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }

        public async Task<JOL_UserSession> Get(string email)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var session = await db.JOL_UserSession.FirstOrDefaultAsync(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                    if (session != null)
                    {
                        return session;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }

            return null;
        }
    }

}