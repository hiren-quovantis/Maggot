using Atlassian.Jira.Remote;
using Maggot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using Maggot.Common.Helper;

namespace Maggot.Common.OAuthHelper
{
    public class JiraAuthHelper
    {
        private const string RequestTokenUrlPart = "/plugins/servlet/oauth/request-token";
        private const string AuthorizeUrlPart = "/plugins/servlet/oauth/authorize";
        private const string AccessTokenUrl = "/plugins/servlet/oauth/access-token";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jiraBaseUrl"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetJiraOAuthUrl(string jiraBaseUrl, RequestToken token)
        {
            return jiraBaseUrl + AuthorizeUrlPart + "?" + "oauth_token=" + token.OAuthToken;
        }

        public static T MakeGetRequest<T>(AccessToken token, string url, List<KeyValuePair<string, string>> extraParameter = null)
        {
            string response;
            var content = GetFormContent(token.OAuthToken, url, extraParameter, "GET");

            using (HttpClient client = new HttpClient())
            {
                var result = client.GetAsync(url + ConvertToQueryString(content));
                response = result.Result.Content.ReadAsStringAsync().Result;
            }

            return JsonConvert.DeserializeObject<T>(response.ToString());
        }

        public static T MakePostRequest<T>(AccessToken token, string url, List<KeyValuePair<string, string>> extraParameter = null)
        {
            string response;
            var content = GetFormContent(token.OAuthToken, url);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url + ConvertToQueryString(content));


            if (extraParameter != null && extraParameter.Count > 0)
            {
                requestMessage.Content = new StringContent("{\"jql\":\""+extraParameter.Where(a=>a.Key == "jql").FirstOrDefault().Value+"\"}", Encoding.UTF8, "application/json");
            }

            using (HttpClient client = new HttpClient())
            {
                var result = client.SendAsync(requestMessage);
                response = result.Result.Content.ReadAsStringAsync().Result;
            }

            return JsonConvert.DeserializeObject<T>(response.ToString());
        }

        public static object MakePutRequest<T>(AccessToken token, string url, List<KeyValuePair<string, string>> extraParameter = null)
        {
            string response;
            var content = GetFormContent(token.OAuthToken, url,null,"PUT");

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put,url + ConvertToQueryString(content));

            if (extraParameter != null && extraParameter.Count > 0)
            {
                requestMessage.Content = new StringContent("{ \"fields\" : { \"assignee\" :{ \"name\" : \""+extraParameter.Where(a=>a.Key == "name").FirstOrDefault().Value+"\"}  } }", Encoding.UTF8, "application/json");
            }

            using (HttpClient client = new HttpClient())
            {
                var result = client.SendAsync(requestMessage);
                response = result.Result.Content.ReadAsStringAsync().Result;
            }

