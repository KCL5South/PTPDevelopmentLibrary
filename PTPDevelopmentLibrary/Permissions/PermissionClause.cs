using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;
using System;

namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// Represents an individual clause used to determine if a user has appropriate permissions.
    /// </summary>
    [TypeConverter(typeof(PermissionClauseToStringTypeConverter))]
    public class PermissionClause
    {
        /// <summary>
        /// A <see cref="TypeConverter"/> for the <see cref="PermissionClause"/> object so that it can be parsed within Xaml.
        /// </summary>
        public class PermissionClauseToStringTypeConverter : TypeConverter
        {
            /// <summary>
            /// Call this method to see if a given <see cref="Type"/> can be converted into a <see cref="PermissionClause"/>.
            /// </summary>
            /// <param name="context">An object that provides the format context.</param>
            /// <param name="sourceType">The source <see cref="Type"/>.</param>
            /// <returns>A value indicating wether an object of the given <see cref="Type"/> can be 
            /// converted into a <see cref="PermissionClause"/>.</returns>
            public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// Call this method to see if a <see cref="PermissionClause"/> can be converted into an object of the given <see cref="Type"/>.
            /// </summary>
            /// <param name="context">An object that provides the format context.</param>
            /// <param name="destinationType">The destination <see cref="Type"/>.</param>
            /// <returns>A value indicating wether an <see cref="PermissionClause"/> can be 
            /// converted into an object of the given <see cref="Type"/>.</returns>
            public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// Performs the conversion from the given <see cref="Type"/> to a <see cref="PermissionClause"/>.
            /// </summary>
            /// <param name="context">An object that provides a format context.</param>
            /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> to use as the current culture.</param>
            /// <param name="value">The value to convert to the type of this converter.</param>
            /// <returns>The converted value.</returns>
            /// <seealso cref="TypeConverter"/>
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                PermissionClause result = new PermissionClause();

                if (value is string)
                {
                    result.Clause = value.ToString();
                }

                return result;
            }

            /// <summary>
            /// Converts the specified value object to the specified type.
            /// </summary>
            /// <param name="context">An object that provides a format context.</param>
            /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> to use as the current culture.</param>
            /// <param name="value">The value to convert to the type of this converter.</param>
            /// <param name="destinationType">The type to convert the object to.</param>
            /// <returns>The converted object.</returns>
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (destinationType == null)
                    throw new ArgumentNullException("destinationType");

                if (destinationType == typeof(string))
                {
                    if (!(value is PermissionClause))
                        throw new ArgumentException("The value parameter must be of type PermissionClause");

                    PermissionClause clause = (value as PermissionClause);
                    return clause.Clause;
                }
                else
                    throw new ArgumentException("The destinationType parameter must be a type of type string.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PermissionClause() {}

        /// <summary>
        /// The clause representing the permissions.  Operators you may use: AND, OR, XOR, NOT
        /// Ex. <code>RoleA AND RoleB NOT RoleC</code> This means that if the user is within
        /// RoleA and RoleB but not RoleC then they have permission to use the target Module.
        /// </summary>
        public string Clause { get; set; }

        /// <summary>
        /// Evaluates the clause represented in <see cref="P:PermissionClause.Clause"/>.
        /// </summary>
        /// <param name="userRoles">A <see cref="List{String}"/> object containing a set of roles.</param>
        /// <returns>The result of the clause.</returns>
        public bool Evaluate(List<string> userRoles)
        {
            PermissionClauseParser parser = new PermissionClauseParser(Clause, userRoles);
            return parser.IsValid;
        }

        /// <summary>
        /// Implicit conversion.  <see cref="String"/> to <see cref="PermissionClause"/>
        /// </summary>
        /// <param name="clause">The string representation of the clause</param>
        /// <returns>A <see cref="PermissionClause"/> representing the clause.</returns>
        public static implicit operator PermissionClause(string clause)
        {
            return new PermissionClause() { Clause = clause };
        }

        /// <summary>
        /// Implicit conversion.  <see cref="PermissionClause"/> to <see cref="String"/>
        /// </summary>
        /// <param name="clause">A <see cref="PermissionClause"/> to convert from.</param>
        /// <returns>The string representation of the clause.</returns>
        public static implicit operator string(PermissionClause clause)
        {
            return clause.Clause;
        }
    }
}