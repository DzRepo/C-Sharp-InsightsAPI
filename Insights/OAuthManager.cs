using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
namespace Insights
{
    public class OAuthManager
    {
        #region class private properties
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _token;
        private readonly string _tokenSecret;
        private string _baseUrl = "https://data-api.twitter.com/insights/";
        #endregion

        #region Constructor
        public OAuthManager(string consumerKey, string consumerSecret, string token = null, string tokenSecret = null)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;
        }
        #endregion

        #region Public Methods


        // Created from scratch, but with parts and after reviewing close to 10 different OAuth code libraries.
        public OAuthResponse GetOAuthResponse(string method, string endpointUrl, string postData = null)
        {
            var oauthResponse = new OAuthResponse();
            const string oauthVersion = "1.0";
            const string oauthSignatureMethod = "HMAC-SHA1";

            // if an entire URL is passed, use it, otherwise assume the base url needs to be prepended.
            string url;
            if (endpointUrl.ToUpper().StartsWith("HTTP"))
                url = endpointUrl;
            else
                url = _baseUrl + endpointUrl;

            // unique request details
            var oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauthTimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            SortedDictionary<string, string> basestringParameters = new SortedDictionary<string, string>();
            basestringParameters.Add("oauth_version", oauthVersion);
            basestringParameters.Add("oauth_consumer_key", _consumerKey);
            basestringParameters.Add("oauth_nonce", oauthNonce);
            basestringParameters.Add("oauth_signature_method", oauthSignatureMethod);
            basestringParameters.Add("oauth_timestamp", oauthTimestamp);
            // Some requests don't require a token/tokenSecret, so don't assume it's there.
            if (_token != null)
                basestringParameters.Add("oauth_token", _token);

            //Build the signature string
            StringBuilder baseString = new StringBuilder();
            baseString.Append(method.ToUpper() + "&");
            baseString.Append(Uri.EscapeDataString(url) + "&");
            foreach (KeyValuePair<string, string> entry in basestringParameters)
            {
                baseString.Append(Uri.EscapeDataString(entry.Key + "=" + entry.Value + "&"));
            }

            // This strips off the ending 3 characters, which are the URL encoded &
            string finalBaseString = baseString.ToString().Substring(0, baseString.Length - 3);
            
            //Build the signing key
            string signingKey;
            // Some requests don't require a token/tokenSecret, so don't assume it's there.  
            // Note the trailing "&" is required in that case though.
            if (_tokenSecret == null)
                signingKey = Uri.EscapeDataString(_consumerSecret) + "&";
            else
                signingKey = Uri.EscapeDataString(_consumerSecret) + "&" + Uri.EscapeDataString(_tokenSecret);

#if TRACE

            Console.WriteLine("Signing Key:");
            Console.WriteLine(signingKey);
#endif
            //Sign the request
            HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));
            string signatureString =
                Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(finalBaseString)));

#if TRACE
            Console.WriteLine("Signature:");
            Console.WriteLine(signatureString);
#endif
            //  Tell Twitter we don't expect HTTP 100 responses, so don't bother.
            ServicePointManager.Expect100Continue = false;

            HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(url);

            StringBuilder authorizationHeaderParams = new StringBuilder();
            authorizationHeaderParams.Append("OAuth ");
            authorizationHeaderParams.Append("oauth_consumer_key=" + "\"" + Uri.EscapeDataString(_consumerKey) + "\",");
            authorizationHeaderParams.Append("oauth_nonce=" + "\"" + Uri.EscapeDataString(oauthNonce) + "\",");
            authorizationHeaderParams.Append("oauth_signature=" + "\"" + Uri.EscapeDataString(signatureString) + "\",");
            authorizationHeaderParams.Append("oauth_signature_method=" + "\"" + Uri.EscapeDataString(oauthSignatureMethod) + "\",");
            authorizationHeaderParams.Append("oauth_timestamp=" + "\"" + Uri.EscapeDataString(oauthTimestamp) + "\",");
            if (_token != null)
                authorizationHeaderParams.Append("oauth_token=" + "\"" + Uri.EscapeDataString(_token) + "\",");
            authorizationHeaderParams.Append("oauth_version=" + "\"" + Uri.EscapeDataString(oauthVersion) + "\"");

            webRequest.Headers.Add("Authorization", authorizationHeaderParams.ToString());
            webRequest.Method = method.ToUpper();

            // If you are having trouble with a different OAuth library, but would rather use it than this one (but why?)
            // This setting is probably the one that's keeping it from working.
            webRequest.AutomaticDecompression = DecompressionMethods.GZip;

            //  Required as of GA of the Insights APIs.
            webRequest.Headers.Add("Accept-Response", "gzip");

#if TRACE
            Console.WriteLine("Headers:");
            Console.WriteLine(webRequest.Headers);
#endif
            if (postData != null)
            {
                //  For this library, any data posted will be in JSON form.  For more general usage, this would be a parameter.
                webRequest.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    streamWriter.Write(postData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
#if TRACE
                Console.WriteLine("Body:");
                Console.WriteLine(postData);
                Console.WriteLine("Sending " + webRequest.ContentLength + " bytes");
#endif
            }
            //Allow us a reasonable timeout in case Twitter's busy.  (30 seconds seems fair.)
            webRequest.Timeout = 30*1000;  
            HttpWebResponse webResponse = null;

            // Because httpWebResponse likes throwing exceptions for common HTTP responses, we need to trap them.
            try
            {
                webResponse = webRequest.GetResponse() as HttpWebResponse;
            }
            catch (WebException wex)
            {
                // start with exception text
                oauthResponse.ResponseString = wex.Message;
                oauthResponse.ErrorFlag = true;
                oauthResponse.Error = wex;

                // Next try to improve error message if server sent a response.
                using (WebResponse response = wex.Response)
                {
                    using (Stream data = response.GetResponseStream())
                        if (data != null)
                            using (var reader = new StreamReader(data))
                            {
                                string text = reader.ReadToEnd();
                                oauthResponse.ResponseString = text;
                            }
                }
            }

            // If the server response is valid, grab it.
            if (webResponse != null)
            {
                Stream dataStream = webResponse.GetResponseStream();
                if (dataStream != null)
                {
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
#if TRACE
                        Console.WriteLine("Response:" + responseFromServer);
#endif
                    oauthResponse.ResponseString = responseFromServer;
                }
            }
            return oauthResponse;
        }
        #endregion
    }
}
    