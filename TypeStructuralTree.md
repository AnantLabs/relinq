Structural tree (s-tree) of a certain type is a tree that describes relations between types used for construction of the source type. Structural units that can group types in the tree are generic type definitions and arrays. Examples:
  * The `int` type has a tree that consists of a single node: `[int]`.
  * The `Dictionary<List<T>, double>>` type is represented by a tree with 4 nodes as follows: `[Dictionary<,> --> [List<> --> [T]], double]]`.
  * The `int[,][]` type has the following structural tree: `[[] --> [[,] --> [int]]]`.

S-tree is just another way to describe a type, and is completely equivalent (has the same expressive way) to describing types with a single constructed instance of `System.Type` class. However, s-trees can be very useful for analyzing relations between types, especially when dealing with generic types.

Example 1. Usage in generic arguments inference. If a method argument's type is `Dictionary<List<int>, double>>` and the corresponding parameter type is `Dictionary<T, U>>`, then by using s-trees we can easily perform type inference by matching `T` with `List<int>` and `U` with `double`.

Example 2. Usage in overload resolution of generic methods. C# spec states that methods with type generic arguments are preferable over method with method generic arguments. To easily check this fact we take a generic definition of the declaring type, acquire both method signatures and build s-trees for their parameters. By comparing s-tree nodes with generic parameters of type and methods we produce so-called generic arguments mapping that helps to resolve this tricky situation.

```
private class Generic2<T1, T2>
{
    public T3[,][] Meth<T3>(T1 o1, Func<T1, T3> o2){}
}

var t = typeof(Generic2<,>);
var f1 = t.GetMethod("Meth");
var mapping = f1.GetGenericArgsMapping();

// mapping will be as follows:
ret[0][0] <- m[0]
a0 <- t[0]
a1[0] < t[0]
a1[1] <- m[0]
```