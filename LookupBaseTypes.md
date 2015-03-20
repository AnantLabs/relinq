Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp)starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Closed generic or non-generic .NET type `T`.

**Errors:** Algorithm never fails.

**Output:** A set of types `Ti` each of those being a base type of the input type `T`.

### The algorithm ###

For purposes of member lookup, a type `T` is considered to have the following base types:
  * If `T` is `object`, then `T` has no base type.
  * If `T` is an enum-type, the base types of `T` are the class types `System.Enum`, `System.ValueType`, and `object`.
  * If `T` is a struct-type, the base types of `T` are the class types `System.ValueType` and `object`.
  * If `T` is a class-type, the base types of `T` are the base classes of `T`, including the class type `object`.
  * If `T` is an interface-type, the base types of `T` are the base interfaces of `T` and the class type `object`.
  * If `T` is an array-type, the base types of `T` are the class types `System.Array` and `object`.
  * If `T` is a delegate-type, the base types of `T` are the class types `System.Delegate` and `object`.
  * If `T` is a nullable-type, the base types of `T` are the class types `System.ValueType` and `object`.