using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.Lua.Const
{
    public class LuaConst
    {
        public const string ServiceName = "TinaX.Lua";

        public static readonly string ConfigPath_Resources = $"{TinaX.Const.FrameworkConst.Framework_Configs_Folder_Path}/{ConfigFileName}";
        public const string ConfigFileName = "LuaRuntime";

    }
}
