Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site).

To break AppDomain boundaries objects other than expression trees are serialized into JSON. Current implementation is pretty naive and has certain shortcomings, so I'm planning to sometimes replace it with an awesome [Json.NET library](http://james.newtonking.com/pages/json-net.aspx).

This document only describes the serialization algorithm since deserialization one is quite trivial. Logics is as simple as instantiating an object based on existing type hints and filling it with the data provided. Getting type hints is totally another story - it's usually untrivial and is a courtesy of the receiver.

### Contract ###

**Input:** (Object, Type) pair: a root of an object graph and an expected type of the root.

**Errors:** Certain objects cause algorithm to fail if present in the input object graph (see algorithm descriptions below). Also certain graph peculiaritiess are not supported, namely:
  * Graphs with circular references crash the serializer.
  * Too large graphs will cause string overflows.

**Output:** String with [JSON-equivalent](http://tools.ietf.org/html/rfc4627) of the input graph.

### The algorithm ###

Serialization algorithm assumes that receiver has enough info to successfully and unambiguously resolve the type of serialized object, that's why it doesn't save any type hints. Different receivers differently mine this info:
  * Client of a remote LINQ query apriori knows the result type, and thus is fully aware about the structure of serialized object.
  * JS -> C# transformer tries to use context info (e.g. method/operator signature) to find out required type of the object. In cases when this info is insufficient (e.g. direct calls to the object), or ambiguous (e.g. two method overloads, one of which accepts strings and another one - guids, and the object serialized is a Guid) transformer will crash.

Serializer recursively traverses both graphs using breadth-first algorithm, and on every step matches expected type `T` and actual object `O`. Depending on the result of the match it either produces the string representation of the object in certain format, or crashes.

1. Input is checked against one of predefined conditions:
  * If `O` is null, it's serialized as `null`.
  * If `O` is an instance of a nullable type, or `T` is a nullable type, they're both undecorated and serialized as their undecorated versions.
  * If `T` is either `IEnumerable<U>`, or `ICollection<U>`, or `IList<U>`. In that case `O` is wrapped in `List<U>` and serialized as that type.
  * If `T` is `IDictionary<K, V>`. In that case `O` is wrapped in `Dictionary<K, V>` and serialized as that type.
  * If `T` is `IGrouping<TKey, TElement>`. In that case `O` is wrapped in `Grouping<TKey, TElement>` and serialized as that type. `Grouping` is in fact a Relinq-internal trivial implementation of `IGrouping`.

2. If `T` is not equal to the type of actual object `O`, serialization crashes.
Otherwise `O` is checked for the following special cases:
  * If the object is a JS primitive, i.e. "a member of one of the types Undefined, Null, Boolean, Number, or String" ([ECMAScript v3 spec](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf), 4.3.2 Primitive Value), then it's serialized as a JS literal. This corresponds to the following [.NET types](http://en.csharp-online.net/ECMA-334:_8.2.1_Predefined_types): `bool`, `byte`, `sbyte`, `ushort`, `short`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `string`.
  * If the object is an enum, its underlying integer value is serialized instead.
  * If the object is of char type, it's serialized as a `string` that consists of a single character.
  * If the object supports bi-directional conversions with `System.String`, i.e.:
```
var converter = TypeDescriptor.GetConverter(@object.GetType());
converter.CanConvertTo(typeof(String)) && converter.CanConvertFrom(typeof(String)))
```
> then it's serialized its string representation.

3. If no special cases are applicable, serializer detects serialization strategy: PropertyBag, List, Dictionary or Mixed according to the following:
  * If `T` implements a single `IEnumerable<U>` interface, and either of the following is true, it's up for List serialization strategy:
    * Has a default constructor and the `void Add(U)` method.
    * Has at least one of the constructors with signature: `.ctor(IEnumerable<U>)` or/and the `.ctor(params U[])`.
  * If `T` implements a single `IDictionary<K, V>` interface, and has a default constructor, it's no more up for List strategy but uses Dictionary strategy instead.
  * If `T` has a default constructor and non-zero amount of properties annotated with `[JsonProperty]` attribute it's up for PropertyBag serialization strategy.
  * If `T` is up for both PropertyBag and List/Dictionary serialization strategy, it's no more up for any serialization strategies but uses Mixed mode instead.
  * If `T` isn't up for any of serialization strategies, serializer crashes.

4. Serializer generates JSON representation of the object depending on detected serialization strategy:
  * PropertyBag: Visits all properties annotated with the `[JsonProperty]` attribute and recursively serializes them using property type as expected type and property value as actual object. The result of serializing the entire object is:
> `{<propName1> : <propValue1>, ... <propNameN> : <propValueN>`}
  * List: Iterates over all collection elements and recursively serializes them using collection's element type as expected type and element as actual object. The result of serializing the entire object is:
> `[<element1>, ... <elementN>]`
  * Dictionary: Iterates over keys and corresponding values of the dictionary and recursively serializes them using collection's key/value type as expected type and element as actual object. The result of serializing the entire object is:
> `[[<key1>, <value1>], ... [<keyN>, <valueN>]]`
  * Mixed: Uses the PropertyBag strategy to serialize all suitable properties, and then serializes collection content into a `$` property using either List or Dictionary strategy.