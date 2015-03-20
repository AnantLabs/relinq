Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Implicit conversions `C1` and `C2` that convert objects from type `S` to type `T1`, and converts from type `S` to type `T2` respectively.

**Errors:** Algorithm never fails.

**Output:** A tri-state flag that indicates that `C1` is better, or `C2` is better, or neither conversion is better.

### The algorithm ###

The better conversion of the two conversions is determined [as follows](http://en.csharp-online.net/ECMA-334:_14.4.2.3_Better_conversion):
  * If `T1` and `T2` are the same type, neither conversion is better.
  * If `S` is `T1`, `C1` is the better conversion.
  * If `S` is `T2`, `C2` is the better conversion.
  * If an implicit conversion from `T1` to `T2` exists, and no implicit conversion from `T2` to `T1` exists, `C1` is the better conversion.
  * If an implicit conversion from `T2` to `T1` exists, and no implicit conversion from `T1` to `T2` exists, `C2` is the better conversion.
  * Given `S` is a lambda and types `T1` and `T2` are delegates. If `T1` and `T2` have identical parameter lists and the implicit conversion from `S`'s inferred return type to `T1`'s return type is a better conversion than the implicit conversion from `S`'s inferred return type to `T2`'s return type, then `C1` is better conversion.
  * If the reverse is true, then `C2` is better conversion.
  * Otherwise, neither conversion is better.