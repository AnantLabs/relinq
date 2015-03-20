Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that builds an equivalent C# expression tree and resolves member access.

### Contract ###

**Input:** [AST created by JS lexer+parser suite](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax) created from [a complete ECMAScript 3 grammar for ANTLR 3](http://research.xebic.com/es3/).

**Errors:** Error occurs if the AST corresponds to JS code with [unsupported constructs that cause transformation errors](http://code.google.com/p/relinq/wiki/UnsupportedJSConstructs). Otherwise algorithm produces the output as described below.

**Output:** [RelinqScript AST](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax) that is ready to be compiled into C# expression tree.

### The algorithm ###

The parser recursively traverses the input AST, validates it and emits the ouput AST according to the rules described below.

Expressions covered here reference the [ECMAScript v3 spec](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf). If certain construct is not supported, consult the [UnsupportedJSConstructs](UnsupportedJSConstructs.md) document for extra information and more details.

| Expressions | Emitted AST node | Notes |
|:------------|:-----------------|:------|
| 11.1.1 The this Keyword |  | Not supported. |
| 11.1.2 Identifier Reference | Variable/Keyword | If the usage is inconsistent (scoping rules described in 10.1.4.) a compilation error will occur during one of the next steps. Variable `ctx` is assumed to be declared in the global context and reference the data context that hosts `IQueryable` beans. If an identifier clashes with any of JS keywords bar literal definitions (see the table below), compilation error occurs.<br /> http://relinq.googlecode.com/svn/wiki/images/allJsKeywords.PNG |
| 11.1.3 Literal Reference | Constant | This includes a `null` literal, `false` and `true` boolean literals, numbers and strings (7.8 Literals). |
| 11.1.4 Array Initialiser | Constant | Not supported for dynamic creation of collections, i.e. if not a constant expression (constant array initializer has values that are either constant array/object initializers or literals). |
| 11.1.5 Object Initialiser | Constant/New | If it's a constant expression (e.g. all values assigned are either constant array/object initializers or literals), then represented as a Constant node, else - as a New node. |
| 11.1.6 The Grouping Operator |  | Gets ignored and execution then proceeds to the parenthesized expression. |
| 11.2.1 Property Accessors (dot notation) | MemberAccess | Can be used to reference either fields/properties or method groups. |
| 11.2.1 Property Accessors (bracket notation) | Indexer | Gets mapped onto C# indexers or ArrayIndex semantics. |
| 11.2.2 The `new` Operator |  | Not supported. |
| 11.2.3 Function Calls, 11.2.4 Argument Lists | Invoke | Expresses call semantics for both instance methods and function objects. |
| 11.2.5 Function Expressions | Lambda | Function body block can only contain a single `return` statement. Assigning (and, thus, referencing created functions) identifiers to created functions is not supported. |
| 11.3.1 Postfix Increment Operator |  | Not supported. |
| 11.3.2 Postfix Decrement Operator |  | Not supported. |
| 11.4.1 The `delete` Operator |  | Not supported. |
| 11.4.2 The `void` Operator |  | Not supported. |
| 11.4.3 The `typeof` Operator |  | Not supported. |
| 11.4.4 Prefix Increment Operator |  | Not supported. |
| 11.4.5 Prefix Decrement Operator |  | Not supported. |
| 11.4.6 Unary `+` Operator | Operator, type = `UnaryPlus` |  |
| 11.4.7 Unary `-` Operator | Operator, type = `UnaryMinus` |  |
| 11.4.8 Bitwise NOT Operator ( `~` ) | Operator, type = `OnesComplement` | If this is the reverse transformation of C# -> JS -> C# roundtrip, it will never contain `~` operator, since both bitwise and logical NOT operators are represented by the same C# expression type: ExpressionType.Not. However, JS -> C# is capable of distinguishing between these operators. |
| 11.4.9 Logical NOT Operator ( `!` ) | Operator, type = `LogicalNot` |  |
| 11.5.1 Applying the `*` Operator | Operator, type = `Multiply` |  |
| 11.5.2 Applying the `/` Operator | Operator, type = `Divide` |  |
| 11.5.3 Applying the `%` Operator | Operator, type = `Modulo` |  |
| 11.6.1 The Addition operator ( `+` ) | Operator, type = `Add` |  |
| 11.6.2 The Subtraction operator ( `-` ) | Operator, type = `Subtract` |  |
| 11.7.1 The Left Shift Operator ( `<<` ) | Operator, type = `LeftShift` |  |
| 11.7.2 The Signed Right Shift Operator ( `>>` ) | Operator, type = `RightShift` |  |
| 11.7.3 The Unsigned Right Shift Operator ( `>>>` ) |  | Not supported. |
| 11.8.1 The Less-than Operator ( `<` ) | Operator, type = `LessThan` |  |
| 11.8.2 The Greater-than Operator ( `>` ) | Operator, type = `GreaterThan` |  |
| 11.8.3 The Less-than-or-equal Operator ( `<=` ) | Operator, type = `LessThanOrEqual` |  |
| 11.8.4 The Greater-than-or-equal Operator ( `>=` ) | Operator, type = `GreaterThanOrEqual` |  |
| 11.8.6 The instanceof operator |  | Not supported. |
| 11.8.7 The in operator |  | Not supported. |
| 11.9.1 The Equals Operator ( `==` ) | Operator, type = `Equal` |  |
| 11.9.2 The Does-not-equals Operator ( `!=` ) | Operator, type = `NotEqual` |  |
| 11.9.4 The Strict Equals Operator ( `===` ) |  | Not supported. |
| 11.9.5 The Strict Does-not-equal Operator ( `!==` ) |  | Not supported. |
| 11.10 Binary Bitwise Operators | Operator, type = `And`, `Or`, `ExclusiveOr` |  |
| 11.11 Binary Logical Operators | Operator, type = `AndAlso`, `OrElse` |  |
| 11.12 Conditional Operator ( `?:` ) | Conditional |  |
| 11.13.1 Simple Assignment ( `=` ) |  | Not supported. |
| 11.13.2 Compound Assignment ( `op=` ) |  | Not supported. |