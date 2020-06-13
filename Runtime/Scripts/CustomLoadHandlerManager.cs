using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace TinaX.Lua.Internal
{
    public class CustomLoadHandlerManager
    {
        public delegate byte[] LoadLuaHandler();
        public delegate byte[] LoadLuaFileHandler(ref string fileName);

        private Dictionary<string, LoadLuaHandler> m_Handlers = new Dictionary<string, LoadLuaHandler>();

        public void Add(string fileName, LoadLuaHandler handler)
        {
            if(m_Handlers.ContainsKey(fileName))
            {
                Debug.LogError($"[TinaX.Lua] Add \"CustomLoadHandler\" failed, the file name \"{fileName}\" already exists");
                return;
            }

            m_Handlers.Add(fileName, handler);
        }

        public bool TryGetHandler(string fileName, out LoadLuaHandler handler)
            => this.m_Handlers.TryGetValue(fileName, out handler);

    }
}
