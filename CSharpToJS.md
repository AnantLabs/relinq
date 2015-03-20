Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site).

This document describes the algorithm of transforming an input C# expression tree to an equivalent piece of JS code. The document itself provides all necessary information about unsupported C# constructs, however that information is available in more structured way in the [UnsupportedCSharpConstructs](UnsupportedCSharpConstructs.md) document. Roundtrip concerns (C# -> JS -> C#) are covered in the [Roundtrip](Roundtrip.md) document.

### Contract ###

**Input:** C# expression tree. This implicitly imposes certain pre-conditions:
  * There is no way that input will contain C# features that can't be described with expressions (flow-control statements, ref parameters, assignment operators, declarations except inlined lambdas and `let` syntactic sugar).

  * C# expression tree a result of compiler-performed transformation of C# source code into `System.Linq.Expressions.Expression`-based AST. The transformation is sometimes lossy as follows:
    * Both Power and ExplicitOr expression types correspond to the same `^` operator.
    * Both `-` (`op_UnaryMinus`) and `~` (`op_OnesComplement`) correspond to the same ExpressionType.Not.
    * Certain constructs e.g. operator `+` (`op_UnaryPlus`) for constant operands get stripped by the compiler during [compile-time constant evaluation](http://en.csharp-online.net/ECMA-334:_14.16_Constant_expressions).

**Errors:** An error occurs if the input contains [unsupported constructs](http://code.google.com/p/relinq/wiki/UnsupportedCSharpConstructs). Otherwise algorithm produces the output as described below.

**Output:** String with JS code that is equivalent to the input C# expression tree. Relinq guarantees that produced JS code will be syntactically and semantically compatible with C#, i.e. not contain [unsupported JS constructs that cause transformation errors](http://code.google.com/p/relinq/wiki/UnsupportedJSConstructs). Relinq also guarantees that the subsequent JS -> C# transformation will produce an identical expression tree given that [certain conditions are met](http://code.google.com/p/relinq/wiki/Roundtrip).

### Design considerations ###

As Relinq was originally developed as a framework for remote LINQ invocations one of the most important design concerns was making sure that source C# expressions after being transformed to JS and subsequently back to C# will be exactly the same as the original ones (will contain exactly the same nodes ordered in the same structure). However there can be three major difficulties with reaching this goal: transformation errors, introduced mess and irreversible AST transformations that occur during the roundtrip. These concerns are covered in [Roundtrip](Roundtrip.md) document.

Original intention of author was to ensure that the produced JS source will be correctly executed given certain functions/classes are defined in the execution context (i.e. a special `$` initializer method of Object metaclass (described in [StuffToDo](StuffToDo.md) document, "Suggested extensions to the spec" section) and all classes and their methods invoked by transformed C# expression). However, JS lacks operator overloading semantics and its primitives treatment semantics is different from one defined by CTS, so this point needs certain attention.

### Transformation algorithm ###

1. After an expression arrives, Relinq makes it pass (optional, configurable) series of irreversible transformations in the order defined below:
  * [Inlining](http://code.google.com/p/relinq/wiki/Inliner) that is capable of disassembling suitable methods/delegates/lambdas and integrating them into an expression.
  * "?? -> ?:" that transforms a null coalescing operator not supported by JS into a fully equivalent conditional operator.
  * [Funcletization](http://code.google.com/p/relinq/wiki/Funcletizer) that locally pre-evaluates certain parts of an expression tree.

2. After being preprocessed (or not, if this step is omitted according to the configuration) an expression tree is recursively visited and JS code is produced according to the following table (you can make sure that the expression types list below is exhaustive by comparing it to [MSDN documentation](http://msdn.microsoft.com/en-us/library/bb361179.aspx)). Certain expressions are not supported:

### Declarations and references ###

| Constant | some JSON | Serialized according to [JsonSerialization](JsonSerialization.md) rules with expected type equal to the what the parent node expects (i.e. target/parameter type if the parent is MethodCallExpression, operand type of the parent is either UnaryExpression or BinaryExpression, or node's own Type in any other case). |
|:---------|:----------|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Constant (implementor of IBean) | `ctx.<bean name>` |  |
| Lambda | `function(<p1>, ... <pN>) { return <body>; `} |  |
| Parameter | `<name>` | Transparent identifiers introduced by let keyword of LINQ (7.15.2.7 Transparent identifiers) get more human-friendly names instead of auto-generated mess, namely: $1, $2 and so on. |

### Member access and invocations ###

| Call (instance) | `<object>.<method>(<arg1>,...<argN>)` | (If applicable) An array pseudo-parameter used for varargs invocations is flattened. <br /> Once we limit constants that are serialized into JS, certain calls (e.g. some unsupported constant expression -> some method) will be apriori unsupported if they aren't inlined during step 2. Might also cause a subsequent JS -> C# transformation failure if the call can't be unambiguously resolved by a [member lookup and type inference](http://code.google.com/p/relinq/wiki/JSToCSharp) algorithms under conditions of lacking type hints. |
|:----------------|:--------------------------------------|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Call (indexer) | `<object>[<arg1>,...<argN>]` | Either user-defined indexer (method with the name `get_<name>`, where `<name>` is the value of `System.Reflection.DefaultMemberAttribute` of the _declaring_ class) or array hardcoded indexer (`<array type>::Get`) fit this criteria. |
| Call (`Get` method of an array) | `<object>[<arg1>,...<argN>]` | `Get` method is a fancy thingie: it is declared in all array types (with number of parameters that depends on array dimensions), but it doesn't belong to the `Array` type, and thus it can't be called from the code (of course, it can be called via reflection). In fact, it's a strongly-typed alternative to the multitude of `Array.GetValue` methods. C# compiler uses it to express array indexer semantics for rectangular multidimensional arrays. |
| Call (extension) | `<arg1>.<method>(<arg2>,...<argN>)` | Static methods that are marked with `ExtensionAttribute` are treated as instance methods of the first parameter. Also see above about general Call restrictions. |
| Call (LINQ SQO) | `<arg1>.<method>(<arg2>,...<argN>)` | Same as for extension methods. All SQO except Cast<>, OfType<>, Empty<> and certain Range<> calls are supported. |
| Call (static) | Not supported | Support for static invocations requires mechanisms that are otherwise unnecessary: for type importing and for explicit generic arguments specification since constructor call generic parameters can't be inferred according to C# spec. |
| Invoke | `<target>(<arg1>,...<argN>)` |  |
| MemberAccess (instance) | `<expression>.<member>` | Might cause a subsequent JS -> C# transformation failure if the member can't be unambiguously resolved by [member lookup algorithm](http://code.google.com/p/relinq/wiki/JSToCSharp). |
| MemberAccess (static) | Not supported | Support for static invocations requires mechanisms that are otherwise unnecessary: for type importing and for explicit generic arguments specification since constructor call generic parameters can't be inferred according to C# spec. |
| New (anonymous) | `{<n1> : <expr1>, ... <nN> : <exprN>`} |  |
| New, MemberInit, NewArrayInit, NewArrayBounds, ListInit, ElementInit | Not supported | Support for explicit constructor invocations requires mechanisms that are otherwise unnecessary: for type importing and for explicit generic arguments specification since constructor call generic parameters can't be inferred according to C# spec. <br /> NewArrayInit is only supported when used to express varargs invocation style. |

### Typesystem-unrelated operators ###

| Add | `<left> + <right>` | |
|:----|:-------------------|:|
| AddChecked | Not supported | JS can't naturally express the checked/unchecked diversity. |
| And | `<left> | <right>` |  |
| AndAlso | `<left> && <right>` |  |
| ArrayIndex | `<left>[<right>]` |  |
| ArrayLength | `<operand>.Length` |  |
| Coalesce | Not supported | Null coalescing operator ?? is not supported directly. If configured, it's transformed into an equivalent conditional operator ?: during step 2. |
| Conditional | `<test> ? <ifTrue> : <ifFalse>` |  |
| Divide | `<left> / <right>` |  |
| Equal | `<left> == <right>` |  |
| ExclusiveOr | `<left> ^ <right>` | Same as for ExpressionType.Power. |
| GreaterThan | `<left> > <right>` |  |
| GreaterThanOrEqual | `<left> >= <right>` |  |
| LeftShift | `<left> << <right>` |  |
| LessThan | `<left> < <right>` |  |
| LessThanOrEqual | `<left> <= <right>` |  |
| Modulo | `<left> % <right>` |  |
| Multiply | `<left> * <right>` |  |
| MultiplyChecked | Not supported | JS can't naturally express the checked/unchecked diversity. |
| Negate | `-<operand>` | Represents both `-` (`op_UnaryMinus`) and `~` (`op_OnesComplement`). |
| NegateChecked | Not supported | JS can't naturally express the checked/unchecked diversity. |
| Not | `!<operand>` |  |
| NotEqual | `<left> != <right>` |  |
| Or | `<left> | <right>` |  |
| OrElse | `<left> or <right>` |  |
| Power | `<left> ^ <right>` | Same as for ExpressionType.ExclusiveOr. |
| Quote | `<operand>` | Produces no JS code and just proceeds with the quoted operand. |
| RightShift | `<left> >> <right>` |  |
| Subtract | `<left> - <right>` |  |
| SubtractChecked | Not supported | Not supported since JS can't naturally express the checked/unchecked diversity. |
| UnaryPlus | `+<operand>` | For constant operands operator stripped by the compiler during [compile-time constant evaluation](http://en.csharp-online.net/ECMA-334:_14.16_Constant_expressions). |

Notes:
  * Yielded JS code gets parenthesized according to the following rules:
    * Constant, Lambda, Parameter, Quote and supported Converts are never parenthesized.
    * If current expression is an argument of an ArrayIndex, Call or Invoke, or a body of Lambda or Quote it never gets parenthesized.
    * If current expression is a primary op (MemberAccess, ArrayLength, ArrayIndex, Call or Invoke) it never gets parenthesized.
    * If current expression is an unary op (UnaryPlus, UnaryMinus, Negate, OnesComplement), it gets parenthesized only to avoid ambiguities (e.g. so that double negations do not get misinterpreted as decrement, and double pluses - as increment).
    * If current node is an operand of a Convert op, it gets parenthesized if and only if it should get parenthesized if covered by the first non-convert ancestor node.
    * Otherwise priority of current op is compared with priority of parent expression node according to the table below. If current op has prio that's less or equal than parent's priority, it gets parenthesized, otherwise - it doesn't:
> > http://relinq.googlecode.com/svn/wiki/images/allOperators.PNG

  * Both Power and ExplicitOr expression types correspond to the same `^` operator.

  * Both `-` (`op_UnaryMinus`) and `~` (`op_OnesComplement`) correspond to the same ExpressionType.Not.

### Typesystem-related operators ###
| Convert | Not supported unless one of the following is true:<li>The node represents an <a href='http://code.google.com/p/relinq/wiki/ImplicitConversions'>implicit conversion</a> that's a child of an argument of a New/Call/ArrayIndex expression. In that case the convert is ignored and the algorithm produces no JS code and just proceeds with the wrapped operand.</li><li>The node represents an <a href='http://code.google.com/p/relinq/wiki/ImplicitConversions'>implicit conversion</a> that's a child of an operand of an Unary/Binary/Ternary operator expression (in case of conditional expression, if the cast is a child of a branch, it's supported only if another branch doesn't feature a cast). In that case the convert is ignored and the algorithm produces no JS code and just proceeds with the wrapped operand.</li><li>This is an explicit cast of a result of creating a delegate via <code>Delegate.CreateDelegate(TDelegate, Object, MethodInfo)</code>. This cast represents an implicit conversion of a method group to a compatible delegate, and is rewritten as <code>&lt;object&gt;.&lt;method name&gt;</code>.</li> |
|:--------|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ConvertChecked | Not supported. |
| TypeIs | Not supported. |
| TypeAs | Not supported. |

Notes:
  * By the way, this also means that [certain SQO operators](http://code.google.com/p/relinq/wiki/UnsupportedCSharpConstructs) are not supported as well.