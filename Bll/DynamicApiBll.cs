using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NLua;
using Lever.Common;
using Lever.Core;
using Lever.Dal;
using Lever.DBUtility;
using Lever.IBLL;
using Lever.Plugins;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Lever.Bll
{
    public class DynamicApiBll : IDynamicApiBll
    {
        private readonly DynamicApiDal _dal;
        private readonly ILogger<DynamicApiBll> _logger;
        public DynamicApiBll(DynamicApiDal dal, ILogger<DynamicApiBll> logger)
        {
            _dal = dal;
            _logger = logger;
        }

        public object DynamicFetch(string code)
        {
            IDictionary<string, object> inputParameters = new Dictionary<string, object>();
            object result = this.DynamicFetch(code, inputParameters);
            return result;
        }

        public object DynamicFetch(string code, IDictionary<string, object> inputParameters)
        {
            try
            {
                using (_dal)
                {
                    IDictionary<string, object> config = _dal.GetApiConfig(code);//接口配置信息
                    if (config == null)
                        throw new CustomException(10, "接口不存在");
                    int isAuth = (int)config["IsAuth"];
                    //登录验证
                    if (isAuth == 1)
                        this.GetUserClaimsPrincipal(config);
                    //初始化系统变量参数
                    this.InitSystemParams(config);
                    this.ParamsCheck(code, config, inputParameters);
                    return _dal.DynamicFetch(config, code, inputParameters);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"接口{code}调用异常");
                throw;
            }

        }

        private void InitSystemParams(IDictionary<string, object> config)
        {
            string pluginsAssembly = config.GetValue<string>("SystemPramsPluginAssemblyPath");
            string pluginsClassName = config.GetValue<string>("SystemPramsPluginClassName"); ;
            ParamsPlugin paramsPlugin = ReflectorHelper.GetPluginInstance<ParamsPlugin>(pluginsAssembly, pluginsClassName);
            if (paramsPlugin != null)
                paramsPlugin.InitParams();
        }

        private string GetToken()
        {
            var headers = RequestDataHelper.GetHeaders();
            return headers.ContainsKey("Authorization") ? headers["Authorization"].ToString() : "";
        }

        /// <summary>
        /// Token是否是符合要求的标准 Json Web 令牌
        /// </summary>
        /// <param name="tokenStr"></param>
        /// <returns></returns>
        private bool IsCanReadToken(string tokenStr)
        {
            if (string.IsNullOrWhiteSpace(tokenStr) || tokenStr.Length < 7)
                return false;
            if (!tokenStr.Substring(0, 6).Equals(JwtBearerDefaults.AuthenticationScheme))
                return false;
            tokenStr = tokenStr.Substring(7);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            return jwtTokenHandler.CanReadToken(tokenStr);
        }

        /// <summary>
        /// 验证token，并获取其中的信息
        /// </summary>
        /// <param name="tokenStr"></param>
        /// <returns></returns>
        private ClaimsPrincipal ValidateToken(IDictionary<string, object> config, string tokenStr)
        {
            try
            {
                tokenStr = tokenStr.Substring(7);
                string securityKey = (string)config["SecurityKey"];
                byte[] aesKeyByte = Encoding.UTF8.GetBytes(AppConfigurtaionHelper.Configuration.GetValue<string>("AesCrypto:Key"));
                byte[] aesIvByte = Encoding.UTF8.GetBytes(AppConfigurtaionHelper.Configuration.GetValue<string>("AesCrypto:Iv"));
                securityKey = AesCryptoUtils.Decrypt(securityKey, aesKeyByte, aesIvByte);
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var tokenParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),    // 加密解密Token的密钥

                    // 是否验证发布者
                    ValidateIssuer = true,
                    // 发布者名称
                    ValidIssuer = (string)config["Issuer"],

                    // 是否验证订阅者
                    ValidateAudience = true,
                    // 订阅者名称
                    ValidAudience = (string)config["Audience"],

                    // 是否验证令牌有效期
                    ValidateLifetime = true,
                    //注意这是缓冲过期时间，总的有效时间等于这个时间加上jwt的过期时间，如果不配置，默认是5分钟
                    ClockSkew = TimeSpan.FromMinutes((int)config["ClockSkew"])
                };
                SecurityToken securityToken;
                return jwtTokenHandler.ValidateToken(tokenStr, tokenParameters, out securityToken);
            }
            catch (SecurityTokenExpiredException e)
            {
                RequestDataHelper.GetHttpContext().Response.Headers.Add("Token-Expired", "true");
                throw new CustomException(2, "token已过期");
            }
            catch (Exception e)
            {
                throw new CustomException(1, "无效token");
            }
        }

        private void GetUserClaimsPrincipal(IDictionary<string, object> config)
        {
            string tokenStr = this.GetToken();
            if (!IsCanReadToken(tokenStr))
                throw new CustomException(1, "无效token");
            HttpContext context = RequestDataHelper.GetHttpContext();
            if (context != null)
                context.User = this.ValidateToken(config, tokenStr);
        }

        /// <summary>
        /// 参数验证
        /// </summary>
        /// <param name="code">接口编码</param>
        private void ParamsCheck(string code, IDictionary<string, object> config, IDictionary<string, object> inputParameters)
        {
            using (Lua lua = new Lua())
            {
                lua.State.Encoding = Encoding.UTF8;
                //参数整体验证
                string checkScript = config["CheckScript"].ToString();
                IDictionary<string, object> parameters = RequestDataHelper.GetMixParams();
                object bodyJson = RequestDataHelper.GetBodyJsonParameters();
                IDictionary<string, IList<IFormFile>> fileDic = RequestDataHelper.GetAllFiles();
                if (!string.IsNullOrWhiteSpace(checkScript))
                {

                    object[] result = LuaScriptRunner.ExecuteLuaScript(lua, checkScript, parameters, bodyJson);//第一个返回值为验证是否通过（true|false）,第二个参数为验证错误信息，为true时没有
                    if (!(bool)result[0])
                    {
                        if (result.Length > 1)
                            throw new CustomException(11, result[1].ToString());//通过自定义异常抛出验证失败信息
                        else
                            throw new CustomException(11, "参数验证失败");
                    }

                }
                IDictionary<string, object> paramData = new Dictionary<string, object>(parameters);
                //单个参数验证
                IList<IDictionary<string, object>> apiParams = _dal.GetApiParams(code);//配置参数信息
                if (apiParams.Count > 0)
                {
                    foreach (IDictionary<string, object> dic in apiParams)
                    {
                        int paramType = (int)dic["ParamType"]; //0 = String,1 = Integer,2 = Long,3 = Double,4 = Float,5 = Decimal,6 = Boolean,7 = Date,8 = DateTime,9=Ulong,10 = Key/Value,11= List,12 = File
                        string paramCode = dic["ParamCode"].ToString();
                        string paramName = dic["ParamName"].ToString();
                        short isRequire = (short)dic["IsRequire"];
                        short paramsKind = (short)dic["ParamsKind"];//ParamsKind 0 = 普通参数；1 = 系统参数；2=Id值；
                        string checkRule = dic["CheckRule"].ToString();//验证使用的正则表达式
                        string ruleError = dic["RuleError"].ToString();//正则表达式验证不通过时候的错误提示信息
                        string paramCheckScript = dic["CheckScript"].ToString();//验证单个参数的lua脚本
                        if (paramsKind == 1)
                        {
                            if (isRequire == 1 && !ParamsPlugin.ContainsKey(paramCode))
                                throw new CustomException(11, "系统参数" + paramName + "不能为空");
                            var sysParamValue = ParamsPlugin.Get(paramCode);
                            if (isRequire == 1 && sysParamValue == null)
                                throw new CustomException(11, "系统参数" + paramName + "不能为空");
                            inputParameters[paramCode] = sysParamValue;
                            paramData[paramCode] = sysParamValue;
                        }
                        else if (paramsKind == 2)
                        {
                            var id = DbHelper.NewLongId();
                            inputParameters[paramCode] = id;
                            paramData[paramCode] = id;
                        }
                        //检查必录项
                        if (isRequire == 1)
                        {
                            this.CheckRequire(paramType, paramCode, paramName, paramData, fileDic);
                        }
                        //正则检查
                        if (!string.IsNullOrWhiteSpace(checkRule))
                        {
                            this.CheckRegexRule(paramType, paramCode, paramName, checkRule, ruleError, paramData);
                        }
                        //脚本验证
                        if (!string.IsNullOrWhiteSpace(paramCheckScript))
                        {
                            this.LuaScriptCheck(lua, paramType, paramCode, paramName, paramCheckScript, paramData);
                        }
                        //转换参数类型
                        this.ConvertParamsType(paramType, paramCode);
                    }
                }
            }
        }

        private void ConvertParamsType(int paramType, string paramCode)
        {
            //0 = String,1 = Integer,2 = Long,3 = Double,4 = Float,5 = Decimal,6 = Boolean,7 = Date,8 = DateTime,9=Ulong,10 = Key/Value,11 = List,12 = File
            IDictionary<string, object> mixParams = RequestDataHelper.GetMixParams();
            IDictionary<string, object> queryParams = RequestDataHelper.GetQueryParameters();
            IDictionary<string, object> formParams = RequestDataHelper.GetFormParameters();
            object bodyJson = RequestDataHelper.GetBodyJsonParameters();
            this.ConvertParamsType(mixParams, paramType, paramCode);
            this.ConvertParamsType(queryParams, paramType, paramCode);
            this.ConvertParamsType(formParams, paramType, paramCode);
            this.ConvertParamsType(bodyJson, paramType, paramCode);
        }

        private void ConvertParamsType(object paramsObject, int paramType, string paramCode)
        {
            if (!(paramsObject is IDictionary<string, object>) || paramsObject == null) return;
            IDictionary<string, object> parameters = (IDictionary<string, object>)paramsObject;
            if (parameters.ContainsKey(paramCode))
            {
                object result = parameters[paramCode];
                try
                {
                    switch (paramType)
                    {
                        case 0:
                            result = parameters[paramCode]?.ToString();
                            break;
                        case 1:
                            result = int.Parse(parameters[paramCode].ToString());
                            break;
                        case 2:
                            result = long.Parse(parameters[paramCode].ToString());
                            break;
                        case 3:
                            result = double.Parse(parameters[paramCode].ToString());
                            break;
                        case 4:
                            result = float.Parse(parameters[paramCode].ToString());
                            break;
                        case 5:
                            result = decimal.Parse(parameters[paramCode].ToString());
                            break;
                        case 6:
                            result = bool.Parse(parameters[paramCode].ToString());
                            break;
                        case 7:
                            result = DateTime.Parse(parameters[paramCode].ToString()).Date;
                            break;
                        case 8:
                            result = DateTime.Parse(parameters[paramCode].ToString());
                            break;
                        case 9:
                            result = ulong.Parse(parameters[paramCode].ToString());
                            break;
                    }
                }
                catch (Exception e)
                {
                    throw new CustomException(99, "参数类型与配置类型不符");
                }
                parameters[paramCode] = result;
            }
        }

        //0 = String,1 = Integer,2 = Long,3 = Double,4 = Float,5 = Decimal,6 = Boolean,7 = Date,8 = DateTime,9=Ulong,10 = Key/Value,11 = List,12 = File
        private void LuaScriptCheck(Lua lua, int paramType, string paramCode, string paramName, string paramCheckScript, IDictionary<string, object> parameters)
        {
            if (paramType != 10 && paramType != 11 && paramType != 12 && parameters.ContainsKey(paramCode))
            {
                IDictionary<string, object> param = new Dictionary<string, object>();
                param[paramCode] = parameters[paramCode];
                object[] result = LuaScriptRunner.ExecuteLuaScript(lua, paramCheckScript, param);//第一个返回值为验证是否通过（true|false）,第二个参数为验证错误信息，为true时没有
                if (!(bool)result[0])
                {
                    if (result.Length > 1)
                        throw new CustomException(11, result[1].ToString());//通过自定义异常抛出验证失败信息
                    else
                        throw new CustomException(11, paramName + "验证失败");
                }

            }
        }

        //0 = String,1 = Integer,2 = Long,3 = Double,4 = Float,5 = Decimal,6 = Boolean,7 = Date,8 = DateTime,9=Ulong,10 = Key/Value,11 = List,12 = File
        private void CheckRegexRule(int paramType, string paramCode, string paramName, string checkRule, string ruleError, IDictionary<string, object> parameters)
        {
            if (paramType != 10 && paramType != 11 && paramType != 12 && parameters.ContainsKey(paramCode))
            {
                string value = parameters[paramCode].ToString();
                if (!Regex.Match(value, checkRule).Success)
                {
                    if (!string.IsNullOrWhiteSpace(ruleError))
                        throw new CustomException(11, ruleError);
                    else
                        throw new CustomException(11, paramName + "验证失败");
                }
            }
        }

        private void CheckRequire(int paramType, string paramCode, string paramName, IDictionary<string, object> parameters, IDictionary<string, IList<IFormFile>> fileDic)
        {
            if (paramType == 11)
            {
                if (string.IsNullOrWhiteSpace(paramCode))
                {
                    if (!parameters.ContainsKey("__BodyJson"))
                        throw new CustomException(11, paramName + "不能为空");
                }
                else
                {
                    if (!parameters.ContainsKey(paramCode))
                        throw new CustomException(11, paramName + "不能为空");
                }
            }
            else if (paramType == 12)
            {
                if (fileDic == null || !fileDic.ContainsKey(paramCode))
                    throw new CustomException(11, paramName + "不能为空,请上传文件");
            }
            else
            {
                if (!parameters.ContainsKey(paramCode))
                    throw new CustomException(11, paramName + "不能为空");
            }
        }
    }
}
