using System.Collections.Generic;
using System;
namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// Represents an operator used within a <see cref="PermissionClause"/> object.
    /// </summary>
    public class PermissionClauseOperators
    {
        /// <summary>
        /// Constructor. NOTE: This is private because the programmer should not be creating these 
        /// outside the scope of this object.  Use the defined static properties instead.
        /// </summary>
        /// <param name="representation">The string representation of the operator. Ex.  And = "AND"</param>
        /// <param name="precedence">Each operator has a precedence so that the parser knows which to evaluate before the other.</param>
        /// <param name="associativity">A <see cref="PermissionClauseOperatorAssociativities"/> enumeration representing how the operator is evaluated.</param>
        private PermissionClauseOperators(string representation, int precedence, PermissionClauseOperatorAssociativities associativity)
        {
            Representation = representation;
            Precedence = precedence;
            Associativity = associativity;
        }

        /// <summary>
        /// The string representation of this operator
        /// </summary>
        public string Representation { get; private set; }

        /// <summary>
        /// Each operator has a precedence so that the parser knows which to evaluate before the other.  
        /// The higher the precedence the stronger the operator is.
        /// </summary>
        public int Precedence { get; private set; }

        /// <summary>
        /// A <see cref="PermissionClauseOperatorAssociativities"/> enumeration representing how the operator is evaluated.
        /// </summary>
        public PermissionClauseOperatorAssociativities Associativity { get; private set; }

        /// <summary>
        /// Overriden from <see cref="object"/>.  Tests the equality of two <see cref="PermissionClauseOperators"/> objects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is PermissionClauseOperators)
            {
                if ((obj as PermissionClauseOperators).Representation == this.Representation)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Overriden from <see cref="object"/>.
        /// </summary>
        /// <returns>A hashcode representing this object.</returns>
        public override int GetHashCode()
        {
            return Representation.GetHashCode();
        }

        /// <summary>
        /// Implicit conversion from a <see cref="PermissionClauseOperators"/> object to a string.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static implicit operator string(PermissionClauseOperators op)
        {
            return op.Representation;
        }

        /// <summary>
        /// Tests the equality of two <see cref="PermissionClauseOperators"/> objects.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(PermissionClauseOperators lhs, PermissionClauseOperators rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Tests the non-equality of two <see cref="PermissionClauseOperators"/> objects.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(PermissionClauseOperators lhs, PermissionClauseOperators rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// A boolean and operator.
        /// </summary>
        public static PermissionClauseOperators And
        {
            get
            {
                return new PermissionClauseOperators("AND", 3, PermissionClauseOperatorAssociativities.LeftToRight);
            }
        }
        /// <summary>
        /// A boolean or operator.
        /// </summary>
        public static PermissionClauseOperators Or
        {
            get
            {
                return new PermissionClauseOperators("OR", 2, PermissionClauseOperatorAssociativities.LeftToRight);
            }
        }
        /// <summary>
        /// A boolean xor operator.
        /// </summary>
        public static PermissionClauseOperators XOr
        {
            get
            {
                return new PermissionClauseOperators("XOR", 1, PermissionClauseOperatorAssociativities.LeftToRight);
            }
        }
        /// <summary>
        /// A boolean negative operator.
        /// </summary>
        public static PermissionClauseOperators Not
        {
            get
            {
                return new PermissionClauseOperators("NOT", 4, PermissionClauseOperatorAssociativities.LeftToRight);
            }
        }
        /// <summary>
        /// Represents an open parethesis.
        /// </summary>
        public static PermissionClauseOperators OpenPhrase
        {
            get
            {
                return new PermissionClauseOperators("(", 0, PermissionClauseOperatorAssociativities.None);
            }
        }
        /// <summary>
        /// Represents a closed parethesis.
        /// </summary>
        public static PermissionClauseOperators ClosePhrase
        {
            get
            {
                return new PermissionClauseOperators(")", 0, PermissionClauseOperatorAssociativities.None);
            }
        }
    }
}