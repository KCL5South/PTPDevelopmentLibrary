using System.Collections.Generic;
using System;
namespace PTPDevelopmentLibrary.Permissions
{
    /// <summary>
    /// This object is used to parse a clause represented by <see cref="P:PermissionClause.Clause"/> property.
    /// </summary>
    public class PermissionClauseParser
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clause">The string representation of the clause</param>
        /// <param name="userRoles">The set of roles to test the clause against.</param>
        public PermissionClauseParser(string clause, List<string> userRoles)
        {
            ParsingException = null;
            IsValid = false;

            //If the clause is null or empty, don't validate anything.
            if (clause == null || clause == string.Empty)
            {
                IsValid = true;
                return;
            }

            if (userRoles == null)
                throw new ArgumentNullException("userRoles");

            try
            {
                Queue<string> rpnClause = ShuntYard(clause);
                Stack<bool> resultStack = new Stack<bool>();

                while (rpnClause.Count > 0)
                {
                    string target = rpnClause.Dequeue();
                    if (target == PermissionClauseOperators.And)
                    {
                        bool first, second;
                        if (resultStack.Count >= 2)
                        {
                            first = resultStack.Pop();
                            second = resultStack.Pop();
                            resultStack.Push(first & second);
                        }
                    }
                    else if (target == PermissionClauseOperators.Not)
                    {
                        bool first;
                        if (resultStack.Count >= 1)
                        {
                            first = resultStack.Pop();
                            resultStack.Push(!first);
                        }
                    }
                    else if (target == PermissionClauseOperators.Or)
                    {
                        bool first, second;
                        if (resultStack.Count >= 2)
                        {
                            first = resultStack.Pop();
                            second = resultStack.Pop();
                            resultStack.Push(first | second);
                        }
                    }
                    else if (target == PermissionClauseOperators.XOr)
                    {
                        bool first, second;
                        if (resultStack.Count >= 2)
                        {
                            first = resultStack.Pop();
                            second = resultStack.Pop();
                            resultStack.Push(first ^ second);
                        }
                    }
                    else
                    {
                        resultStack.Push(userRoles.Contains(target));
                    }
                }

                if (resultStack.Count != 1)
                    throw new PermissionClauseParserException("Multiple resulting clauses resulting.");

                IsValid = resultStack.Pop();
                ParsingException = null;
            }
            catch (Exception ex)
            {
                IsValid = false;
                ParsingException = ex;
            }
        }

