using TinaX.Lua;

namespace TinaX.Services
{
    public static class LuaProvicerExtend
    {
        public static IXCore UseLuaRuntime(this IXCore core)
        {
            core.RegisterServiceProvider(new LuaProvider());
            return core;
        }
    }
}
