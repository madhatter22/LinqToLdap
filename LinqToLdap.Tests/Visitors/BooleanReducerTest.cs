using System;
using System.Linq.Expressions;
using LinqToLdap.Tests.TestSupport;
using LinqToLdap.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using System.Linq;

namespace LinqToLdap.Tests.Visitors
{
    [TestClass]
    public class BooleanReducerTest
    {
        public DateTime? Property1 { get; set; }
        public string Property2 { get; set; }
        public bool Property3 { get; set; }
        public bool? Property4 { get; set; }

        private MockQueryContext _queryContext;

        [TestInitialize]
        public void SetUp()
        {
            _queryContext = new MockQueryContext();
        }

        [TestMethod]
        public void Reduce_Duplicate_True_RemovesDuplicates()
        {
            //prepare
            _queryContext.Query<BooleanReducerTest>().Where(e => true && true);
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanReducerTest>().Where(e => true);
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanReducer().Reduce(expression1);

            //assert
            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_BoolAtFrontAndEnd_RemovesDuplicates()
        {
            //prepare
            _queryContext.Query<BooleanReducerTest>().Where(e => true && true && e.Property1 == DateTime.Now && true).Select(e => new { Prop2 = e.Property3 ? "test" : "go" });
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanReducerTest>().Where(e => e.Property1 == DateTime.Now).Select(e => new { Prop2 = e.Property3 ? "test" : "go" });
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanReducer().Reduce(expression1);

            //assert
            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_BoolAtEnd_RemovesDuplicates()
        {
            //prepare
            _queryContext.Query<BooleanReducerTest>().Where(e => e.Property1 == DateTime.Now && true && true).Select(e => new { Prop2 = e.Property3 ? "test" : "go" });
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanReducerTest>().Where(e => e.Property1 == DateTime.Now).Select(e => new { Prop2 = e.Property3 ? "test" : "go" });
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanReducer().Reduce(expression1);

            //assert
            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_MultipleTrue_ReturnsTrue()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true && true && true && true;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => true;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_MultipleOrElseTrueAtFront_ReturnsTrue()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true || false || false || true || true;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => true;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_MultipleOrElseFalseAtFront_ReturnsTrue()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => false || false || false || false || true;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => true;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_MultipleAndAlsoTrueAndNestedAndAlso_ReturnsTrue()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true && true && (true && true);
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => true;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_MultipleAndAlsoTrueAndNestedFalseAndAlso_ReturnsFalse()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true && true && (false && true);
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => false;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_MultipleAndAlsoTrueAndMultipleAndAlsoFalse_ReturnsFalse()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true && true && false && false && true && true && false;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => false;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_MultipleAndAlsoFalse_ReturnsFalse()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => false && false && false && false;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => false;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_BoolAtFrontAndEndWithFalseInMiddle_ReturnsFalse()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true && e.Property1 == DateTime.Now && false && true;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => false;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_OrElseWithLeftTrue_ReturnsTrue()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true || e.Property1 == DateTime.Now;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => true;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_OrElseWithTrueAtEnd_RemovesRedundantTrue()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => e.Property1 == DateTime.Now || true;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => true;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_ComplexAndAlsoWithOrElseAtEnd_ReturnsCorrectExpression()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => true && e.Property1 == DateTime.Now && (true || e.Property1 == DateTime.Now);
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => e.Property1 == DateTime.Now;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_ComplexAndAlsoWithOrElseAtFront_ReturnsCorrectExpression()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => (true || e.Property1 == DateTime.Now) && true && e.Property1 == DateTime.Now;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => e.Property1 == DateTime.Now;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }

        [TestMethod]
        public void Reduce_ComplexAndAlsoWithIrreducableOrElseAtFront_ReturnsCorrectExpression()
        {
            Expression<Func<BooleanReducerTest, bool>> expression1 = e => (false || e.Property1 == DateTime.Now) && true && e.Property1 == DateTime.Now;
            Expression<Func<BooleanReducerTest, bool>> expression2 = e => e.Property1 == DateTime.Now && e.Property1 == DateTime.Now;

            var rewritten = new BooleanReducer().Reduce(expression1);

            expression2.ToString().Should().Be.EqualTo(rewritten.ToString());
        }
    }
}
