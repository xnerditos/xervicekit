using System;
using System.Linq.Expressions;
using System.Reflection;

namespace XKit.Lib.Common.Utility.Invocation {

    // attribution:  https://github.com/tdupont750/tact.net/blob/master/framework/src/Tact/Reflection/EfficientInvoker.cs

    public static class MethodInvokerFactory {

        public static Func<object[], object> ForConstructor(ConstructorInfo constructor) {

            if (constructor == null) {
                throw new ArgumentNullException(nameof(constructor));
            }

            CreateParamsExpressions(constructor, out ParameterExpression argsExp, out Expression[] paramsExps);

            var newExp = Expression.New(constructor, paramsExps);
            var resultExp = Expression.Convert(newExp, typeof(object));
            var lambdaExp = Expression.Lambda(resultExp, argsExp);
            var lambda = lambdaExp.Compile();
            return (Func<object[], object>) lambda;
        }

        public static Func<object, object[], object> ForDelegate(Delegate del) {
            if (del == null) {
                throw new ArgumentNullException(nameof(del));
            }

            var type = del.GetType();
            var method = del.GetMethodInfo();
            return CreateMethodWrapper(type, method, true);
        }

        public static Func<object, object[], object> ForMethod(Type type, string methodName) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            if (methodName == null) {
                throw new ArgumentNullException(nameof(methodName));
            }

            var method = type.GetTypeInfo().GetMethod(methodName);
            if (method == null) {
                return null;
            }
            return CreateMethodWrapper(type, method, false);
        }

        public static Func<object, object[], object> ForMethod(MethodInfo methodInfo) {
            if (methodInfo == null) {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            return CreateMethodWrapper(methodInfo.DeclaringType, methodInfo, false);
        }

        public static Func<object, object[], object> ForProperty(Type type, string propertyName) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return CreatePropertyWrapper(type, propertyName);
        }

        private static Func<object, object[], object> CreateMethodWrapper(Type type, MethodInfo method, bool isDelegate) {
            
            CreateParamsExpressions(method, out ParameterExpression argsExp, out Expression[] paramsExps);

            var targetExp = Expression.Parameter(typeof(object), "target");
            var castTargetExp = Expression.Convert(targetExp, type);
            var invokeExp = isDelegate ?
                (Expression) Expression.Invoke(castTargetExp, paramsExps) :
                Expression.Call(castTargetExp, method, paramsExps);
            
            LambdaExpression lambdaExp;

            if (method.ReturnType != typeof(void)) {
                var resultExp = Expression.Convert(invokeExp, typeof(object));
                lambdaExp = Expression.Lambda(resultExp, targetExp, argsExp);
            } else {
                var constExp = Expression.Constant(null, typeof(object));
                var blockExp = Expression.Block(invokeExp, constExp);
                lambdaExp = Expression.Lambda(blockExp, targetExp, argsExp);
            }

            var lambda = lambdaExp.Compile();
            return (Func<object, object[], object>) lambda;
        }

        private static void CreateParamsExpressions(MethodBase method, out ParameterExpression argsExp, out Expression[] paramsExps) {
            var parameters = method.GetParameters();

            argsExp = Expression.Parameter(typeof(object[]), "args");
            paramsExps = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++) {
                var constExp = Expression.Constant(i, typeof(int));
                var argExp = Expression.ArrayIndex(argsExp, constExp);
                paramsExps[i] = Expression.Convert(argExp, parameters[i].ParameterType);
            }
        }

        private static Func<object, object[], object> CreatePropertyWrapper(Type type, string propertyName) {
            var property = type.GetRuntimeProperty(propertyName);
            if (property == null) {
                return null;
            }
            
            var targetExp = Expression.Parameter(typeof(object), "target");
            var argsExp = Expression.Parameter(typeof(object[]), "args");
            var castArgExp = Expression.Convert(targetExp, type);
            var propExp = Expression.Property(castArgExp, property);
            var castPropExp = Expression.Convert(propExp, typeof(object));
            var lambdaExp = Expression.Lambda(castPropExp, targetExp, argsExp);
            var lambda = lambdaExp.Compile();
            
            return (Func<object, object[], object>) lambda;
        }
    }
}