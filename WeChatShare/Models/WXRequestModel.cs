using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace WeChatShare.Models
{
    /// <summary>
    /// access_token Model
    /// </summary>
    public class AccessTokenModel
    {
        /// <summary>
        /// 获取到的凭证
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// 凭证有效时间，单位：秒
        /// </summary>
        public string expires_in { get; set; }
    }
    /// <summary>
    /// jsapi_ticket Model
    /// </summary>
    public class JSAPITicketModel
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public string errcode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string errmsg { get; set; }
        /// <summary>
        /// 凭证
        /// </summary>
        public string ticket { get; set; }
        /// <summary>
        /// 凭证有效时间，单位：秒
        /// </summary>
        public string expires_in { get; set; }
    }
    /// <summary>
    /// 微信请求数据类
    /// </summary>
    public class WXRequestModel
    {
        /// <summary>
        /// AppID，由相关微信应用提供
        /// </summary>
        public string AppID { get; set; }
        /// <summary>
        /// APP Secret，由相关微信应用提供
        /// </summary>
        public string AppSecret { get; set; }
        /// <summary>
        /// 生成签名的时间戳
        /// </summary>
        public string TimeStamp { get; set; }
        /// <summary>
        /// 生成签名的随机串
        /// </summary>
        public string NonceStr { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns>时间戳字符串</returns>  
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        /// <summary>
        /// 获取随机字符串
        /// </summary>
        /// <returns></returns>
        public static string GetRandomString()
        {
            return Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 获取 access_token
        /// </summary>
        /// <param name="appId">AppID</param>
        /// <param name="appSecret">AppSecret</param>
        /// <returns>access_token</returns>
        public static string GetAccessToken(string appId, string appSecret)
        {
            //从缓存中获取数据
            string key = "Index_WeiXin_AccessToken_Object";
            object cacheValue = Caching.Get(key);
            AccessTokenModel model = new AccessTokenModel();
            if (null != cacheValue)
            {
                model = (AccessTokenModel)cacheValue;
            }
            else
            {
                string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";
                url = string.Format(url, appId, appSecret);
                //数据格式：{"access_token":"9Mq11o7k7k8JRqCg3wA6rCvfn19mX-_jMIwHRQJjn1V4o_fIbasJh_HZvNB30eiFTB3DgcGXd7S0zyxucpY7CajIMHJWLvfLnxwP8hV09kMWeyF8A7Ab_xCMLYQlw1fgPLGfCHAGCX","expires_in":7200}
                string jsonTokenStr = HttpReqHelper.Get(url, "application/json; encoding=utf-8");
                model = JsonConvert.DeserializeObject<AccessTokenModel>(jsonTokenStr);
                //写入缓存
                int cacheMinute = int.Parse(model.expires_in) / 60;
                Caching.Set(key, model, cacheMinute);
            }
            return model.access_token;
        }
        /// <summary>
        /// 获取 jsapi_ticket
        /// </summary>
        /// <param name="accessToken">access_token</param>
        /// <returns>jsapi_ticket</returns>
        public static string GetJSAPITicket(string accessToken)
        {
            //从缓存中获取数据
            string key = "Index_WeiXin_JSAPITicket_Object";
            object cacheValue = Caching.Get(key);
            JSAPITicketModel model = new JSAPITicketModel();
            if (null != cacheValue)
            {
                model = (JSAPITicketModel)cacheValue;
            }
            else
            {
                string url = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi";
                url = string.Format(url, accessToken);
                //数据格式：{"errcode":0,"errmsg":"ok","ticket":"sM4AOVdWfPE4DxkXGEs8VFJBGFGvY36lPcvYFhMXlNiDijbMlzrPpY1utBBzZj5QAglcHlbx4MZsB5LRza8nwQ","expires_in":7200}
                string jsonTokenStr = HttpReqHelper.Get(url, "application/json; encoding=utf-8");
                model = JsonConvert.DeserializeObject<JSAPITicketModel>(jsonTokenStr);
                //写入缓存
                int cacheMinute = int.Parse(model.expires_in) / 60;
                Caching.Set(key, model, cacheMinute);
            }
            return model.ticket;
        }
        /// <summary>
        /// 获取 Signature
        /// </summary>
        /// <param name="jsAPITicket">jsapi_ticket</param>
        /// <param name="nonceStr">生成签名的随机串</param>
        /// <param name="timeStamp">生成签名的时间戳</param>
        /// <param name="url">使用页面的 url</param>
        /// <returns>Signature</returns>
        public static string GetSignature(string jsAPITicket, string nonceStr, string timeStamp, string url)
        {
            //注意这里参数名必须全部小写，且必须有序
            //步骤1. 对所有待签名参数按照字段名的 ASCII 码从小到大排序（字典序）后，使用 URL 键值对的格式（即 key1=value1&key2=value2…）拼接成字符串 string1 
            string string1 = "jsapi_ticket=" + jsAPITicket + "&noncestr=" + nonceStr
                    + "&timestamp=" + timeStamp + "&url=" + url;
            //步骤2. 对 string1 进行 sha1 签名，得到 signature
            string signature = SHA1Encrypt(string1);// FormsAuthentication.HashPasswordForStoringInConfigFile(string1, "SHA1");
            return signature;
        }


        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="intput">输入字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string SHA1Encrypt(string intput)
        {
            byte[] StrRes = Encoding.Default.GetBytes(intput);
            HashAlgorithm mySHA = new SHA1CryptoServiceProvider();
            StrRes = mySHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte Byte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", Byte);
            }
            return EnText.ToString();
        }
        /// <summary>
        /// 获取 Signature
        /// </summary>
        /// <param name="appId">AppID</param>
        /// <param name="appSecret">AppSecret</param>
        /// <param name="nonceStr">生成签名的随机串</param>
        /// <param name="timeStamp">生成签名的时间戳</param>
        /// <param name="url">使用页面的 url</param>
        /// <returns>Signature</returns>
        public static string GetSignature(string appId, string appSecret, string nonceStr, string timeStamp, string url)
        {
            string accessToken = WXRequestModel.GetAccessToken(appId, appSecret);
            string jsAPITicket = WXRequestModel.GetJSAPITicket(accessToken);
            string signature = GetSignature(jsAPITicket, nonceStr, timeStamp, url);
            return signature;
        }
    }

    internal class HttpReqHelper
    {
        internal static string Get(string url, string v)
        {
            return HttpUtils.HttpGet(url, v);
        }
    }

    public static class HttpUtils
    {
        /// <summary>
        /// httpPost
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="postDataStr">数据</param>
        /// <param name="contentType">类型</param>
        /// <returns></returns>
        public static string HttpPost(string url, string postDataStr, string contentType)
        {
            //请求服务
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Timeout = 20000;
            byte[] btBody = Encoding.UTF8.GetBytes(postDataStr);
            httpWebRequest.ContentLength = btBody.Length;
            httpWebRequest.GetRequestStream().Write(btBody, 0, btBody.Length);

            //服务器响应
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            httpWebResponse.Close();
            streamReader.Close();
            httpWebRequest.Abort();
            httpWebResponse.Close();
            return responseContent;
        }

        /// <summary>
        /// httpGet请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="contentType">内容类型</param>
        /// <returns></returns>
        public static string HttpGet(string url, string contentType)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 20000;
            httpWebRequest.ContentType = contentType;
            string responsetContent = string.Empty;
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                if (httpWebResponse != null)
                {
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            responsetContent = reader.ReadToEnd();
                        }
                    }

                }
            }
            return responsetContent;
        }
    }
}