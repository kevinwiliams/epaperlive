using System;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DeviceDetectorNET;
using ePaperLive.Models;
using System.Web.Script.Serialization;
using System.Globalization;

namespace ePaperLive
{
    public class Util
    {
        private static RijndaelManaged myRijndael = new RijndaelManaged();
        private static int iterations;
        private static byte[] salt;

        public class ObjectSerializer<T> where T : class
        {
            public static string Serialize(T obj)
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
                //remove generated namespaces
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, String.Empty);

                using (var sww = new StringWriter())
                {
                    using (XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = System.Xml.Formatting.Indented })
                    {
                        xsSubmit.Serialize(writer, obj, ns);
                        return sww.ToString();
                    }
                }
            }
        }

        public static IEnumerable<SelectListItem> GetEnumSelectList<T>()
        {
            return (Enum.GetValues(typeof(T)).Cast<T>().Select(
                enu => new SelectListItem() { Text = enu.ToString(), Value = enu.ToString() })).ToList();
        }

        public static void LogErrror(Exception ex)
        {
            if (ex.InnerException != null)
            {
                LogError(ex.ToString(), ex.InnerException.Message);
            }
            else
            {
                HttpServerUtility server = HttpContext.Current.Server;
                string location = server.MapPath($@"~/{ConfigurationManager.AppSettings["ErrorLogLocation"]}");
                //HttpContext.Current.Server.MapPath($"~/")
                string dateStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                StringBuilder sb = new StringBuilder();
                string time = DateTime.Now.ToShortTimeString();
                string today = DateTime.Now.ToShortDateString();
                ASCIIEncoding encoding = new ASCIIEncoding();
                sb.Append(string.Format("Error occurred at {1} on {2}: {0} in the method {3} at location {4}", ex.Message, time, today, ex.TargetSite.Name, ex.StackTrace));
                Task.Run(async () =>
                {
                    await FileHelper(location, $"epaper_app_log_{dateStamp}.txt", encoding.GetBytes(sb.ToString()));
                });
            }

        }

        public static bool LogError(Exception exc)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DBEntities"].ConnectionString);
            bool result = false;

            try
            {

                StringBuilder errmsg = new StringBuilder();
                if (exc.InnerException != null)
                {
                    errmsg.AppendLine(exc.InnerException.ToString());
                    if (exc.InnerException.InnerException != null)
                    {
                        errmsg.AppendLine(exc.InnerException.InnerException.ToString());
                        if (exc.InnerException.InnerException.Message != null) { errmsg.AppendLine(exc.InnerException.InnerException.Message.ToString()); }
                    }
                }
                if (exc.Message != null) { errmsg.AppendLine(exc.Message.ToString()); }

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.JOL_Epaper_LogError";
                cmd.Parameters.AddWithValue("err_msg", errmsg.ToString());
                cmd.Parameters.AddWithValue("err_date", DateTime.Now.ToShortDateString());
                cmd.Parameters.AddWithValue("err_time", DateTime.Now.ToShortTimeString());
                cmd.Parameters.AddWithValue("err_name", exc.TargetSite.Name);
                cmd.Parameters.AddWithValue("stacktrace", exc.StackTrace);

                cmd.Connection = con;
                con.Open();

                cmd.ExecuteNonQuery();

                con.Close();
                result = true;

            }
            catch (Exception ex)
            {
                result = false;
                Util.LogErrror(ex);
            }

            return result;
        }

        public static void LogError(string shortMessage, string exceptionMessage)
        {
            HttpServerUtility server = HttpContext.Current.Server;
            string location = server.MapPath($@"~/{ConfigurationManager.AppSettings["ErrorLogLocation"]}");
            string dateStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            StringBuilder sb = new StringBuilder();
            string time = DateTime.Now.ToShortTimeString();
            string today = DateTime.Now.ToShortDateString();
            ASCIIEncoding encoding = new ASCIIEncoding();
            sb.Append(string.Format("The following errors were found: {0}, Message: {1}, On: {2}, At:{3}", shortMessage, exceptionMessage, time, today));
            Task.Run(async () =>
            {
                await FileHelper(location, $"epaper_app_log_{dateStamp}.txt", encoding.GetBytes(sb.ToString()));
            });
        }

        public static bool IsLocaIP(string host)
        {
            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
        private static string RemoveSpecialCharacters(string content)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(content))
            {
                result = string.Join("", content.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)));
            }
            return result;//string.Join("", content.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)));
        }

        public static string DeliveryFreqToDate(string dayPattern)
        {
            var daysOfWeek = new Dictionary<string, string>()
            {
                ["SUN"] = "1",
                ["MON"] = "1",
                ["TUE"] = "1",
                ["WED"] = "1",
                ["THU"] = "1",
                ["FRI"] = "1",
                ["SAT"] = "1"
            };
            char[] charsToTrim = { ',', '.', ' ' };
            var newFrequency = "";

            var pattern = dayPattern;
            for (int i = 0; i < pattern.Length; i++)
            {
                var day = pattern[i].ToString();
                var dayListItem = daysOfWeek.Values.ElementAt(i);
                if (day == dayListItem)
                {
                    newFrequency += daysOfWeek.Keys.ElementAt(i) + ", ";
                }
            }

            newFrequency = newFrequency.TrimEnd(charsToTrim);

            if (newFrequency.Split(',').Count() <= 2)
            {
                newFrequency += " ONLY";
                newFrequency = newFrequency.Replace(",", " &");
            }

            dayPattern = newFrequency;
            return dayPattern;
        }

        public static bool IsCreditCardValid(string cardnumber)
        {

            bool isValid = true;

            int cardlength = 0;

            string cardType = GetCreditCardType(cardnumber);

            //trim card number
            cardnumber = cardnumber.Trim();

            //get length of card number
            cardlength = cardnumber.Length;

            //if credit card length is not between 12 and 16 digits, or a number
            //if (cardlength < 12 || cardlength > 16 || Information.IsNumeric(cardnumber)) {
            //    return false;
            //}



            //validate credit card number using mod10 luhn algorithm - do not validate NCB Key Card
            if (cardType.ToLower().Contains("key card"))
            {
                if (!IsCreditCardValid(cardnumber))
                {
                    return false;
                }
            }
            else if (cardType.ToLower().Contains("Unknown"))
            {
                return false;
            }


            return isValid;
        }

        public static string GetCreditCardType(string cardnumber)
        {
            string cardType = "PAY by JNLIVE";
            if (!(string.IsNullOrWhiteSpace(cardnumber)))
            {
                string firstNumber = cardnumber.Substring(0, 1);
                double firstSix = double.Parse(cardnumber.Substring(0, 6));

                switch (firstNumber)
                {
                    case "7":
                        cardType = "NCB Key Card";
                        break;

                    case "4":
                        cardType = "Visa";
                        break;

                    case "5":
                    case "2":
                        if ((firstSix >= 222100 && firstSix <= 272099) || (firstSix >= 510000 && firstSix <= 559999))
                        {
                            cardType = "Master Card";
                        }
                        else { cardType = "Invalid"; }
                        break;

                    default:
                        cardType = "Unknown";
                        break;
                }
            }
            return cardType;
        }
        /// <summary>
        /// This method takes a credit number, and returns a boolean variable to indicate if the credit number is valid
        /// </summary>
        /// <param name="cardnumber">Credit Card Number</param>
        /// <returns></returns>
        public static Boolean VerifyCreditCardNumber(string cardnumber)
        {

            // ----- Given a card number, make sure it is valid. This method
            //  uses the Luhn algorithm to verify the number. This routine
            //   assumes that cardNumber contains only digits.

            int counter = 0;
            int digitTotal = 0;
            int holdValue = 0;
            int checkDigit = 0;
            int calcDigit = 0;
            string useCard = "";

            //Perform some initial checks
            useCard = (cardnumber.Trim());
            //if (!Information.IsNumeric(useCard)) return false ;

            int cardlength = useCard.Length;

            //separate last digit, the check digit. for cards with
            //and odd number of digits, prepend with a zero
            if ((cardlength % 2) != 0)
            {
                useCard = "0" + useCard;
            }

            checkDigit = int.Parse(useCard.Substring(cardlength - 1, 1));
            useCard = useCard.Substring(0, cardlength - 1);

            //  ----- Process each digit.
            digitTotal = 0;
            for (counter = 1; (counter <= useCard.Length); counter++)
            {
                if (((counter % 2)
                            == 1))
                {
                    //  ----- This is an odd digit position. Double the number.
                    holdValue = (int.Parse(useCard.Substring((counter - 1), 1)) * 2);
                    if ((holdValue > 9))
                    {
                        //  ----- Break the digits (e.g., 19 becomes 1+9).
                        digitTotal = digitTotal + (holdValue / 10) + (holdValue - 10);

                    }
                    else
                    {
                        digitTotal = (digitTotal + holdValue);
                    }

                }
                else
                {
                    //  ----- This is an even digit position. Simply add it.
                    digitTotal = (digitTotal + int.Parse(useCard.Substring((counter - 1), 1)));
                }

            }

            //  ----- Calculate the 10's complement of both values.
            calcDigit = (10
                        - (digitTotal % 10));
            if ((calcDigit == 10))
            {
                calcDigit = 0;
            }

            if ((checkDigit == calcDigit))
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        public static string GetBrowserName()
        {
            var request = HttpContext.Current.Request;
            var browser = request.Browser.Browser;
            if (HttpContext.Current.Request.UserAgent.IndexOf("Edge") > -1)
            {
                browser = "Edge";
            }

            if (browser.Equals("InternetExplorer", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in browser)
                {
                    if (item == 'E')
                    {
                        browser = browser.Replace("E", " E");
                    }

                }
            }

            return browser;
        }

        public static string GetOSName(string userAgent)
        {

            DeviceDetectorSettings.RegexesDirectory = HostingEnvironment.MapPath("~/bin/");
            var dd = new DeviceDetector(userAgent);

            dd.Parse();
            var clientInfo = dd.GetClient();
            var os = dd.GetOs();

            return $"{os?.Match?.Name} {os?.Match?.Version}";
        }
        public static string GetIPAddress()
        {
            var req = HttpContext.Current.Request;
            string ipList = req.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string ipHeader = req.ServerVariables["REMOTE_ADDR"];

            //if (!string.IsNullOrEmpty(ipList))
            //{
            //    return ipList.Split(',')[0];
            //}
            string ipAddress = "";
            if (isLocal())
            {
                IPHostEntry Host = default(IPHostEntry);
                string Hostname = null;
                Hostname = req.UserHostAddress;
                Host = Dns.GetHostEntry(Hostname);
                foreach (IPAddress IP in Host.AddressList)
                {
                    if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddress = Convert.ToString(IP);
                    }
                }
            }
            else
            {
                ipAddress = req.UserHostAddress;
            }



            return ipAddress;
        }

        public static UserLocation GetUserLocation()
        {
            var req = HttpContext.Current.Request;
            string ipList = req.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string ipHeader = req.ServerVariables["REMOTE_ADDR"];

            string ipAddress = req.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = req.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrEmpty(ipAddress.Split(':').FirstOrDefault()))
            {
                IPHostEntry Host = default;
                string Hostname = null;
                Hostname = Environment.MachineName;
                Host = Dns.GetHostEntry(Hostname);
                foreach (IPAddress IP in Host.AddressList)
                {
                    if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddress = Convert.ToString(IP);
                    }
                }
            }
            UserLocation location = new UserLocation();

            if (Util.IsLocaIP(ipAddress))
            {
                location.Country_Code = "JM";
            }
            else
            {

                try
                {
                    string apiUrl = String.Format("http://ip-api.com/json/{0}", ipAddress);
                    using (WebClient client = new WebClient())
                    {
                        string json = client.DownloadString(apiUrl);

                        dynamic jsonResult = new JavaScriptSerializer().Deserialize<dynamic>(json);
                        if (jsonResult["status"] == "success")
                        {
                            location.Country_Code = jsonResult["countryCode"];
                        }
                        else
                        {
                            try
                            {
                                string url = string.Format("https://api.ip2location.io/?ip={0}&key={1}", ipAddress, "D799EE85BF1EC31305E91E2AF1F6B166"); //
                                using (WebClient webClient = new WebClient())
                                {
                                    string jsonRes = webClient.DownloadString(url);
                                    location = new JavaScriptSerializer().Deserialize<UserLocation>(jsonRes);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogError(ex);
                                location.Country_Code = "JM";
                            }
                        }

                    }
                }
                catch (Exception exs)
                {
                    LogError(exs);
                    try
                    {
                        string url = string.Format("https://api.ip2location.io/?ip={0}&key={1}", ipAddress, "D799EE85BF1EC31305E91E2AF1F6B166"); //
                        using (WebClient webClient = new WebClient())
                        {
                            string jsonRes = webClient.DownloadString(url);
                            location = new JavaScriptSerializer().Deserialize<UserLocation>(jsonRes);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        location.Country_Code = "JM";
                    }
                }

            }

            return location;
        }
        public static bool isLocal()
        {
            string host = HttpContext.Current.Request.Url.Host.ToLower();
            return (host == "localhost" || host == "127.0.0.1" || host == "::1");
        }

        public static int MonthDiff(DateTime firstDate, DateTime secondDate)
        {
            int m1, m2;
            if (firstDate < secondDate)
            {
                m1 = (secondDate.Month - firstDate.Month); //for years
                m2 = (secondDate.Year - firstDate.Year) * 12; //for months
            }
            else
            {
                m1 = (firstDate.Month - secondDate.Month); //for years
                m2 = (firstDate.Year - secondDate.Year) * 12; //for months
            }

            return m1 + m2;
        }

        public static string ZeroPadNumber(int maxLength, int numberToPad)
        {

            var numberAsString = numberToPad.ToString();
            var amountInParts = numberAsString.Split('.');

            // Length of string cannot be greater than max length
            if (numberAsString.Length > maxLength)
            {
                return null;
            }
            else if (numberAsString.Length == maxLength)
            {
                // return if requirements are already met
                return numberAsString;
            }



            var paddedNumber = numberAsString.PadLeft((maxLength - numberAsString.Length) + numberAsString.Length, '0');


            // Split according to decimal point

            return paddedNumber;
        }

        #region Encryption Functions
        public static string EncryptData(string plaintext)
        {

            TripleDESCryptoServiceProvider TripleDes = new TripleDESCryptoServiceProvider();

            byte[] plaintextBytes = System.Text.Encoding.Unicode.GetBytes(plaintext);

            // Create the stream. 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            // Create the encoder to write to the stream. 
            CryptoStream encStream = new CryptoStream(ms, TripleDes.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write);

            // Use the crypto stream to write the byte array to the stream.
            encStream.Write(plaintextBytes, 0, plaintextBytes.Length);
            encStream.FlushFinalBlock();

            // Convert the encrypted stream to a printable string. 
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DecryptData(string encryptedtext)
        {
            TripleDESCryptoServiceProvider TripleDes = new TripleDESCryptoServiceProvider();

            // Convert the encrypted text string to a byte array. 
            byte[] encryptedBytes = Convert.FromBase64String(encryptedtext);

            // Create the stream. 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            // Create the decoder to write to the stream. 
            CryptoStream decStream = new CryptoStream(ms, TripleDes.CreateDecryptor(), CryptoStreamMode.Write);

            // Use the crypto stream to write the byte array to the stream.
            decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            decStream.FlushFinalBlock();

            // Convert the plaintext stream to a string. 
            return Encoding.Unicode.GetString(ms.ToArray());
        }

        public static dynamic EncryptRijndaelManaged(string raw, string action)
        {
            string data = "";
            byte[] IV = Encoding.UTF8.GetBytes("4554ds5fg45sdf78");
            byte[] Key = Encoding.UTF8.GetBytes("4554ds5fg45sdf78");

            try
            {
                switch (action)
                {
                    case "E":
                        // Encrypt string    
                        data = Encrypt(raw, Key, IV);
                        break;

                    case "D":
                        // Decrypt the bytes to a string.  
                        data = Decrypt(raw, Key, IV);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exp)
            {
                Util.LogError(exp);
            }

            return data;
        }

        public static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static dynamic EncryptRijndaelManaged(string raw, string action, bool byPass)
        {
            string data = "";
            byte[] IV = Encoding.UTF8.GetBytes("4554ds5fg45sdf78");
            byte[] Key = Encoding.UTF8.GetBytes("4554ds5fg45sdf78");

            if (byPass)
            {
                return raw;
            }

            try
            {
                switch (action)
                {
                    case "E":
                        // Encrypt string    
                        data = Encrypt(raw, Key, IV);
                        break;

                    case "D":
                        // Decrypt the bytes to a string.  
                        data = Decrypt(raw, Key, IV);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exp)
            {
                Util.LogError(exp);
            }

            return data;
        }

        public static void ClsCrypto(string strPassword)
        {
            myRijndael.BlockSize = 128;
            myRijndael.KeySize = 128;
            myRijndael.IV = HexStringToByteArray("4554ds5fg45sdf78"); // //e84ad660c4721ae0e84ad660c4721ae0

            myRijndael.Padding = PaddingMode.PKCS7;
            myRijndael.Mode = CipherMode.CBC;
            iterations = 1000;
            salt = Encoding.UTF8.GetBytes("insight123resultxyz");
            myRijndael.Key = GenerateKey(strPassword);
        }

        public static dynamic Encrypt(string strPlainText, byte[] Key, byte[] IV)
        {
            // myRijndael.BlockSize = 128;
            myRijndael.Key = Key;
            myRijndael.IV = IV;
            myRijndael.KeySize = 128;
            myRijndael.Padding = PaddingMode.PKCS7;
            myRijndael.Mode = CipherMode.CBC;

            byte[] strText = Encoding.UTF8.GetBytes(strPlainText);
            ICryptoTransform transform = myRijndael.CreateEncryptor(Key, IV);
            byte[] cipherText = transform.TransformFinalBlock(strText, 0, strText.Length);

            return Convert.ToBase64String(cipherText);
        }

        public static dynamic Decrypt(string encryptedText, byte[] Key, byte[] IV)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            ICryptoTransform decryptor = myRijndael.CreateDecryptor(Key, IV);
            byte[] originalBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(originalBytes);
        }

        public static byte[] HexStringToByteArray(string strHex)
        {
            dynamic r = new byte[strHex.Length / 2];
            for (int i = 0; i <= strHex.Length - 1; i += 2)
            {
                r[i / 2] = Convert.ToByte(Convert.ToInt32(strHex.Substring(i, 2), 16));
            }
            return r;
        }

        private static byte[] GenerateKey(string strPassword)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(System.Text.Encoding.UTF8.GetBytes(strPassword), salt, iterations);

            return rfc2898.GetBytes(128 / 8);
        }

        #endregion

        public static Dictionary<string, string> StripCardNumber(string cardNumber)
        {
            Dictionary<string, string> results = new Dictionary<string, string>()
            {
                ["firstSix"] = "",
                ["lastFour"] = ""
            };


            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                if (cardNumber.Length == 16)
                {

                    results["firstSix"] = string.Join("", cardNumber.Take(6));
                    results["lastFour"] = string.Join("", cardNumber.Skip(12));

                    return results;
                }

            }
            return null;
        }

        public static async Task<DataTable> SQLHelper(string cmdText, CommandType cmdType, Dictionary<string, object> parameters, string connString = "DBEntities")
        {

            string cString = ConfigurationManager.ConnectionStrings[connString].ConnectionString;
            DataTable dt = new DataTable();
            try
            {
                // Open Connection to database.
                using (SqlConnection sqlCon = new SqlConnection(cString))
                {
                    SqlCommand cmd = new SqlCommand(cmdText, sqlCon);
                    cmd.CommandType = cmdType;
                    if (parameters != null)
                    {
                        // Add all the parameters if they exist.
                        if (parameters.Any())
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
                    }

                    await sqlCon.OpenAsync();

                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    sqlCon.Close();
                    sda.Dispose();

                    return dt;
                }
            }
            catch (SqlException sqlEx)
            {
                LogError(sqlEx);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return null;
        }

        public static async Task<bool> FileHelper(string location, string filename, byte[] fileContents)
        {
            bool success = false;
            try
            {
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                using (FileStream fs = File.OpenWrite(Path.Combine(location, filename)))
                {
                    MemoryStream ms = new MemoryStream(fileContents);
                    await ms.CopyToAsync(fs);
                    return success = true;
                }

            }
            catch (Exception ex)
            {
                LogError(ex);
                success = false;
                return success;
            }
        }

        public static string ConvertTwoLetterNameToThreeLetterName(string name)
        {
            if (name.Length != 2)
            {
                //throw new ArgumentException("name must be two letters.");
                return "JAM";
            }

            name = name.ToUpper();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo culture in cultures)
            {
                RegionInfo region = new RegionInfo(culture.LCID);
                if (region.TwoLetterISORegionName.ToUpper() == name)
                {
                    return region.ThreeLetterISORegionName; //TwoLetterISORegionName
                }
            }

            return null;
        }

    }

    

    public static class DateUtils
    {
        public static List<DateTime> GetWeekdayInRange(this DateTime from, DateTime to, DayOfWeek day)
        {
            const int daysInWeek = 7;
            var result = new List<DateTime>();
            var daysToAdd = ((int)day - (int)from.DayOfWeek + daysInWeek) % daysInWeek;

            do
            {
                from = from.AddDays(daysToAdd);
                result.Add(from);
                daysToAdd = daysInWeek;
            } while (from < to);

            return result;
        }
    }
}