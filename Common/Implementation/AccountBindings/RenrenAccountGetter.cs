﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;
using Tunynet.Utilities;
using System.Web.Helpers;
using System.Text;
using System.Web;
using RestSharp;
using Tunynet;

namespace Spacebuilder.Common
{
    /// <summary>
    /// 人人帐号获取器
    /// </summary>
    public class RenrenAccountGetter : ThirdAccountGetter
    {
        private RestClient _restClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RenrenAccountGetter()
        {
            _restClient = new RestClient();
        }

        /// <summary>
        /// 名称
        /// </summary>
        public override string AccountTypeName
        {
            get { return "人人帐号"; }
        }

        /// <summary>
        /// 官方网站地址
        /// </summary>
        public override string AccountTypeUrl
        {
            get { return "http://www.renren.com/"; }
        }

        /// <summary>
        /// 帐号类型Key
        /// </summary>
        public override string AccountTypeKey
        {
            get { return AccountTypeKeys.Instance().Renren(); }
        }

        /// <summary>
        /// 获取第三方网站空间主页地址
        /// </summary>
        /// <param name="identification"></param>
        /// <returns></returns>
        public override string GetSpaceHomeUrl(string identification)
        {
            return string.Format("http://www.renren.com/{0}/profile", identification);
        }

        /// <summary>
        /// 获取身份认证Url
        /// </summary>
        /// <returns></returns>
        public override string GetAuthorizationUrl()
        {
            string getAuthorizationCodeUrlPattern = "https://graph.renren.com/oauth/authorize?client_id={0}&redirect_uri={1}&response_type=code&scope=status_update";
            return string.Format(getAuthorizationCodeUrlPattern, AccountType.AppKey, WebUtility.UrlEncode(CallbackUrl));
        }

        /// <summary>
        /// 获取当前第三方帐号上的访问授权
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="expires_in">有效期（单位：秒）</param>
        /// <returns></returns>
        public override string GetAccessToken(System.Web.HttpRequestBase Request, out int expires_in)
        {
            string code = Request.QueryString.GetString("code", string.Empty);
            _restClient.BaseUrl = "https://graph.renren.com";
            _restClient.Authenticator = null;
            var request = new RestRequest(Method.GET);
            request.Resource = "oauth/token?grant_type=authorization_code&client_id={appkey}&client_secret={appsecret}&code={code}&redirect_uri={callbackurl}";
            request.AddParameter("appkey", AccountType.AppKey, ParameterType.UrlSegment);
            request.AddParameter("appsecret", AccountType.AppSecret, ParameterType.UrlSegment);
            request.AddParameter("code", code, ParameterType.UrlSegment);
            request.AddParameter("callbackurl", CallbackUrl, ParameterType.UrlSegment);
            var response = Execute(_restClient, request);
            dynamic json = Json.Decode(response.Content);
            expires_in = json.expires_in;
            return json.access_token;
        }

        /// <summary>
        /// 获取当前第三方帐号上的用户
        /// </summary>
        /// <param name="accessToken">访问授权</param>
        /// <param name="identification"></param>
        /// <returns></returns>
        public override ThirdUser GetThirdUser(string accessToken, string identification = null)
        {
            _restClient.BaseUrl = "https://api.renren.com/v2/user/get";
            _restClient.Authenticator = new CommonOAuthAuthenticator(accessToken);
            var request = new RestRequest(Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("access_token", accessToken);
            var response = Execute(_restClient, request);
            var renrenUser = Json.Decode(response.Content);
            if (renrenUser.Count == 0)
                return null;

            if (renrenUser.error_code != null && renrenUser.error_msg != null)
                return null;

            int avatorCount = renrenUser.response.avatar.Length;
            return new ThirdUser
            {
                AccountTypeKey = AccountType.AccountTypeKey,
                Identification = renrenUser.response.id.ToString(),
                AccessToken = accessToken,
                NickName = renrenUser.response.name,
                Gender = renrenUser.response.basicInformation.sex == "FEMALE" ? GenderType.Male : GenderType.FeMale,
                UserAvatarUrl = avatorCount > 0 ? renrenUser.response.avatar[avatorCount - 1].url : string.Empty
            };
        }

        /// <summary>
        /// 发一条纯文本的微博消息
        /// </summary>
        /// <param name="accessToken">访问授权</param>
        /// <param name="content">微博内容</param>
        /// <param name="identification">身份标识</param>
        public override bool CreateMicroBlog(string accessToken, string content, string identification = null)
        {
            _restClient.BaseUrl = "https://api.renren.com/v2/status/put";
            _restClient.Authenticator = new CommonOAuthAuthenticator(accessToken);
            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("access_token", accessToken);
            request.AddParameter("content", content);
            var response = Execute(_restClient, request);
            var data = Json.Decode(response.Content);
            return data.response.id >0;
        }

        /// <summary>
        /// 发一条可带图片的微博消息
        /// </summary>
        /// <param name="accessToken">访问授权</param>
        /// <param name="content">微博内容</param>
        /// <param name="bytes">图片流</param>
        /// <param name="identification">身份标识</param>
        public override bool CreatePhotoMicroBlog(string accessToken, string content, byte[] bytes, string fileName, string identification = null)
        {
            return CreateMicroBlog(accessToken, content, identification);
        }

        /// <summary>
        /// 关注指定帐号
        /// </summary>
        /// <param name="accessToken">访问授权</param>
        /// <param name="userName">指定帐号</param>
        /// <param name="identification">身份标识</param>
        public override bool Follow(string accessToken, string userName, string identification = null)
        {
            throw new ExceptionFacade("人人网不支持关注用户功能");
        }
    }
}