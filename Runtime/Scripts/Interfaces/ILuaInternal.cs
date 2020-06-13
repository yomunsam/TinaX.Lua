using System.Threading.Tasks;
using XLua;

namespace TinaX.Lua.Internal
{
    public interface ILuaInternal
    {
        bool Inited { get; }
        LuaEnv LuaVM { get; }

        Task<XException> Start();
        void RequireEntryFile();
    }
}
