using NetMud.DataAccess;
using NLua;
using System;

namespace NetMud.Data
{
    public static class LuaUtility
    {
        public static bool Validate(string luaCode)
        {
            var luaObj = new Lua();

            try
            {
                var validity = true;
                luaObj.DoString(luaCode);

                //Verify all the needed functions
                validity = validity && luaObj["LaunchGame"] as LuaFunction != null;
                validity = validity && luaObj["EndGame"] as LuaFunction != null;
                validity = validity && luaObj["SkipMove"] as LuaFunction != null;
                validity = validity && luaObj["ExecuteMove"] as LuaFunction != null;
                validity = validity && luaObj["GameStatus"] as LuaFunction != null;

                return validity;
            }
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return false;
        }
    }
}
