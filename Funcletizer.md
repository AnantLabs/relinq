Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site).

To widen the class of C# expression trees that can be successfully transformed into JS, Relinq features (optional, configurable) preprocessors that are executed prior to [C# -> JS transformation](http://code.google.com/p/relinq/wiki/CSharpToJS). One of these is a **funcletizer** (credit for the name goes to `System.Data.Linq.SqlClient` authors) that is capable of pre-evaluating certain expression tree parts in an attempt to eliminate unsupported parts.

### Contract ###

**Input:** C# expression tree.

**Errors:** The algorithm never produces errors - unsupported constructs are ignored. The  output of the algorithm is described below.

**Output:** An equivalent C# expression tree with certain sub-trees pre-evaluated and transformed into equivalent ConstantExpressions.

### Design considerations ###

The funcletizing transformation is irreversible so we need to be sure that possible semantic losses are insignificant. However, this problem is not so critical, since some frameworks (e.g. already mentioned Linq-to-SQL) perform funcletization by themselves. Anyways, currently it's unclear what funcletizations can be performed painlessly. Examples of questionably harmful behaviour:
  * Funcletization inside a quoted expression.
  * Funcletization of a static field that itself can't be serialized under conditions when the static type itself is supported by the receiver.

### The algorithm ###

1. Mark all nodes of a given expression tree as "local" or "remote":
  * If node has any child marked as "remote" then it's marked as "remote" else (if it has no children or all are marked as "local") then it's marked as "local".
  * If an expression node is a constant expression that represents a Relinq bean (i.e. implements the marker interface `IBean`) it's marked as "remote".

2. Perform the following for every node marked "local":
  * If node is not a lambda declaration (Lambda), and not an expression quote (Quote), then perform funcletization, i.e. replace the node with
```
  // "e" is assumed to hold a reference to the processed node
  var value = Expression.Lambda(e, new [0]).Compile().DynamicInvoke()
  e = Expression.Constant(value, e.Type);
```

3. Try to simplify the expression tree using the rules below. If any simplification succeeds, return to step 1, else the algorithm ends:
  * If there exists a conditional operator with a test that is ConstantExpression, replace it with corresponding branch of execution.
  * `Where(<expr>, _ => true)` is replaced with `<expr>`.
  * `Where(<expr>, _ => false)` is replaced with an empty array of `<expr>`'s element type.
  * `Select(<expr>, p => p)` is replaced with `<expr>`.
  * If there exists a transparent identifier that isn't used by the logics of the query, it's stripped away.
  * Various simplifications that are based on predefined operators properties, i.e. when operator node has `Method == null`, namely:
    * Numeric operators that can be simplified according to algebraic and bitwise logics (full list of predefined operators is located within the [LookupOperators](LookupOperators.md) document).
    * I'm pretty sure there is a special algorithm for simplifying logical expressions, so more info will arrive eventually.
    * If a certain lifted operator has `null` as its operand, operator can be simplified according to [the spec](http://en.csharp-online.net/ECMA-334:_14.2.7_Lifted_operators) (simplification is different for different operator types).