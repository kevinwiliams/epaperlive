using System;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace ePaperLive
{
    public class Util
    {
        public static string PasswordHash(string value)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(value))
                );
        }

        public class ObjectSerializer<T> where T : class
        {
            public static string Serialize(T obj)
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
                //remove generated namespaces
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                using (var sww = new StringWriter())
                {
                    using (XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented })
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
    }
}