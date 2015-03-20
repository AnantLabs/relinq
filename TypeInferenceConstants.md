Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Constant node of [RelinqScript AST](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax).

**Errors:** Errors might occur in one of the cases mentioned below. Otherwise algorithm produces the output as described.
  * If Constant is classified as a number but doesn't fit into supported ranges for .NET number types,
  * If the corresponding JS grammar token is not supported by the compiler (e.g. regular expression literal).

**Output:** .NET type of the constant, or one of surrogate types (Null or UnknownConstant, i.e. one of its descendants: UnknownStringizedObject, UnknownPropertyBag, UnknownList) that will possibly be later inferred from the context.

### The algorithm ###

| Constant | Inferred type |
|:---------|:--------------|
| `null` | `Null` (auxiliary type defined by the Compiler and used internally, will get replaced by the type inferred from the context). |
| `false` | `bool` |
| `true` | `bool` |
| Integer number | One of .NET integer types according to [9.4.4.2 Integer literals](http://en.csharp-online.net/ECMA-334:_9.4.4.2_Integer_literals) and [11.1.5 Integral types](http://en.csharp-online.net/ECMA-334:_11.1.5_Integral_types) as defined below (ranges are all inclusive). A compilation error occurs if the number can't be represented in 64 bits. |
| -2<sup>63</sup> to -2<sup>31</sup>-1 | `long` |
| –2<sup>31</sup> to 2<sup>31</sup>-1 | `int` |
| 2<sup>31</sup> | `int` if its parent node is Operator with OperatorType.UnaryMinus, `long` otherwise. |
| 2<sup>31</sup> + 1 to 2<sup>32</sup> - 1 | `uint` |
| 2<sup>32</sup> to 2<sup>63</sup>-1 | `long` |
| 2<sup>63</sup> | `long` if its parent node is Operator with OperatorType.UnaryMinus, `ulong` otherwise. |
| 2<sup>63</sup> + 1 to 2<sup>64</sup> - 1 | `ulong` |
| Floating-point number | `double` according to [9.4.4.3 Real literals](http://en.csharp-online.net/ECMA-334:_9.4.4.3_Real_literals). A compilation error occurs if the number can't be represented by [64-bit double-precision IEC 60559 format](http://en.wikipedia.org/wiki/IEEE_754-1985#Double-precision_64_bit). |
| String | `UnknownStringizedObject` |
| Object initializer | `UnknownPropertyBag` |
| Array initializer | `UnknownList` |