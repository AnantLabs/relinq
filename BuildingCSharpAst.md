Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** [RelinqScript AST](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax) with types, fields and methods inferred on the previous step of [JS -> C#](http://code.google.com/p/relinq/wiki/JSToCSharp) algorithm.

**Errors:** Algorithm crashes if there exist nodes in the AST that don't have their types inferred yet (i.e. unknown constants, method groups and lambdas used outside of method/operator invocations).

**Output:** C# expression tree equivalent to the input AST. If this is a reverse transformation in the C# -> JS -> C# the output will be identical to the source expression tree provided [certain conditions are met](http://code.google.com/p/relinq/wiki/Roundtrip).

### The algorithm ###

The algorithm recursively traverses the RelinqScript AST using depth-first algorithm and creating the AST with `Expresssion.*` methods:

<table cellpadding='5' border='1'>

<tr>
<td width='15%' valign='top'>Node type</td>
<td width='15%' valign='top'>LINQ expression type</td>
<td valign='top'>Notes</td>
</tr>

<tr>
<td valign='top'>Keyword</td>
<td valign='top'>Keyword</td>
<td valign='top'>
Represents a single keyword of RelinqScript: <code>ctx</code> that corresponds to the data context that hosts <code>IQueryable</code> beans.</td>
</tr>

<tr>
<td valign='top'>Variable</td>
<td valign='top'>Parameter</td>
<td valign='top'>
Gets the corresponding ParameterExpression from the parent Lambda's cache. .NET is extremely picky regarding ParameterExpressions and might crash if they are not coherent with each other and/or their Lambda.<br>
</td>
</tr>

<tr>
<td valign='top'>Constant</td>
<td valign='top'>Constant</td>
<td valign='top'>
Typically just creates a ConstantExpression with the inferred type and the object <a href='http://code.google.com/p/relinq/wiki/JsonSerialization'>deserialized from Constant's data</a>. However there exist certain special cases:<br>
<ul><li>Node that has <code>null</code> type and is used in context of a Conditional node inherits its type from the parent.<br>
</li><li>Node that has <code>null</code> type and is used in context of an Invoke/Operator/Indexer nodes inherits its type from the corresponding argument of the parent.<br>
</li><li>Node that has <code>UnknownConstant</code> type and is used in context of a Conditional node inherits its type from the parent.<br>
</li><li>Node that has <code>UnknownConstant</code> type and is used in context of an Invoke/Operator/Indexer nodes and inherits its type from the corresponding argument of the parent.<br>
</li><li>In any other case when the expression is of an auxiliary type, builder crashes.<br>
</td>
</tr></li></ul>

<tr>
<td valign='top'>New</td>
<td valign='top'>New</td>
<td valign='top'>
Calls the constructor of the suitable Tuple (Relinq's internal type) that emulates an anonymous class with given number of fields. This is the only one supported construct that explicitly creates an object.<br>
</td>
</tr>

<tr>
<td valign='top'>Lambda</td>
<td valign='top'>Lambda</td>
<td valign='top'>
Creates a set of parameter expressions and a new lambda expression with these parameters.<br>
</td>
</tr>

<tr>
<td valign='top'>MemberAccess</td>
<td valign='top'>MemberAccess or ArrayLength or a special construct that creates a delegate from the method group</td>
<td valign='top'>
Creates a new MemberExpression. As a sidenote: one should use <code>Expression.Field</code> or <code>Expression.Property</code> factory method here, since <code>Expression.PropertyOrField</code> doesn't allow to specify visibility rules. An exception from this rule:<br>
<ul><li>If the target has an array type and we access the <code>Length</code> member then the algorithm creates the ArrayLength expression instead.<br>
</li><li>If access has a type of <code>MethodGroup</code> and it's an argument/operand/branch of an Invoke/Operator/Indexer/Conditional node, a special construct that creates a delegate from the method group is emitted, namely: <code>Delegate.CreateDelegate(TDelegate, &lt;target&gt;, MethodInfo)</code>, where <code>TDelegate</code> is a type of corresponding parameter/operand and <code>MethodInfo</code> is a resolved signature from the method group.<br>
</li><li>Otherwise if node is of any auxiliary type, it crashes the builder.<br>
</td>
</tr></li></ul>

<tr>
<td valign='top'>Invoke</td>
<td valign='top'>Call or Invoke</td>
<td valign='top'>

Depending on the type of call binding (early-bound, i.e. method invocation, or late-bound, i.e. invocation of a delegate), different expressions are generated:<br>
<br>
<table cellpadding='5' border='1'>

<tr>
<td valign='top'>Early-bound calls</td>
<td valign='top'>Late-bound calls</td>
</tr>

<tr>
<td valign='top'>
Creates a MethodCallExpression with arguments wrapped in conversion expressions to fit the parameters of the methods, as follows (given <code>TA</code> is the type of an argument expression <code>A</code>, and <code>TP</code> is the type of method parameter):<br>
<ul><li>If <code>TP</code> is an <code>Expression</code> or its descendant, then <code>A</code> is wrapped in a Quote expression.<br>
</li><li>If <code>TP</code> is not a <a href='http://code.google.com/p/relinq/wiki/LookupBaseTypes'>base type</a> of <code>TA</code> or an interface implemented by <code>TA</code>, then <code>A</code> is wrapped in a Convert expression that corresponds to <a href='ImplicitConversions.md'>ImplicitConversions</a> used during <a href='TypeInferenceMethods.md'>TypeInferenceMethods</a> step of the compilation with (if it's involved) the specified user-defined conversion. <i>As a sidenote: C# expression trees fail to support spec-compliant u/d conversions that consist of three step: implicit -> u/d -> implicit, so all involved implicit conversions are created manually according to the rules described above.</i>
</td>
<td valign='top'>
Constructs an InvokeExpression and wraps its arguments in conversion expressions to fit parameters of the delegate, as described to the left.<br>
</td>
</tr></li></ul>

</table>

</td>
</tr>

<tr>
<td valign='top'>Indexer</td>
<td valign='top'>Call or ArrayIndex</td>
<td valign='top'>
Creates a MethodCallExpression with arguments wrapped in conversion expressions to fit the parameters of the method (as described above).<br>
<br>
If the target of the method is an array, then under certain conditions the algorithm emits different nodes (tho arguments are still wrapped as described above):<br>
<ul><li>If the target is a one-dimensional or multi-dimensional jagged array, then algorithm emits a hierarchy of ArrayIndex nodes (number of nodes is equal to number of array dimensions = number of method arguments).<br>
</td>
</tr></li></ul>

<tr>
<td valign='top'>Operator</td>
<td valign='top'>A multitude of possible types</td>
<td valign='top'>
Finds out the LINQ expression type that corresponds to the operator type (see the table below). According to acquired type creates UnaryExpression or BinaryExpression with operands wrapped in conversion expressions to fit the parameters of the methods, as described above.<br>
<br>
<table>
<tr>
<td valign='top'><a href='http://relinq.googlecode.com/svn/wiki/images/allRelinqOperators.PNG'>http://relinq.googlecode.com/svn/wiki/images/allRelinqOperators.PNG</a></td>
<td>
<table><thead><th> Relinq operator type </th><th> LINQ expression type </th></thead><tbody>
<tr><td> UnaryPlus </td><td> UnaryPlus </td></tr>
<tr><td> UnaryMinus </td><td> Negate </td></tr>
<tr><td> - </td><td> NegateChecked </td></tr>
<tr><td> OnesComplement </td><td> Not </td></tr>
<tr><td> LogicalNot </td><td> Not </td></tr>
<tr><td> Multiply </td><td> Multiply </td></tr>
<tr><td> - </td><td> MultiplyChecked </td></tr>
<tr><td> Divide </td><td> Divide </td></tr>
<tr><td> Modulo </td><td> Modulo </td></tr>
<tr><td> Add </td><td> Add </td></tr>
<tr><td> - </td><td> AddChecked </td></tr>
<tr><td> Subtract </td><td> Subtract </td></tr>
<tr><td> - </td><td> SubtractChecked </td></tr>
<tr><td> LeftShift </td><td> LeftShift </td></tr>
<tr><td> RightShift </td><td> RightShift </td></tr>
<tr><td> Equal </td><td> Equal </td></tr>
<tr><td> NotEqual </td><td> NotEqual </td></tr>
<tr><td> GreaterThan </td><td> GreaterThan </td></tr>
<tr><td> LessThan </td><td> LessThan </td></tr>
<tr><td> GreaterThanOrEqual </td><td> GreaterThanOrEqual </td></tr>
<tr><td> LessThanOrEqual </td><td> LessThanOrEqual </td></tr>
<tr><td> And </td><td> And </td></tr>
<tr><td> Or </td><td> Or </td></tr>
<tr><td> ExclusiveOr </td><td> ExclusiveOr </td></tr>
<tr><td> - </td><td> Power </td></tr>
<tr><td> AndAlso </td><td> AndAlso </td></tr>
<tr><td> OrElse </td><td> OrElse </td></tr>
<tr><td> - </td><td> Coalesce </td></tr>
<tr><td> (see below) </td><td> Conditional </td></tr>
</td>
</tr>
</table></tbody></table>

</td>
</tr>

<tr>
<td valign='top'>Conditional</td>
<td valign='top'>Conditional</td>
<td valign='top'>
Constructs a ConditionalExpression and wrapps its clauses in conversion expressions to fit the parameters of the methods, as described above.<br>
</td>
</tr>

</table>