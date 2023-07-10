using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using LinqToLdap.Collections;
using LinqToLdap.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;

namespace LinqToLdap.Tests.TestSupport
{
    [TestClass]
    public class MockQueryTest
    {
        [TestMethod]
        public void ToList_Query_Returns_Result()
        {
            //arrange
            var list = new List<string>();
            var context = new Mock<IDirectoryContext>();
            var query = new MockQuery<IDirectoryAttributes>(new List<object> { list });
            context.Setup(x => x.Query("test", SearchScope.Subtree, null, null, null))
                .Returns(query);

            //act
            var result = context.Object.Query("test")
                .Where(x => Filter.Equal(x, "x", "y", false))
                .Select(x => x.GetString("whatever"))
                .ToList();

            //assert
            query.MockProvider.ExecutedExpressions.Should().HaveCount(1);
            result.Should().BeSameAs(list);
        }

        [TestMethod]
        public void MultipleExecutions_Query_Returns_Result()
        {
            //arrange
            var array = new[] { "one" };
            var item = "two";
            var context = new Mock<IDirectoryContext>();
            var query = new MockQuery<IDirectoryAttributes>(new List<object> { array, item });
            context.Setup(x => x.Query("test", SearchScope.Subtree, null, null, null))
                .Returns(query);

            //act
            var result1 = context.Object.Query("test")
                .Where(x => Filter.Equal(x, "x", "y", false))
                .Select(x => x.GetString("whatever"))
                .ToArray();

            var result2 = context.Object.Query("test")
                .Select(x => x.GetString("whatever"))
                .FirstOrDefault();

            query.MockProvider.ExecutedExpressions.Should().HaveCount(2);
            query.MockProvider.ExecutedExpressions[0].ToString()
                .Should().Contain("Where")
                .And.NotContain("FirstOrDefault");
            query.MockProvider.ExecutedExpressions[1].ToString()
                .Should().Contain("FirstOrDefault")
                .And.NotContain("Where");
            result1.Should().ContainInOrder(array);
            result2.Should().Be(item);
        }

        [TestMethod]
        public void PredicateBuilder_Query_Returns_Result()
        {
            //arrange
            var array = new[] { "one" };
            var context = new Mock<IDirectoryContext>();
            var query = new MockQuery<IDirectoryAttributes>(new List<object> { array });
            context.Setup(x => x.Query("test", SearchScope.Subtree, null, null, null))
                .Returns(query);
            var expression = PredicateBuilder.Create<IDirectoryAttributes>()
                .And(x => Filter.Equal(x, "x", "y", false))
                .Or(x => Filter.Equal(x, "a", "b", true));

            //act
            var result = context.Object.Query("test")
                .Where(expression)
                .Select(x => x.GetString("whatever"))
                .ToArray();

            query.MockProvider.ExecutedExpressions.Should().HaveCount(1);
            query.MockProvider.ExecutedExpressions[0].ToString()
                .Should().Contain("Equal(x, \"x\", \"y\", False)")
                .And.Contain("OrElse")
                .And.Contain("Equal(x, \"a\", \"b\", True)")
                .And.Contain("x => x.GetString(\"whatever\")");

            result.Should().ContainInOrder(array);
        }
    }
}