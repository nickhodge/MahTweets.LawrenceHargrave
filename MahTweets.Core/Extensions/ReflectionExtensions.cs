using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace MahTweets.Core.Extensions
{
    public static class ReflectionExtensions
    {
        private static bool? _isInDesignMode;

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
                    throw new InvalidOperationException(
                        "The expression is not in the expected format. A member or unary expression was expected");
                memberExpression = (MemberExpression) unaryExpression.Operand;
            }

            return memberExpression.Member as PropertyInfo;
        }

        public static bool IsInDesignMode(this FrameworkElement element)
        {
            if (!_isInDesignMode.HasValue)
            {
                DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                _isInDesignMode
                    = (bool) DependencyPropertyDescriptor
                                 .FromProperty(prop, typeof (FrameworkElement))
                                 .Metadata.DefaultValue;
            }
            if (_isInDesignMode != null)
            {
                // ReSharper disable PossibleInvalidOperationException
                return _isInDesignMode.Value;
                // ReSharper restore PossibleInvalidOperationException
            }

            return false;
        }
    }
}