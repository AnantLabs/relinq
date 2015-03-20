Not yet implemented (important):
  * Analyze failboats of used grammar (only `1.ToString()` so far).
  * Revive lifting of predefined operators, implement its correct compilation.
  * Compilation of user-defined casts.
  * Implement lifting of user-defined operators and casts and unite all lifting routines.

Refactorings (important):
  * Somehow resolve the case of no alternatives because neither match the signature because inner lambda is incorrect.
  * Fix probing for detecting deserialization possibility. Use info about subclasses of UnknownConstant, analyze some stuff manually. Whatever, but not this awful hack.
  * Decouple AST/TIC validation from TIE and CSETB: implement structure checker for type inference engine and c# expression tree builder (RelinqScriptAstChecker and InferredRelinqScriptAstChecker, where the latter is inherited from the prior). When implementing this stuff, think about additional checks.
  * Make everything internal and expose minimally necessary stuff via wrappers (or a single `Relinq.Api` wrapper).
  * Consider stripping off the JS grammar to a supported subset, also make function definition syntax more compact.

Tests (important):
  * Exhaustive tests for all compiler sub-algorithms (Lookup, TypeInference, OverloadResolution and PredefinedOperators).
  * Exhaustive tests for all supported operators/node types and their priorities (including user-defined conditional operators).
  * Exhaustive tests for all existing LINQ keywords (e.g. let, group and likes) and SQO.

Spec polishing (important):
  * Specify all possible exceptions during both transformations and how they will be raised and sync this with the code. I've started this work in [ExceptionSpecDraft](ExceptionSpecDraft.md).
  * Insert examples into spec where appropriate.
  * Populate [Infrastructure](Infrastructure.md) and [HowToUse](HowToUse.md) wiki pages.
  * Introduce the recent changes tracking wiki (featured).
  * Compose a mind map of the documentation: a single medium-sized picture that convers the entire scope of the spec and also provides links between topics.

Not yet implemented (less important):
  * Implement type inference and compilation for passing method groups as arguments to invocations that expect delegates.
  * Implement necessary resolutions in TypeInferenceMethods + move sourceinvocation resolution from TypeInferenceEngine to there.
  * What the hack with Lambda -> FunctionType and MethodGroup -> FunctionType conversions.
  * Implement lambdas in one of branches of a conditional operator.

Refactorings (less important):
  * Check conformance of types during JSON serialization/deserialization.
  * Check correctness of compiling lifted versions of operators: check both regular and relational ops.
  * Rewrite codegen for anonymous types.
  * Thread safety for TypeInferenceEngineCache.
  * Implement cache using weak references (prevents memory leaks, provides a way to trade-off memory for performance).
  * Link RelinqScriptTypes to source nodes, so that 1) the model looks awesome, 2) we can be more restrictive on int -> enum casts.
  * Dynamic switching of Regular/Addonly/Changeonly/Readonly/ strategies for dictionary, and upgrading implementation of existing solutions like e.g. TSM/MSM (I mean: more precisely define their behavior using new capabilities).
  * Rewrite handcrafted JS literals parsing using ANTLR generater lexer/parser suite.
  * Investigate the situation when return type of the lambda doesn't exactly match the expected return type of the delegate.
  * Calculate casts for redirection root methods during TypeInferenceMethods. Somehow resolve the situation with concat (i.e. UnknownConstant -> Object stuff).

Future upgrades (less important):
  * Preevaluation of JS code directly by JS environment just before sending the query to the server.
  * Coffee algorithm for semi-automatic deserialization of complex objects.
  * Extending the notion of context and its beans, so that they can easily expose completely alien objects under the hood of our native stuff.
  * Implementing [Inliner](Inliner.md) for extra convenience and leet flavor.
  * [Compiler2](Compiler2.md)-related researches (the concept is too cool to be abandoned).
  * Implementing stuff abandoned due to [DesignMotivations](DesignMotivations.md).