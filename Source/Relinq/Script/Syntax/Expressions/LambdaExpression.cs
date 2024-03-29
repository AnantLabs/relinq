using System;
using System.Collections.Generic;
using System.Linq;
using Relinq.Helpers.Collections;
using Relinq.Helpers.Strings;

namespace Relinq.Script.Syntax.Expressions
{
    public class LambdaExpression : RelinqScriptExpression
    {
        public IEnumerable<String> Args { get; private set; }
        public RelinqScriptExpression Body { get; private set; }

        public LambdaExpression(IEnumerable<String> args, RelinqScriptExpression body)
            : base(ExpressionType.Lambda, body.AsArray())
        {
            Args = args;
            Body = Children.ElementAt(0);
        }

        protected override string GetTPathNode() { return "λ"; }
        protected override string GetTPathSuffix(int childIndex) { return null; }

        protected override string GetContent()
        {
            return String.Format("{0} => {1}", 
                Args.StringJoin().ParenthesizeIf(Args.Count() != 1), Body.Content);
        }
    }
}