using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.Services;

namespace TinaX.Lua
{
    [XServiceProviderOrder(100)]
    public class LuaProvider : IXServiceProvider
    {
        public string ServiceName => Const.LuaConst.ServiceName;

        public Task<XException> OnInit(IXCore core) => Task.FromResult<XException>(null);



        public void OnServiceRegister(IXCore core)
        {
            core.Services.Singleton<ILua, LuaManager>()
                .SetAlias<Internal.ILuaInternal>();
        }


        public Task<XException> OnStart(IXCore core)
            => core.GetService<Internal.ILuaInternal>().Start();

        public void OnQuit() { }

        public Task OnRestart() => Task.CompletedTask;
        
    }
}
