Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** Operator node of [RelinqScript AST](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax) that references operator with the type `<type>` that has `N` operands `<arg1>` ... `<argN>` of types `T1` ... `TN`.

**Errors:** Error occurs if there doesn't exist any .NET methods that suit specified operator type and operands. Otherwise algorithm produces the output as described below.

**Output:** A lookup list that contains non-zero number of method alternatives (possibly open generic and, thus, susceptible to [GenericArgsInference](GenericArgsInference.md) algorithm).

### Design considerations ###

Algorithm described below closely conforms to C# 3.0 spec: [14.2.5 Candidate user-defined operators](http://en.csharp-online.net/ECMA-334:_14.2.5_Candidate_user-defined_operators) coupled with [14.4.2.1 Applicable function member](http://en.csharp-online.net/ECMA-334:_14.4.2.1_Applicable_function_member).

### The algorithm ###

This algorithm fills the lookup list with suitable methods from the following sources:
  * Predefined operators that match the operator type.
  * User-defined operators according to the following rules:
    * Public static methods that match the operator type, have an IsSpecial flag set and are defined in `Ui` types including their base types given that the U-type is obtained from the corresponding T-type by stripping trailing `?` modifiers if any (i.e. unwrapping the type from possible `Nullable<>` decorations).
    * If either of `Ti` is an `UnknownConstant` the algorithm doesn't check it for u/d operators (scanning the entire AppDomain for all suitable u/d operators is possible as well).
  * After the list is initially filled in it's additionally populated by applying [lifting rules](http://en.csharp-online.net/ECMA-334:_14.2.7_Lifted_operators).

Lifting is applied to the both unary and binary operators (both predefined and user-defined) that satisfy the following criteria:
  * All operands and return value have non-nullable value type.
  * If operator is an equality operator (`==` `!=`) or a relational operator (`<` `>` `<=` `>=`) then it must have return value of type `bool`.

Lifting results in adding a new method to the lookup list (without changing methods that are already in the list). Method being added is a predefined monadic-style implementation that wraps the original method with the signature changed as follows:
  * If operator is an equality operator (`==` `!=`) or a relational operator (`<` `>` `<=` `>=`) then its lifted signature features nullable versions of both operands, but retains the original return value type.
  * In any other case lifted signature has both return value type and parameter types changed to nullable versions.

After being initially filled, the lookup list is then filtered according to the following rules (including those defined in the general version of method lookup algorithm):
  * Methods that are hidden by other methods are removed from the list (this process is governed by the [HidingMethods](HidingMethods.md) algorithm).
  * Methods that have either `ref` or `out` parameters are removed from the list.

<table cellpadding='5' border='1'>

