using System.Collections.ObjectModel;
using System.Linq.Expressions;
namespace PTPDevelopmentLibrary.Expressions
{
    /// <summary>
    /// This visitor is used to replace the parameters in an expression with another set of parameters.
    /// </summary>
    public class ReplaceParametersVisitors : ExpressionVisitor
    {
        private ReadOnlyCollection<Expression> newParameters;
        private ReadOnlyCollection<ParameterExpression> oldParameters;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldParameters">A <see cref="ReadOnlyCollection{ParameterExpression}"/> collection of parameters.</param>
        /// <param name="newParameters">A <see cref="ReadOnlyCollection{Expression}"/> collection of expressions that represent
        /// the new parameters.</param>
        public ReplaceParametersVisitors(ReadOnlyCollection<ParameterExpression> oldParameters,
                                         ReadOnlyCollection<Expression> newParameters)
        {
            this.newParameters = newParameters;
            this.oldParameters = oldParameters;
        }

        /// <summary>
        /// Overridden from <see cref="ExpressionVisitor"/>.
        /// </summary>
        /// <param name="p">A <see cref="ParameterExpression"/>.</param>
        /// <returns>A <see cref="Expression"/>.</returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (oldParameters != null && newParameters != null)
            {
                if (oldParameters.Contains(p))
                    return newParameters[oldParameters.IndexOf(p)];
            }

            return base.VisitParameter(p);
        }

        /// <summary>
        /// This method invokes the initial walk of your expression that will replace the parameters.
        /// </summary>
        /// <param name="exp">Your original <see cref="Expression"/>.</param>
        /// <returns>Your resulting <see cref="Expression"/> with the new parameters.</returns>
        public Expression ReplaceVisit(Expression exp)
        {
            return Visit(exp);
        }
    }
}