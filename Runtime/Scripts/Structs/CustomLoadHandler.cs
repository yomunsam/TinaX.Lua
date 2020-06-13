using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.Lua.Internal;

namespace TinaX.Lua
{
    public struct CustomLoadHandler
    {
        public string FileName;
        public CustomLoadHandlerManager.LoadLuaHandler Handler;
    }
}
