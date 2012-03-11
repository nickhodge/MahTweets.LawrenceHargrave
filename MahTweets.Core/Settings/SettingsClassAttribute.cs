using System;

namespace MahTweets.Core.Settings
{
    ///// <summary>
    ///// Declare a UIElement as a feature of a plugin
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Class,  AllowMultiple = true, Inherited = false)]
    //public class ExportFeatureAttribute : Attribute
    //{
    //    /// <summary>
    //    /// Default Constructor
    //    /// </summary>
    //    /// <param name="type">Type of UIElement</param>
    //    public ExportFeatureAttribute(Type type)
    //    {
    //        this.Value = type;
    //    }

    //    /// <summary>
    //    /// Type of UIElement
    //    /// </summary>
    //    public Type Value
    //    {
    //        get;
    //        private set;
    //    }

    //    //public bool IsMultiple
    //    //{
    //    //    get;
    //    //    set;
    //    //}
    //    }

    /// <summary>
    /// Declare a UIElement as a feature of a plugin
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SettingsClassAttribute : Attribute
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="type">Type of UIElement</param>
        public SettingsClassAttribute(Type type)
        {
            Value = type;
        }

        /// <summary>
        /// Type of UIElement
        /// </summary>
        public Type Value { get; private set; }

        //public bool IsMultiple
        //{
        //    get;
        //    set;
        //}
    }
}