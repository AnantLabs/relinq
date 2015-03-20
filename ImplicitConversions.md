Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Two closed generic or non-generic .NET types: `S` and `T`.

**Errors:** Algorithm never fails.

**Output:** A boolean that indicates whether there exists an implicit conversion from type `S` to type `T` or not.

### Design considerations ###

Sometimes there's a possibility to correctly find out whether an implicit conversion exists or not even if one or both analyzed types are open generics. However, current algorithm doesn't support this feature.

### The algorithm ###

The table below describes a complete way to find out whether a certain closed generic or non-generic type `S` is implicitly convertible to a certain closed generic or non-generic  type `T`:

Classes of implicit conversion described below do not intersect each other (according to [user-defined conversions declaration rules](http://en.csharp-online.net/ECMA-334:_13.4.3_User-defined_implicit_conversions)), so there's no need in tie-breaking rules.

<table cellpadding='5' border='1'>

<tr>
<td width='30%'><a href='http://en.csharp-online.net/ECMA-334:_13.1_Implicit_conversions'>Implicit conversions</a></td>
<td width='10%'><a href='http://en.csharp-online.net/ECMA-334:_13.3.1_Standard_implicit_conversions'>Is standard?</a></td>
<td width='70%'>Conversions</td>
</tr>

<tr>
<td valign='top'>
Implicit conversions that involve predefined types<br>
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_13.1.2_Implicit_numeric_conversions'>Implicit numeric conversions</a></td>
<td valign='top'>Yes</td>
<td valign='top'>
<li>From <code>sbyte</code> to <code>short</code>, <code>int</code>, <code>long</code>, <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>byte</code> to <code>short</code>, <code>ushort</code>, <code>int</code>, <code>uint</code>, <code>long</code>, <code>ulong</code>, <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>short</code> to <code>int</code>, <code>long</code>, <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>ushort</code> to <code>int</code>, <code>uint</code>, <code>long</code>, <code>ulong</code>, <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>int</code> to <code>long</code>, <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>uint</code> to <code>long</code>, <code>ulong</code>, <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>long</code> to <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>ulong</code> to <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
<li>From <code>float</code> to <code>double</code>.</li>
<li>From <code>char</code> to <code>ushort</code>, <code>int</code>, <code>uint</code>, <code>long</code>, <code>ulong</code>, <code>float</code>, <code>double</code>, or <code>decimal</code>.</li>
</td>
</tr>

<tr>
<td valign='top'>Relinq-specific enum conversions</td>
<td valign='top'>Yes</td>
<td valign='top'>
As <a href='http://code.google.com/p/relinq/wiki/JsonSerialization'>enums are represented with ints</a>, Relinq introduces an implicit <code>int</code> -> any <code>enum</code> conversion given that int is represented by a Constant node.<br>
</td>
</tr>

<tr>
<td valign='top'>Relinq-specific string conversions</td>
<td valign='top'>Yes</td>
<td valign='top'>
As <a href='http://code.google.com/p/relinq/wiki/JsonSerialization'>chars are represented with strings</a>, Relinq introduces an implicit <code>string</code> -> any numeric type conversion given that string is represented by a Constant node that has the data of length 1.<br>
</td>
</tr>

<tr>
<td valign='top'>
Implicit conversions that involve reference types<br>
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_13.1.4_Implicit_reference_conversions'>Implicit reference conversions</a></td>
<td valign='top'>Yes</td>
<td valign='top'>
<li>From any reference-type to <code>object</code>.</li>
<li>From any class-type <code>S</code> to any class-type <code>T</code>, provided <code>S</code> is derived from <code>T</code>.</li>
<li>From any class-type <code>S</code> to any interface-type <code>T</code>, provided <code>S</code> implements <code>T</code>.</li>
<li>From any interface-type <code>S</code> to any interface-type <code>T</code>, provided <code>S</code> is derived from <code>T</code>.</li>
<li>From an array-type <code>S</code> with an element type <code>SE</code> to an array-type <code>T</code> with an element type <code>TE</code>, provided all of the following are true:<br>
<ul>
<blockquote><li>1) <code>S</code> and <code>T</code> differ only in element type. In other words, <code>S</code> and <code>T</code> have the same number of dimensions.</li>
<li>2) An implicit reference conversion exists from <code>SE</code> to <code>TE</code>.</li>
</ul></li>
<li>From a one-dimensional array-type <code>S[]</code> to <code>System.Collections.Generic.IList&lt;S&gt;</code> and base interfaces of this interface.</li>
<li>From a one-dimensional array-type <code>S[]</code> to <code>System.Collections.Generic.IList&lt;T&gt;</code> and base interfaces of this interface, provided there is an implicit reference conversion from <code>S</code> to <code>T</code>.</li>
<li>From any array-type to <code>System.Array</code>.</li>
<li>From any delegate-type to <code>System.Delegate</code>.</li>
<li>From any array-type to any interface implemented by <code>System.Array</code>.</li>
<li>From any delegate-type to <code>System.ICloneable</code>.</li>
</td>
</tr></blockquote>

