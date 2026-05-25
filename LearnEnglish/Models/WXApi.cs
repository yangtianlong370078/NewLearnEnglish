using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace LearnEnglish.Models
{
    public class WXApi
    {
        /// <summary>
        /// appid
        /// </summary>
       // private static string AppID = "wx7693a822c95407d8";

        public static string AppID = "wx229d8ad441b0b9fa";

        /// <summary> 
        /// 小程序密钥
        /// </summary>
       // private static string AppSecret = "08491cc7f85688e430c4ffa8a1b8db35--b7ad502e23b9f83b8f32735436dcc40c";

        public static string AppSecret = "1e9fe6674e0dfb393214909a7d6dc2ab";


        /// <summary>
        /// 获得用户唯一标识（OpenID）
        /// </summary>
        /// <param name="js_code"></param>
        /// <returns></returns>
        public static string GetOpenID(string js_code)
        {
            string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", AppID, AppSecret, js_code);
            return RequestResult(url);
        }

        /// <summary>
        /// 获取token
        /// </summary>
        /// <returns></returns>
        public static string GetToken()
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", AppID, AppSecret);
            string result = RequestResult(url);
            dynamic obj = JsonConvert.DeserializeObject(result, typeof(object));
            return obj.access_token;
        }

        /// <summary>
        /// 生成16位的随机字符串
        /// </summary>
        /// <returns></returns>
        public static string GenerateNonceStr()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16); 
        }

        /// <summary>
        /// 返回的是秒级时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); 
        }



        public static string GetJsApiTicket(string accessToken)
        {
            string url = $"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={accessToken}&type=jsapi";

           var result= RequestResult(url);
            dynamic obj = JsonConvert.DeserializeObject(result, typeof(object));
            return obj.ticket;
        }

        public static string GetSignature(string url,string jsapiTicket, string nonceStr, string timestamp)
        {
            
            // 3. 生成string1  
            string string1 = $"jsapi_ticket={jsapiTicket}&noncestr={nonceStr}&timestamp={timestamp}&url={UrlEncode(url)}";

            // 4. 对string1进行SHA1加密，生成signature  
            string signature = GetSha1(string1);

            return signature;
        }

        private static string GetSha1(string original)
        {
            original = original.ToLower();
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(original));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string UrlEncode( string value)
        {
            return Uri.EscapeDataString(value);
        }

        /// <summary>
        /// 网络请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string RequestResult(string url)
        {
            HttpWebRequest requeest = (HttpWebRequest)WebRequest.Create(url);
            requeest.Method = "GET";
            requeest.ContentType = "text/html;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)requeest.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }

    }
}
