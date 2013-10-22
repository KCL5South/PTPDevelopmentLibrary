using System;
using System.Data.Metadata.Edm;
using System.IO;
using System.Collections.Generic;
namespace PTPDevelopmentLibrary.EntityGeneration
{
    /// <summary>
    /// Implementing this contract will mark an object as an entity generator.
    /// </summary>
    public interface IEntityGenerator
    {
        /// <summary>
        /// Call this method to populate the <paramref name="writer"/> with
        /// this particular generator's implementation of the type.
        /// </summary>
        /// <param name="targetType">A <see cref="EdmType"/> object containing the metadata describing the type.</param>
        /// <param name="container">A <see cref="IEnumerable{EdmType}"/> that represents all types defined in the conceptual model.</param>
        /// <param name="writer">A <see cref="TextWriter"/> that represents the output target for the method.</param>
        void WriteObject(EdmType targetType, IEnumerable<EdmType> container, TextWriter writer);

        /// <summary>
        /// Call this method to populate the <paramref name="writer"/> with
        /// this particular generator's implementation of the object context.
        /// </summary>
        /// <param name="container">A <see cref="EntityContainer"/> object containing the necessary information to generate an object context.</param>
        /// <param name="writer">A <see cref="TextWriter"/> that represents the output target for the method.</param>
        void WriteObjectContext(EntityContainer container, TextWriter writer);
    }
}