<tr>
<td valign='top'>
Implicit conversions that involve value types<br>
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_13.1.5_Boxing_conversions'>Boxing conversions</a></td>
<td valign='top'>Yes</td>
<td valign='top'>
<ul><li>A boxing conversion permits any non-nullable-value-type to be implicitly converted to the type object or System.ValueType or to any interface-type implemented by the non-nullable-value-type.</li></ul>

<ul><li>An enum can be boxed to the type System.Enum, since that is the direct base class for all enums.</li></ul>

<ul><li>A nullable-type has a boxing conversion to the same set of types to which the nullable-type’s underlying type has boxing conversions.<br>
</td>
</tr></li></ul>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_13.7.2_Nullable_conversions'>Nullable conversions</a></td>
<td valign='top'>Yes</td>
<td valign='top'>
For each of the predefined implicit conversions that convert from a non-nullable value type <code>S</code> to a non-nullable value type <code>T</code>, the following implicit nullable conversions exist:<br>
<ul><li>An implicit conversion from <code>S?</code> to <code>T?</code>.<br>
</li><li>An implicit conversion from <code>S</code> to <code>T?</code>.<br>
</td>
</tr></li></ul>

<tr>
<td valign='top'>
Special implicit conversions<br>
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_13.1.1_Identity_conversion'>Identity conversion</a></td>
<td valign='top'>Yes</td>
<td valign='top'>
An identity conversion converts from any type to the same type. This conversion exists only such that an entity that already has a required type can be said to be convertible to that type.<br>
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_13.1.8_User-defined_implicit_conversions'>User-defined implicit conversions</a></td>
<td valign='top'>No</td>
<td valign='top'>
Exact definitions of evaluation of user-defined implicit conversions are given below. The definitions make use of the following terms:<br>
<ul><li>If a standard implicit conversion exists from a type <code>A</code> to a type <code>B</code>, and if neither <code>A</code> nor <code>B</code> are interface-types, then <code>A</code> is said to be encompassed by <code>B</code>, and <code>B</code> is said to encompass <code>A</code>.<br>
</li><li>The most encompassing type in a set of types is the one type that encompasses all other types in the set. If no single type encompasses all other types, then the set has no most encompassing type. In more intuitive terms, the most encompassing type is the "largest" type in the set—the one type to which each of the other types can be implicitly converted.<br>
</li><li>The most encompassed type in a set of types is the one type that is encompassed by all other types in the set. If no single type is encompassed by all other types, then the set has no most encompassed type. In more intuitive terms, the most encompassed type is the "smallest" type in the set—the one type that can be implicitly converted to each of the other types.</li></ul>

