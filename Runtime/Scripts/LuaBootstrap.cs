using System;

namespace TinaX.Lua.Internal
{
    public class LuaBootstrap : IXBootstrap
    {
        public void OnInit(IXCore core) { }
        public void OnStart(IXCore core)
        {
            if(core.Services.TryGet<ILuaInternal>(out var lua))
            {
                try
                {
                    lua.RequireEntryFile();
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }
        public void OnAppRestart() { }

        public void OnQuit() { }

    }
}