        /// <summary>
        /// The Shunting-Yard Algorithm.
        /// http://en.wikipedia.org/wiki/Shunting_yard_algorithm
        /// This algorithm parses the clause into Reverse polish notation.
        /// http://en.wikipedia.org/wiki/Reverse_Polish_notation
        /// </summary>
        /// <param name="clause">The string representation of the clause</param>
        /// <returns>A <see cref="Queue{String}"/> object containing the Reverse Polish Notation representation of the clause.</returns>
        private Queue<string> ShuntYard(string clause)
        {
            Queue<string> result = new Queue<string>();
            Stack<PermissionClauseOperators> operators = new Stack<PermissionClauseOperators>();

            int currentIndex = 0;
            int lastIndex = clause.Length - 1;
            try
            {
                while (currentIndex <= lastIndex)
                {
                    string targetPhrase = ExtractNextElement(clause, ref currentIndex, ref lastIndex);

                    if (PermissionClauseOperators.OpenPhrase == targetPhrase)
                    {
                        operators.Push(PermissionClauseOperators.OpenPhrase);
                    }
                    else if (targetPhrase == PermissionClauseOperators.ClosePhrase)
                    {
                        while (operators.Count > 0 && operators.Peek() != PermissionClauseOperators.OpenPhrase)
                            result.Enqueue(operators.Pop());
                        if (operators.Count == 0)
                            throw new PermissionsClauseParserSyntaxException("There was no matching '(' for the closing ')'");

                        operators.Pop();
                    }
                    else if (targetPhrase == PermissionClauseOperators.And)
                    {
                        PermissionClauseOperators target = PermissionClauseOperators.And;
                        OperatorTest(ref result, ref operators, ref target);
                    }
                    else if (targetPhrase == PermissionClauseOperators.Or)
                    {
                        PermissionClauseOperators target = PermissionClauseOperators.Or;
                        OperatorTest(ref result, ref operators, ref target);
                    }
                    else if (targetPhrase == PermissionClauseOperators.Not)
                    {
                        PermissionClauseOperators target = PermissionClauseOperators.Not;
                        OperatorTest(ref result, ref operators, ref target);
                    }
                    else if (targetPhrase == PermissionClauseOperators.XOr)
                    {
                        PermissionClauseOperators target = PermissionClauseOperators.XOr;
                        OperatorTest(ref result, ref operators, ref target);
                    }
                    else
                    {
                        result.Enqueue(targetPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PermissionsClauseParserSyntaxException("Syntax error at position: " + currentIndex + ".", ex);
            }

            while (operators.Count > 0)
                result.Enqueue(operators.Pop());

            return result;
        }

        /// <summary>
        /// This method simply extracts the next element from the given clause, <paramref name="source"/>.
        /// Given the <paramref name="currentIndex"/> and <paramref name="lastIndex"/> of the clause, this
        /// algorithm determines the next element of the clause and returns it, also updating <paramref name="currentIndex"/>.
        /// NOTE: Whenever whitespace is encountered in the clause, the method is called recursively.
        /// </summary>
        /// <param name="source">The source clause to extract the element from.</param>
        /// <param name="currentIndex">The current index within the clause to test for the next element.</param>
        /// <param name="lastIndex">The last index of the clause.</param>
        /// <returns>The next element given <paramref name="currentIndex"/></returns>
        private string ExtractNextElement(string source, ref int currentIndex, ref int lastIndex)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (source == string.Empty)
                throw new ArgumentException("source must not be empty.");
            if (currentIndex > lastIndex)
                throw new ArgumentException("currentIndex must be less than or equal to lastIndex");

            if (char.IsWhiteSpace(source[currentIndex]))
            {
                currentIndex++;
                return ExtractNextElement(source, ref currentIndex, ref lastIndex);
            }

            if (source[currentIndex].ToString() == PermissionClauseOperators.OpenPhrase)
            {
                currentIndex++;
                return PermissionClauseOperators.OpenPhrase;
            }
            else if (source[currentIndex].ToString() == PermissionClauseOperators.ClosePhrase)
            {
                currentIndex++;
                return PermissionClauseOperators.ClosePhrase;
            }

            int start = currentIndex;
            while (currentIndex <= lastIndex && !char.IsWhiteSpace(source[currentIndex]) &&
                  source[currentIndex].ToString() != PermissionClauseOperators.OpenPhrase &&
                  source[currentIndex].ToString() != PermissionClauseOperators.ClosePhrase)
            {
                currentIndex++;
            }

            return source.Substring(start, currentIndex - start);
        }

        /// <summary>
        /// Given the way that the Shunting-Yard algorithm works (http://en.wikipedia.org/wiki/Shunting_yard_algorithm)
        /// it made more sense to isolate the tests involving operators.
        /// This method tests the current operator with the operators on the operator stack (<paramref name="operators"/>).
        /// According to the Shunting-Yard algorithm if the current operator (<paramref name="target"/>) is left associative
        /// (<see cref="E:PermissionClauseOperatorAssociativities.LeftToRight"/>) then it loops through all the operators on the operator stack until
        /// it finds one that has a greater precedence than the current operator.  All the while placing those operators on the RPN Queue
        /// (<paramref name="result"/>).  If the current operator is right associative (<see cref="E:PermissionClauseOperatorAssociativities.RightToLeft"/>)
        /// then it loops through all the operators on the operator stack until it find one with precedence that is greater or equal to 
        /// the current operators precedence.  Again placing the operators into the RPN queue if they fail the test.
        /// </summary>
        /// <param name="result">The RPN Queue</param>
        /// <param name="operators">The operator stack</param>
        /// <param name="target">The current operator</param>
        private void OperatorTest(ref Queue<string> result, ref Stack<PermissionClauseOperators> operators, ref PermissionClauseOperators target)
        {
            if (result == null)
                throw new ArgumentNullException("result");
            if (operators == null)
                throw new ArgumentNullException("operators");
            if (target == null)
                throw new ArgumentNullException("target");

            if (operators.Count > 0 && (target.Associativity == PermissionClauseOperatorAssociativities.LeftToRight || target.Associativity == PermissionClauseOperatorAssociativities.None))
            {
                while (operators.Count > 0 && operators.Peek().Precedence > target.Precedence)
                    result.Enqueue(operators.Pop());
            }
            else if (operators.Count > 0 && target.Associativity == PermissionClauseOperatorAssociativities.RightToLeft)
            {
                while (operators.Count > 0 && operators.Peek().Precedence >= target.Precedence)
                    result.Enqueue(operators.Pop());
            }

            operators.Push(target);
        }

        /// <summary>
        /// Represents the evaluation of the parse.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// If an exception is encountered, it is placed here.  So, if <see cref="P:PermissionClauseParser.IsValid"/> is false then this
        /// property should be checked for an exception.
        /// </summary>
        public Exception ParsingException { get; private set; }
    }
}