A user-defined implicit conversion from type <code>S</code> to type <code>T</code> is looked up <a href='http://en.csharp-online.net/ECMA-334:_13.4.3_User-defined_implicit_conversions'>as follows</a>.<br>
<br>
1) A list <code>U</code> of candidate conversions is filled in according to the following algorithm:<br>
<ul><li>Determine the types <code>S0</code> and <code>T0</code> that result from removing the trailing <code>?</code> modifiers, if any, from <code>S</code> and <code>T</code>.<br>
</li><li>Find the set of types, <code>D</code>, from which user-defined conversion operators will be considered. This set consists of :<br>
<ul><li><code>S0</code> (if <code>S0</code> is a class or struct),<br>
</li><li>The base classes of <code>S0</code> (if <code>S0</code> is a class),<br>
</li><li><code>T0</code> (if <code>T0</code> is a class or struct),<br>
</li><li>The base classes of <code>T0</code> (if <code>T0</code> is a class).<br>
</li></ul></li><li>Find the set of applicable conversion operators, <code>U</code>. This set consists of:<br>
<ul><li>User-defined and declared by the classes or structs in <code>D</code> that convert from a type encompassing <code>S</code> to a type encompassed by <code>T</code>.<br>
</li><li>If both parameter and result type of a certain operator in the set have non-nullable value types, <a href='http://en.csharp-online.net/ECMA-334:_13.7.3_Lifted_conversions'>a lifted implicit conversion operator</a> is added to the set as well (without changing methods that are already in the list). Method being added is a predefined monadic-style implementation that wraps the original method. Lifted signature has both return value type and parameter types changed to nullable versions.</li></ul></li></ul>

2) If <code>U</code> is not empty the following algorithm attempts to choose the most specific conversion operator from the list <code>U</code>:<br>
<ul><li>Find the most specific source type, <code>SX</code>, of the operators in <code>U</code>:<br>
<ul><li>If any of the operators in <code>U</code> convert from <code>S</code>, then <code>SX</code> is <code>S</code>.<br>
</li><li>Otherwise, <code>SX</code> is the most encompassed type in the combined set of source types of the operators in <code>U</code>. If exactly one most encompassed type cannot be found, then the conversion is ambiguous.<br>
</li></ul></li><li>Find the most specific target type, <code>TX</code>, of the operators in <code>U</code>:<br>
<ul><li>If any of the operators in <code>U</code> convert to <code>T</code>, then <code>TX</code> is <code>T</code>.<br>
</li><li>Otherwise, <code>TX</code> is the most encompassing type in the combined set of target types of the operators in <code>U</code>. If exactly one most encompassing type cannot be found, then the conversion is ambiguous.<br>
</li></ul></li><li>Find the most specific conversion operator:<br>
<ul><li>If <code>U</code> contains exactly one user-defined conversion operator that converts from <code>SX</code> to <code>TX</code>, then this is the most specific conversion operator.<br>
</li><li>Otherwise, if <code>U</code> contains exactly one lifted conversion operator that converts from <code>SX</code> to <code>TX</code>, then this is the most specific conversion operator.<br>
</li><li>Otherwise, the conversion is ambiguous.</li></ul></li></ul>

3) Depending on the results of previous steps, the following result of looking up a user-defined implicit conversion from type <code>S</code> to type <code>T</code> is returned:<br>
<ul><li>If <code>U</code> is empty, there doesn't exist an implicit user-defined conversion from type <code>S</code> to type <code>T</code>.<br>
</li><li>If <code>U</code> is not empty but there doesn't exist the most specific conversion operator in U, the conversion is ambiguous and for the purposes of type inference it doesn't exist.<br>
</li><li>Else there exists an implicit user-defined conversion from type <code>S</code> to type <code>T</code>.<br>
</td>
</tr></li></ul>

<tr>
<td valign='top'>
Implicit conversions that work with auxiliary types<br>
</td>
</tr>

