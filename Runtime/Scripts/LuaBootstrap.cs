using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;

namespace TinaX.Lua.Internal
{
    public class LuaBootstrap : IXBootstrap
    {
        public void OnInit() { }
        public void OnStart()
        {
            if(TinaX.XCore.MainInstance.TryGetService<ILuaInternal>(out var lua))
            {
                lua.RequireEntryFile();
            }
        }
        public void OnAppRestart() { }

        public void OnQuit() { }

    }
}
