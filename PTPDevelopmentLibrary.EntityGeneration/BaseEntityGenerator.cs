using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;
using System.Security.Principal;
using System.Data.SqlTypes;
namespace PTPDevelopmentLibrary.EntityGeneration
{
    /// <summary>
    /// A base object that implements the <see cref="IEntityGenerator"/> interface.
    /// </summary>
    public abstract class BaseEntityGenerator : IEntityGenerator
    {
        /// <summary>
        /// An enumeration that distinguishes between an Entity and ComplexType.
        /// </summary>
        protected enum CurrentTypes
        {
            /// <summary>
            /// This value represents an Entity.
            /// </summary>
            Entity,
            /// <summary>
            /// This value represents a ComplexType.
            /// </summary>
            ComplexType
        }

        /// <summary>
        /// The base Constructor.
        /// </summary>
        public BaseEntityGenerator()
        {
            TabQueue = new TabQueue();
        }

        /// <summary>
        /// Gets the currently available <see cref="TabQueue"/>.
        /// </summary>
        protected TabQueue TabQueue { get; private set; }
        /// <summary>
        /// Gets an enumeration that distiguishes what kind of Entity Type you are working with.
        /// </summary>
        protected CurrentTypes Type { get; private set; }
        /// <summary>
        /// Gets the Parent <see cref="EdmType"/> for the current type.
        /// </summary>
        protected EdmType ParentType { get; private set; }
        /// <summary>
        /// Gets all the child types for the current type.
        /// </summary>
        protected IEnumerable<EdmType> ChildTypes { get; private set; }
        /// <summary>
        /// Gets the name of the namespace defined within the current type.
        /// </summary>
        protected string NamespaceName { get; private set; }
        /// <summary>
        /// Gets or Sets the namespace to use when serializing.
        /// </summary>
        public string SerializationNamespace { get; set; }

