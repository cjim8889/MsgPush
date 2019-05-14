using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TelePush.Backend.Attribute;
using TelePush.Backend.Core;

namespace TelePush.Backend.Utility
{
    class Loader
    {

        private static Delegate CreateDelegate(MethodInfo methodInfo, object target)
        {
            Func<Type[], Type> getType;
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            getType = Expression.GetFuncType;
            types = types.Concat(new[] { methodInfo.ReturnType });

            return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }

        public static void LoadToList(IList<Method> list, Type type)
        {
            MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var methodInfo in methodInfos)
            {
                var method = new Method() { MethodInfo=methodInfo };
                InjectAttributeInfo(methodInfo, method);

                list.Add(method);
            }
        }

        public static void InjectAttributeInfo(MethodInfo methodInfo, Method method)
        {
            CommandAttribute commandAttribute = methodInfo.GetCustomAttribute<CommandAttribute>();
            if (commandAttribute != null)
            {
                method.Command = commandAttribute.CommandName;
                method.IsCommand = true;
            }

            TypeAttribute typeAttribute = methodInfo.GetCustomAttribute<TypeAttribute>();
            method.Type = typeAttribute == null ? DispatcherType.Any : typeAttribute.Type;

        }
    }
}
