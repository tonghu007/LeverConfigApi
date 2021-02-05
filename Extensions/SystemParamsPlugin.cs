using Microsoft.Extensions.Logging;
using Lever.Common;
using Lever.Plugins;
using System.Security.Claims;

namespace Lever.Extensions
{
    /// <summary>
    /// 初始化系统变量，以供配置字段获取
    /// </summary>
    public class SystemParamsPlugin : ParamsPlugin
    {
        private readonly ILogger<SystemParamsPlugin> _logger;
        public SystemParamsPlugin(ILogger<SystemParamsPlugin> logger)
        {
            _logger = logger;
        }
        public override void InitParams()
        {
            base.InitParams();
            //将登录信息缓存入系统变量字典
            var context = RequestDataHelper.GetHttpContext();
            if (context.User != null)
            {
                Claim claim = context.User.FindFirst(ClaimTypes.Sid);
                if (claim != null)
                {
                    string userId = claim.Value;
                    ParamsPlugin.Set("UserId", long.Parse(userId == "" ? "0" : userId));
                }

            }
        }
    }
}
