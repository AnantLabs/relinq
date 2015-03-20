Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically building a strongly-typed C# expression tree that is equivalent to a given JS expression provided certain conditions are met.

[The JS -> C# transformation](http://code.google.com/p/relinq/wiki/JSToCSharp) starts with syntax verification of provided JS code that makes sure that the code doesn't contain unsupported language constructs that apriori can't be transformed to C#. Upon success of verification Relinq starts actual compilation that resolves node types and builds an equivalent C# expression tree.

### Contract ###

**Input:** MemberAccess node of [RelinqScript AST](http://code.google.com/p/relinq/wiki/RelinqScriptSyntax) with the Name `<name>` and the Target of a closed generic or non-generic type `T`.

**Errors:** Error occurs if within type `T` there doesn't exist a .NET field, property or method group that suits specified name and access style. Otherwise algorithm produces the output as described below.

**Output:** Unambiguously resolved .NET MemberInfo or a method group (set of MethodInfos).

### Design considerations ###

Algorithm described below closely conforms to C# 3.0 spec: [C# 2.0: 14.3 Member lookup](http://en.csharp-online.net/ECMA-334:_14.3_Member_lookup) augmented by [Extension Methods, Importing Extension Methods and Extension Method Invocations](http://msdn.microsoft.com/en-us/library/ms364047(VS.80).aspx#cs3spec_topic3) coupled with [14.4.2.1 Applicable function member](http://en.csharp-online.net/ECMA-334:_14.4.2.1_Applicable_function_member).

We do not have to worry about handling ambiguities, since .NET doesn't allow one to define a field/property and a method with the same name (except explicit interface implementations, but they ain't a target of this lookup algorithm anyways). _upd. Actually, this is untrue. An ambiguity might happen if one defines an extension method with the same name as as a field/property in the class._

Static invocations (except operator invocations and extension methods) require mechanisms that are otherwise unnecessary: [type importing and explicit generic arguments specification](http://code.google.com/p/relinq/wiki/StuffToDo) and thus are not supported.

C# spec disallows using `varargs` methods in method groups, but this algorithm omits the restriction.


### The algorithm ###

1) Compiler1 looks for public instance fields or properties with the name `<name>` defined in either `T` or its [base types](http://code.google.com/p/relinq/wiki/LookupBaseTypes). If the lookup is unsuccessful, algorithm proceeds to the next step.

If the lookup is successful, another check is being made to safeguard ourselves from possible name collisions. If the parent of the MemberAccess node is an InvokeExpression, results of lookup are abandoned and algorithm proceeds to the next step.

2) If there are no fields/properties with the name `name`, the algorithm performs lookup of the method group as described below:

The lookup list is first filled with suitable methods from the following sources:
  * Public instance (generic and non-generic) methods declared within the type `T` and its base types.
  * Extension methods that belong to either `Enumerable` or `Queryable` classes (note that matching the `this` argument with the target type `T` isn't required on this step - everything will be done during [TypeInferenceMethods](TypeInferenceMethods.md) step). Possible widening of the list of allowed host classes [is proposed](http://code.google.com/p/relinq/wiki/StuffToDo) as an extension to the spec.

After being initially filled, the lookup list is then filtered according to the following rules:
  * Methods that are hidden by other methods are removed from the list (this process is governed by the [HidingMethods](HidingMethods.md) algorithm).
  * Methods that do not match the `<name>` name are removed from the list.
  * Methods that have either `ref` or `out` parameters are removed from the list.