Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a string that represents a JS expression equivalent to a given C# expression tree provided certain conditions are met.

"Certain conditions" are defined using the rule as follows. **C# expression tree can be losslessly transformed into a JS expression if it doesn't contain unsupported constructs**. The document describes such constructs: it describes general rules and and also mentions important particular cases that are unsupported. If you wonder why certain stuff doesn't work, check the [DesignMotivations](DesignMotivations.md) document for more information.

To check out details the transformation algorithm and how every supported C# construct is transformed into a JS expression consult the [CSharpToJS](CSharpToJS.md) document. It's important to mention that certain C# expression trees might be successfully transformed into JS expressions, but subsequent reverse transformation will fail or will produce non-identical expression tree. To get more info on these point check the "Unsupported constructs that cause compilation errors" section of [UnsupportedJSConstructs](UnsupportedJSConstructs.md) document and the [Roundtrip](Roundtrip.md) document.

### Unsupported constructs ###
  * Syntactic features of C# 3.0 that can't be defined in an expression (e.g. `ref` parameters, assignment operators, lambda with statement bodies and so on) along with method invocations that contain `out` parameters.

  * Explicitly specified type information (most of the time, all necessary type information can be successfully inferred from the context). This includes:
    * `Is` operator.
    * Explicitly specified typecasts/`as` operator (tho implicit typecasts required for method invocations, and for promoting numeric objects are supported).
    * Explicit type specification for lambda expression parameters.
    * Explicit type arguments provided for generic method calls.
    * SQO operators Cast<>, OfType<>, Empty<> and certain invocations of Repeat<>.
    * Explicit constructor invocations except for anonymous objects.
    * Static fields and methods.

  * Non-bean constant expressions that can't be correctly [serialized to JSON](http://code.google.com/p/relinq/wiki/JsonSerialization). Notes:
    * If configured so C# -> JS transformer might try to [funcletize unsupported constants](http://code.google.com/p/relinq/wiki/Funcletizer) and, hopefully, transform source expression tree into its supported equivalent.
    * If serialization is successful, this doesn't mean that deserialization will be successful as well. Reverse transformation will most likely lack complete type info about the object, and if it will be unable to infer missing information from the context, this will result in a compilation error.

  * Checked operators (all numeric operators are assumed to be unchecked since JS can't naturally express the checked/unchecked diversity).

  * Quoted non-lambda expressions. According to the spec (4.6 Expression tree types), only lambda expressions can be auto-converted to expressions, i.e. only they can be quoted for being passed to a certain method in verbatim format.

### Unsupported or partially supported expression types ###

(using `System.Linq.Expressions` namespace):

http://relinq.googlecode.com/svn/wiki/images/allExpressions.PNG

  * Constant nodes are supported only for `IBean`s and JSON-serializable objects depending on the context (as described above). If configured so C# -> JS transformer might try to [funcletize unsupported constants](http://code.google.com/p/relinq/wiki/Funcletizer) and, hopefully, transform source expression tree into its supported equivalent.

  * Call nodes are supported only if they reference either instance or extension methods (static methods are unsupported).
  * MemberAccess nodes are supported only if they reference instance members including method groups (static fields are unsupported).
  * Invoke nodes are supported, but often an expression that contains invoke won't be supported by C# -> JS transformation due to unrelated reasons, e.g. because of delegate being invoked coming from a closure (but the closure itself is a Constant node that is invalid for transformation).

  * New nodes are supported only for creating anonymous types.
  * ListInit, ElementInit nodes are unsupported.
  * MemberInit, MemberAssignment, MemberBinding nodes are unsupported.
  * NewArrayBounds is unsupported.
  * NewArrayInit is only supported when used to express varargs invocation style.
  * Quote nodes are supported only when the operand is a lambda.

  * AddChecked, MultiplyChecked, SubtractChecked, ConvertChecked nodes are unsupported.
  * TypeIs, TypeAs nodes are unsupported.
  * Convert nodes are only supported when used to implicitly coerce objects to match used method/operator/conditional signature.
  * Coalesce node is not supported directly. Instead it's transformed into an equivalent conditional expression by one of C# -> JS stage preprocessors.
  * Negate node can potentially bring ambiguity since it corresponds to two different unary operators (see above).
  * Power and ExclusiveOr nodes can potentially bring ambiguity since they correspond to the same operator: `^`.

### Unsupported or partially supported C# operators ###

(using operators table from [C# 2.0 spec](http://en.csharp-online.net/ECMA-334:_14.2.1_Operator_precedence_and_associativity)):

http://relinq.googlecode.com/svn/wiki/images/allOperators.PNG

  * Postfix operators: -- and ++ are unsupported since they aren't allowed in expression trees.
  * `new` operator is partially supported (only for anonymous types construction).
  * `typeof` operator is unsupported.
  * `checked`/`unchecked` operators are unsupported.
  * Prefix operators: `--` (`op_Decrement`) and `++` (`op_Increment`) are unsupported since they aren't allowed in expression trees.
  * Simultaneously defined `-` (`op_UnaryMinus`) and `~` (`op_OnesComplement`) will lead to ambiguous overload resolution, since they both correspond to ExpressionType.Negate.
  * Typecast operator is unsupported.
  * `^` cannot be unambiguously resolved because it corresponds to both Power and ExclusiveOr nodes.
  * `is`/`as` operators are unsupported.
  * Null coalescing operator `??` is not supported directly. Instead it's transformed into an equivalent conditional operator `?:`.
  * All assignment operators are unsupported since they aren't allowed in expression trees

### Unsupported or partially supported SQO ###
(using SQO classification by [Hooked on LINQ](http://www.hookedonlinq.com/LinqToObjects5MinuteOVerview.ashx)):

http://relinq.googlecode.com/svn/wiki/images/allSqo.PNG

  * SQO from Conversion group: Cast<> and OfType<> are not supported.
  * SQO from Generation group: Empty<> is not supported, Repeat<> is only supported if method generic type parameter can be inferred from the argument, Range is not supported