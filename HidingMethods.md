Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Two .NET methods: `M1` and `M2` that have type parameters `nt1` and `nt2` (respectively), and formal parameters `P11` ... `P1K` and `P21` .. `P2L` (respectively).

**Errors:** Algorithm never fails

**Output:** A flag that indicates whether method `M2` hides method `M1`.

### Design considerations ###

Algorithm described below closely conforms to C# 3.0 spec: [10.7.1.2 Hiding through inheritance](http://en.csharp-online.net/ECMA-334:_10.7.1.2_Hiding_through_inheritance) with "signature" defined as in [10.6 Signatures and overloading](http://en.csharp-online.net/ECMA-334:_10.6_Signatures_and_overloading).

Unlike C# spec that doesn't consider `params` modifier to be a part of signature, this algorithm treats it as a native part of the sig.

Hiding for extension methods is non-existent since 1) they can only be declared in static classes, and the latter cannot be inherited, 2) extension methods with `this` parameters having inheritance relations will be resolved during [TypeInferenceMethods](TypeInferenceMethods.md).

### The algorithm (regular methods) ###

The signature of a method consists of the name of the method, the number of type parameters, and the type and kind (value, reference, or output) of each of its formal parameters, considered in the order left to right. The signature of a method specifically does not include the return type, parameter names, or type parameter names, however it DOES include the params modifier that can be specified for the right-most parameter. When a parameter type includes a type parameter of the method, the ordinal position of the type parameter is used for type equivalence, not the name of the type parameter.

A method introduced in a class or struct hides all non-method base class members with the same name and either, the same number of type parameters or no type parameters, and all base class methods with the same signature.

### The algorithm (indexers) ###

The signature of an indexer consists of the type of each of its formal parameters, considered in the order left to right. The signature of an indexer specifically does not include the element type or parameter names, however it DOES include the params modifier that can be specified for the right-most parameter.

An indexer introduced in a class or struct hides all base class indexers with the same signature (parameter count and types).

### The algorithm (operators) ###

The signature of an operator consists of the name of the operator and the type of each of its formal parameters, considered in the order left to right. The signature of an operator specifically does not include the result type or parameter names.

Operators cannot be hidden (this means that in case when both base and derived classes declare an operator with the same signature, both of those will remain in the list).