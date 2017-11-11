using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToLdap.Tests.Visitors
{
    [TestClass]
    public class NullableEvaluatorTest
    {
        public int Property1 { get; set; }
        public string Property2 { get; set; }

        [Test]
        public void Evaluate_Something()
        {
            var evaluator = new LocalEvaluator();
            int? id = 1;
            int? id2 = null;
            Expression<Func<NullableEvaluatorTest, bool>> expresion = e => id.HasValue && !"test".Equals("test") || id2.HasValue && e.Property1 == id.Value;
            var expression = evaluator.Visit(expresion, typeof (NullableEvaluatorTest));
        }
    }
}
