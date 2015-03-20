Born as a framework for remote invocation of LINQ queries, Relinq is now a **tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf)** (referenced versions of languages are shorthanded as C# and JS on this site). Framework provides means of programmatically performing the following tasks in .NET 3.5 environment:
  * [C# -> JS](http://code.google.com/p/relinq/wiki/CSharpToJS): Given a C# expression tree build a string that represents an equivalent JS expression provided [certain conditions are met](http://code.google.com/p/relinq/wiki/UnsupportedCSharpConstructs).
  * [JS -> C#](http://code.google.com/p/relinq/wiki/JSToCSharp): Given a string representing a JS expression build an equivalent C# expression tree provided [certain conditions are met](http://code.google.com/p/relinq/wiki/UnsupportedJSConstructs).

The example below features a typical scenario that is made possible by Relinq. Given a JS expression in plain text format you can get an equivalent LINQ query in your .NET code. The transformation is bi-directional, i.e. the resulting LINQ expression tree can be transformed back to a string, and the result of the round-trip will be the same as the input.

```
// LINQ query in plain text format
ctx.Companies
  .Where(function(c){ return c.Employees.Count == 2; })
  .Select(function(c){ return { Name : c.Name }; });

↑
Relinq
↓

// Equivalent C# 3.0 expression tree
from c in ctx.Companies 
where c.Employees.Count == 2 
select new { c.Name };
```

Below are the most prominent **scenarios that can be implemented with Relinq** ([HowToUse](HowToUse.md) document contains description of Relinq from a user viewpoint and has instructions on reproducing these scenarios in arbitrary applications):
  * **Querying LINQ datasources from JavaScript**. Relinq provides a framework for processing remote queries to LINQ-enabled datasources from languages and environments that are .NET/LINQ-agnostic. By implementing a service endpoint that accepts queries in plain text format and reifies them using Relinq you enable the client to perform rich queries to your back-end. Such queries aren't limited to a predefined set of operations, but rather can call any custom code you allow them. Since Relinq fully exposes compilation and marshalling logic, you're free to implement whatever security you might need. For instance, this can be useful for implementing client-side data access layer in web applications.

  * **Invoking cross-tier LINQ queries**. Relinq makes it possible to remotely invoke LINQ queries and includes all infrastructure classes needed to use that functionality. Invoking a remote LINQ query is as simple as running the query against a [simple provider](http://relinq.googlecode.com/svn/trunk/Source/Relinq/Infrastructure/Client/DataContexts/ClientDataContext.cs). The query will then be serialized to a string, passed between tiers, reified with Relinq and executed. The result will be passed back to the caller in JSON format.

  * **Constructing LINQ queries dynamically**. Relinq provides means of building LINQ expressions using trivial string operations, parsing built expressions (given certain conditions are met) and then using them in possibly cross-tier invocations. We also plan to support strongly-typed and object-oriented construction of expression trees with the use of [invocations inlining](http://code.google.com/p/relinq/wiki/Inliner).