<tr>
<td valign='top'><code>null</code> conversions <a href='http://en.csharp-online.net/ECMA-334:_13.7.1_Null_type_conversions'>(C# spec)</a></td>
<td valign='top'>Yes</td>
<td valign='top'>
An implicit conversion exists from the <code>null</code> type to any reference and/or nullable type.<br>
</td>

<tr>
<td valign='top'>Lambda conversions <a href='http://msdn.microsoft.com/en-us/library/ms364047(VS.80).aspx#cs3spec_topic4'>(C# spec)</a></td>
<td valign='top'>Yes</td>
<td valign='top'>
Lambda-expression <code>L</code> has an implicit conversion to a delegate type <code>D</code> provided:<br>
<ul><li><code>D</code> and <code>L</code> have the same number of parameters.<br>
</li><li><code>D</code> has no <code>ref</code> or <code>out</code> parameters.<br>
</li><li>If <code>D</code> has a void return type and the body of <code>L</code> is an expression, when each parameter of <code>L</code> is given the type of the corresponding parameter in <code>D</code>, the body of <code>L</code> is a valid expression.<br>
</li><li>If <code>D</code> has a non-void return type and the body of <code>L</code> is an expression, when each parameter of <code>L</code> is given the type of the corresponding parameter in <code>D</code>, the body of <code>L</code> is a valid expression that is implicitly convertible to the return type of <code>D</code>.</li></ul>

Lambda-expression <code>L</code> has an implicit conversion to <code>System.Linq.Expressions.Expression&lt;D&gt;</code> given <code>D</code> is a delegate and there exists an implicit conversion from <code>L</code> to <code>D</code>.<br>
<br>
Lambda-expression <code>L</code> does NOT have an implicit conversion to <code>System.Linq.Expressions.LambdaExpression</code> and any of its <a href='http://code.google.com/p/relinq/wiki/LookupBaseTypes'>base types</a>, because if used for overload resolution such conversion will prevent lambda parameter types from being inferred.<br>
</td>
</tr>

</tr>
<tr>
<td valign='top'><code>UnknownConstant</code> conversions</td>
<td valign='top'>Yes</td>
<td valign='top'>
An implicit conversion exists from the <code>UnknownConstant</code> type to any non-auxiliary type provided that the constant will be <a href='http://code.google.com/p/relinq/wiki/JsonSerialization'>successfully deserialized</a> as an instance of that type.<br>
</td>
</tr>

<tr>
<td valign='top'><code>MethodGroup</code> conversions <a href='http://msdn.microsoft.com/en-us/library/ms364047(VS.80).aspx#cs3spec_topic4'>(C# spec)</a></td>
<td valign='top'>Yes</td>
<td valign='top'>

Given a delegate type <code>D</code> with a set of arguments <code>A</code>: <code>A1</code>..<code>AM</code> and a method group <code>M</code> that features a set of alternatives <code>M1</code>..<code>MN</code>. The algorithm of finding out whether <code>M</code> is convertible to <code>D</code> works as follows:<br>
<ul><li>First of all alternatives <code>Mi</code> are filtered into <code>M'j</code> according to the following rules:<br>
<ul><li>Instance non-varargs methods that have number of parameters different from N are removed from the list.<br>
</li><li>Instance varargs methods that do not have normal form with N parameters are removed from the list (even if the expanded form is available, it's not taken into account).<br>
</li><li>Extension non-varargs methods that have number of parameters different from N+1 are removed from the list.<br>
</li><li>Extension varargs methods that do not have normal form with N+1 parameters are removed from the list (even if the expanded form is available, it's not taken into account).</li></ul></li></ul>

<ul><li>Then, a single best method <code>M'</code> for the set of arguments <code>A</code> is selected from <code>M'1</code>..<code>M'K</code> using the <a href='TypeInferenceMethods.md'>TypeInferenceMethods</a> algorithm. If the overload resolution/type inference algorithm fails, no conversion exists.</li></ul>

<ul><li>After that the method <code>M'</code> with the set of parameters <code>P</code>: <code>P1</code>..<code>PM</code> is checked for compatibility with <code>D</code> according to the following rules:<br>
<ul><li>For each corresponding pair of parameters: <code>Pi</code> and <code>Ai</code> there should exist an implicit identity or reference conversion from <code>Ai</code> to <code>Pi</code>.<br>
</li><li>An identity or implicit reference conversion exists from the return type of <code>M'</code> to the return type of <code>D</code>.<br>
</td>
</tr></li></ul></li></ul>

</table>