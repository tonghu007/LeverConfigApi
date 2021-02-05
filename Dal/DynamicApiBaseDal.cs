using NLua;
using Lever.Core;
using Lever.DBUtility;
using System;
using System.Text;
using System.Threading;

namespace Lever.Dal
{
    public class DynamicApiBaseDal : IDisposable
    {
        private readonly static AsyncLocal<Lua> threadLocalLua = new AsyncLocal<Lua>();
        protected readonly MultipleDbContext _dbContext;
        public DynamicApiBaseDal(MultipleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected Lua GetLua()
        {
            if (threadLocalLua.Value == null)
            {
                Lua lua = new Lua();
                lua.State.Encoding = Encoding.UTF8;
                threadLocalLua.Value = lua;
                //lua脚本中调用配置好的接口，获得lua 的table返回值
                lua["executeCS"] = (Func<string, LuaTable, object>)this.LuaInvokeDynamicApi;
                //脚本中执行指定类型的sql语句
                lua["executeSQL"] = (Func<string, int, LuaTable, object>)this.LuaExecuteSql;
                //取一个新Id
                //脚本中执行指定类型的sql语句
                lua["newId"] = (Func<object>)this.NewId;

                lua["objectToString"] = (Func<object,string>)this.ObjectToLuaScriptString;

                //声明Lua eval函数，用于将字符串执行为lua结果
                string script = @"
                function eval(script)
                    if(type(script) == ""string"") then
                        local eval = load(""local _ENV =""..script..""return _ENV"");
                            if (type(eval) == ""function"") then
                            return eval();
                            end
                        end
                end";
                lua.DoString(script);
            }

            return threadLocalLua.Value;
        }

        public virtual object LuaInvokeDynamicApi(string code, LuaTable table)
        {
            return null;
        }

        public virtual object LuaExecuteSql(string sql, int sqlKind, LuaTable table)
        {
            return null;
        }

        public virtual object NewId()
        {
            return DbHelper.NewLongId();
        }

        public virtual string ObjectToLuaScriptString(object obj)
        {
            string luaScriptString = LuaScriptRunner.ToLuaScript(obj);
            return luaScriptString;
        }

        public void Dispose()
        {
            Lua lua = GetLua();
            lua.Dispose();
            _dbContext.Dispose();
        }
    }
}