        /// <summary>
        /// This method will take a string and convert it into the form that should be used for fields.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <returns>Converts the first character to lowercase and adds an underscore to the beginning.</returns>
        protected string FieldizePropertyName(string property)
        {
            if (string.IsNullOrEmpty(property))
                return string.Empty;

            char startingCharacter = property[0];
            return "_" + char.ToLower(startingCharacter) + property.Remove(0, 1);
        }
        /// <summary>
        /// This method returns the string representation of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">An <see cref="EdmType"/> object representing the type.</param>
        /// <param name="nullable">A boolean value signifiying whether the type is nullable or not.</param>
        /// <returns>The string representatin of the given type.</returns>
        protected string GetTypeString(EdmType type, bool nullable = false)
        {
            if (type.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                PrimitiveTypeKind pTypeKind = (type as PrimitiveType).PrimitiveTypeKind;
                switch (pTypeKind)
                {
                    case PrimitiveTypeKind.Binary:
                        return "global::System.Byte[]";
                    case PrimitiveTypeKind.Boolean:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "bool") : "bool";
                    case PrimitiveTypeKind.Byte:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "byte") : "byte";
                    case PrimitiveTypeKind.DateTime:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "global::System.DateTime") : "global::System.DateTime";
                    case PrimitiveTypeKind.DateTimeOffset:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "global::System.DateTimeOffset") : "global::System.DateTimeOffset";
                    case PrimitiveTypeKind.Decimal:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "decimal") : "decimal";
                    case PrimitiveTypeKind.Double:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "double") : "double";
                    case PrimitiveTypeKind.Guid:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "global::System.Guid") : "global::System.Guid";
                    case PrimitiveTypeKind.Int16:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "short") : "short";
                    case PrimitiveTypeKind.Int32:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "int") : "int";
                    case PrimitiveTypeKind.Int64:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "long") : "long";
                    case PrimitiveTypeKind.SByte:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "byte") : "byte";
                    case PrimitiveTypeKind.Single:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "global::System.Single") : "global::System.Single";
                    case PrimitiveTypeKind.String:
                        return "string";
                    case PrimitiveTypeKind.Time:
                        return nullable ? string.Format("global::System.Nullable<{0}>", "global::System.DateTime") : "global::System.DateTime";
                    default:
                        return "global::" + type.FullName;
                }
            }
            else if (type.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
            {
                CollectionType collType = (CollectionType)type;
                return string.Format("global::PTPDevelopmentLibrary.Framework.CollectionModel<global::{0}>", collType.TypeUsage.EdmType.FullName);
            }
            else
                return "global::" + type.FullName;
        }
        /// <summary>
        /// This method returns the string representation of the given <paramref name="property"/>.
        /// </summary>
        /// <param name="property">An <see cref="EdmMember"/> representing the property.</param>
        /// <returns>The string representation of the given property.</returns>
        protected string GetTypeString(EdmMember property)
        {
            return GetTypeString(property.TypeUsage.EdmType, (property is EdmProperty ? (property as EdmProperty).Nullable : false));
        }
        /// <summary>
        /// This method returns the string representation of the given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">A <see cref="FunctionParameter"/> representing the function perameter.</param>
        /// <returns>The string representation of the given parameter.</returns>
        protected string GetTypeString(FunctionParameter parameter)
        {
            return GetTypeString(parameter.TypeUsage.EdmType, true);
        }
        /// <summary>
        /// This method will convert a camelized string into it's sepperate words.
        /// </summary>
        /// <example>
        /// "HiThereMitch" would convert to "Hi There Mitch"
        /// </example>
        /// <param name="source">The string to convert.</param>
        /// <returns>An expanded string.</returns>
        protected string ExpandCamelizedString(string source)
        {
            StringBuilder result = new StringBuilder();
            char? last = null, cur = null, next = null;
            for (int i = 0; i < source.Length; i++)
            {
                cur = source[i];
                if (i < (source.Length - 1))
                    next = source[i + 1];

                if (last == null)
                    result.Append(cur);
                else if (char.IsDigit(last.Value) && !char.IsDigit(cur.Value))
                    result.Append(" " + cur.Value);
                else if (!char.IsDigit(last.Value) && char.IsDigit(cur.Value))
                    result.Append(" " + cur.Value);
                else if (char.IsDigit(last.Value) && char.IsDigit(cur.Value))
                    result.Append(" " + cur.Value);
                else if (char.IsLower(last.Value) && char.IsLower(cur.Value))
                    result.Append(cur.Value);
                else if (char.IsLower(last.Value) && char.IsUpper(cur.Value))
                    result.Append(" " + cur.Value);
                else if (char.IsUpper(last.Value) && char.IsUpper(cur.Value))
                {
                    if (char.IsUpper(next.Value))
                        result.Append(cur.Value);
                    else
                        result.Append(" " + cur.Value);
                }
                else if (char.IsUpper(last.Value) && char.IsLower(cur.Value))
                    result.Append(cur.Value);

                last = cur;
            }

            return result.ToString();
        }

        /// <summary>
        /// Inheritors would override this method to supply a custom using list.
        /// </summary>
        /// <param name="targetType">The <see cref="EdmType"/> that was passed to the <see cref="M:WriteObject"/> method.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeUsingStatements(EdmType targetType)
        {
            return string.Empty;
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom list of attributes to apply to the class.
        /// </summary>
        /// <param name="targetType">The <see cref="EdmType"/> that was passed to the <see cref="M:WriteObject"/> method.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeClassAttributes(EdmType targetType)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(SerializationNamespace))
                builder.AppendLine(string.Format("[global::System.Runtime.Serialization.DataContract(Name = \"{0}\", Namespace = {1}, IsReference = true)]", targetType.Name, SerializationNamespace), TabQueue);
            else
                builder.AppendLine(string.Format("[global::System.Runtime.Serialization.DataContract(Name = \"{0}\", IsReference = true)]", targetType.Name), TabQueue);
            foreach (EdmType type in ChildTypes)
                builder.AppendLine(string.Format("[global::System.Runtime.Serialization.KnownType(typeof(global::{0}))]", type.FullName), TabQueue);

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom namespace declaration.
        /// </summary>
        /// <param name="targetType">The <see cref="EdmType"/> that was passed to the <see cref="M:WriteObject"/> method.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeBeginNamespace(EdmType targetType)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("namespace {0}", targetType.NamespaceName), TabQueue);
            builder.AppendLine("{", TabQueue);
            TabQueue.Push();

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom class declaration.
        /// </summary>
        /// <param name="targetType">The <see cref="EdmType"/> that was passed to the <see cref="M:WriteObject"/> method.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeBeginClass(EdmType targetType)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(string.Format("public{0} partial class {1} : {2}",
                                             (targetType.Abstract ? " abstract " : string.Empty),
                                             targetType.Name,
                                             (targetType.BaseType != null ? string.Format("global::{0}", targetType.BaseType.FullName) : "global::PTPDevelopmentLibrary.Framework.BaseModel")),
                               TabQueue);
            builder.AppendLine("{", TabQueue);
            TabQueue.Push();

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom ending namespace declaration.
        /// </summary>
        /// <param name="targetType">The <see cref="EdmType"/> that was passed to the <see cref="M:WriteObject"/> method.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeEndNamespace(EdmType targetType)
        {
            StringBuilder builder = new StringBuilder();
            TabQueue.Pop();
            builder.AppendLine("}", TabQueue);
            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom ending class declaration.
        /// </summary>
        /// <param name="targetType">The <see cref="EdmType"/> that was passed to the <see cref="M:WriteObject"/> method.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeEndClass(EdmType targetType)
        {
            StringBuilder builder = new StringBuilder();

            if (Type == CurrentTypes.Entity)
            {
                EntityType entity = targetType as EntityType;

                //Determin if there are collections involved...
                if ((entity.BaseType == null ?
                    (entity.NavigationProperties.Any(a => a.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)) :
                    (entity.NavigationProperties.Any(a => a.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many &&
                     entity.BaseType is EntityType && !(entity.BaseType as EntityType).NavigationProperties.Select(b => b.Name).Contains(a.Name)))))
                {
                    builder.AppendLine("protected override void OnDeserializing(global::System.Runtime.Serialization.StreamingContext sc)", TabQueue);
                    builder.AppendLine("{", TabQueue);
                    TabQueue.Push();
                    builder.AppendLine("base.OnDeserializing(sc);", TabQueue);

                    foreach (NavigationProperty navProperty in entity.NavigationProperties.Where(a => a.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many))
                        builder.AppendLine(string.Format("{0} = new {1}();", FieldizePropertyName(navProperty.Name), GetTypeString(navProperty)), TabQueue);

                    TabQueue.Pop();
                    builder.AppendLine("}", TabQueue);
                }
            }

            TabQueue.Pop();
            builder.AppendLine("}", TabQueue);

            return builder.ToString();
        }

        /// <summary>
        /// Inheritors would override this method to supply custom attributes for a field.
        /// </summary>
        /// <param name="property">An <see cref="EdmMember"/> representing the property that the field is being created for.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeFieldAttributes(EdmMember property)
        {
            return string.Empty;
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom field definition.
        /// </summary>
        /// <param name="property">An <see cref="EdmMember"/> representing the property that the field is being created for.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeField(EdmMember property)
        {
            StringBuilder builder = new StringBuilder();
            if (property.BuiltInTypeKind == BuiltInTypeKind.EdmProperty)
                builder.AppendLine(string.Format("{0} {1};", GetTypeString(property), FieldizePropertyName(property.Name)), TabQueue);
            else if (property.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
            {
                NavigationProperty navProperty = property as NavigationProperty;
                switch (navProperty.ToEndMember.RelationshipMultiplicity)
                {
                    case RelationshipMultiplicity.Many:
                        //builder.AppendLine(string.Format("{0} {1};", string.Format("global::System.Collections.Generic.IList<{0}>", GetTypeString(property)), FieldizePropertyName(property.Name)), TabQueue);
                        //break;
                    case RelationshipMultiplicity.One:
                    case RelationshipMultiplicity.ZeroOrOne:
                        builder.AppendLine(string.Format("{0} {1};", GetTypeString(property), FieldizePropertyName(property.Name)), TabQueue);
                        break;
                }
            }

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply custom attributes for the given property.
        /// </summary>
        /// <param name="property">An <see cref="EdmMember"/> object representing the property to output.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypePropertyAttributes(EdmMember property)
        {
            StringBuilder builder = new StringBuilder();
            bool autoGenerator = true;
            bool isPrimitiveType = property.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType;

            //Key Attribute
            if (Type == CurrentTypes.Entity)
            {
                EntityType declaringType = property.DeclaringType as EntityType;
                if (declaringType.KeyMembers.Contains(property))
                {
                    builder.AppendLine("[global::System.ComponentModel.DataAnnotations.Key]", TabQueue);
                    autoGenerator &= false;
                }
            }

            //Display Attribute
            builder.AppendLine(string.Format("[global::System.ComponentModel.DataAnnotations.Display( Name = \"{0}\", {1}{2})]", ExpandCamelizedString(property.Name),
                                string.Format("AutoGenerateField = {0}", ((autoGenerator && isPrimitiveType) ? "true" : "false")),
                                (property.Documentation == null ? string.Empty : (property.Documentation.IsEmpty ? string.Empty : string.Format(", Description = \"{0}\"", property.Documentation.Summary)))), TabQueue);

            //DataMember Attribute
            builder.AppendLine(string.Format("[global::System.Runtime.Serialization.DataMember(Name = \"{0}\")]", property.Name), TabQueue);

            //Validation Attributes
            if (property.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                switch ((property.TypeUsage.EdmType as PrimitiveType).PrimitiveTypeKind)
                {
                    case PrimitiveTypeKind.DateTime:
                        builder.AppendLine(string.Format("[global::System.ComponentModel.DataAnnotations.Range(typeof(global::System.DateTime), \"{0}\", \"{1}\", ErrorMessage = \"{2}\")]", ((DateTime)SqlDateTime.MinValue).ToShortDateString(), ((DateTime)SqlDateTime.MaxValue).ToShortDateString(), "The value of {0} must be between {1} and {2}."), TabQueue);
                        break;
                    case PrimitiveTypeKind.String:
                        if(property.TypeUsage.Facets.Contains("MaxLength") && property.TypeUsage.Facets["MaxLength"].Value != null)
                            builder.AppendLine(string.Format("[global::System.ComponentModel.DataAnnotations.StringLength({0}, ErrorMessage = \"{1}\")]", property.TypeUsage.Facets["MaxLength"].Value.ToString() , "The value of {0} must be no longer than {1} characters."), TabQueue);
                        break;
                }
            }

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom property declaration.
        /// </summary>
        /// <param name="property">An <see cref="EdmMember"/> object representing the property to output.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteTypeProperty(EdmMember property)
        {
            StringBuilder builder = new StringBuilder();
            if (property.BuiltInTypeKind == BuiltInTypeKind.EdmProperty)
            {
                builder.AppendLine(string.Format("public {0} {1}", GetTypeString(property), property.Name), TabQueue);
                builder.AppendLine("{", TabQueue);
                TabQueue.Push();
                builder.AppendLine("get", TabQueue);
                builder.AppendLine("{", TabQueue);
                TabQueue.Push();
                builder.AppendLine(string.Format("return {0};", FieldizePropertyName(property.Name)), TabQueue);
                TabQueue.Pop();
                builder.AppendLine("}", TabQueue);
                builder.AppendLine("set", TabQueue);
                builder.AppendLine("{", TabQueue);
                TabQueue.Push();
                builder.AppendLine(string.Format("ChangeValue(ref {0}, value, \"{1}\");", FieldizePropertyName(property.Name), property.Name), TabQueue);
                TabQueue.Pop();
                builder.AppendLine("}", TabQueue);
                TabQueue.Pop();
                builder.AppendLine("}", TabQueue);
            }
            else if (property.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
            {
                NavigationProperty navProperty = property as NavigationProperty;
                switch (navProperty.ToEndMember.RelationshipMultiplicity)
                {
                    case RelationshipMultiplicity.Many:
                        //builder.AppendLine(string.Format("public virtual {0} {1}", string.Format("global::System.Collections.Generic.IList<{0}>", GetTypeString(property)), property.Name), TabQueue);
                        //break;
                    case RelationshipMultiplicity.One:
                    case RelationshipMultiplicity.ZeroOrOne:
                        builder.AppendLine(string.Format("public virtual {0} {1}", GetTypeString(property), property.Name), TabQueue);
                        break;
                }
                builder.AppendLine("{", TabQueue);
                TabQueue.Push();
                builder.AppendLine("get", TabQueue);
                builder.AppendLine("{", TabQueue);
                TabQueue.Push();
                builder.AppendLine(string.Format("return {0};", FieldizePropertyName(property.Name)), TabQueue);
                TabQueue.Pop();
                builder.AppendLine("}", TabQueue);
                if (navProperty.ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
                {
                    builder.AppendLine("set", TabQueue);
                    builder.AppendLine("{", TabQueue);
                    TabQueue.Push();
                    builder.AppendLine(string.Format("ChangeValue(ref {0}, value, \"{1}\");", FieldizePropertyName(property.Name), property.Name), TabQueue);
                    TabQueue.Pop();
                    builder.AppendLine("}", TabQueue);
                }
                TabQueue.Pop();
                builder.AppendLine("}", TabQueue);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Inheritors would override this method to supply custom documetation for the entire document.
        /// </summary>
        /// <param name="item">A <see cref="MetadataItem"/> object representing the object the documentation is being written for.</param>
        /// <returns>The desired documentation.</returns>
        protected virtual string WriteDocumentation(MetadataItem item)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("/*", TabQueue);
            builder.AppendLine(string.Format(" * Generated on {0} with {1}", DateTime.Now.ToLongDateString(), this.GetType().FullName), TabQueue);
            builder.AppendLine(string.Format(" * Generated by {0}", WindowsIdentity.GetCurrent().Name), TabQueue);
            builder.AppendLine(" */", TabQueue);

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply custom documentation for a given type.
        /// </summary>
        /// <param name="item">A <see cref="MetadataItem"/> object representing the object the documentation is being written for.</param>
        /// <returns>The desired documentation.</returns>
        protected virtual string WriteDocumentationType(MetadataItem item)
        {
            StringBuilder builder = new StringBuilder();

            if (item.Documentation != null)
            {
                builder.AppendLine("/// <summary>", TabQueue);
                builder.AppendLine(string.Format("/// {0}", item.Documentation.Summary), TabQueue);
                if (!string.IsNullOrWhiteSpace(item.Documentation.LongDescription))
                    builder.AppendLine(string.Format("/// {0}", item.Documentation.LongDescription), TabQueue);
                builder.AppendLine("/// </summary>", TabQueue);
            }

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply custom documentation for a given property.
        /// </summary>
        /// <param name="item">A <see cref="MetadataItem"/> object representing the object the documentation is being written for.</param>
        /// <returns>The desired documentation.</returns>
        protected virtual string WriteDocumentationProperty(MetadataItem item)
        {
            StringBuilder builder = new StringBuilder();

            if (item.Documentation != null)
            {
                builder.AppendLine("/// <summary>", TabQueue);
                builder.AppendLine(string.Format("/// {0}", item.Documentation.Summary), TabQueue);
                if (!string.IsNullOrWhiteSpace(item.Documentation.LongDescription))
                    builder.AppendLine(string.Format("/// {0}", item.Documentation.LongDescription), TabQueue);
                builder.AppendLine("/// </summary>", TabQueue);
            }

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply custom documentation for a given method.
        /// </summary>
        /// <param name="item">A <see cref="MetadataItem"/> object representing the object the documentation is being written for.</param>
        /// <returns>The desired documentation.</returns>
        protected virtual string WriteDocumentationMethod(MetadataItem item)
        {
            return string.Empty;
        }

        /// <summary>
        /// Inheritors would override this method to supply a custom using list for the object context.
        /// </summary>
        /// <param name="container">An <see cref="EntityContainer"/> representing the object passed to <see cref="M:WriteObjectContext"/>.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextUsingStatements(EntityContainer container)
        {
            return string.Empty;
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom namespace decleration for the object context.
        /// </summary>
        /// <param name="container">An <see cref="EntityContainer"/> representing the object passed to <see cref="M:WriteObjectContext"/>.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextBeginNamespace(EntityContainer container)
        {
            StringBuilder builder = new StringBuilder();
            if (container.BaseEntitySets.Count > 0 || container.FunctionImports.Count > 0)
            {
                builder.AppendLine(string.Format("namespace {0}", container.BaseEntitySets.Count > 0 ? container.BaseEntitySets[0].ElementType.NamespaceName : container.FunctionImports[0].NamespaceName), TabQueue);
                builder.AppendLine("{", TabQueue);
                TabQueue.Push();
            }

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom ending namespace decleration for the object context.
        /// </summary>
        /// <param name="container">An <see cref="EntityContainer"/> representing the object passed to <see cref="M:WriteObjectContext"/>.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextEndNamespace(EntityContainer container)
        {
            StringBuilder builder = new StringBuilder();
            if (container.BaseEntitySets.Count > 0 || container.FunctionImports.Count > 0)
            {
                TabQueue.Pop();
                builder.AppendLine("}", TabQueue);
            }
            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply custom attributes for the object context.
        /// </summary>
        /// <param name="container">An <see cref="EntityContainer"/> representing the object passed to <see cref="M:WriteObjectContext"/>.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextClassAttributes(EntityContainer container)
        {
            return string.Empty;
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom object context declaration.
        /// </summary>
        /// <param name="container">An <see cref="EntityContainer"/> representing the object passed to <see cref="M:WriteObjectContext"/>.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextBeginClass(EntityContainer container)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("internal partial class {0} : global::System.Data.Objects.ObjectContext", container.Name), TabQueue);
            builder.AppendLine("{", TabQueue);
            TabQueue.Push();

            builder.AppendLine(string.Format("internal {0}(string connectionString)", container.Name), TabQueue);
            TabQueue.Push();
            builder.AppendLine(string.Format(": base(connectionString, \"{0}\")", container.Name), TabQueue);
            TabQueue.Pop();
            builder.AppendLine("{ }", TabQueue);

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom ending object context declaration.
        /// </summary>
        /// <param name="container">An <see cref="EntityContainer"/> representing the object passed to <see cref="M:WriteObjectContext"/>.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextEndClass(EntityContainer container)
        {
            StringBuilder builder = new StringBuilder();

            TabQueue.Pop();
            builder.AppendLine("}", TabQueue);

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom ObjectSet field declaration.
        /// </summary>
        /// <param name="entitySet">An <see cref="EntitySetBase"/> object representing the individual ObjectSet.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextEntitySetField(EntitySetBase entitySet)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("global::System.Data.Objects.ObjectSet<{0}> {1};", entitySet.ElementType.FullName, FieldizePropertyName(entitySet.Name)), TabQueue);
            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would override this method to supply a custom ObjectSet property declaration.
        /// </summary>
        /// <param name="entitySet">An <see cref="EntitySetBase"/> object representing the individual ObjectSet.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextEntitySetProperty(EntitySetBase entitySet)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(string.Format("internal global::System.Data.Objects.ObjectSet<{0}> {1}", entitySet.ElementType.FullName, entitySet.Name), TabQueue);
            builder.AppendLine("{", TabQueue);
            TabQueue.Push();
            builder.AppendLine("get", TabQueue);
            builder.AppendLine("{", TabQueue);
            TabQueue.Push();
            builder.AppendLine(string.Format("if({0} == null)", FieldizePropertyName(entitySet.Name)), TabQueue);
            TabQueue.Push();
            builder.AppendLine(string.Format("{0} = this.CreateObjectSet<{1}>();", FieldizePropertyName(entitySet.Name), entitySet.ElementType.FullName), TabQueue);
            TabQueue.Pop();
            builder.AppendLine(string.Format("return {0};", FieldizePropertyName(entitySet.Name)), TabQueue);
            TabQueue.Pop();
            builder.AppendLine("}", TabQueue);
            TabQueue.Pop();
            builder.AppendLine("}", TabQueue);

            return builder.ToString();
        }
        /// <summary>
        /// Inheritors would overrid ethis method to supply a custom Function Import declaration.
        /// </summary>
        /// <param name="function">An <see cref="EdmFunction"/> representing the individual function.</param>
        /// <returns>A formated string representing the desired output.</returns>
        protected virtual string WriteContextFunctionImport(EdmFunction function)
        {
            StringBuilder builder = new StringBuilder();
            string parameterString = string.Empty;
            string returnValueString = string.Empty;
            bool returnValueIsCollection = function.ReturnParameter != null &&
                                           function.ReturnParameter.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType;

            if (returnValueIsCollection)
            {
                CollectionType collType = (function.ReturnParameter.TypeUsage.EdmType as CollectionType);
                returnValueString = GetTypeString(collType.TypeUsage.EdmType);
            }

            parameterString =
                string.Join(", ", from param in function.Parameters
                                 select string.Format((param.Mode == ParameterMode.InOut ? "ref {0} {1}" :"{0} {1}"), GetTypeString(param), param.Name));

            if (returnValueIsCollection)
                builder.AppendLine(string.Format("internal {0} {1}({2})", string.Format("global::System.Data.Objects.ObjectResult<{0}>", returnValueString), function.Name, parameterString), TabQueue);
            else
                builder.AppendLine(string.Format("internal int {0}({1})", function.Name, parameterString), TabQueue);
            
            builder.AppendLine("{", TabQueue);

            TabQueue.Push();

            foreach (FunctionParameter funcParam in function.Parameters)
            {
                if (funcParam.TypeUsage.EdmType.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
                    throw new Exception("A function parameter is not a primative type.");

                PrimitiveTypeKind typeKind = (funcParam.TypeUsage.EdmType as PrimitiveType).PrimitiveTypeKind;

                builder.AppendLine(string.Format("global::System.Data.Objects.ObjectParameter {0}Parameter;", funcParam.Name), TabQueue);
                if (typeKind == PrimitiveTypeKind.String || typeKind == PrimitiveTypeKind.Binary)
                    builder.AppendLine(string.Format("if ({0} != null)", funcParam.Name), TabQueue);
                else
                    builder.AppendLine(string.Format("if ({0}.HasValue)", funcParam.Name), TabQueue);

                TabQueue.Push();

                builder.AppendLine(string.Format("{0}Parameter = new global::System.Data.Objects.ObjectParameter(\"{0}\", {0});", funcParam.Name), TabQueue);

                TabQueue.Pop();

                builder.AppendLine("else", TabQueue);

                TabQueue.Push();

                builder.AppendLine(string.Format("{0}Parameter = new global::System.Data.Objects.ObjectParameter(\"{0}\", typeof({1}));", funcParam.Name, GetTypeString(funcParam)), TabQueue);

                TabQueue.Pop();

                builder.AppendLine();
            }

            parameterString = string.Join(", ", from param in function.Parameters
                                               select string.Format("{0}Parameter", param.Name));

            if(returnValueIsCollection)
                builder.AppendLine(string.Format("{0} result = base.ExecuteFunction<{1}>(\"{2}\"{3});", string.Format("global::System.Data.Objects.ObjectResult<{0}>", returnValueString), returnValueString, function.Name, (string.IsNullOrEmpty(parameterString) ? string.Empty : ", " + parameterString)), TabQueue);
            else
                builder.AppendLine(string.Format("int result = base.ExecuteFunction(\"{0}\"{1});", function.Name, (string.IsNullOrEmpty(parameterString) ? string.Empty : ", " + parameterString)), TabQueue);

            foreach (FunctionParameter outParam in function.Parameters.Where(a => a.Mode == ParameterMode.InOut))
            {
                builder.AppendLine(string.Format("{0} = ({1}){0}Parameter.Value;", outParam.Name, GetTypeString(outParam)), TabQueue);
            }

            builder.AppendLine("return result;", TabQueue);

            TabQueue.Pop();
            builder.AppendLine("}", TabQueue);

            return builder.ToString();
        }

        #region IEntityGenerator Members

        /// <summary>
        /// Inherited from <see cref="IEntityGenerator"/>.
        /// Provides the content for a single object file.
        /// </summary>
        /// <param name="targetType">An <see cref="EdmType"/> representing the type to generate output for.</param>
        /// <param name="container">An <see cref="IEnumerable{EdmType}"/> list containing all types defined for reference.</param>
        /// <param name="writer">The <see cref="TextWriter"/> object to write too.</param>
        public void WriteObject(EdmType targetType, IEnumerable<EdmType> container, TextWriter writer)
        {
            if (targetType == null)
                throw new ArgumentNullException("targetType must not be null.");
            if (writer == null)
                throw new ArgumentNullException("writer must not be null.");

            if (targetType is EntityType)
                Type = CurrentTypes.Entity;
            else if (targetType is ComplexType)
                Type = CurrentTypes.ComplexType;
            else
                throw new ArgumentException("targetType must be of type EntityType or ComplexType");

            ParentType = targetType.BaseType;
            ChildTypes = container.Where(a => a.BaseType != null && a.BaseType.FullName == targetType.FullName);
            NamespaceName = targetType.NamespaceName;

            IEnumerable<EdmProperty> usableProperties = null;
            IEnumerable<NavigationProperty> usableNavProperties = null;

            if (Type == CurrentTypes.Entity)
            {
                EntityType entity = targetType as EntityType;

                usableProperties = (targetType as EntityType).Properties;
                usableNavProperties = (targetType as EntityType).NavigationProperties;

                usableProperties =
                    entity.BaseType == null ? entity.Properties : entity.Properties.Where(a => !(entity.BaseType as EntityType).Properties.Any(b => b.Name == a.Name));
                usableNavProperties =
                    ParentType == null ? entity.NavigationProperties : entity.NavigationProperties.Where(a => !(entity.BaseType as EntityType).NavigationProperties.Any(b => b.Name == a.Name));
            }
            else if (Type == CurrentTypes.ComplexType)
            {
                ComplexType complex = targetType as ComplexType;

                usableProperties = (targetType as ComplexType).Properties;
                usableNavProperties = Enumerable.Empty<NavigationProperty>();

                usableProperties =
                    complex.BaseType == null ? complex.Properties : complex.Properties.Where(a => !(complex.BaseType as ComplexType).Properties.Any(b => b.Name == a.Name));
            }

            string result = null;

            if (!string.IsNullOrEmpty(result = WriteDocumentation(targetType)))
                writer.Write(result);
            if (!string.IsNullOrEmpty(result = WriteTypeUsingStatements(targetType)))
                writer.Write(result);
            if (!string.IsNullOrEmpty(result = WriteTypeBeginNamespace(targetType)))
                writer.Write(result);
            if (!string.IsNullOrEmpty(result = WriteDocumentationType(targetType)))
                writer.Write(result);
            if (!string.IsNullOrEmpty(result = WriteTypeClassAttributes(targetType)))
                writer.Write(result);
            if (!string.IsNullOrEmpty(result = WriteTypeBeginClass(targetType)))
                writer.Write(result);

            foreach (EdmProperty property in usableProperties)
            {
                if (!string.IsNullOrEmpty(result = WriteTypeFieldAttributes(property)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteTypeField(property)))
                    writer.Write(result);
            }
            foreach (NavigationProperty navProperty in usableNavProperties)
            {
                if (!string.IsNullOrEmpty(result = WriteTypeFieldAttributes(navProperty)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteTypeField(navProperty)))
                    writer.Write(result);
            }

            writer.WriteLine();

            foreach (EdmProperty property in usableProperties)
            {
                if (!string.IsNullOrEmpty(result = WriteTypePropertyAttributes(property)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteTypeProperty(property)))
                    writer.Write(result);
            }
            foreach (NavigationProperty navProperty in usableNavProperties)
            {
                if (!string.IsNullOrEmpty(result = WriteTypePropertyAttributes(navProperty)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteTypeProperty(navProperty)))
                    writer.Write(result);
            }

            if (!string.IsNullOrEmpty(result = WriteTypeEndClass(targetType)))
                writer.Write(result);
            if (!string.IsNullOrEmpty(result = WriteTypeEndNamespace(targetType)))
                writer.Write(result);
        }

        /// <summary>
        /// Inherited from <see cref="IEntityGenerator"/>.
        /// Provides the content for a ObjectContext file.
        /// </summary>
        /// <param name="container">An <see cref="EntityContainer"/> object representing the object context.</param>
        /// <param name="writer">The <see cref="TextWriter"/> object to write too.</param>
        public void WriteObjectContext(EntityContainer container, TextWriter writer)
        {
            if (container == null)
                throw new ArgumentNullException("container must not be null.");
            if (writer == null)
                throw new ArgumentNullException("writer must not be null.");

            if (container.BaseEntitySets.Count > 0 || container.FunctionImports.Count > 0)
            {
                string result = null;
                var entitysets = container.BaseEntitySets.Where(a => a.BuiltInTypeKind == BuiltInTypeKind.EntitySet);

                NamespaceName = entitysets.Count() > 0 ? entitysets.First().ElementType.NamespaceName :
                    container.FunctionImports.Count > 0 ? container.FunctionImports[0].NamespaceName : string.Empty;
                
                if (!string.IsNullOrEmpty(result = WriteDocumentation(container)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteContextUsingStatements(container)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteContextBeginNamespace(container)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteDocumentationType(container)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteContextClassAttributes(container)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteContextBeginClass(container)))
                    writer.Write(result);
                foreach (EntitySetBase esb in entitysets)
                {
                    if (!string.IsNullOrEmpty(result = WriteContextEntitySetField(esb)))
                        writer.Write(result);
                }
                foreach (EntitySetBase esb in entitysets)
                {
                    if (!string.IsNullOrEmpty(result = WriteDocumentationProperty(esb)))
                        writer.Write(result);
                    if (!string.IsNullOrEmpty(result = WriteContextEntitySetProperty(esb)))
                        writer.Write(result);
                }
                foreach (EdmFunction ef in container.FunctionImports)
                {
                    if (!string.IsNullOrEmpty(result = WriteDocumentationMethod(ef)))
                        writer.Write(result);
                    if (!string.IsNullOrEmpty(result = WriteContextFunctionImport(ef)))
                        writer.Write(result);
                }
                if (!string.IsNullOrEmpty(result = WriteContextEndClass(container)))
                    writer.Write(result);
                if (!string.IsNullOrEmpty(result = WriteContextEndNamespace(container)))
                    writer.Write(result);
            }
        }

        #endregion
    }
}
