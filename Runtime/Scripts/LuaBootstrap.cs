using System;

namespace TinaX.Lua.Internal
{
    public class LuaBootstrap : IXBootstrap
    {
        public void OnInit() { }
        public void OnStart()
        {
            if(TinaX.XCore.MainInstance.TryGetService<ILuaInternal>(out var lua))
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
