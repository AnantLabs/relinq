Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:**
  * Invoke/Operator node of [RelinqScript AST](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax) that has `N` arguments/operands `<arg1>` ... `<argN>` of types `T1` ... `TN`.
  * A list of method alternatives from either [LookupMemberAccess](LookupMemberAccess.md) or [LookupIndexers](LookupIndexers.md) or [LookupOperators](LookupOperators.md) algorithms, referenced as "the list" below.

**Errors:** Error occurs if there doesn't exist a .NET method that suits specified name/operator and arguments/operands, or the lookup is ambiguous. Otherwise algorithm produces the output as described below.

**Output:** Unambiguously resolved pre-defined or user-defined .NET method.

### Design considerations ###

The algorithm is based on [C# 2.0: 14.4.2 Overload resolution](http://en.csharp-online.net/ECMA-334:_14.4.2_Overload_resolution) augmented by [C# 3.0: Lambda Expressions: Lambda Expression Conversions, Overload Resolution](http://msdn.microsoft.com/en-us/library/ms364047(VS.80).aspx#cs3spec_topic4). In short, Compiler1 uses the same overload/conversion comparison and tie-breaking rules as described in C# spec.

### The algorithm ###

1) After initialization the algorithm filters the lookup list according to the following rules:
  * Instance non-varargs methods that have number of parameters different from `N` are removed from the list.
  * Instance varargs methods that do not have normal or expanded form with `N` parameters are removed from the list.
  * Extension non-varargs methods that have number of parameters different from `N+1` are removed from the list.
  * Extension varargs methods that do not have normal or expanded form with `N+1` parameters are removed from the list.

2) After being filtered, certain elements of the lookup list are then transformed according to the following rules:
  * Non-varargs methods are not changed.
  * Instance varargs methods that have normal form with `N` parameters are not changed.
  * Instance varargs methods that do not have normal form with `N` parameters are replaced with their expanded form with `N` parameters.
  * Extension varargs methods that have normal form with `N+1` parameters are not changed.
  * Extension varargs methods that do not have normal form with `N+1` parameters are replaced with their expanded form with `N+1` parameters.

3) After polishing the lookup list algorithm then uses [generic arguments inference algorithm](http://code.google.com/p/relinq/wiki/GenericArgsInference) for all generic methods in the list. Those methods that fail the inference algorithm are removed from the list.

4) Algorithm enforces the following imperative for every method alternative M in the list. Methods that don't fit it are removed from the list:
  * For every argument `altArgK` of type `altTypeK` there exists an [implicit conversion](http://code.google.com/p/relinq/wiki/ImplicitConversions) from `altTypeK` to `TK`.

5) Algorithm enforces the following imperative for every method alternative M in the list. Methods that don't fit it are removed from the list:
  * There doesn't exist method M0 that [is better than](http://code.google.com/p/relinq/wiki/BetterMethodAlternative) M.

6) The algorithm finishes its work with the result determined as follows:
  * If the list contains zero alternatives, the method/operator cannot be resolved and compilation error occurs.
  * If the list contains a single alternative, the method/operator invocation is successfully resolved. If the input node is a conditional logical Operator, then the resolved alternative is validated according to the rules below.
  * If the list contains multiple alternatives, the method/operator resolution is ambiguous and compilation error occurs.

### Conditional logical operators validation algorithm ###

1) If overload resolution selects one of the [predefined integer logical operators](http://code.google.com/p/relinq/wiki/LookupOperators), compilation error occurs.

2) Otherwise, if the selected operator is one of the [predefined boolean logical operators](http://code.google.com/p/relinq/wiki/LookupOperators), the resolution is considered to be valid.

3) Otherwise, the selected operator is a user-defined operator, and the resolution is valid if both of the following is true. Compilation error occurs if either of these requirements is not satisfied (given `T` is the name of the type that declares the operator):
  * The return type and the type of each parameter of the selected operator shall be `T`. In other words, the operator shall compute the logical AND or the logical OR of two operands of type `T`, and shall return a result of type `T`.
  * `T` shall contain declarations of operator `true` and operator `false`.