<tr>
<td width='10%'>Operator type(s) as defined by <a href='RelinqScriptSyntax.md'>RelinqScriptSyntax</a></td>
<td width='30%'>Predefined operator(s)</td>
<td width='15%'>User-defined operator(s)</td>
<td>Notes</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.6.1_Unary_plus_operator'>UnaryPlus (+)</a></td>
<td valign='top'>
<code>int operator +(int x);</code><br />
<code>uint operator +(uint x);</code><br />
<code>long operator +(long x);</code><br />
<code>ulong operator +(ulong x);</code><br />
<code>float operator +(float x);</code><br />
<code>double operator +(double x);</code><br />
<code>decimal operator +(decimal x);</code><br />
</td>
<td valign='top'><code>op_UnaryPlus</code></td>
<td valign='top'></td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.6.2_Unary_minus_operator'>UnaryMinus (-)</a></td>
<td valign='top'>
<code>int operator –(int x);</code><br />
<code>long operator –(long x);</code><br />
<code>void operator –(ulong x);</code><br />
<code>float operator –(float x);</code><br />
<code>double operator –(double x);</code><br />
<code>decimal operator –(decimal x);</code><br />
</td>
<td valign='top'><code>op_UnaryMinus</code></td>
<td valign='top'>Selection of predefined ulong-operator by <a href='http://code.google.com/p/relinq/wiki/JSToCSharp'>type inference</a> engine will result in compilation error. Consequently, if the operand of the negation operator is of type ulong, a compilation error occurs.</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.6.4_Bitwise_complement_operator'>OnesComplement (~)</a></td>
<td valign='top'>
<code>int operator ~(int x);</code><br />
<code>uint operator ~(uint x);</code><br />
<code>long operator ~(long x);</code><br />
<code>ulong operator ~(ulong x);</code><br />
<code>E operator ~(E x)</code><br />
</td>
<td valign='top'><code>op_OnesComplement</code></td>
<td valign='top'>
If this is the reverse transformation of C# -> JS -> C# roundtrip, it will never contain ~ operator, since both bitwise and logical NOT operators are represented by the same C# expression type: ExpressionType.Not. Read above how this gets resolved.<br />
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.6.3_Logical_negation_operator'>LogicalNot (!)</a></td>
<td valign='top'>
<code>bool operator !(bool x);</code><br />
<code>int operator ~(int x);</code><br />
<code>uint operator ~(uint x);</code><br />
<code>long operator ~(long x);</code><br />
<code>ulong operator ~(ulong x);</code><br />
<code>E operator ~(E x)</code><br />
</td>
<td valign='top'><code>op_LogicalNot</code>, <code>op_OnesComplement</code></td>
<td valign='top'>Here Compiler1 searches for both logical and bitwise versions of the operator since they both are represented as ExpressionType.Not in C# expression trees and are written into JS as logical not (during C# -> JS serialization.</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.7.1_Multiplication_operator'>Multiply</a>,<br /> <a href='http://en.csharp-online.net/ECMA-334:_14.7.2_Division_operator'>Divide (/)</a>,<br /> <a href='http://en.csharp-online.net/ECMA-334:_14.7.3_Remainder_operator'>Modulo (%)</a></td>
<td valign='top'>
<code>int operator &lt;op&gt;(int x, int y);</code><br />
<code>uint operator &lt;op&gt;(uint x, uint y);</code><br />
<code>long operator &lt;op&gt;(long x, long y);</code><br />
<code>ulong operator &lt;op&gt;(ulong x, ulong y);</code><br />
<code>void operator &lt;op&gt;(long x, ulong y);</code><br />
<code>void operator &lt;op&gt;(ulong x, long y);</code><br />
<code>float operator &lt;op&gt;(float x, float y);</code><br />
<code>double operator &lt;op&gt;(double x, double y);</code><br />
<code>decimal operator &lt;op&gt;(decimal x, decimal y);</code><br />
</td>
<td valign='top'><code>op_Multiply</code>,<br /> <code>op_Division</code>,<br /> <code>op_Modulus</code></td>
<td valign='top'>
The operators with void return type always produce a compile-time error. Consequently, it is an error for one operand to be of type long and the other to be of type ulong.<br>
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.7.4_Addition_operator'>Add ( + )</a></td>
<td valign='top'>
<code>int operator +(int x, int y);</code><br />
<code>uint operator +(uint x, uint y);</code><br />
<code>long operator +(long x, long y);</code><br />
<code>ulong operator +(ulong x, ulong y);</code><br />
<code>void operator +(long x, ulong y);</code><br />
<code>void operator +(ulong x, long y);</code><br />
<code>float operator +(float x, float y);</code><br />
<code>double operator +(double x, double y);</code><br />
<code>decimal operator +(decimal x, decimal y);</code><br />
<code>E operator +(E x, U y)</code><br />
<code>E operator +(U x, E y)</code><br />
<code>string operator +(string x, string y);</code><br />
<code>string operator +(string x, object y);</code><br />
<code>string operator +(object x, string y);</code><br />
<code>D operator +(D x, D y);</code><br />
</td>
<td valign='top'><code>op_Addition</code></td>
<td valign='top'>
The operators with void return type always produce a compile-time error. Consequently, it is an error for one operand to be of type long and the other to be of type ulong.<br />

Every enumeration type implicitly provides mentioned predefined operators, where E is the enum type, and U is the underlying type of E.<br />

The binary + operator performs string concatenation when one or both operands are of type string. If an operand of string concatenation is null, an empty string is substituted. Otherwise, any non-string operand is converted to its string representation by invoking the virtual ToString method inherited from type object. If ToString returns null, an empty string is substituted. <br />

The binary + operator performs delegate combination when both operands are convertible to some delegate type D. (If the operands have different delegate types, overload resolution finds no applicable operator and compilation error occurs.)<br />
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.7.5_Subtraction_operator'>Subtract ( - )</a></td>
<td valign='top'>
<code>int operator –(int x, int y);</code><br />
<code>uint operator –(uint x, uint y);</code><br />
<code>long operator –(long x, long y);</code><br />
<code>ulong operator –(ulong x, ulong y);</code><br />
<code>void operator -(long x, ulong y);</code><br />
<code>void operator -(ulong x, long y);</code><br />
<code>float operator –(float x, float y);</code><br />
<code>double operator –(double x, double y);</code><br />
<code>decimal operator –(decimal x, decimal y);</code><br />
<code>U operator –(E x, E y)</code><br />
<code>E operator –(E x, U y)</code><br />
<code>D operator –(D x, D y);</code><br />
</td>
<td valign='top'><code>op_Subtraction</code></td>
<td valign='top'>
The operators with void return type always produce a compile-time error. Consequently, it is an error for one operand to be of type long and the other to be of type ulong.<br />

Every enumeration type implicitly provides mentioned predefined operators, where E is the enum type, and U is the underlying type of E.<br />

