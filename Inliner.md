Relinq is a tool for bi-directional transformation between [C# 3.0](http://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/csharp%20language%20specification.doc) expression trees and expressions of [EcmaScript v3](http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf) (referenced versions of languages are shorthanded as C# and JS on this site).

To widen the class of C# expression trees that can be successfully transformed into JS, Relinq features (optional, configurable) preprocessors that are executed prior to [C# -> JS transformation](http://code.google.com/p/relinq/wiki/CSharpToJS). One of these is an **inliner** that is capable of disassembling of suitable delegate invocations/method calls and integrating them into an expression tree.

### Contract ###

**Input:** C# expression tree.

**Errors:** The algorithm never produces errors - unsupported constructs are ignored. The  output of the algorithm is described below.

**Output:** An equivalent C# expression tree with suitable delegate invocations/method calls from the input expression tree disassembled and integrated into the output expression tree.

### Design considerations ###

TomasP says in http://tomasp.net/blog/dynamic-linq-queries.aspx:
> Since the first beta versions of LINQ we could hear comments that it is perfect for queries known at compile-time, however it is not possible to use it for building queries dynamically at runtime. In this article I show that this can be actually done very well for most of the common cases. [The solution offered by Microsoft](http://209.85.135.104/search?hl=en&q=cache%3Ahttp%3A%2F%2Fforums.microsoft.com%2FMSDN%2FShowPost.aspx%3FPostID%3D1752078%26SiteID%3D1) is to build query from a string, however this has many limitations and it in fact goes completely against what LINQ tries to achieve, which is writing queries in a type-safe way with full compile-time checking.

To address this problem (providing strongly-typed and object-oriented means of dynamic query composition) Tomas introduces `ToExpandable()` extension of `IQueryable` provider and implements dynamic attaching of arbitrary lambda to a LINQ query via a special construct `<lambda>.Expand(<params>)`.

```
// Lambda expression that calculates the price
Expression<Func<Nwind.Product, decimal?>> calcPrice = 
  (p) => p.UnitPrice * 1.19m;

// Query that selects products 
var q = 
  from p in db.Products.ToExpandable()  
  where calcPrice.Expand(p) > 30.0m
  select new { 
    p.ProductName, 
    OriginalPrice = p.UnitPrice,
    ShopPrice = calcPrice.Expand(p) };
```

Inliner (implemented as an expression visitor) _will (inliner is the only thingie in the entire spec I didn't start working at yet)_ solve this problem in a different way that preserves call semantics and doesn't require any special constructs to implement calls. If the inliner witnesses invocation of a lambda or even local method/delegate, it tries to disassemble the callee into an expression tree and if it succeeds then inliner integrates the resulting expression into an original one:

```
// Lambda expression that calculates the price
Func<Nwind.Product, decimal?> calcPrice = (p) => p.UnitPrice * 1.19m;

// This can also be a method that belongs to the closure
private decimal? CalcPrice(Nwind.Product p){ return p.UnitPrice * 1.19m; }

// Wrapper around IQueryable
var dbProducts = new InlinerContext(db.Products);

// Query that selects products 
var q = 
  from p in dbProducts
  where calcPrice(p) > 30.0m
  select new { 
    p.ProductName, 
    OriginalPrice = p.UnitPrice,
    ShopPrice = calcPrice(p) };
```

The inlining transformation is irreversible so we need to be sure that possible semantic losses are insignificant. For example, inlining a `String.Contains` call in a Linq-to-SQL query will distort original intentions of query author. Currently it's unclear what invocations can be painlessly inlined. To be discussed.

### The algorithm ###

Under construction.