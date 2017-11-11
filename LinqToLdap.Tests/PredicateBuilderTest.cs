using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests
{
    [TestClass]
    public class PredicateBuilderTest
    {
        ExpressionTestClass _t1;
        ExpressionTestClass _t2;
        ExpressionTestClass _t3;

        [TestInitialize]
        public void Setup()
        {
            _t1 = new ExpressionTestClass
                      {
                          Stuff = Stuff.Enum1,
                          Number = 1,
                          Eval = true,
                          Test = "test",
                          Complex = new ExpressionTestPropertyClass
                                        {
                                            TestProperty = "testsub",
                                            Number = 1
                                        }
                      };

            _t2 = new ExpressionTestClass
                      {
                          Stuff = Stuff.Enum2,
                          Number = 2,
                          Eval = false,
                          Test = "test2",
                          Complex = new ExpressionTestPropertyClass
                                        {
                                            TestProperty = "testsub2",
                                            Number = 2
                                        }
                      };

            _t3 = new ExpressionTestClass
                      {
                          Stuff = Stuff.Enum1,
                          Number = 3,
                          Eval = true,
                          Test = "test3",
                          Complex = new ExpressionTestPropertyClass
                                        {
                                            TestProperty = "testsub3",
                                            Number = 3
                                        }
                      };

            _t1.EnumerableChildren = _t1.ListChildren = new List<ExpressionTestClass> {_t2, _t3};
            _t2.EnumerableChildren = _t2.ListChildren = new List<ExpressionTestClass> { _t1, _t3 };
            _t3.EnumerableChildren = _t3.ListChildren = new List<ExpressionTestClass> { _t1, _t2 };
        }

        [TestMethod]
        public void GetPropertyExpression_ReturnsPropertyExpression()
        {
            //prepare
            var expression = PredicateBuilder.GetPropertyExpression<ExpressionTestClass>("Number");

            //act
            var value = expression.Compile().Invoke(_t3);

            //assert
            value.Should().Be.EqualTo(3);
        }
        
        [TestMethod]
        public void Can_Get_Where_Equal_Expression()
        {
            //prepare
            var list = new List<ExpressionTestClass> {_t1, _t2};

            //act on string
            var function = PredicateBuilder.WhereEqual<ExpressionTestClass>("Test", "test").Compile();
            //assert
            list.Where(function).Should().Contain(_t1);
            list.Where(function).Should().Not.Contain(_t2);

            //act on boolean
            function = PredicateBuilder.WhereEqual<ExpressionTestClass>("Eval", "true").Compile();
            //assert 
            list.Where(function).Should().Contain(_t1);
            list.Where(function).Should().Not.Contain(_t2);

            //act on int
            function = PredicateBuilder.WhereEqual<ExpressionTestClass>("Number", "1").Compile();
            //assert 
            list.Where(function).Should().Contain(_t1);
            list.Where(function).Should().Not.Contain(_t2);

            //act on enum
            function = PredicateBuilder.WhereEqual<ExpressionTestClass>("Stuff", "Enum1").Compile();
            //assert 
            list.Where(function).Should().Contain(_t1);
            list.Where(function).Should().Not.Contain(_t2);

            //act on enum number value
            function = PredicateBuilder.WhereEqual<ExpressionTestClass>("Stuff", "2").Compile();
            //assert 
            list.Where(function).Should().Not.Contain(_t1);
            list.Where(function).Should().Contain(_t2);

            //act on sub property string
            function = PredicateBuilder.WhereEqual<ExpressionTestClass>("Complex.TestProperty", "testsub").Compile();
            //assert 
            list.Where(function).Should().Contain(_t1);
            list.Where(function).Should().Not.Contain(_t2);

            //act on sub property int
            function = PredicateBuilder.WhereEqual<ExpressionTestClass>("Complex.Number", "1").Compile();
            //assert 
            list.Where(function).Should().Contain(_t1);
            list.Where(function).Should().Not.Contain(_t2);
        }

        [TestMethod]
        public void Can_Get_Where_Not_Equal_Expression()
        {
            var list = new List<ExpressionTestClass> {_t1, _t2};
            Assert.IsTrue(!list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Test", "test").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Test", "test").Compile()).Contains(_t2));

            Assert.IsTrue(!list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Eval", "true").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Eval", "True").Compile()).Contains(_t2));

            Assert.IsTrue(!list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Number", "1").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Number", "1").Compile()).Contains(_t2));

            Assert.IsTrue(!list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Stuff", "Enum1").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Stuff", "Enum1").Compile()).Contains(_t2));

            Expression<Func<ExpressionTestClass, Boolean>> expression =
                PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Complex.TestProperty", "testsub");
            Assert.IsTrue(!list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t2));

            expression =
                PredicateBuilder.WhereNotEqual<ExpressionTestClass>("Complex.Number", "1");
            Assert.IsTrue(!list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t2));
        }

        [TestMethod]
        public void Can_Get_Where_Greater_Than_Expression()
        {
            var list = new List<ExpressionTestClass> {_t1, _t2};
            Assert.IsTrue(!list.Where(PredicateBuilder.WhereGreaterThan<ExpressionTestClass>("Number", "1").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereGreaterThan<ExpressionTestClass>("Number", 1).Compile()).Contains(_t2));

            Expression<Func<ExpressionTestClass, Boolean>> expression =
                PredicateBuilder.WhereGreaterThan<ExpressionTestClass>("Complex.Number", "1");
            Assert.IsTrue(!list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t2));
        }

        [TestMethod]
        public void Can_Get_Where_Greater_Than_Or_Equal_Expression()
        {
            var list = new List<ExpressionTestClass> {_t1, _t2};
            Assert.IsTrue(!list.Where(PredicateBuilder.WhereGreaterThanOrEqual<ExpressionTestClass>("Number", "2").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereGreaterThanOrEqual<ExpressionTestClass>("Number", 2).Compile()).Contains(_t2));

            Expression<Func<ExpressionTestClass, Boolean>> expression =
                PredicateBuilder.WhereGreaterThanOrEqual<ExpressionTestClass>("Complex.Number", "2");
            Assert.IsTrue(!list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t2));
        }

        [TestMethod]
        public void Can_Get_Where_Less_Than_Expression()
        {
            var list = new List<ExpressionTestClass> { _t1, _t2 };
            Assert.IsTrue(list.Where(PredicateBuilder.WhereLessThan<ExpressionTestClass>("Number", "2").Compile()).Contains(_t1));
            Assert.IsTrue(!list.Where(PredicateBuilder.WhereLessThan<ExpressionTestClass>("Number", 2).Compile()).Contains(_t2));

            Expression<Func<ExpressionTestClass, Boolean>> expression =
                PredicateBuilder.WhereLessThan<ExpressionTestClass>("Complex.Number", "2");
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(!list.Where(expression.Compile()).Contains(_t2));
        }

        [TestMethod]
        public void Can_Get_Where_Less_Than_Or_Equal_Expression()
        {
            var list = new List<ExpressionTestClass> { _t1, _t2 };
            Assert.IsTrue(list.Where(PredicateBuilder.WhereLessThanOrEqual<ExpressionTestClass>("Number", "1").Compile()).Contains(_t1));
            Assert.IsTrue(!list.Where(PredicateBuilder.WhereLessThanOrEqual<ExpressionTestClass>("Number", 1).Compile()).Contains(_t2));

            Expression<Func<ExpressionTestClass, Boolean>> expression =
                PredicateBuilder.WhereLessThanOrEqual<ExpressionTestClass>("Complex.Number", "1");
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(!list.Where(expression.Compile()).Contains(_t2));
        }

        [TestMethod]
        public void Can_Get_Where_Like_Expression()
        {
            var list = new List<ExpressionTestClass> {_t1, _t2};
            Assert.IsTrue(list.Where(PredicateBuilder.WhereLike<ExpressionTestClass>("Test", "test").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereLike<ExpressionTestClass>("Test", "test").Compile()).Contains(_t2));

            Assert.IsTrue(!list.Where(PredicateBuilder.WhereLike<ExpressionTestClass>("Test", "test2").Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(PredicateBuilder.WhereLike<ExpressionTestClass>("Test", "test2").Compile()).Contains(_t2));

            Expression<Func<ExpressionTestClass, Boolean>> expression =
                PredicateBuilder.WhereLike<ExpressionTestClass>("Complex.TestProperty", "testsub");
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t2));

            expression =
                PredicateBuilder.WhereLike<ExpressionTestClass>("Complex.TestProperty", "testsub2");
            Assert.IsTrue(!list.Where(expression.Compile()).Contains(_t1));
            Assert.IsTrue(list.Where(expression.Compile()).Contains(_t2));
        }

        [TestMethod]
        public void WhereContains_IEnumerable_CreatesExpression()
        {
            //prepare
            var list = new List<ExpressionTestClass> { _t1, _t2 };
            var propertyName = "EnumerableChildren";
            var predicate = PredicateBuilder.WhereContains<ExpressionTestClass>(propertyName, _t2).Compile();

            list.Where(predicate).Should().Contain(_t1).And.Not.Contain(_t2);
        }

        [TestMethod]
        public void WhereContains_IList_CreatesExpression()
        {
            //prepare
            var list = new List<ExpressionTestClass> { _t1, _t2 };
            var propertyName = "ListChildren";
            var predicate = PredicateBuilder.WhereContains<ExpressionTestClass>(propertyName, _t2).Compile();

            list.Where(predicate).Should().Contain(_t1).And.Not.Contain(_t2);
        }

        [TestMethod]
        public void WhereNotContains_IEnumerable_CreatesExpression()
        {
            //prepare
            var list = new List<ExpressionTestClass> { _t1, _t2 };
            var propertyName = "EnumerableChildren";
            var predicate = PredicateBuilder.WhereNotContains<ExpressionTestClass>(propertyName, _t2).Compile();

            list.Where(predicate).Should().Contain(_t2).And.Not.Contain(_t1);
        }

        [TestMethod]
        public void Test_Can_Chain_Or()
        {
            Expression<Func<ExpressionTestClass, bool>> expression = t => t.Stuff == Stuff.Enum1;

            var list = new List<ExpressionTestClass> { _t1, _t2, _t3 };

            var orList = list.Where(expression.Compile());
            orList.Should().Contain(_t1);
            orList.Should().Not.Contain(_t2);
            orList.Should().Contain(_t3);

            expression = expression.Or(t => t.Stuff == Stuff.Enum2);

            orList = list.Where(expression.Compile());
            orList.Should().Contain(_t1);
            orList.Should().Contain(_t2);
            orList.Should().Contain(_t3);
        }

        [TestMethod]
        public void Test_Can_Chain_And()
        {
            Expression<Func<ExpressionTestClass, bool>> expression = t => t.Test.Contains("test");

            var list = new List<ExpressionTestClass> { _t1, _t2, _t3 };

            var andList = list.Where(expression.Compile());
            andList.Should().Contain(_t1);
            andList.Should().Contain(_t2);
            andList.Should().Contain(_t3);

            expression = expression.And(t => t.Number == 1);

            andList = list.Where(expression.Compile());
            andList.Should().Contain(_t1);
            andList.Should().Not.Contain(_t2);
            andList.Should().Not.Contain(_t3);
        }
    }

    public enum Stuff { Enum1 = 1, Enum2 = 2 }

    public class ExpressionTestClass
    {
        public String Test
        {
            get;
            set;
        }

        public Boolean Eval { get; set; }

        public Int32 Number { get; set; }

        public Stuff Stuff { get; set; }

        public ExpressionTestPropertyClass Complex { get; set; }

        public IEnumerable<ExpressionTestClass> EnumerableChildren { get; set; }

        public IList<ExpressionTestClass> ListChildren { get; set; }

        public string GetterOnly { get { return ""; } }
    }

    public class ExpressionTestPropertyClass
    {
        public String TestProperty
        {
            get;
            set;
        }

        public Int32 Number { get; set; }
    }
}
