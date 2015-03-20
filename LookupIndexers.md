Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Indexer node of [RelinqScript AST](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax) that references the indexing applied to the target with the type `<type>`.

**Errors:** Error occurs if there doesn't exist any .NET methods declared by the target that suit the argument and indexing semantics. Otherwise algorithm produces the output as described below.

**Output:** A lookup list that contains non-zero number of method alternatives (possibly open generic and, thus, susceptible to [GenericArgsInference](GenericArgsInference.md) algorithm).

### Design considerations ###

Algorithm described below closely conforms to C# 3.0 spec: [17.8 Indexers](http://en.csharp-online.net/ECMA-334:_17.8_Indexers), [Indexer Declaration rules](http://msdn.microsoft.com/en-us/library/2549tw02(VS.71).aspx) coupled with [14.4.2.1 Applicable function member](http://en.csharp-online.net/ECMA-334:_14.4.2.1_Applicable_function_member).

### The algorithm ###

1) This algorithm fills the lookup list with suitable methods from the following sources:
  * Public instance methods that are defined in the `T` type including its base type and are of name `get_<name>`, where `<name>` is the value of `System.Reflection.DefaultMemberAttribute` that annotates the _declaring_ class.
  * (Only if the `T` type is derived from `System.Array`) Public instance `Get` method declared by the type itself.

2) After being initially filled the algorithm filters the lookup list according to the following rules:
  * Methods that are hidden by other methods are removed from the list (this process is governed by the [HidingMethods](HidingMethods.md) algorithm).
  * Methods that have either `ref` or `out` parameters are removed from the list.