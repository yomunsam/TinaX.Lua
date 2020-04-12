using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XLua;

namespace TinaXEditor.Lua.Internal
{
    public static class LuaTypesConfig
    {
        [LuaCallCSharp]
        //[ReflectionUse]
        public static List<Type> LuaCallCSharpNestedTypes
        {
            get
            {
                var types = new List<Type>();
                foreach (var type in LuaCallCSharpList)
                {
                    foreach (var nested_type in type.GetNestedTypes(BindingFlags.Public))
                    {
                        if ((!nested_type.IsAbstract && typeof(Delegate).IsAssignableFrom(nested_type))
                            || nested_type.IsGenericTypeDefinition)
                        {
                            continue;
                        }
                        types.Add(nested_type);
                    }
                }
                return types;
            }
        }

        [LuaCallCSharp]
        //[ReflectionUse]
        public static List<Type> LuaCallCSharpList = new List<Type>() {

            #region Unity
            //Unity
            typeof(UnityEngine.Application),
            typeof(GameObject),
            typeof(MonoBehaviour),
            typeof(Behaviour),
            typeof(Component),
            typeof(RectTransform),
            typeof(Transform),
            typeof(UnityEngine.UI.Text),
            typeof(UnityEngine.UI.Button),
            typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.Events.UnityEvent),
            typeof(UnityEngine.Events.UnityEventBase),
            typeof(AudioClip),
            typeof(UnityEngine.Object),
            typeof(Application),
            typeof(Vector2),
            typeof(Vector3),
            #endregion

            #region TinaX
            typeof(TinaX.IXCore),
            typeof(TinaX.TimeMachine),
            typeof(TinaX.Systems.ITimeTicket),
            typeof(TinaX.XEvent),
            typeof(TinaX.Systems.IEventTicket),

            typeof(TinaX.GameObjectExtends),
            typeof(TinaX.StringExtend),
            typeof(TinaX.UObjectExtend),
            #endregion


            typeof(System.Object),
            typeof(List<int>),
            typeof(Action<string>),


            
        };

        [CSharpCallLua]
        public static List<Type> CSharpCallLuaList = new List<Type>()
        {
            typeof(Action),
            typeof(Action<string>),
            typeof(Action<string, string>),
            typeof(Action<string, int>),
            typeof(Action<double>),
            typeof(Action<bool>),
            typeof(Action<Collider>),
            typeof(Action<Collision>),
            typeof(Action<UnityEngine.Object>),
            typeof(Action<UnityEngine.Object,TinaX.XException>),
            typeof(Action<LuaFunction, Exception>),
            typeof(Action<object>),
        };

        [BlackList]
        public static Func<MemberInfo, bool> MethodFilter = (memberInfo) =>
        {
            if (memberInfo.DeclaringType.IsGenericType && memberInfo.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (memberInfo.MemberType == MemberTypes.Constructor)
                {
                    ConstructorInfo constructorInfo = memberInfo as ConstructorInfo;
                    var parameterInfos = constructorInfo.GetParameters();
                    if (parameterInfos.Length > 0)
                    {
                        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(parameterInfos[0].ParameterType))
                        {
                            return true;
                        }
                    }
                }
                else if (memberInfo.MemberType == MemberTypes.Method)
                {
                    var methodInfo = memberInfo as MethodInfo;
                    if (methodInfo.Name == "TryAdd" || methodInfo.Name == "Remove" && methodInfo.GetParameters().Length == 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        };

        [BlackList]
        public static List<List<string>> BlackList = new List<List<string>>()  {
            //Unity--------------------------------------------
            new List<string>(){ "UnityEngine.UI.Text", "OnRebuildRequested"},
            new List<string>(){ "UnityEngine.MonoBehaviour", "runInEditMode"},

        };

    }
}

