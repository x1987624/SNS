using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Tunynet.Common
{
    /// <summary>
    /// 远程页面请求
    /// </summary>
    public class HttpWebHelper
    {
        /// <summary>
        /// 获取请求结果
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="timeout">超时时间(秒)</param>
        /// <param name="requestXML">请求xml内容</param>
        /// <param name="isPost">是否post提交</param>
        /// <param name="encoding">编码格式 例如:utf-8</param>
        /// <param name="msg">抛出的错误信息</param>
        /// <returns>返回请求结果</returns>
        public static string HttpWebRequest(string requestUrl, string requestXML, bool isPost, string encoding, out string msg, int timeout = 60)
        {
            msg = string.Empty;
            string result = string.Empty;
            try
            {
                byte[] bytes = Encoding.GetEncoding(encoding).GetBytes(requestXML);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Referer = requestUrl;
                request.Method = isPost ? "POST" : "GET";
                request.ContentLength = bytes.Length;
                request.Timeout = timeout * 1000;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding(encoding));
                    result = reader.ReadToEnd();
                    reader.Close();
                    responseStream.Close();
                    request.Abort();
                    response.Close();
                    return result.Trim();
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
            }

            return result;
        }
    }
}
