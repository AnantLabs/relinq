Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site).

As Relinq was originally developed as a framework for remote LINQ invocations one of the most important design concerns was making sure that source C# expressions after being transformed to JS and subsequently back to C# will be exactly the same as the original ones (will contain exactly the same nodes ordered in the same structure). However there can be three major difficulties with reaching this goal: transformation errors, introduced mess and irreversible AST transformations that occur during the roundtrip.

### Transformation errors ###

Transformation errors occur if either source C# expression tree or produced JS expression contain unsupported constructs. These errors can be caused by a number of reasons mentioned in [UnsupportedCSharpConstructs](UnsupportedCSharpConstructs.md) and [UnsupportedJSConstructs](UnsupportedJSConstructs.md).

Relinq guarantees that produced JS code will be syntactically and semantically compatible with C#. However reverse JS -> C# transformation might fail if [type inference](http://code.google.com/p/relinq/wiki/JSToCSharp) engine is unable to unambiguously resolve generic types / method overloads due to JS code lacking type hints. The latter reason might even cause more significant consequences: the reverse transformation might yield incorrect AST (infer wrong implicit casts, or choose wrong overload).

### Equivalent but non identical AST transformations ###

Sometimes the roundtrip produces an equivalent but not the same expression tree. This can happen either because of unmitigable reasons (e.g. presence of anonymous types in an expression) or as a result of (optional, configurable) preprocessing on C# -> JS stage performed by Relinq.

  * Conversion of IL instructions into an expression tree (that's courtesy of C# compiler) features the following peculiarities:
    * Both Power and ExplicitOr expression types correspond to the same `^` operator.
    * Both `-` (`op_UnaryMinus`) and `~` (`op_OnesComplement`) correspond to the same ExpressionType.Not.
    * Certain constructs e.g. operator `+` (`op_UnaryPlus`) for constant operands get stripped by the compiler during [compile-time constant evaluation](http://en.csharp-online.net/ECMA-334:_14.16_Constant_expressions).

  * Null coalescing operator ?? is not supported directly since it can't be expressed in JS. If configured, it is transformed into an equivalent conditional operator ?:, and this transformation is obviously irreversible.

  * To widen the class of C# expressions that can be successfully transformed into JS, Relinq features (optional, configurable) preprocessors during C# -> JS transformation: [funcletizer](http://code.google.com/p/relinq/wiki/Funcletizer) and [inliner](http://code.google.com/p/relinq/wiki/Inliner). Both preprocessings are irreversible so one needs to be sure that no semantics is lost during the transformation.

  * Anonymous types are specific for a compilation unit, so they get changed to dynamically built Relinq classes that are functionally equivalent.

  * Transparent identifiers introduced by `let` keyword of LINQ (7.15.2.7 Transparent identifiers) get more human-friendly names instead of auto-generated mess, namely: `$1`, `$2` and so on.

### Mess introduced when serializing ConstantExpressions ###

According to [json serialization rules](http://code.google.com/p/relinq/wiki/JsonSerialization), most of constant expressions are serialized without information that's necessary for 100% precise inference of their type during reverse transformation, and this might cause fatal ambiguity or incorrect result of method resolution.

If constant expression is used via an interface, but not it's actual type (e.g. a custom collection is passed into a LINQ SQO) then either of the following will occur  (read more info in [JsonSerialization](JsonSerialization.md) document):
  * C# -> JS transformation will fail because interface type is unsupported,
  * Reverse JS -> C# transformation will deserialize the constant as an object of a predefined type regardless of what type it had before serialization.