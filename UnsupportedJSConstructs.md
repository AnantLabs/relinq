Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

"Certain conditions" are defined using the rule as follows. **JS expression can be losslessly transformed into a C# expression tree if it doesn't contain unsupported constructs**. The document describes such constructs.

To check out details the transformation algorithm and how every supported JS construct is transformed into a C# expression tree consult the [JSToCSharp](JSToCSharp.md) document.

This document actively references JS spec: [Ecma-262 (ECMAScript v3)](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf).

### Unsupported constructs that cause transformation errors ###

  * Since the target of JS -> C# transformation is to build an equivalent _expression_ tree, neither statements (12 Statements) nor declarations (13 Function Definition, tho inline function declarations work fine) are supported.

  * Due to CLR implementation peculiarities of C# expression trees certain expression-related JS language constructs are unsupported as well:
    * Increment and decrement operators (11.3.1 Postfix Increment Operator, 11.3.2 Postfix Decrement Operator, 11.4.4 Prefix Increment Operator, 11.4.5 Prefix Decrement Operator).
    * Simple and compound assignments (11.13 Assignment Operators).
    * 11.1.1 The this Keyword.

  * Certain JS language constructs can't be expressed in CLR or have contradicting semantics. Such constructs will cause transformation-time errors. This includes:
    * Type conversions between primitive types (9 Type Conversion).
    * `delete` operator (11.4.1).
    * `void` operator (11.4.2).
    * `in` operator (11.8.7).
    * Unsigned right shift (>>>) operator (11.7.3).
    * Strict comparison operators (11.9.4 The Strict Equals Operator (`===`), 11.9.5 The Strict Does-not-equal Operator (`!==`)).

  * Certain JS language constructs can be expressed in CLR, but corresponding functionality is unsupported (by C# -> JS transformation as well):
    * Typesystem-related operators (11.4.3 The typeof Operator, 11.8.6 The instanceof operator).
    * Explicit object creation (11.2.2 The new Operator) is unsupported.
    * Implicit creation of objects (11.1.5 Object Initialiser) IS supported and is used to express anonymous objects in C#.

  * References to native JS objects (namely: 7.8.5 Regular Expression Literals, 15 Native ECMAScript Objects) are not supported.

### Unsupported constructs that cause compilation errors ###

After the JS code is proven to be syntactially correct and free of symantic errors detected by parser, JS -> C# transformation proceeds to compile the JS AST into a strongly-typed C# expression. Even if certain constructs are syntactically correct and do not meet any of above failure criterias JS -> C# compilation might fail due to one of the reasons described in [JSToCSharp](JSToCSharp.md) document.