**General guidelines.** Info about error span, link to relevant spec page, link to the issues pages ("blah-blah.. if you consider this behavior to be an error, please visit the link blah-blah").

**C# -> JS.** Several of classes, of those only one has significant size, namely the RelinqScriptBuilder. It's a typical visitor, so I think it's appropriate to use the technique I've tried in Elf: http://elf4b.googlecode.com/svn/trunk/Elf/Syntax/AstBuilders/ElfAstBuilder.cs: a special exception ala ErroneousCSharpAST and a catch-all guard that covers the main Visit method, and wraps everything bar known stuff into the UnexpectedCSharpToJSException cover. Faults classification:
  * Unexpected (everything bar described below): root of the ast, precise node that caused the failure, no spec URL,
  * Unsupported expression type: root of the ast, precise node that caused the failure, [UnsupportedCSharpConstructs](UnsupportedCSharpConstructs.md),
  * Integration exceptions (0 or multiple JS factories registered): root of the ast, precise node that caused the failure, why we failed: 0 or >1 (too bad we can't yet disassemble the delegates and report them), [Infrastructure](Infrastructure.md),
  * JSON serialization exceptions, maintain information about the root object being serialized, so that possible exception info looks like the following: (error xforming c# ast (root)) -> (error xforming node (node)) -> (error serializing (root) as (type)) -> (error serializing (propertyPath) as (type) because ...), [JsonSerialization](JsonSerialization.md).

**JS -> C#.** A multi-stage process, so it needs quite a bit of thought. General approach remains the same tho: a set of expected exceptions that indicate erroneous input and a single unexpected stuff that covers everything vicious that might happen.

**1st phase. Acquiring RelinqScriptExpression-based AST.** First of all we need to migrate to technique of handling ANTLR exceptions used in Elf: http://elf4b.googlecode.com/svn/trunk/Elf/Exceptions/Parser/RecognitionExceptionHelper.cs. Secondly, we need to put an effort into tracking position information about failboats, like it's done e.g. here: http://elf4b.googlecode.com/svn/trunk/Elf/Exceptions/Parser/SyntaxErrorException.cs. Faults classification:
  * Unexpected (everything bar described below): script text, ast root and precise node that caused the failure (if it's a semantic error), no spec URL,
  * Syntax errors (reported by lexer/parser of ANTLR): script text, error span, link to ESv3 spec.
  * Semantic errors (failures to transform JS AST to RS AST): script text, error span, ANTLR AST root, ANTLR AST node, [UnsupportedJSConstructs](UnsupportedJSConstructs.md) and [BuildingRelinqScriptAst](BuildingRelinqScriptAst.md). Or more precisely:
    * No idea how to parse the node XXX (occurs when no rules match the node, multiple rules is an unexpected exception),
    * Unsupported format of the ANTLR node XXX (matches a rule, but fails to match the structure),

Under construction.