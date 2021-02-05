using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Lever.Common;
using Lever.DBUtility;
using Lever.IBLL;
using Lever.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lever.Extensions
{
    public class GetMobileNumberPlugin : ConfigApiPlugin
    {
        private readonly IDynamicApiBll _bll;
        private readonly ILogger<GetMobileNumberPlugin> _logger;
        public GetMobileNumberPlugin(IDynamicApiBll bll, ILogger<GetMobileNumberPlugin> logger)
        {
            _bll = bll;
            _logger = logger;
        }

        public override void Before(IDbHelper db, IDictionary<string, object> config, IEnumerable<KeyValuePair<string, object>> parameters, IDictionary<string, IList<IFormFile>> files, object bodyJson)
        {
            IDictionary<string, object> paramDic = (IDictionary<string, object>)parameters;
            db.AddInputParameter("UserId", paramDic["UserId"]);
            IDictionary<string, object> user = db.SelectRow("select * from Leverxin.s_user where id=@UserId");
            string iv = paramDic.GetValue<string>("iv");
            string res = AesCryptoUtils.Decrypt(paramDic.GetValue<string>("encryptedData"), Convert.FromBase64String(user["SessionKey"].ToString()), Convert.FromBase64String(iv));
            IDictionary<string, object> mobileInfo = JsonConvert.DeserializeObject<IDictionary<string, object>>(res);
            //将用户信息合并到一个字典中
            RequestDataHelper.MergeDictionary(ref paramDic, mobileInfo);
        }

        public override object After(IDbHelper db, IDictionary<string, object> config, IEnumerable<KeyValuePair<string, object>> parameters, object bodyJson, object result)
        {
            return result;
        }
    }
}
