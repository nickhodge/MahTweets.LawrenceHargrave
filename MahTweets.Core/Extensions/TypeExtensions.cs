using System;

namespace MahTweets.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool Matches(this Type type, string typeName, string namespaceName)
        {
            string assemblyQualifiedName = type.AssemblyQualifiedName;

            if (assemblyQualifiedName == null)
                return false;
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
                return false;

            string[] currentAssemblyValues = assemblyQualifiedName.Split(new[] {','},
                                                                         StringSplitOptions.RemoveEmptyEntries);

            string compareTypeName = currentAssemblyValues[0].Trim();
            string compareNamespaceName = currentAssemblyValues[1].Trim();

            if (!compareTypeName.Matches(typeName, false))
                return false;

            if (!compareNamespaceName.Matches(namespaceName, false))
                return false;

            return true;
        }

        public static bool Matches(this Type type, string fullClassName)
        {
            string[] currentAssemblyValues = fullClassName.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            if (currentAssemblyValues.Length < 2)
                return false;

            string compareTypeName = currentAssemblyValues[0].Trim();
            string compareNamespaceName = currentAssemblyValues[1].Trim();

            return type.Matches(compareTypeName, compareNamespaceName);
        }

        public static string GetValueForSettings(this Type type)
        {
            string typeName = type.AssemblyQualifiedName;

            if (typeName == null)
                throw new InvalidOperationException("Cannot convert this type as it has not AssemblyQualifiedName value");

            string[] currentAssemblyValues = typeName.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            if (currentAssemblyValues.Length < 2)
                throw new InvalidOperationException("Cannot convert this type as it has not AssemblyQualifiedName value");

            string compareTypeName = currentAssemblyValues[0].Trim();
            string compareNamespaceName = currentAssemblyValues[1].Trim();

            return string.Format("{0}, {1}", compareTypeName, compareNamespaceName);
        }
    }
}