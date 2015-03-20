Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Argument list `A` with a sequence of argument types `A1` ... `AN` and two method alternatives `M1` and `M2` after expansion and generic parameter substitution with parameter types `P` = `P1` ... `PN` and `Q` = `Q1` ... `QN`.

**Errors:** Algorithm never fails.

**Output:** A tri-state flag that indicates that `M1` is better, or `M2` is better, or neither alternative is better.

### The algorithm ###

Relation between method candidates `M1` and `M2` is found out as follows:
  * If for every `i = 1..N` conversion `Ai -> Pi` [is not worse](http://code.google.com/p/relinq/wiki/BetterImplicitConversion) than conversion `Ai -> Qi`, and there exists such `j` that `Aj -> Pj` [is better than](http://code.google.com/p/relinq/wiki/BetterImplicitConversion) conversion `Aj -> Qj`, then M1 is better than M2.
  * If the reverse is true, then M2 is better than M1.
  * If signatures of M1 and M2 are identical, the tie-breaking rules described below are applied.
  * In any other case neither M1 nor M2 is better.

Tie-breaking algorithm calculates the **rank** of both method candidates. Rank is a `[Flags]` enum with the following bit values starting from the most significant:
  * User-defined/predefined. yes/no? (1 bit).
  * Instance/extension. yes/no? (1 bit).
  * Generic/non-generic. yes/no? (1 bit).
  * Number of parameters in non-expanded form (4 bit).

If method ranks are not equal, the one with bigger rank is better. Else, the next step of tie-breaking algorithm is performed.

The algorithm calculates **generic arguments mapping** for both method candidates. For every parameter of method's signature the mapping contains [a structural tree](http://code.google.com/p/relinq/wiki/TypeStructuralTree) of corresponding type. **Ranks** are assigned to all nodes of the trees in the mapping. Node rank is a `Flags` enum with the following bit values starting from the most significant:
  * Type is a substituted generic parameter. no/yes? (1 bit).
  * Type is a substituted type generic parameter. yes/no? (1 bit).
  * Type is a substituted method generic parameter. yes/no? (1 bit).

After the mapping is built the algorithm traverses generic parameters mapping and compares both methods' trees for every parameter:
  * Certain tree (and its corresponding parameter) is considered more specific than another one if all its nodes' ranks are not less than corresponding nodes' ranks of another tree, and there exists at least one node with the rank bigger than the corresponding node of another tree has.
  * A method is considered to be more specific than another one if all its parameters are not less specific then corresponding parameters of another method, and there exists at least one parameter that is more specific than the corresponding parameter of another method.

If a method is more specific than another method, then it is better. If neither of candidates is more specific, neither of them is better.