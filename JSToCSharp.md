Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

This document describes all steps of JS -> C# transformation: from syntactical verification of provided JS code to compilation into C# expression trees. These steps are quite algorithm-heavy, so most of those are specified in separate documents. This very document provides all necessary information about unsupported JS constructs, however this information is available in more structured way in the [UnsupportedJSConstructs](UnsupportedJSConstructs.md) document. C# -> JS -> C# roundtrip concerns are covered in the [Roundtrip](Roundtrip.md) document.

### Contract ###

**Input:** String with JS code.

**Errors:** An error occurs if the input contains [unsupported constructs](http://code.google.com/p/relinq/wiki/UnsupportedJSConstructs). Otherwise algorithm produces the output as described below.

**Output:** C# expression tree that is equivalent to the input JS code. If this is a reverse transformation in the C# -> JS -> C# the output will be identical to the source expression tree provided [certain conditions are met](http://code.google.com/p/relinq/wiki/Roundtrip).

### Design considerations ###

As Relinq was originally developed as a framework for remote LINQ invocations one of the most important design concerns was making sure that source C# expressions after being transformed to JS will be successfully transformed back to C#. Major challenge of the reverse transformation is resolving member access constructs in conditions of lacking most type hints usually available in C# (typecast operators, explicit generic parameters, explicit lambda/delegate argument types).

### The algorithm ###

1) Syntactical verification of provided JS code. A separate document describes [RelinqScriptSyntax](RelinqScriptSyntax.md) and corresponding lexer+parser that are used for the verification. RelinqScript is a sub-set of JS that doesn't contain [unsupported language constructs](http://code.google.com/p/relinq/wiki/UnsupportedJSConstructs).

This step of the algorithm [builds a RelinqScript AST](http://code.google.com/p/relinq/wiki/BuildingRelinqScriptAst) that is equivalent to the input JS code. AST is composed of the following nodes that correspond to [supported JS constructs](http://code.google.com/p/relinq/wiki/BuildingRelinqScriptAst):

| Expression type | Structural notes | Notes |
|:----------------|:-----------------|:------|
| Keyword | Leaf node | Represents a single keyword of RelinqScript: `ctx` that corresponds to the data context that hosts `IQueryable` beans. |
| Variable | Leaf node | Denotes usage of a declared variable (11.1.2 Identifier Reference). If the usage is inconsistent (say, it's referenced within the closure that doesn't define the variable) a compilation error will occur later. |
| Constant | Leaf node | Represents an in-place definition of an object that doesn't involve any constructors (correspond to [json-serialized](http://code.google.com/p/relinq/wiki/JsonSerialization) ConstantExpressions of C# expression trees). Maps on either a json-style object initializer, or an array initializer or a JS literal. |
| New | Has `1..*` children | So far is used solely for creating instances of anonymous types, so doesn't support invoking constructors of explicitly declared types. Maps on a json-style object initializer and stores a name -> expression mapping of the object's structure. |
| Lambda | Has 1 child | Corresponds to a function expression, stores all argument names and a reference to the body - its single child. |
| MemberAccess | Has 1 child | Represents a field-style member access that doesn't use bracket notation. This node can either produce an object or a method group. |
| Invoke | Has 1 + `0..*` children | Is used to express a call-style member access including early- and late-bound invocations. |
| Indexer | Has 1 + `0..*` children | Corresponds to bracket-based member access syntax constucts, namely: 11.2.1 Property Accessors, bracket productions. |
| Operator | Has `1..2` children | Represents usage of [one of the supported operators](http://code.google.com/p/relinq/wiki/UnsupportedJSConstructs) described in 11 Expressions. Needs node type separate from the Invoke because it might reference one of the predefined operators that do not have implementing methods and some of those can't be expressed by method semantics (e.g. logical conditional operators). |
| Conditional | Has 3 children | The only one supported language construct that offers functionality from the statements world.  |

2) The next step binds RelinqScript AST nodes to corresponding .NET metadata (fields, methods, types) by depth-first recursive algorithm in the way described in the table below. It uses both .NET types and compiler-specific [AuxiliaryTypes](AuxiliaryTypes.md), namely: `null`, `Lambda`, `UnknownStringizedObject`, `UnknownPropertyBag`, `UnknownList` and `MethodGroup`.

| Node type | Notes |
|:----------|:------|
| Keyword | RelinqScript has a single keyword: `ctx` that represents data context that hosts `IQueryable` beans. Currently it's not yet decided how in particular Relinq will look up the context, but it's pretty much clear that no type inference is necessary here since the type of the context will be known apriori. |
| Variable | Variables are bound to declaring lambdas, so they inherit type information from them, or will crash the type inference engine is they are used outside of the context (i.e. undeclared). |
| Constant | In most cases type of the literal can be unambiguously inferred from the corresponding JS string according to the rules specified in the [TypeInferenceConstants](TypeInferenceConstants.md) document. Otherwise it'll receive the type of `UnknownConstant` (or more precisely: `UnknownEntity` or `UnknownCollection`) that can be inferred only if this constant is an argument of a suitable method call or an operand of a suitable operator invocation. |
| New | New nodes are bound to constructors of Relinq analogues of anonymous types - dynamically generated functionally equivalent classes. |
| Lambda | Lambdas are processed as if they are declared with implicitly typed arguments. That's they can be inferred only if the lambda is an argument of a suitable method call. |
| MemberAccess | Represents either plain old property paths or referencing a method group. Both of these cases are resolved by [LookupMemberAccess](LookupMemberAccess.md) algorithm so we have little to no work here. If the acess references a method group, its type can only be inferred if it's an argument or target of a suitable method call. |
| Invoke | Invoke nodes represent either compile-time or late-bound invocations. In the first case Invoke nodes are associated with corresponding MethodInfos via [TypeInferenceMethods](TypeInferenceMethods.md) algorithm that performs overload resolution (if necessary) and possibly method generic parameters type inference for the method alternatives list produced by a child node. In contrast to early-bound calls, late-bound invocations don't need resolution algorithms since they already have a signature. Successful processing of an Invoke node might also infer types for certain Constant, MemberAccess and Lambda nodes. |
| Indexer | Gets the same treatment as the Invoke node, though alternatives list is populated by a different algorithm: [LookupIndexers](LookupIndexers.md). |
| Operator | Gets the same treatment as the Invoke node, though alternatives list is populated by a different algorithm: [LookupOperators](LookupOperators.md). |
| Conditional | Processing of the Conditional node doesn't really involve any type inference. Everything it needs is making sure that IfTrue and IfFalse clause types do not contradict each other. |

3) After all nodes of RelinqScript AST are unambiguously bound to certain .NET metadata, it's a fairly simple task to produce C# expression tree equivalent to the input JS code. Details of creating such a tree are described in the [BuildingCSharpAst](BuildingCSharpAst.md) document.