The binary - operator performs delegate removal when both operands are convertible to some delegate type D. (If the operands have different delegate types, overload resolution finds no applicable operator and compilation error occurs.)<br />
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.8_Shift_operators'>LeftShift ( &lt;&lt; ), RightShift ( &gt;&gt; )</a></td>
<td valign='top'>
<code>int operator &lt;op&gt;(int x, int count);</code><br />
<code>uint operator &lt;op&gt;(uint x, int count);</code><br />
<code>long operator &lt;op&gt;(long x, int count);</code><br />
<code>ulong operator &lt;op&gt;(ulong x, int count);</code><br />
</td>
<td valign='top'><code>op_LeftShift</code>,<br /> <code>op_RightShift</code></td>
<td valign='top'></td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.9_Relational_and_type-testing_operators'>Equals ( == ), NotEquals ( != ), GreaterThan ( &gt; ), LessThan ( &lt; ), GreaterThanOrEquals ( &gt;= ), LessThanOrEquals ( &lt;= )</a></td>
<td valign='top'>
<code>bool operator &lt;op&gt;(int x, int y);</code><br />
<code>bool operator &lt;op&gt;(uint x, uint y);</code><br />
<code>bool operator &lt;op&gt;(long x, long y);</code><br />
<code>bool operator &lt;op&gt;(ulong x, ulong y);</code><br />
<code>void operator &lt;op&gt;(long x, ulong y);</code><br />
<code>void operator &lt;op&gt;(ulong x, long y);</code><br />
<code>bool operator &lt;op&gt;(float x, float y);</code><br />
<code>bool operator &lt;op&gt;(double x, double y);</code><br />
<code>bool operator &lt;op&gt;(decimal x, decimal y);</code><br />
<code>bool operator &lt;op&gt;(E x, E y)</code><br />
<code>bool operator ==(bool x, bool y);</code><br />
<code>bool operator !=(bool x, bool y);</code><br />
<code>bool operator ==(string x, string y);</code><br />
<code>bool operator !=(string x, string y);</code><br />
<code>bool operator ==(D x, D y);</code><br />
<code>bool operator !=(D x, D y);</code><br />
<code>bool operator ==(System.Delegate x, System.Delegate y);</code><br />
<code>bool operator !=(System.Delegate x, System.Delegate y);</code><br />
<code>bool operator ==(C x, C y);</code><br />
<code>bool operator !=(C x, C y);</code><br />
<code>bool operator ==(S? x, Null y);</code><br />
<code>bool operator ==(Null x, S? y);</code><br />
<code>bool operator !=(S? x, Null y);</code><br />
<code>bool operator !=(Null x, S? y);</code><br />
</td>
<td valign='top'><code>op_Equality</code>,<br /> <code>op_Inequality</code>,<br /> <code>op_GreaterThan</code>,<br /> <code>op_LessThan</code>,<br /> <code>op_GreaterThanOrEquals</code>,<br /> <code>op_LessThanOrEquals</code> </td>
<td valign='top'>
The operators with void return type always produce a compile-time error. Consequently, it is an error for one operand to be of type long and the other to be of type ulong.<br />

Boolean type defines only equality and inequality operators, but not other ones.<br />

Every class type C implicitly provides predefined reference type equality operators unless predefined equality operators otherwise exists for C (for example, when C is string or System.Delegate).<br />

Every delegate type implicitly provides predefined comparison operators. They are even available for comparing delegates when the delegate type is not known at compile time.<br />

The <code>==</code> and <code>!=</code> operators permit one operand to be a value of a nullable type and the other to have the null type, even if no predefined or user-defined operator (in unlifted or lifted form) exists for the operation. In that case, the result is instead computed from the HasValue property of a nullable object.<br />
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.10_Logical_operators'>And ( &amp; ), Or ( | ), ExclusiveOr ( ^ )</a></td>
<td valign='top'>
<code>int operator &lt;op&gt;(int x, int y);</code><br />
<code>uint operator &lt;op&gt;(uint x, uint y);</code><br />
<code>long operator &lt;op&gt;(long x, long y);</code><br />
<code>ulong operator &lt;op&gt;(ulong x, ulong y);</code><br />
<code>void operator &lt;op&gt;(long x, ulong y);</code><br />
<code>void operator &lt;op&gt;(ulong x, long y);</code><br />
<code>bool operator &lt;op&gt;(bool x, bool y);</code><br />
<code>E operator &lt;op&gt;(E x, E y)</code><br />
</td>
<td valign='top'><code>op_BitwiseAnd</code>,<br /> <code>op_BitwiseOr</code>,<br /> <code>op_ExclusiveOr</code></td>
<td valign='top'>
The operators with void return type always produce a compile-time error. Consequently, it is an error for one operand to be of type long and the other to be of type ulong.<br />

To ensure that the results produced by the & and | operators for <code>bool?</code> operands are consistent with SQL’s three-valued logic, <a href='http://en.csharp-online.net/ECMA-334:_14.10.4_The_bool%3F_logical_operators'>special rules for boolean operator lifting</a> are provided.<br />
</td>
</tr>

<tr>
<td valign='top'><a href='http://en.csharp-online.net/ECMA-334:_14.11_Conditional_logical_operators'>AndAlso ( &amp;&amp; ), OrElse ( || )</a></td>
<td valign='top'>
An operation of the form <code>x &amp;&amp; y</code> or <code>x || y</code> is processed as if the operation was written <code>x &amp; y</code> or <code>x | y</code>.<br />
</td>
</tr>

</table>