using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace SAM.WPF.Core
{
    public static class ReflectionHelper
    {
        [Pure]
        [NotNull]
        public static string GetPropertyNameFromLambda<T>([NotNull] Expression<Func<T, object>> expression)
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression unaryExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null)
                throw new ArgumentException("Property name lambda expression needs to be in the form: n = > n.PropertyName");

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo == null)
                throw new InvalidOperationException("Bug, memberExpression.Member as PropertyInfo cast failed.");

            return propertyInfo.Name;
        }

        [Pure]
        public static Type LocateTypeFromName(string type)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var appCacheType = Type.GetType($"{type}, {assembly.GetName().Name}");

                if (appCacheType != null) return appCacheType;
            }

            throw new Exception($"Couldn't find {type}");
        }
    }
}
