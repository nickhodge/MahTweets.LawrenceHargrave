using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MahTweets.Core
{
    public static class ReflectionHelper
    {
        public static IEnumerable<string> GetPropertyNames(params Expression<Func<object>>[] expressions)
        {
            return expressions.Select(GetPropertyName);
        }

        public static string GetPropertyName(Expression<Func<object>> expression)
        {
            return GetPropertyInfo(expression).Name;
        }

        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
        {
            var lambda = expression as LambdaExpression;
            var memberExpression = lambda.Body as MemberExpression;
            if (memberExpression == null)
            {
                var unaryExpression = (lambda.Body as UnaryExpression);
                if (unaryExpression == null)
                    throw new InvalidOperationException("The expression is not in the expected format. A member or unary expression was expected");
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }

            return memberExpression.Member as PropertyInfo;
        }
    }
}
