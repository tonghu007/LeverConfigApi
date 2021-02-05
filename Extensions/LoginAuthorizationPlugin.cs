using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Lever.Common;
using Lever.DBUtility;
using Lever.IBLL;
using Lever.Plugins;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lever.Extensions
{
    public class LoginAuthorizationPlugin : ConfigApiPlugin
    {
        private readonly IDynamicApiBll _bll;
        private readonly ILogger<LoginAuthorizationPlugin> _logger;
        public LoginAuthorizationPlugin(IDynamicApiBll bll, ILogger<LoginAuthorizationPlugin> logger)
        {
            _bll = bll;
            _logger = logger;
        }

        public override void Before(IDbHelper db, IDictionary<string, object> config, IEnumerable<KeyValuePair<string, object>> parameters, IDictionary<string, IList<IFormFile>> files, object bodyJson)
        {
            try
            {
                IDictionary<string, object> paramDic = (IDictionary<string, object>)parameters;
                IDictionary<string, object> dic = this.GetOpenId(paramDic.GetValue<string>("code"));
                string sessionKey = dic.GetValue<string>("session_key");
                string iv = paramDic.GetValue<string>("iv");
                _logger.LogInformation($"sessionKey={sessionKey}\niv={iv}\nencryptedData={paramDic.GetValue<string>("encryptedData")}");
                string res = AesCryptoUtils.Decrypt(paramDic.GetValue<string>("encryptedData"), Convert.FromBase64String(sessionKey), Convert.FromBase64String(iv));
                IDictionary<string, object> userInfo = JsonConvert.DeserializeObject<IDictionary<string, object>>(res);
                //将用户信息合并到一个字典中
                RequestDataHelper.MergeDictionary(ref paramDic, dic, userInfo);
            }
            catch (Exception e) {
                _logger.LogError(e, "登录验证扩展异常");
                throw;
            }
            
        }

        public override object After(IDbHelper db, IDictionary<string, object> config, IEnumerable<KeyValuePair<string, object>> parameters, object bodyJson, object result)
        {
            object res = this.CreateToken(config, (IDictionary<string, object>)result);
            return res;
        }

        private IDictionary<string, object> CreateToken(IDictionary<string, object> config, IDictionary<string, object> user)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Sid, user.GetValue<string>("Id")),
                new Claim(ClaimTypes.Name, user.GetValue<string>("Nick")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub,user.GetValue<string>("Nick")),
                new Claim(JwtRegisteredClaimNames.NameId, user.GetValue<string>("OpenId")),
                new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddMinutes((int)config["Expires"])).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
            };
            string securityKey = (string)config["SecurityKey"];
            byte[] aesKeyByte = Encoding.UTF8.GetBytes(AppConfigurtaionHelper.Configuration.GetValue<string>("AesCrypto:Key"));
            byte[] aesIvByte = Encoding.UTF8.GetBytes(AppConfigurtaionHelper.Configuration.GetValue<string>("AesCrypto:Iv"));
            securityKey = AesCryptoUtils.Decrypt(securityKey, aesKeyByte, aesIvByte);
            // 和 Startup 中的配置一致
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: (string)config["Issuer"],
                audience: (string)config["Audience"],
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes((int)config["Expires"]),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            IDictionary<string, object> result = new Dictionary<string, object>();
            result["Token"] = jwtToken;
            result["Nick"] = user.GetValue<string>("Nick");
            result["AvatarUrl"] = user.GetValue<string>("AvatarUrl");
            result["Tel"] = user.GetValue<string>("Tel");
            result["Gender"] = user.GetValue<string>("Gender");
            return result;
        }

        //根据微信小程序登录返回code取用户信息
        private IDictionary<string, object> GetOpenId(string jsCode)
        {
            string appId = AppConfigurtaionHelper.Configuration.GetValue<string>("AuthSetting:AppID");
            string appSecret = AppConfigurtaionHelper.Configuration.GetValue<string>("AuthSetting:AppSecret");
            string url = $"https://api.weixin.qq.com/sns/jscode2session";
            IDictionary<string, object> data = new Dictionary<string, object>();
            data.Add("appid", appId);
            data.Add("secret", appSecret);
            data.Add("js_code", jsCode);
            data.Add("grant_type", "authorization_code");
            string res = HttpRequestHelper.Request(url, "get", "", data);
            return JsonConvert.DeserializeObject<IDictionary<string, object>>(res);
        }
    }
}
