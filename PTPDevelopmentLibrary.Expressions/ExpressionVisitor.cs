using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
namespace PTPDevelopmentLibrary.Expressions
{
    /// <summary>
    /// This abstract class is used to traverse an expression tree.  
    /// Inheriting objects will use it's methods to perform
    /// the desired operations on the expression tree.
    /// </summary>
    public abstract class ExpressionVisitor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected ExpressionVisitor()
        {
        }

        /// <summary>
        /// This method represents the heart of tree walk.  
        /// Passing an expression to this method will initialize the tree walk.
        /// </summary>
        /// <param name="exp">An <see cref="Expression"/> object reperesenting the tree to walk.</param>
        /// <returns>The resulting <see cref="Expression"/> object after the walk has been performed.</returns>
        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        /// <summary>
        /// When a <see cref="MemberBinding"/> node is encountered, this method is executed.
        /// </summary>
        /// <param name="binding">A <see cref="MemberBinding"/>.</param>
        /// <returns>A <see cref="MemberBinding"/>.</returns>
        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        /// <summary>
        /// When an <see cref="ElementInit"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="initializer">An <see cref="ElementInit"/>.</param>
        /// <returns>An <see cref="ElementInit"/>.</returns>
        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        /// <summary>
        /// When an <see cref="UnaryExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="u">A <see cref="UnaryExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/>.</returns>
        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = this.Visit(u.Operand);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        /// <summary>
        /// When a <see cref="BinaryExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="b">A <see cref="BinaryExpression"/> expression.</param>
        /// <returns>A <see cref="BinaryExpression"/> expression.</returns>
        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            Expression conversion = this.Visit(b.Conversion);
            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        /// <summary>
        /// When a <see cref="TypeBinaryExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="b">A <see cref="TypeBinaryExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expr = this.Visit(b.Expression);
            if (expr != b.Expression)
            {
                return Expression.TypeIs(expr, b.TypeOperand);
            }
            return b;
        }

        /// <summary>
        /// When a <see cref="ConstantExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="c">A <see cref="ConstantExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        /// <summary>
        /// When a <see cref="ConditionalExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="c">A <see cref="ConditionalExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = this.Visit(c.Test);
            Expression ifTrue = this.Visit(c.IfTrue);
            Expression ifFalse = this.Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        /// <summary>
        /// When a <see cref="ParameterExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="p">A <see cref="ParameterExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        /// <summary>
        /// When a <see cref="MemberExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="m">A <see cref="MemberExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = this.Visit(m.Expression);
            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }
            return m;
        }

        /// <summary>
        /// When a <see cref="MethodCallExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="m">A <see cref="MethodCallExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression obj = this.Visit(m.Object);
            IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
            if (obj != m.Object || args != m.Arguments)
            {
                return Expression.Call(obj, m.Method, args);
            }
            return m;
        }

        /// <summary>
        /// This method can be used to visit a collection of expressions.
        /// </summary>
        /// <param name="original">The original <see cref="ReadOnlyCollection{Expression}"/> collection of expressions.</param>
        /// <returns>The modified <see cref="ReadOnlyCollection{Expression}"/> collection of expressions.</returns>
        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = this.Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            if (list != null)
            {
                return list.AsReadOnly();
            }
            return original;
        }

        /// <summary>
        /// Whenever a <see cref="MemberAssignment"/> is encountered within the tree, this method is called.
        /// </summary>
        /// <param name="assignment">A <see cref="MemberAssignment"/>.</param>
        /// <returns>A <see cref="MemberAssignment"/>.</returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = this.Visit(assignment.Expression);
            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }

        /// <summary>
        /// Whenever a <see cref="MemberMemberBinding"/> is encountered within the tree, this method is called.
        /// </summary>
        /// <param name="binding">A <see cref="MemberMemberBinding"/>.</param>
        /// <returns>A <see cref="MemberMemberBinding"/>.</returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }

        /// <summary>
        /// Whenever a <see cref="MemberListBinding"/> is encountered within the tree, this method is called.
        /// </summary>
        /// <param name="binding">A <see cref="MemberListBinding"/>.</param>
        /// <returns>A <see cref="MemberListBinding"/>.</returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        /// <summary>
        /// Whenever a collection of <see cref="MemberBinding"/> objects is encountered within the tree, this method is called.
        /// </summary>
        /// <param name="original">The original <see cref="ReadOnlyCollection{MemberBinding}"/> collection.</param>
        /// <returns>The resulting <see cref="IEnumerable{MemberBinding}"/> collection.</returns>
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = this.VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        /// <summary>
        /// Whenever a collection of <see cref="ElementInit"/> objects is encountered within the tree, this method is called.
        /// </summary>
        /// <param name="original">The original <see cref="ReadOnlyCollection{ElementInit}"/> collection.</param>
        /// <returns>The resulting <see cref="IEnumerable{ElementInit}"/> collection.</returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = this.VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        /// <summary>
        /// When a <see cref="LambdaExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="lambda">A <see cref="LambdaExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = this.Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        /// <summary>
        /// When a <see cref="NewExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="nex">A <see cref="NewExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments)
            {
                if (nex.Members != null)
                    return Expression.New(nex.Constructor, args, nex.Members);
                else
                    return Expression.New(nex.Constructor, args);
            }
            return nex;
        }

        /// <summary>
        /// When a <see cref="MemberInitExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="init">A <see cref="MemberInitExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }

        /// <summary>
        /// When a <see cref="ListInitExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="init">A <see cref="ListInitExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }
            return init;
        }

        /// <summary>
        /// When a <see cref="NewArrayExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="na">A <see cref="NewArrayExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = this.VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
                }
            }
            return na;
        }

        /// <summary>
        /// When a <see cref="InvocationExpression"/> is encountered, this method is executed.
        /// </summary>
        /// <param name="iv">A <see cref="InvocationExpression"/> expression.</param>
        /// <returns>An <see cref="Expression"/> expression.</returns>
        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(iv.Arguments);
            Expression expr = this.Visit(iv.Expression);
            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }
            return iv;
        }
    }
}