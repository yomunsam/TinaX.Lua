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

        public Task<bool> OnInit() => Task.FromResult(true);

        public XException GetInitException() => null;


        public void OnServiceRegister()
        {
            XCore.GetMainInstance().BindSingletonService<ILua, LuaManager>().SetAlias<Internal.ILuaInternal>();
        }


        public Task<bool> OnStart()
        {
            return XCore.GetMainInstance().GetService<Internal.ILuaInternal>().Start();
        }
        public XException GetStartException()
        {
            return XCore.GetMainInstance().GetService<Internal.ILuaInternal>().GetStartException();
        }

        

        public void OnQuit() { }

        public Task OnRestart() => Task.CompletedTask;

        

        
    }
}
