Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Argument list `A` with a sequence of argument types `A1` ... `AN` and an open generic method alternative `M` after expansion that has parameter types `P` = `P1` ... `PN`.

**Errors:** Error occurs due to one of the following reasons:
  * Certain pair of types `Ai` and `Pi` is [structurally incompatible](http://code.google.com/p/relinq/wiki/TypeStructuralTree) and this prevents further type inference.
  * Algorithm produces either incomplete or inconsistent set of inferences.

**Output:** A closed generic method that constructed from the input open generic method according to a complete (each type parameter of the method had a type argument inferred for it), consistent (for each type parameter, all of the inferences for that type parameter infer the same type argument) and compliant (satisfies all the constraints) set of inferences acquired during the execution.

### Design considerations ###

The algorithm is based on [C# 2.0: 25.6.4 Inference of type arguments](http://en.csharp-online.net/ECMA-334:_25.6.4_Inference_of_type_arguments) augmented by [C# 3.0: Lambda Expressions, Type Inference](http://msdn.microsoft.com/en-us/library/ms364047(VS.80).aspx#cs3spec_topic4). In short, to infer missing generic arguments compiler matches argument types with method signature. Inferrer is also able to analyze and discover return type of declared lambdas.

### The algorithm ###

**Is planned to be synced with [the latest version of C# 3.0 spec](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/CSharp%20Language%20Specification.doc). However the algorithm described below even in its current form is quite an adequate analogue.**

Type arguments inference algorithm is an iterative process that works as follows:
  * At the start the algorithm marks all argument types `Ai` (but not return value!) with the "Not processed" tag.
  * Given the set `R` of `Ai` that are either marked with as "Not processed", or marked as "Deferred" but fit the readiness criteria. If `R` is not empty, then the algorithm executes its iteration for an argument randomly picked from the set, else algorithm finishes its work.

An iteration of the algorithm processes the corresponding types `Ai` and  `Pi`, and as a result updates set of inferences and/or `Ai`'s tag. It works as follows:
  * If `Pi` does not involve any method type parameters, set of inferences is not updated and `Ai` is marked with "Processed" tag.
  * If `Ai` is a `null` type or an `UnknownConstant` type, set of inferences is not updated and `Ai` is marked with "Processed" tag.
  * If `Ai` is not a lambda. If there exists such `Aij` from the inheritance chain of `Ai` (self, all implemented interfaces and all base types) that compare [type structural trees](http://code.google.com/p/relinq/wiki/TypeStructuralTree) of types `Aij` and `Pi` have the same structure and corresponding nodes of `Pi` feature either the very same types as `Aij` or types inferrable from `Aij`, set of inferences is updated accordingly and `Ai` gets marked with "Processed" tag, else type inference fails.
  * If `Ai` is a lambda and `Pi` is a delegate type or an `Expression<D>` type, where `D` is a delegate type. If the readiness criteria defined below is satisfied, the algorithm marks `Ai` with "Processed" tag and recursively matches inferred return type of the `Ai` lambda and return type of the `Pi` delegate according to current rules for possible type inference. Else set of inferences is not updated and `Ai` is marked with "Deferred" tag. The readiness criteria:
    * `Pi` and `Ai` have the same number of parameters.
    * `Pi`'s signature types involve no method type parameters or involve only method type parameters for which a consistent set of inferences have already been made.
    * When inferred types are substituted for method type parameters in `Pi` and the resulting parameter types are given to the parameters of `Ai`, the body of `Ai` is a valid expression.

After the algorithm iterations are finished, its result is determined as follows:
  * If there exists an `Ai` that has the tag different from `Processed`, type inference fails.
  * If one of the following is true about set of inferences, type inference fails:
    * Incompleteness (there exists a type parameter of the method that doesn't have a type argument inferred for it)
    * Inconsistency (there exists a type parameter with different type arguments inferred for it).
    * Non-compliance (there exists a type parameter that doesn't match declared constraints).
  * Else type inference algorithm substitutes all method generic parameters with inferred types and succeeds.