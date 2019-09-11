//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace JCHVRF_New.Utility.WebClientCall
{
    //public enum ContentType
    //{
    //    XML,
    //    JSON
    //}
    //public abstract class WebClientProxyBase
    //{
    //    #region ----Properties----
    //    private string loggedInUserName { get; set; }
    //    private string loggedInUserPassword { get; set; }
    //    private string apiBaseAddress { get; set; }
    //    private string authorizationCredentials
    //    {
    //        get
    //        {
    //            byte[] credBuf = new System.Text.UTF8Encoding().GetBytes(string.Format("{0}:{1}", loggedInUserName, loggedInUserPassword));
    //            string authorization = Convert.ToBase64String(credBuf);
    //            return authorization;
    //        }
    //    }
    //    #endregion

    //    #region ----Constructors----
    //    private WebClientProxyBase() { }

    //    public WebClientProxyBase(string loggedInUserName, string loggedInUserPassword, string apiUrl)
    //    {
    //        this.loggedInUserName = loggedInUserName;
    //        //EncryptionAndDecryption encryptionAndDecryption = new EncryptionAndDecryption();
    //        //this.loggedInUserPassword = encryptionAndDecryption.Decrypt(loggedInUserPassword);
    //        this.loggedInUserPassword = loggedInUserPassword;
    //        // this.loggedInUserPassword = "abc@123";
    //        this.apiBaseAddress = apiUrl;
    //    }
    //    #endregion

    //    private HttpClient GetHttpClient(ContentType contentType)
    //    {
    //        HttpClient client = new HttpClient();
    //        client.BaseAddress = new Uri(apiBaseAddress);
    //        client.DefaultRequestHeaders.Accept.Clear();
    //        switch (contentType)
    //        {
    //            case ContentType.XML:
    //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
    //                break;
    //            case ContentType.JSON:
    //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //                break;
    //            default:
    //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //                break;
    //        }

    //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationCredentials);
    //        return client;
    //    }

    //    private ByteArrayContent GetRequestContent<T>(T request)
    //    {
    //        string serializedRequest = JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.None);
    //        var buffer = System.Text.Encoding.UTF8.GetBytes(serializedRequest);
    //        var byteContent = new ByteArrayContent(buffer);
    //        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    //        return byteContent;
    //    }
    //    public virtual R ExecuteJsonRequest<T, R>(string controller, string action, T request) where T : class where R : class
    //    {
           
    //        object response = null;
    //        string controllerUri = string.Format(@"{0}/{1}", controller, action);
    //        HttpClient client = GetHttpClient(ContentType.JSON);
    //        var byteContent = GetRequestContent(request);
    //        string responseMsg = string.Empty;
    //        HttpResponseMessage message = client.PostAsync(controllerUri, byteContent).Result;
    //        responseMsg = message.Content.ReadAsStringAsync().Result;
    //        if (message.IsSuccessStatusCode)
    //            response = JsonConvert.DeserializeObject<R>(responseMsg);
    //        else
    //        {
               
    //            responseMsg = string.Empty;
    //        }
    //        return ((R)response);
    //    }
    //    public virtual R ExecuteXmlRequest<T, R>(string controller, string action, T request)
    //    {
    //        object response = null;
    //        string controllerUri = string.Format(@"{0}/{1}", controller, action);
    //        HttpClient client = GetHttpClient(ContentType.XML);
    //        var byteContent = GetRequestContent<T>(request);
    //        string responseMsg = string.Empty;
    //        HttpResponseMessage message = client.PostAsync(controllerUri, byteContent).Result;
    //        responseMsg = message.Content.ReadAsStringAsync().Result;
    //        if (message.IsSuccessStatusCode)
    //        {
    //            responseMsg = RemoveAllNamespaces(responseMsg);
    //            System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(R));
    //            using (StringReader reader = new StringReader(responseMsg))
    //                response = xmlSerializer.Deserialize(reader);
    //        }
    //        else
    //        {
                
    //            responseMsg = string.Empty;
    //        }
    //        return ((R)response);
    //    }

    //    private string RemoveAllNamespaces(string xmlDocument)
    //    {
    //        XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

    //        return xmlDocumentWithoutNs.ToString();
    //    }

    //    private XElement RemoveAllNamespaces(XElement xmlDocument)
    //    {
    //        if (!xmlDocument.HasElements)
    //        {
    //            XElement xElement = new XElement(xmlDocument.Name.LocalName);
    //            xElement.Value = xmlDocument.Value;

    //            foreach (XAttribute attribute in xmlDocument.Attributes())
    //                xElement.Add(attribute);

    //            return xElement;
    //        }
    //        return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
    //    }
    //    /// <summary>
    //    public virtual R ExecuteRequest<T, R>(string controller, string action, T request, ContentType contentType) where T : class where R : class
    //    {
    //        object response = null;
    //        switch (contentType)
    //        {
    //            case ContentType.XML:
    //                response = ExecuteXmlRequest<T, R>(controller, action, request);
    //                break;
    //            case ContentType.JSON:
    //                response = ExecuteJsonRequest<T, R>(controller, action, request);
    //                break;
    //            default:
    //                response = ExecuteJsonRequest<T, R>(controller, action, request);
    //                break;
    //        }
    //        return ((R)response);
    //    }
    //    public string WebClientRequest(string soap ,string Url,string UserName,string Password,string IPAddress)
    //    {
    //        string url = Url;
    //        string host = IPAddress;
    //        byte[] credBuf = new System.Text.UTF8Encoding().GetBytes(UserName + ":" + Password);
    //        string authorization = "Basic " + Convert.ToBase64String(credBuf);

    //        try
    //        {
    //            byte[] soapBytes = Encoding.UTF8.GetBytes(soap);

    //            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
    //            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
    //            req.Host = host;
    //            req.Accept = "text/*";
    //            req.ContentType = "text/xml";
    //            req.Headers.Add("");
    //            req.ContentLength = soapBytes.Length;
    //            req.Headers.Add("Authorization: " + authorization);
    //            req.Method = "POST";

    //            Stream stm = req.GetRequestStream();
    //            stm.Write(soapBytes, 0, soapBytes.Length);
    //            //stm.Close();

    //            using (WebResponse response = req.GetResponse())
    //            {
    //                Stream responseStream = response.GetResponseStream();
    //                StreamReader reader = new StreamReader(responseStream);
    //                string responseFromServer = reader.ReadToEnd();
    //                return responseFromServer;
    //            }
    //        }
    //        catch (WebException e)
    //        {
    //            try
    //            {
    //                using (WebResponse response = e.Response)
    //                {
    //                    HttpWebResponse httpResponse = (HttpWebResponse)response;       //httpResponse.StatusCode
    //                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
    //                    return streamReader.ReadToEnd().ToString();
    //                }
    //            }
    //            catch (Exception exc)
    //            {
                   
    //                return e.Status.ToString();
    //            }

    //        }
    //        catch (Exception exc)
    //        {
               
    //            return exc.Message;
    //        }
    //    }
    //}
}

