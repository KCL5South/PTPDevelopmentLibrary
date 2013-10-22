using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Collections.Generic;
namespace PTPDevelopmentLibrary.Expressions
{
    /// <summary>
    /// This class houses helper methods that can be used as extension methods.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// This method replaces all parameters within <paramref name="oldParameters"/> with the 
        /// parameters in <paramref name="newParameters"/>.
        /// </summary>
        /// <param name="target">The <see cref="Expression"/> to perform the switch on.</param>
        /// <param name="oldParameters">A <see cref="ReadOnlyCollection{ParameterExpression}"/> collection of parameters representing the parameters to replace.</param>
        /// <param name="newParameters">A <see cref="ReadOnlyCollection{Expression}"/> collection of parameters representing the new parameters.</param>
        /// <returns>An <see cref="Expression"/> with the new parameters.</returns>
        public static Expression ReplaceParameters(this Expression target, ReadOnlyCollection<ParameterExpression> oldParameters, ReadOnlyCollection<Expression> newParameters)
        {
            ReplaceParametersVisitors visitor = new ReplaceParametersVisitors(oldParameters, newParameters);
            return visitor.ReplaceVisit(target);
        }

        /// <summary>
        /// Use this method to remove any <see cref="InvocationExpression"/> expression from your expression tree.
        /// </summary>
        /// <typeparam name="T">A delegate type.</typeparam>
        /// <param name="target">The <see cref="Expression{T}"/> that the conversion will happen on.</param>
        /// <param name="parameters">A <see cref="ReadOnlyCollection{ParameterExpression}"/> collection representing the parameters within the given lambda expression.</param>
        /// <returns>The converted <see cref="Expression{T}"/>.</returns>
        public static Expression<T> RemoveInvocationExpressions<T>(this Expression<T> target, ReadOnlyCollection<ParameterExpression> parameters)
        {
            ExpressionRemoveInvokeVisitor<T> visitor = new ExpressionRemoveInvokeVisitor<T>(parameters);
            return visitor.RemoveInvokeVisit(target);
        }

        /// <summary>
        /// Use this method to remove any <see cref="InvocationExpression"/> expression from your expression tree.
        /// </summary>
        /// <typeparam name="T">A delegate type.</typeparam>
        /// <param name="target">The <see cref="Expression{T}"/> that the conversion will happen on.</param>
        /// <param name="parameters">An array representing the parameters within the given lambda expression.</param>
        /// <returns>The converted <see cref="Expression{T}"/>.</returns>
        public static Expression<T> RemoveInvocationExpressions<T>(this Expression<T> target, params ParameterExpression[] parameters)
        {
            ExpressionRemoveInvokeVisitor<T> visitor = new ExpressionRemoveInvokeVisitor<T>(new ReadOnlyCollection<ParameterExpression>(new List<ParameterExpression>(parameters)));
            return visitor.RemoveInvokeVisit(target);
        }
    }
}