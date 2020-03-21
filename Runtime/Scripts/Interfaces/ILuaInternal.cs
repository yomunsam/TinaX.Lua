using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLua;

namespace TinaX.Lua.Internal
{
    public interface ILuaInternal
    {
        bool Inited { get; }
        LuaEnv LuaVM { get; }

        XException GetStartException();
        Task<bool> Start();
        void RequireEntryFile();
    }
}
