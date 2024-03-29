using System;
using NUnit.Framework;
using Relinq.Exceptions.Core;
using Relinq.Exceptions.JSToCSharp;
using Relinq.Exceptions.JSToCSharp.TypeInference;

namespace Relinq.Playground
{
    [TestFixture]
    public class RelinqScriptCompilerExceptionsTests1
    {
        [Test]
        public void TestUndeclaredVariable()
        {
            try
            {
                var js = "ctx.Companies.Where(function(cmp){return c.Name == '';})";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("UndeclaredVariable", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.Where(function(cmp){return c.Name == '';})", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(UndeclaredVariableException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("[cmp]", ex1_kvps["Closure"]);
                Assert.AreEqual(@"
  Key: cmp
  Value: [/()/arg0:λ -> cmp => c.Name == '']", ex1_kvps["Closure[0]"]);
                Assert.AreEqual("UndeclaredVariable", ex1_kvps["Type"]);
                Assert.AreEqual("[/() -> ctx.Companies.Where(cmp => c.Name == '')]", ex1_kvps["Root"]);
                Assert.AreEqual("[/()/arg0:λ/==/l:f:Name/v:c -> c]", ex1_kvps["Expression"]);
                Assert.AreEqual("Variable", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestConstantInferenceFailed()
        {
            try
            {
                var js = "ctx.Companies.Where(function(c){return /.*/.Matches(c.Name);})";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("ConstantInferenceFailed", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.Where(function(c){return /.*/.Matches(c.Name);})", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(ConstantInferenceFailedException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("EcmaScriptV3Lexer.RegularExpressionLiteral (155)", ex1_kvps["TokenType"]);
                Assert.AreEqual("/.*/", ex1_kvps["Constant"]);
                Assert.AreEqual("ConstantInferenceFailed", ex1_kvps["Type"]);
                Assert.AreEqual("[/() -> ctx.Companies.Where(c => /.*/.Matches(c.Name))]", ex1_kvps["Root"]);
                Assert.AreEqual("[/()/arg0:λ/()/tar:f:Matches/c:(/.*/, RegularExpressionLiteral) -> /.*/]", ex1_kvps["Expression"]);
                Assert.AreEqual("Constant", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestCannotForgeAnonymousType()
        {
            try
            {
                var js = "ctx.Companies.Where(function(c){return {Name : c.Name, Id : '" + Guid.NewGuid() + "'}})";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("CannotForgeAnonymousType", ex0_kvps["Type"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids("ctx.Companies.Where(function(c){return {Name : c.Name, Id : '41501df7-4b02-4804-a781-321a6f758ae5'}})", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(CannotForgeAnonymousTypeException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("Id", ex1_kvps["OffendingProperty"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids("<unknown> ([/()/arg0:λ/new/c:('41501df7-4b02-4804-a781-321a6f758ae5', StringLiteral) -> '41501df7-4b02-4804-a781-321a6f758ae5'])", ex1_kvps["OffendingType"]);
                Assert.AreEqual("CannotForgeAnonymousType", ex1_kvps["Type"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids("[/() -> ctx.Companies.Where(c => new {Name = c.Name, Id = '41501df7-4b02-4804-a781-321a6f758ae5'})]", ex1_kvps["Root"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids("[/()/arg0:λ/new -> new {Name = c.Name, Id = '41501df7-4b02-4804-a781-321a6f758ae5'}]", ex1_kvps["Expression"]);
                Assert.AreEqual("New", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestRedeclaredVariable()
        {
            try
            {
                var js = "ctx.Companies.Where(function(c){return c.LolMethod9(c, function(c){return 2;});})";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("FruitlessMethodGroupResolution", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.Where(function(c){return c.LolMethod9(c, function(c){return 2;});})", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(MethodGroupResolutionFailedException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("[MG Where: System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, System.Boolean] predicate), System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, System.Boolean] predicate), System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, System.Boolean]] predicate), System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, System.Boolean]] predicate)]", ex1_kvps["MethodGroup"]);
                Assert.AreEqual("System.Collections.Generic.Dictionary`2[System.Reflection.MethodInfo,Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodResolutionException]", ex1_kvps["Failboats"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, System.Boolean] predicate)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, System.Boolean] predicate)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`2[TSource, System.Boolean]
    RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: a3d11219-e0ce-42d1-881f-679ab35e3449
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, System.Boolean] predicate)
      InferredSignature: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] [Extension] Where`1[Playground.Domain.Company](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`2[Playground.Domain.Company, System.Boolean] predicate)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`2[Playground.Domain.Company, System.Boolean]
      RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Playground.Domain.Company, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 4ff286d8-478d-41f1-8476-a49e1adda745
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.RedeclaredVariableException
        Closure: [c]
        Closure[0]: 
          Key: c
          Value: [/()/arg0:λ -> c => c.LolMethod9(c, c => 2)]
        Name: c
        Type: RedeclaredVariable
        Root: [/()/arg0:λ/() -> c.LolMethod9(c, c => 2)]
        Expression: [/()/arg0:λ/()/arg1:λ -> c => 2]
        ExpressionType: Lambda
        IsUnexpected: False
        Id: 6827788e-ede4-4507-83df-b3f98686b688
", ex1_kvps["Failboats[0]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, System.Boolean] predicate)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, System.Boolean] predicate)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`3[TSource, System.Int32, System.Boolean]
    RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 33d3a7ff-0d25-4502-8206-9175b00a6f85
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TSource] [Extension] Where`1[TSource](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, System.Boolean] predicate)
      InferredSignature: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] [Extension] Where`1[Playground.Domain.Company](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`3[Playground.Domain.Company, System.Int32, System.Boolean] predicate)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`3[Playground.Domain.Company, System.Int32, System.Boolean]
      RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 32475b13-8334-4f32-833d-ad2d8abd7bd0
", ex1_kvps["Failboats[1]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, System.Boolean]] predicate)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, System.Boolean]] predicate)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[TSource, System.Boolean]]
    RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: f7d7c6bb-2f3c-492c-9d98-f19189711bcc
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, System.Boolean]] predicate)
      InferredSignature: System.Linq.IQueryable`1[Playground.Domain.Company] [Extension] Where`1[Playground.Domain.Company](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, System.Boolean]] predicate)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, System.Boolean]]
      RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Playground.Domain.Company, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 86cdd37b-40ac-4768-aa7d-e2bab89f4ae2
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.RedeclaredVariableException
        Closure: [c]
        Closure[0]: 
          Key: c
          Value: [/()/arg0:λ -> c => c.LolMethod9(c, c => 2)]
        Name: c
        Type: RedeclaredVariable
        Root: [/()/arg0:λ/() -> c.LolMethod9(c, c => 2)]
        Expression: [/()/arg0:λ/()/arg1:λ -> c => 2]
        ExpressionType: Lambda
        IsUnexpected: False
        Id: 858a0f41-70e0-4445-965a-0029c314b7ef
", ex1_kvps["Failboats[2]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, System.Boolean]] predicate)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, System.Boolean]] predicate)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, System.Boolean]]
    RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 6377424f-b77e-4a0a-a64b-d534ded3c72c
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TSource] [Extension] Where`1[TSource](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, System.Boolean]] predicate)
      InferredSignature: System.Linq.IQueryable`1[Playground.Domain.Company] [Extension] Where`1[Playground.Domain.Company](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, System.Boolean]] predicate)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, System.Boolean]]
      RootExpression: [/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 3317825c-48c2-4cea-adaa-bda426c44f76
", ex1_kvps["Failboats[3]"]);
                Assert.AreEqual("FruitlessMethodGroupResolution", ex1_kvps["Type"]);
                Assert.AreEqual("[/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]", ex1_kvps["Root"]);
                Assert.AreEqual("[/() -> ctx.Companies.Where(c => c.LolMethod9(c, c => 2))]", ex1_kvps["Expression"]);
                Assert.AreEqual("Invoke", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestNoSuchFieldOrProp()
        {
            try
            {
                var js = "ctx.Companies.ElementAt(0).MissingProp";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("NoSuchFieldOrProp", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.ElementAt(0).MissingProp", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(NoSuchMemberException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("<Relinq.Playground.Domain.Company>", ex1_kvps["InferredTypeOfTarget"]);
                Assert.AreEqual("MissingProp", ex1_kvps["MemberName"]);
                Assert.AreEqual("NoSuchFieldOrProp", ex1_kvps["Type"]);
                Assert.AreEqual("[/f:MissingProp -> ctx.Companies.ElementAt(0).MissingProp]", ex1_kvps["Root"]);
                Assert.AreEqual("[/f:MissingProp -> ctx.Companies.ElementAt(0).MissingProp]", ex1_kvps["Expression"]);
                Assert.AreEqual("MemberAccess", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestNoSuchMethod()
        {
            try
            {
                var js = "ctx.Companies.ElementAt(0).MissingMethod('hello')";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("NoSuchMethod", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.ElementAt(0).MissingMethod('hello')", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(NoSuchMemberException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("<Relinq.Playground.Domain.Company>", ex1_kvps["InferredTypeOfTarget"]);
                Assert.AreEqual("MissingMethod", ex1_kvps["MemberName"]);
                Assert.AreEqual("NoSuchMethod", ex1_kvps["Type"]);
                Assert.AreEqual("[/() -> ctx.Companies.ElementAt(0).MissingMethod('hello')]", ex1_kvps["Root"]);
                Assert.AreEqual("[/()/tar:f:MissingMethod -> ctx.Companies.ElementAt(0).MissingMethod]", ex1_kvps["Expression"]);
                Assert.AreEqual("MemberAccess", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestNoSuchIndexer()
        {
            try
            {
                var js = "ctx.Companies.ElementAt(0).Employees.ElementAt(0)['no such indexer']";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("NoSuchIndexer", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.ElementAt(0).Employees.ElementAt(0)['no such indexer']", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(NoSuchIndexerException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("<Relinq.Playground.Domain.Employee>", ex1_kvps["InferredTypeOfTarget"]);
                Assert.AreEqual("NoSuchIndexer", ex1_kvps["Type"]);
                Assert.AreEqual("[/[] -> ctx.Companies.ElementAt(0).Employees.ElementAt(0)['no such indexer']]", ex1_kvps["Root"]);
                Assert.AreEqual("[/[] -> ctx.Companies.ElementAt(0).Employees.ElementAt(0)['no such indexer']]", ex1_kvps["Expression"]);
                Assert.AreEqual("Indexer", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestCannotBeInvoked()
        {
            try
            {
                var js = "ctx.Companies.Select(function(c){return c();})";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("FruitlessMethodGroupResolution", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.Select(function(c){return c();})", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(MethodGroupResolutionFailedException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("[MG Select: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector), System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector), System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector), System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)]", ex1_kvps["MethodGroup"]);
                Assert.AreEqual("System.Collections.Generic.Dictionary`2[System.Reflection.MethodInfo,Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodResolutionException]", ex1_kvps["Failboats"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`2[TSource, TResult]
    RootExpression: [/() -> ctx.Companies.Select(c => c())]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: dc3c3244-76b5-429b-94a1-502a11bffb52
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
      InferredSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`2[Playground.Domain.Company, TResult] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`2[Playground.Domain.Company, TResult]
      RootExpression: [/() -> ctx.Companies.Select(c => c())]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 1d87cae7-5c04-41ac-a6c1-6c4064f2917a
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.CannotBeInvokedException
        InferredTypeOfTarget: <Playground.Domain.Company>
        Type: CannotBeInvoked
        Root: [/() -> ctx.Companies.Select(c => c())]
        Expression: [/()/arg0:λ/() -> c()]
        ExpressionType: Invoke
        IsUnexpected: False
        Id: e0bdbb51-73e9-4ae2-b6a9-a71eab6ed736
", ex1_kvps["Failboats[0]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`3[TSource, System.Int32, TResult]
    RootExpression: [/() -> ctx.Companies.Select(c => c())]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 3d3b045e-7104-4dbd-8fda-57767d47cb57
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
      InferredSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`3[Playground.Domain.Company, System.Int32, TResult] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`3[Playground.Domain.Company, System.Int32, TResult]
      RootExpression: [/() -> ctx.Companies.Select(c => c())]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: bfd3c714-15e5-4d2f-922d-044d8454aab0
", ex1_kvps["Failboats[1]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]]
    RootExpression: [/() -> ctx.Companies.Select(c => c())]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: a07a219c-b476-4b61-a247-454a541f231c
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
      InferredSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, TResult]] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, TResult]]
      RootExpression: [/() -> ctx.Companies.Select(c => c())]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 188c367e-e142-401c-bef9-6d281b379073
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.CannotBeInvokedException
        InferredTypeOfTarget: <Playground.Domain.Company>
        Type: CannotBeInvoked
        Root: [/() -> ctx.Companies.Select(c => c())]
        Expression: [/()/arg0:λ/() -> c()]
        ExpressionType: Invoke
        IsUnexpected: False
        Id: 2071e9dc-f223-4f67-a28c-a9ccc55e99f9
", ex1_kvps["Failboats[2]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]]
    RootExpression: [/() -> ctx.Companies.Select(c => c())]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 22661f6e-b81d-4a38-9f85-12370c8cbd71
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
      InferredSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, TResult]] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, TResult]]
      RootExpression: [/() -> ctx.Companies.Select(c => c())]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 94bdd5a0-4bd2-45a3-9f93-a484c44eab6f
", ex1_kvps["Failboats[3]"]);
                Assert.AreEqual("FruitlessMethodGroupResolution", ex1_kvps["Type"]);
                Assert.AreEqual("[/() -> ctx.Companies.Select(c => c())]", ex1_kvps["Root"]);
                Assert.AreEqual("[/() -> ctx.Companies.Select(c => c())]", ex1_kvps["Expression"]);
                Assert.AreEqual("Invoke", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestConditionalTestInvalidType()
        {
            try
            {
                var js = "2 ? 'test has' : 'invalid type'";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("ConditionalTestInvalidType", ex0_kvps["Type"]);
                Assert.AreEqual("2 ? 'test has' : 'invalid type'", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(InconsistentConditionalExpression), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("<System.Int32>", ex1_kvps["InferredTypeOfTest"]);
                Assert.AreEqual("<unknown> ([/test/0:c:('test has', StringLiteral) -> 'test has'])", ex1_kvps["InferredTypeOfIfTrue"]);
                Assert.AreEqual("<unknown> ([/test/1:c:('invalid type', StringLiteral) -> 'invalid type'])", ex1_kvps["InferredTypeOfIfFalse"]);
                Assert.AreEqual("ConditionalTestInvalidType", ex1_kvps["Type"]);
                Assert.AreEqual("[/test -> (2 ? 'test has' : 'invalid type')]", ex1_kvps["Root"]);
                Assert.AreEqual("[/test -> (2 ? 'test has' : 'invalid type')]", ex1_kvps["Expression"]);
                Assert.AreEqual("Conditional", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestConditionalClausesNoCommonTypeWithOnlyOneClauseBeingCast()
        {
            try
            {
                var js = "ctx.Companies.Select(function(c){return true ? c.B : c.C;})";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("FruitlessMethodGroupResolution", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.Select(function(c){return true ? c.B : c.C;})", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(MethodGroupResolutionFailedException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("[MG Select: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector), System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector), System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector), System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)]", ex1_kvps["MethodGroup"]);
                Assert.AreEqual("System.Collections.Generic.Dictionary`2[System.Reflection.MethodInfo,Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodResolutionException]", ex1_kvps["Failboats"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`2[TSource, TResult]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: fa0502d0-a0c3-4008-b01b-e74454dda06b
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
      InferredSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`2[Playground.Domain.Company, TResult] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`2[Playground.Domain.Company, TResult]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: ee44e0f8-befd-4a03-8e79-d93aa4c5418c
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.InconsistentConditionalExpression
        InferredTypeOfTest: <System.Boolean>
        InferredTypeOfIfTrue: <Playground.Domain.TypeB>
        InferredTypeOfIfFalse: <Playground.Domain.TypeC>
        Type: ConditionalClausesNoCommonTypeWithOnlyOneClauseBeingCast
        Root: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
        Expression: [/()/arg0:λ/test -> ((true ? c.B : c.C))]
        ExpressionType: Conditional
        IsUnexpected: False
        Id: aebedb2a-9dbb-43a4-b93c-07f3b7b3b783
", ex1_kvps["Failboats[0]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`3[TSource, System.Int32, TResult]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: bd2334d6-aa1b-42f1-b203-39ef2a8f4a5b
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
      InferredSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`3[Playground.Domain.Company, System.Int32, TResult] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`3[Playground.Domain.Company, System.Int32, TResult]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: baab1f37-320f-42cd-b18b-feefebd08ea8
", ex1_kvps["Failboats[1]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 89b75da8-9a0d-4641-9485-eeacb3309c61
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
      InferredSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, TResult]] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, TResult]]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 954e8877-e595-497e-b74c-cba5611e76cc
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.InconsistentConditionalExpression
        InferredTypeOfTest: <System.Boolean>
        InferredTypeOfIfTrue: <Playground.Domain.TypeB>
        InferredTypeOfIfFalse: <Playground.Domain.TypeC>
        Type: ConditionalClausesNoCommonTypeWithOnlyOneClauseBeingCast
        Root: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
        Expression: [/()/arg0:λ/test -> ((true ? c.B : c.C))]
        ExpressionType: Conditional
        IsUnexpected: False
        Id: a70ad0b0-5039-4b3d-9bbd-b4b8e7f70718
", ex1_kvps["Failboats[2]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: cf96b20d-2e66-4c3e-9a51-1c10f0acfec3
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
      InferredSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, TResult]] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, TResult]]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 5e955270-36cc-41df-9305-c47cd125501d
", ex1_kvps["Failboats[3]"]);
                Assert.AreEqual("FruitlessMethodGroupResolution", ex1_kvps["Type"]);
                Assert.AreEqual("[/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]", ex1_kvps["Root"]);
                Assert.AreEqual("[/() -> ctx.Companies.Select(c => ((true ? c.B : c.C)))]", ex1_kvps["Expression"]);
                Assert.AreEqual("Invoke", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }

        [Test]
        public void TestConditionalClausesAmbiguousCommonType()
        {
            try
            {
                var js = "ctx.Companies.Select(function(c){return true ? c.E : c.F;})";
                TestTransformationFramework.JSToCSharp(js);
            }
            catch (Exception ex)
            {
                var ex0 = ex;
                Assert.AreEqual(typeof(JSToCSharpException), ex0.GetType());
                Assert.IsNotNull(ex0.InnerException);

                var ex0_kvps = ((RelinqException)ex0).PrettyProperties;
                Assert.AreEqual("FruitlessMethodGroupResolution", ex0_kvps["Type"]);
                Assert.AreEqual("ctx.Companies.Select(function(c){return true ? c.E : c.F;})", ex0_kvps["Input"]);
                Assert.AreEqual("False", ex0_kvps["IsUnexpected"]);

                var ex1 = ex0.InnerException;
                Assert.AreEqual(typeof(MethodGroupResolutionFailedException), ex1.GetType());
                Assert.IsNull(ex1.InnerException);

                var ex1_kvps = ((RelinqException)ex1).PrettyProperties;
                Assert.AreEqual("[MG Select: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector), System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector), System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector), System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)]", ex1_kvps["MethodGroup"]);
                Assert.AreEqual("System.Collections.Generic.Dictionary`2[System.Reflection.MethodInfo,Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodResolutionException]", ex1_kvps["Failboats"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`2[TSource, TResult]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 47795ad8-72c8-4db0-9b1e-1a1a07cfbebf
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`2[TSource, TResult] selector)
      InferredSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`2[Playground.Domain.Company, TResult] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`2[Playground.Domain.Company, TResult]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 22db5c61-d2e2-45e2-994c-a1fc158959e3
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.InconsistentConditionalExpression
        InferredTypeOfTest: <System.Boolean>
        InferredTypeOfIfTrue: <Playground.Domain.TypeE>
        InferredTypeOfIfFalse: <Playground.Domain.TypeF>
        Type: ConditionalClausesAmbiguousCommonType
        Root: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
        Expression: [/()/arg0:λ/test -> ((true ? c.E : c.F))]
        ExpressionType: Conditional
        IsUnexpected: False
        Id: 8589de49-ccee-4fe2-89e3-cd7a380f23f5
", ex1_kvps["Failboats[0]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Collections.Generic.IEnumerable`1[TSource]
    FormalArguments[1]: System.Func`3[TSource, System.Int32, TResult]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: e17faab6-cbec-4778-b179-0b5d3b0fd00b
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[TSource, TResult](System.Collections.Generic.IEnumerable`1[TSource] source, System.Func`3[TSource, System.Int32, TResult] selector)
      InferredSignature: System.Collections.Generic.IEnumerable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Collections.Generic.IEnumerable`1[Playground.Domain.Company] source, System.Func`3[Playground.Domain.Company, System.Int32, TResult] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Collections.Generic.IEnumerable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Func`3[Playground.Domain.Company, System.Int32, TResult]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: b63afb8a-7900-46b7-a4b5-fbf3d0a8955f
", ex1_kvps["Failboats[1]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 8a7f6355-4e7f-4772-b978-830621dce9e4
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaInconsistentBody
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`2[TSource, TResult]] selector)
      InferredSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, TResult]] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`2[Playground.Domain.Company, TResult]]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: 45bcd997-070f-4fe3-8ae1-a369140f591f
      InnerException: 
        ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.InconsistentConditionalExpression
        InferredTypeOfTest: <System.Boolean>
        InferredTypeOfIfTrue: <Playground.Domain.TypeE>
        InferredTypeOfIfFalse: <Playground.Domain.TypeF>
        Type: ConditionalClausesAmbiguousCommonType
        Root: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
        Expression: [/()/arg0:λ/test -> ((true ? c.E : c.F))]
        ExpressionType: Conditional
        IsUnexpected: False
        Id: aebf8581-dc02-4e90-9df2-4a2eae0802eb
", ex1_kvps["Failboats[2]"]);
                RelinqExceptionSupport.AssertAreEqualExceptGuids(@"
  Key: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
  Value: 
    ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.MethodMatchingException
    Type: GenericArgInferenceFailed
    IsUnexpected: False
    OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
    FormalArguments: System.Type[]
    FormalArguments[0]: System.Linq.IQueryable`1[TSource]
    FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]]
    RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
    ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
    ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
    ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
    Id: 68334151-97d4-4b9b-adc0-3e7e208118ee
    InnerException: 
      ExceptionType: Relinq.Exceptions.JSToCSharp.TypeInference.MethodResolution.GenericArgumentsInferenceException
      Type: LambdaArgcMismatch
      IsUnexpected: False
      OriginalSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[TSource, TResult](System.Linq.IQueryable`1[TSource] source, System.Linq.Expressions.Expression`1[System.Func`3[TSource, System.Int32, TResult]] selector)
      InferredSignature: System.Linq.IQueryable`1[TResult] [Extension] Select`2[Playground.Domain.Company, TResult](System.Linq.IQueryable`1[Playground.Domain.Company] source, System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, TResult]] selector)
      FormalArguments: System.Type[]
      FormalArguments[0]: System.Linq.IQueryable`1[Playground.Domain.Company]
      FormalArguments[1]: System.Linq.Expressions.Expression`1[System.Func`3[Playground.Domain.Company, System.Int32, TResult]]
      RootExpression: [/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]
      ActualArguments: Relinq.Script.Semantics.TypeSystem.RelinqScriptType[]
      ActualArguments[0]: <System.Linq.IQueryable`1[Playground.Domain.Company]>
      ActualArguments[1]: [λ: <System.Func`2[Relinq.Script.Semantics.TypeSystem.Variant, Relinq.Script.Semantics.TypeSystem.Variant]>]
      MismatchIndex: 1
      Id: d2008fe4-f15e-47d6-a638-7f2b62c0df2e
", ex1_kvps["Failboats[3]"]);
                Assert.AreEqual("FruitlessMethodGroupResolution", ex1_kvps["Type"]);
                Assert.AreEqual("[/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]", ex1_kvps["Root"]);
                Assert.AreEqual("[/() -> ctx.Companies.Select(c => ((true ? c.E : c.F)))]", ex1_kvps["Expression"]);
                Assert.AreEqual("Invoke", ex1_kvps["ExpressionType"]);
                Assert.AreEqual("False", ex1_kvps["IsUnexpected"]);

                return;
            }

            Assert.Fail("Expected exception wasn't thrown");
        }
    }
}