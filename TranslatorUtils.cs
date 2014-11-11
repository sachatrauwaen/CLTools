using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using DotNetNuke.Services.Exceptions;

/// <summary>
/// Description résumée de TranslatorUtils
/// </summary>
public class TranslatorUtils
{

    static public string[] TranslateArray(string[] texts, string from, string to) {

        //string[] r = texts.Select(t => t + " " + to).ToArray<string>();

        //return r;

        string headerValue = "Bearer " + GetAccesToken();

        var bind = new BasicHttpBinding();
        bind.Name = "BasicHttpBinding_LanguageService";
        var epa = new EndpointAddress("http://api.microsofttranslator.com/V2/soap.svc");


        LanguageServiceClient client = new LanguageServiceClient(bind, epa);

        //Set Authorization header before sending the request
        HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
        httpRequestProperty.Method = "POST";
        httpRequestProperty.Headers.Add("Authorization", headerValue);

        // Creates a block within which an OperationContext object is in scope.
        using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
        {
            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

            //string translationResult;
            //Keep appId parameter blank as we are sending access token in authorization header.

            //translationResult = client.Translate("", sourceText, "en", "fr", "text/html", "general");

            var translatedTexts = client.TranslateArray("", texts, from, to, new Microsoft.MT.Web.Service.V2.TranslateOptions());
            string[] res = translatedTexts.Select(t=> t.TranslatedText).ToArray();
            return res;
        } 
    
    }

    public static bool TranslateConfigured() {
        string clientId = DotNetNuke.Common.Utilities.Config.GetSetting("MSTranslatorClientId");
        string clientSecret = DotNetNuke.Common.Utilities.Config.GetSetting("MSTranslatorClientSecret");

        return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret);    
    }

    private static string GetAccesToken()
    {        
        //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
        //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 

        string clientId = DotNetNuke.Common.Utilities.Config.GetSetting("MSTranslatorClientId");
        string clientSecret = DotNetNuke.Common.Utilities.Config.GetSetting("MSTranslatorClientSecret");

        AdmAuthentication admAuth = new AdmAuthentication(clientId, clientSecret);        
        try
            {
                AdmAccessToken admToken = admAuth.GetAccessToken();
                return admToken.access_token;
            }
        catch (WebException e)
        {
            
            // Obtain detailed error information
            string strResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)e.Response)
            {
                if (response != null)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                }
            }
            throw new Exception(string.Format("Http status code={0}, error message={1}", e.Status, strResponse), e);
        }
        catch (Exception ex)
        {
            throw ex;
            
        }                
    }

    private static void ProcessWebException(WebException e)
    {
        Console.WriteLine("{0}", e.ToString());
        // Obtain detailed error information
        string strResponse = string.Empty;
        using (HttpWebResponse response = (HttpWebResponse)e.Response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                {
                    strResponse = sr.ReadToEnd();
                }
            }
        }
        Console.WriteLine("Http status code={0}, error message={1}", e.Status, strResponse);
    }
}
    [DataContract]
    public class AdmAccessToken
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string scope { get; set; }
    }

    public class AdmAuthentication
    {
        public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        private string clientId;
        private string cientSecret;
        private string request;

        public AdmAuthentication(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.cientSecret = clientSecret;
            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
        }

        public AdmAccessToken GetAccessToken()
        {
            return HttpPost(DatamarketAccessUri, this.request);
        }

        private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
        {
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                //Get deserialized object from JSON stream
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                return token;
            }
        }
    }



