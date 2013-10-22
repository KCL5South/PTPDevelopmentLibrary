using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace PTPDevelopmentLibrary.Expressions
{
    /// <summary>
    /// This expression tree visitor is intented to remove all <see cref="InvocationExpression"/>
    /// objects from the tree.
    /// </summary>
    /// <remarks>
    /// When you start combining expression into single expression trees, parameters become muddled, and
    /// just because the parameter might represent the same thing, they aren't necessarily the same thing
    /// according to the expressions.
    /// 
    /// This object can be used to make sure that when you are preparing expressions to combine them into 
    /// one, their parameters all point to the appropriate parameters.
    /// </remarks>
    /// <typeparam name="T">The type used in the strongly typed Lambda Expression.</typeparam>
    public class ExpressionRemoveInvokeVisitor<T> : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> lambdaParameters;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameters">A <see cref="ReadOnlyCollection{ParameterExpression}"/> collection
        /// representing the parameters used in the target expression.</param>
        public ExpressionRemoveInvokeVisitor(ReadOnlyCollection<ParameterExpression> parameters)
        {
            lambdaParameters = parameters;
        }

        /// <summary>
        /// Overridden from <see cref="ExpressionVisitor"/>.
        /// This method uses the <see cref="ReplaceParametersVisitors"/> visitor inorder to 
        /// replace all parameters in the invoked lambda expression with the parameters supplied
        /// when this object was created.
        /// </summary>
        /// <param name="iv">A <see cref="InvocationExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/>.</returns>
        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            var newPars = iv.Arguments;
            LambdaExpression lambda = (iv.Expression) as LambdaExpression;

            if (lambda != null)
            {
                var oldPars = lambda.Parameters;
                ReplaceParametersVisitors replace =
                    new ReplaceParametersVisitors(oldPars, newPars);

                return this.Visit(replace.ReplaceVisit(lambda.Body));
            }
            else
                return base.VisitInvocation(iv);
        }

        /// <summary>
        /// Call this method to start the tree walk that will remove all <see cref="InvocationExpression"/>
        /// expressions from the tree.
        /// </summary>
        /// <example>
        /// This example was pulled from the AgentAgencyLibrary project where this object was first used.
        ///     <![CDATA[
        ///         Expression<Func<T, bool>> target =
        ///             Expression.Lambda<Func<T, bool>>(Expression.MakeBinary(ExpressionType.Equal, idProperty.Body, Expression.Constant(result_Primary.InformationID)),
        ///                                                                    idProperty.Parameters);
        ///
        ///         target = Expression.Lambda<Func<T, bool>>(Expression.Invoke(target, targetT), targetT);
        ///         target = new ExpressionRemoveInvokeVisitor<Func<T, bool>>(target.Parameters).RemoveInvokeVisit(target);
        ///     ]]>
        /// </example>
        /// <param name="exp">A <see cref="Expression{T}"/> representing the tree to walk.</param>
        /// <returns>The resulting <see cref="Expression{T}"/>.</returns>
        public Expression<T> RemoveInvokeVisit(Expression<T> exp)
        {
            return (Expression<T>)Visit(exp);
        }
    }
}
