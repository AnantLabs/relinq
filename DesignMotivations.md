...or why doesn't Relinq support constructors?

Below are the reasons why this or that feature in the design of Relinq is missing at the moment:
  * Explicit argument specification for generic methods and/or lambda expressions:
    * No equivalent in JS => requres additional twists that are simply not worth it in the light of the next fact.
    * Absence of this feature is rarely essential, since most of the time C# type inference + overload resolution mechanisms handle this finely. Speaking about LINQ SQO, only Cast<>, OfType<> and Empty<> have problems with type inference.

  * Explicit typecasts
    * No equivalent in JS => requres additional twists that are simply not worth it in the light of the next fact.
    * Absence of this feature works relatively fine, since implicit typecasts required for method/operator invocations can be inferred, and explicit typecasts are used quite rarely. After all we've abandoned type specification for generic methods and lambdas, so let's continue this trend.

  * Constructors for objects other than anonymous:
    * Requires type importing mechanism.
    * Requires generic type arguments specification mechanism to remain consistent with generic types.
    * Requires mechanism for explicit type argument specification to remain consistent with generic methods.
    * Now when we've introduced all of those, we should introduce mechanism for explicit typecasts to remain consistent with usage of types in .NET applications.
    * And finally... was it really worth it?

  * Static fields/methods support:
    * Requires type importing mechanism.
    * Requires generic type arguments specification mechanism to remain consistent across all .NET types.
    * Conclusions: too much hassle for much less essential feature than constructors support.

  * Serializing complex objects from ConstantExpressions:
    * The idea is not to use explicit type specification info due to reasons specified above.
    * So the best we can do is to hope that reverse JS -> C# transformation will be able to get type information from the context (nearby AST nodes). According to my theoretical considerations this will work quite fine in most of the cases.

  * C#-style object initializers (ListInit and MemberInit):
    * No equivalent in JS => requres additional twists.
    * Has supported (tho, less convernient) analogues.

Yeah, as a result we've got quite stripped version of C# that is supported, however it seems to me that it finely covers everyday usage scenarios. If the implementation fails practical testing, I already have prepared necessary extensions to Relinq (each of those will require quite minor changes in the architecture):

  * Type importing mechanism. Can be implemented by either client- and server-side configuration of supported namespaces/types, or by correspondingly extending JS/RelinqScript grammar.

  * Generic argument specification. Add support for invoking constructors of generic types and using generic methods that otherwise is missing in JS. Examples:
    * `new Foo<Bar>(1, 2)` -> `new Foo.$(Bar)(1, 2)`
    * `foo.Bar<Foobar>(1, 2)` -> `foo.Bar.$(Foobar)(1, 2)`

  * Typecast mechanism. Name is self-explanatory. An alternative to this is using typecast extension for type inference engine or fall back to standard C# 3.0-style member resolution strategies. Examples:
    * `(Bar)foo` -> `$(Bar, foo)`

  * Typecast extension for type inference engine. Explicitly specified typecasts can actually be guessed with sufficient precision and decent unambiguousness (by building an AppDomain-wide implicit/explicit coercions graph).

  * Serializing complex objects from ConstantExpressions. It's a common practice that object that expected to be passed through AppDomain-boundaries implement certain serialization technique by themselves, usually using plain text. Passing the type hint and plain text that represents an object is a certain way to make it work with Relinq. `TypeConverter.ConvertTo(String)` or some variant of `IExpressionSerializable` would work as well.

  * C#-style object initializers. Bring support for object and list initializers that are otherwise not expressible in JS. Examples:
    * `new Foo(1, 2){Bar = 'bar'})` -> `new Foo(1, 2).$({Bar : 'bar'})`
    * `new List<int>{1, 2`} -> `new List.$(int)().$([1, 2])`