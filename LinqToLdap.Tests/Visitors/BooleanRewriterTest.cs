using System;
using System.Collections.Generic;
using LinqToLdap.Tests.TestSupport;
using LinqToLdap.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Linq;

namespace LinqToLdap.Tests.Visitors
{
    [TestClass]
    public class BooleanRewriterTest
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
        public void Rewrite_NullableWithHasValueAndPropertyEquality_RewritesHasValue()
        {
            //prepare
            DateTime? now = DateTime.Now;
            var expression1 = _queryContext.Query<BooleanRewriterTest>().Where(e => now.HasValue && e.Property1 == now.Value).Expression;

            var expression2 = _queryContext.Query<BooleanRewriterTest>().Where(e => true && e.Property1 == now.Value).Expression;

            //act
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            //assert
            expression2.ToString().Should().Be(rewritten.ToString());
        }

        [TestMethod]
        [Ignore]
        public void Rewrite_Conditional_RewritesCorrectly()
        {
            //prepare
            DateTime? now = DateTime.Now;
            _queryContext.Query<BooleanRewriterTest>().Where(e => "test".Equals("test") ? e.Property2 == "test" : e.Property1 == now.Value);
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanRewriterTest>().Where(e => e.Property2 == "test");
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            //assert
            expression2.ToString().Should().Be(rewritten.ToString());
        }

        [TestMethod]
        public void Rewrite_NullableWithHasValueAndPropertyEqualityAndBoolCast_RewritesHasValue()
        {
            //prepare
            DateTime? now = DateTime.Now;
            _queryContext.Query<BooleanRewriterTest>().Where(e => (e.Property2 == "test") || (e.Property1 == now.Value && now.HasValue is bool && (bool)(now.HasValue as object) == true));
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanRewriterTest>().Where(e => e.Property2 == "test" || (e.Property1 == now.Value && true && true));
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            //assert
            expression2.ToString().Should().Be(rewritten.ToString());
        }

        [TestMethod]
        public void Rewrite_NullableWithHasValueAndPropertyEqualityAndMultipleWhere_RewritesHasValue()
        {
            //prepare
            DateTime? now = DateTime.Now;
            _queryContext.Query<BooleanRewriterTest>().Where(e => now.HasValue && e.Property1 == now.Value).Where(e => now.HasValue && e.Property1 == now.Value);
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanRewriterTest>().Where(e => true && e.Property1 == now.Value).Where(e => true && e.Property1 == now.Value);
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            //assert
            expression2.ToString().Should().Be(rewritten.ToString());
        }

        [TestMethod]
        public void Rewrite_HasValueAndMethodAndPropertyEquality_RewritesHasValueAndMethodToSingleTrue()
        {
            //prepare
            DateTime? now = DateTime.Now;
            _queryContext.Query<BooleanRewriterTest>().Where(e => now.HasValue && e.Property1 == now.Value && !"true".Equals("true"));
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanRewriterTest>().Where(e => true && e.Property1 == now.Value && false);
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;
            
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            expression2.ToString().Should().Be(rewritten.ToString());
        }
        
        [TestMethod]
        public void Rewrite_HasValueAndMethodWithParameterAndPropertyEquality_RewritesHasValueAndMethod()
        {
            //prepare
            DateTime? now = DateTime.Now;
            var list = new [] {"test"};

            _queryContext.Query<BooleanRewriterTest>().Where(e => list.Length > 0 && list.Contains(e.Property2) && now.HasValue && e.Property1 == now.Value);
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanRewriterTest>().Where(e => true && list.Contains(e.Property2) && true && e.Property1 == now.Value);
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            //assert
            expression2.ToString().Should().Be(rewritten.ToString());
        }

        [TestMethod]
        public void Rewrite_HasValueAndMethodWithParameterAndPropertyEqualityAndNotUnary_RewritesHasValue()
        {
            //prepare
            DateTime? now = DateTime.Now;
            var list = new List<string> { "test" };

            _queryContext.Query<BooleanRewriterTest>().Where(e => list.Contains(e.Property2) && now.HasValue && !e.Property3 && e.Property1 == now.Value);
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanRewriterTest>().Where(e => list.Contains(e.Property2) && true && !e.Property3 && e.Property1 == now.Value);
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            //assert
            expression2.ToString().Should().Be(rewritten.ToString());
        }

        [TestMethod]
        public void Rewrite_HasValueAndCollectionContainsWithOrCondition_RewritesHasValue()
        {
            //prepare
            DateTime? now = DateTime.Now;
            var list = new List<string> { "test" };

            _queryContext.Query<BooleanRewriterTest>().Where(e => (list.Contains(e.Property2) && !e.Property3 && e.Property1 == now.Value) || now.HasValue);
            var expression1 = _queryContext.ActiveProvider.CurrentExpression;

            _queryContext.Query<BooleanRewriterTest>().Where(e => (list.Contains(e.Property2) && !e.Property3 && e.Property1 == now.Value) || true);
            var expression2 = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var rewritten = new BooleanRewriter().Rewrite(expression1);

            //assert
            expression2.ToString().Should().Be(rewritten.ToString());
        }
    }
}