            return JsonConvert.DeserializeObject<object>(response.ToString());
        }

        public static T MakeOAuthRequest<T>(string url, string token = "")
        {
            string response;

            using (HttpClient client = new HttpClient())
            {
                var result = client.PostAsync(url, new FormUrlEncodedContent(GetFormContent(token, url)));
                response = result.Result.Content.ReadAsStringAsync().Result;
            }

            return ParseObject<T>(response);
        }

        public static string EncryptString(string plainText)
        {
            return Base64Encode(plainText);
        }

        public static string DecryptString(string plainText)
        {
            return Base64Decode(plainText);
        }

        #region " Private Helpers "

        public static string ConvertToQueryString(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (!parameters.Any())
                return "";

            var builder = new StringBuilder("?");

            var separator = "";
            foreach (var kvp in parameters.Where(kvp => kvp.Value != null))
            {
                builder.AppendFormat("{0}{1}={2}", separator, WebUtility.UrlEncode(kvp.Key), WebUtility.UrlEncode(kvp.Value.ToString()));

                separator = "&";
            }

            return builder.ToString();
        }

        private static T ParseObject<T>(string response)
        {
            var dict = HttpUtility.ParseQueryString(response);
            string json = JsonConvert.SerializeObject(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static string UrlEncode(string strValue)

        {

            // list of reserved character string which need to encode

            string reservedCharacters = " !*'();:@&=+$,/?%#[]";

            try

            {

                if (String.IsNullOrEmpty(strValue))

                    return String.Empty;

                StringBuilder sbResult = new StringBuilder();

                foreach (char @char in strValue)

                {

                    if (reservedCharacters.IndexOf(@char) == -1)

                        sbResult.Append(@char.ToString());

                    else

                    {

                        sbResult.AppendFormat("%{0:X2}", (int)@char);

                    }

                }

                return sbResult.ToString();

            }

            catch (Exception ex)

            {

                throw ex;

            }

        }

        private static string GenerateSignature(string strSignatureBaseString)

        {


            SHA1Managed shaHASHObject = null;

            try

            {

                // Read the .P12 file to read Private/Public key Certificate

                string certFilePath = Convert.ToString(ConfigurationManager.AppSettings["CertificatePath"]);

                string password = "divyadhuria";

                // Read the Certification from .P12 file. 

                X509Certificate2 cert = new X509Certificate2(certFilePath.ToString(), password);

                // Retrieve the Private key from Certificate.

                RSACryptoServiceProvider RSAcrypt = (RSACryptoServiceProvider)cert.PrivateKey;

                // Create a RSA-SHA1 Hash object 

                shaHASHObject = new SHA1Managed();

                // Create Byte Array of Signature base string

                // byte[] data = System.Text.Encoding.ASCII.GetBytes(strSignatureBaseString);

                var encoder = new UTF8Encoding();
                byte[] data = encoder.GetBytes(strSignatureBaseString);


                // Create Hashmap of Signature base string

                byte[] hash = shaHASHObject.ComputeHash(data);



                // Create Sign Hash of base string 

                // NOTE - 'SignHash' gives correct data. Don't use SignData method

                byte[] rsaSignature = RSAcrypt.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));

                // Convert to Base64 string

                string base64string = Convert.ToBase64String(rsaSignature);

                // Return the Encoded UTF8 string

                return (base64string);

            }

            catch (CryptographicException ex)

            {

                throw ex;

            }

            catch (Exception ex)

            {

                throw ex;

            }

            finally

            {

                // clear the memory allocation 

                if (shaHASHObject != null)

                {

                    shaHASHObject.Dispose();

                }

            }

        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static List<KeyValuePair<string, string>> GetFormContent(string oauthToken, string url, List<KeyValuePair<string, string>> extraParameter = null, string requestType = "POST", string consumerKey = "maggot-bot")
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string timestamp = Convert.ToInt64(ts.TotalSeconds).ToString();

            string nonce = new Random().Next(123400, 9999999).ToString();

            System.Collections.Generic.List<string> parameters = new System.Collections.Generic.List<string>();

            //parameters.Add(("oauth_callback") +  "=" + (CallbackUrl + "?state=abbccd"));
            parameters.Add(("oauth_consumer_key") + "=" + (consumerKey));
            parameters.Add(("oauth_nonce") + "=" + (nonce));
            parameters.Add(("oauth_timestamp") + "=" + (timestamp));
            parameters.Add(("oauth_signature_method") + "=" + ("RSA-SHA1"));

            if (!string.IsNullOrEmpty(oauthToken))
            {
                parameters.Add(("oauth_token") + "=" + (oauthToken));
            }

            parameters.Sort();
            string parametersStr = string.Join("&", parameters.ToArray());

            string baseStr = requestType + "&" +
                             UrlEncode(url) + "&" +
                             UrlEncode(parametersStr);

            string hash = GenerateSignature(baseStr);

            List<KeyValuePair<string, string>> content = new List<KeyValuePair<String, String>>
               {
                   // new KeyValuePair<string, string>("oauth_callback", UrlEncode(CallbackUrl + "?state=abcdef")),
                    new KeyValuePair<string, string>("oauth_consumer_key", HttpUtility.UrlEncode(consumerKey)),
                     new KeyValuePair<string, string>("oauth_nonce", HttpUtility.UrlEncode(nonce)),
                     new KeyValuePair<string, string>("oauth_timestamp", HttpUtility.UrlEncode(timestamp)),
                     new KeyValuePair<string, string>("oauth_signature_method","RSA-SHA1"),
                     new KeyValuePair<string, string>("oauth_signature", (hash))
                };

            if (!string.IsNullOrEmpty(oauthToken))
            {
                content.Add(new KeyValuePair<string, string>("oauth_token", HttpUtility.UrlEncode(oauthToken)));
            }

            return content;
        }

        #endregion

    }
}