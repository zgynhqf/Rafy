/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121109 17:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121109 17:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Rafy.MultiLanguages
{
    public class Bing : TranslationEngine
    {
        private bool _needSleep = false;

        private string _currentToken;

        private string GetCurrentToken()
        {
            if (this._currentToken == null)
            {
                this._currentToken = this.GetMSToken();
            }

            return this._currentToken;
        }

        private string GetMSToken()
        {
            var headers = this.Client.Headers;
            headers[HttpRequestHeader.Host] = "www.microsofttranslator.com";
            headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.4 (KHTML, like Gecko) Chrome/22.0.1229.8 Safari/537.4";
            headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate,sdch";
            headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.8";
            headers[HttpRequestHeader.AcceptCharset] = "ISO-8859-1,utf-8;q=0.7,*;q=0.3";

            var js = this.Client.DownloadString("http://www.microsofttranslator.com/ajax/v2/toolkit.ashx?loc=en&toolbar=none&ref=SALL&176690");

            var match = Regex.Match(js, @"window\['_mstConfig'\]=\{appId:'(?<appId>[^']+)'", RegexOptions.Singleline);
            if (match != null)
            {
                var appId = match.Groups["appId"].Value;
                appId = appId.Replace("\\x2a", "*");
                return appId;
            }

            return string.Empty;
        }

        protected override void OnTranslating()
        {
            base.OnTranslating();

            this._needSleep = false;
        }

        protected override IList<string> Request(IList<MappingInfo> batch, string language)
        {
            if (this._needSleep) Thread.Sleep(2000);//睡眠，以免 IP 被禁止。

            var words = new StringBuilder();
            for (int i = 0, c = batch.Count; i < c; i++)
            {
                var item = batch[i];
                //格式化输入
                var word = item.DevLanguageRD
                    .Replace("\"", "\\\"")
                    .Replace("#", string.Empty)
                    .Replace(Environment.NewLine, string.Empty);

                if (i > 0) { words.Append(','); }
                words.Append('"').Append(word).Append('"');
            }

            //调用 API并获取输出
            var token = this.GetCurrentToken();
            if (string.IsNullOrEmpty(token)) return new string[0];

            string url = string.Format(@"http://api.microsofttranslator.com/v2/ajax.svc/TranslateArray?appId=""{2}""&texts=[{0}]&to=""{1}""", words, language, token);
            var json = this.Client.DownloadString(url);
            if (json.Contains("ArgumentException")) return new string[0];

            json = json.Replace("\\\"", "\"");

            var translatedWords = this.ParseResultInJson(json);

            this._needSleep = true;

            return translatedWords;
        }

        private IList<string> ParseResultInJson(string json)
        {
            var matches = Regex.Matches(json, @"""TranslatedText"":\\?""(?<result>[^""]+)\\?""");//"TranslatedText":"Difference"

            var result = new List<string>(matches.Count);

            foreach (Match match in matches)
            {
                var translatedText = match.Groups["result"].Value;
                result.Add(translatedText);
            }

            return result;
        }

        #region GetMyToken

        private string GetMyToken()
        {
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            var admAuth = new AdmAuthentication("TestMSTranslator", "TestMSTranslator_Client_Secret");

            var admToken = admAuth.GetAccessToken();
            // Create a header with the access_token property of the returned token
            var headerValue = "Bearer " + admToken.access_token;

            return headerValue;
        }

        #region From MSDN

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
                this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", clientId, clientSecret);
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
                using (var outputStream = webRequest.GetRequestStream())
                {
                    outputStream.Write(bytes, 0, bytes.Length);
                }
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    var serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                    //Get deserialized object from JSON stream
                    AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                    return token;
                }
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

        #endregion

        #endregion
    }
}