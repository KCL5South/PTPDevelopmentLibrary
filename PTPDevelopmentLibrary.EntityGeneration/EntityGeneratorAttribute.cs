using System;
namespace PTPDevelopmentLibrary.EntityGeneration
{
    /// <summary>
    /// Use this attribute to tag an implementation of <see cref="IEntityGenerator"/> with 
    /// a name and description that any outside viewer of the generator can see.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EntityGeneratorAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the generator.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets a description of the generator.
        /// </summary>
        public string Description { get; set; }
    }
}