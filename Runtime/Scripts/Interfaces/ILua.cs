using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLua;

namespace TinaX.Lua
{
    public interface ILua
    {
        bool Inited { get; }
        LuaEnv LuaVM { get; }

        void LoadStringAsync(string require_text, Action<LuaFunction, Exception> callback);
    }
}
