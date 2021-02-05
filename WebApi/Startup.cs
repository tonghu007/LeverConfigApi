using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Lever.Bll;
using Lever.Dal;
using Lever.DBUtility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data.SQLite;
using System.Threading;
using WebApi.Extensions;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Controllers;
using WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Lever.Common;
using Lever.IBLL;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
            DbProviderFactories.RegisterFactory("System.Data.SQLite", SQLiteFactory.Instance);
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
            List<MultipleDbSetting> dbSettings = new List<MultipleDbSetting>();
            Configuration.Bind("DbSettings", dbSettings);
            services.AddSingleton<List<MultipleDbSetting>>(dbSettings);//注入数据库配置对象
            services.AddSingleton<MultipleDbContext>();//注入MultipleDbContext
            services.AddSingleton<IDynamicApiBll, DynamicApiBll>();
            services.AddSingleton<DynamicApiDal>();
            services.AddSingleton<IConfigBll, ConfigBll>();
            services.AddSingleton<ConfigDal>();
            services.AddSingleton<ILeverXinBll, LeverXinBll>();
            services.AddSingleton<LeverXinDal>();
            services.AddSingleton<ComponentBll, ComponentBll>();
            services.AddSingleton<ComponentDal>();

            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo($@"{Directory.GetCurrentDirectory()}/temp-keys/"));
            // 设置验证方式为 Bearer Token
            // 也可以添加 using Microsoft.AspNetCore.Authentication.JwtBearer;
            // 使用 JwtBearerDefaults.AuthenticationScheme 代替 字符串 "Brearer"
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            //不在此验证token,如果要在此验证，打开注释
            /*.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("AuthSetting:SecurityKey"))),    // 加密解密Token的密钥

                    // 是否验证发布者
                    ValidateIssuer = true,
                    // 发布者名称
                    ValidIssuer = Configuration.GetValue<string>("AuthSetting:Issuer"),

                    // 是否验证订阅者
                    ValidateAudience = true,
                    // 订阅者名称
                    ValidAudience = Configuration.GetValue<string>("AuthSetting:Audience"),

                    // 是否验证令牌有效期
                    ValidateLifetime = true,
                    //注意这是缓冲过期时间，总的有效时间等于这个时间加上jwt的过期时间，如果不配置，默认是5分钟
                    ClockSkew = TimeSpan.FromMinutes(Configuration.GetValue<int>("AuthSetting:ClockSkew"))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        //Token expired
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });*/
            //配置日志远程访问http://xxxxx:xx/logging 
            services.AddLoggingFileUI();

            services.AddMvc(options =>
            {
                options.InputFormatters.Add(new PlainTextInputFormatter());//支持plain/text
                options.Filters.Add(typeof(WebApiResultFilter));
                options.Filters.Add(typeof(HttpContextFilter));
                options.Filters.Add(typeof(CustomExceptionFilter));
                options.RespectBrowserAcceptHeader = true;
            }).AddJsonOptions(options =>
            {
                //CamelCasePropertyNamesContractResolver 驼峰
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.Converters.Add(new JsonNumberConverter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            // 添加用户Session服务
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
            });
            // 指定Session保存方式:分发内存缓存
            services.AddDistributedMemoryCache();
            ServicesHelper.SetServiceProvider(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            //app.UseMiddleware<HttpContextMiddleWare>();
            //app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
