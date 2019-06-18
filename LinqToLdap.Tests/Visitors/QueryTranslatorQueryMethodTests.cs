using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands;
using LinqToLdap.QueryCommands.Options;
using LinqToLdap.Tests.TestSupport;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using LinqToLdap.Visitors;
using Moq;
using SharpTestsEx;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToLdap.Tests.Visitors
{
    [TestClass]
    public class QueryTranslatorQueryMethodTests
    {
        private Mock<IObjectMapping> _mapping;
        private MockQueryCommandFactory _mockFacotry;
        private Mock<IQueryCommand> _command;
        private MockQueryContext _queryContext;
        private QueryTranslator _translator;
        private Mock<IPropertyMapping> _property1Mapping;
        private Mock<IPropertyMapping> _property2Mapping;
        private Mock<IPropertyMapping> _property3Mapping;

        [TestInitialize]
        public void SetUp()
        {
            _mockFacotry = new MockQueryCommandFactory();
            _command = new Mock<IQueryCommand>();
            _queryContext = new MockQueryContext();
            _mapping = new Mock<IObjectMapping>();
            _property1Mapping = new Mock<IPropertyMapping>();
            _property2Mapping = new Mock<IPropertyMapping>();
            _property3Mapping = new Mock<IPropertyMapping>();

            _property1Mapping.Setup(p => p.AttributeName)
                .Returns("x");
            _property1Mapping.Setup(p => p.PropertyName)
                .Returns("Property1");
            _property1Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<string>()))
                .Returns((string s) => s);
            _property2Mapping.Setup(p => p.AttributeName)
                .Returns("y");
            _property2Mapping.Setup(p => p.PropertyName)
                .Returns("Property2");
            _property2Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<string>()))
                .Returns((string s) => s);
            _property3Mapping.Setup(p => p.AttributeName)
                .Returns("z");
            _property3Mapping.Setup(p => p.PropertyName)
                .Returns("Property3");
            _property3Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<string>()))
                .Returns((string s) => s);

            _mapping.Setup(m => m.GetPropertyMapping("Property1", null))
                .Returns(_property1Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property2", null))
                .Returns(_property2Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property3", null))
                .Returns(_property3Mapping.Object);

            var dictionary = new Dictionary<string, string>
                                 {
                                     {"Property1", "x"},
                                     {"Property2", "y"},
                                     {"Property3", "z"}
                                 }.ToReadOnlyDictionary();
            _mapping.Setup(m => m.Properties)
                .Returns(dictionary);

            _translator = new QueryTranslator(_mapping.Object);
            _translator.SetFieldValue<IQueryCommandFactory>("_factory", _mockFacotry);
            _mockFacotry.QueryCommandToReturn = _command.Object;
        }

        [TestCleanup]
        public void TearDown()
        {
#if NET35
            Console.WriteLine(_mockFacotry.Filter + Environment.NewLine +
                ((_mockFacotry.Options != null && _mockFacotry.Options.AttributesToLoad != null) ? "Attributes: " + string.Join(", ", _mockFacotry.Options.AttributesToLoad.Keys.ToArray()) : ""));
#else
            Console.WriteLine(_mockFacotry.Filter + Environment.NewLine +
                ((_mockFacotry.Options != null && _mockFacotry.Options.AttributesToLoad != null) ? "Attributes: " + string.Join(", ", _mockFacotry.Options.AttributesToLoad.Keys) : ""));
#endif
        }

        [TestMethod]
        public void Translate_LocalFalse_ReturnsCorrectParameters()
        {
            //prepare
            DateTime? now = null;
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => now.HasValue && t.Property4 == now.Value);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Options.YieldNoResults.Should().Be.True();
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_OnlyTrue_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => true).IgnoreOC();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Filter.Should().Be.NullOrEmpty();
            _mockFacotry.Options.YieldNoResults.Should().Be.False();
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_SelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Select(t => t.Property1);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_SelectClauseStringConcatenation_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Select(t => new { Name = string.Format("{0} {1}", t.Property1, t.Property2) });
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.AttributesToLoad.Values.Should().Have.SameSequenceAs(new[] { "x", "y" });
        }

        [TestMethod]
        public void Translate_WhereAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Select(t => t.Property1);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_MultipleWhereClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Where(t => t.Property2 == "b");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_AnyClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Any(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.AnyCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndAnyClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Any(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.AnyCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_FirstClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().First(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.FirstCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndFirstClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").First(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.FirstCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndFirstAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Select(t => t)
                .First(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.FirstCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_FirstOrDefaultClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().FirstOrDefault(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.FirstOrDefaultCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndFirstOrDefaultClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").First(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.FirstCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndFirstOrDefaultAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Select(t => t)
                .First(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.FirstCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereWithSelectCaluseAndGetRequest_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a" && t.Property2 == "b").Select(t => t.Property1).GetRequest();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.GetRequestCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_SingleOrDefaultClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().SingleOrDefault(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.SingleOrDefaultCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndSingleOrDefaultClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").SingleOrDefault(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.SingleOrDefaultCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndSingleOrDefaultAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Select(t => t)
                .SingleOrDefault(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.SingleOrDefaultCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_SingleClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Single(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.SingleCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndSingleClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Single(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.SingleCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndSingleAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Select(t => t)
                .Single(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.SingleCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_ListAttributesClauseWithoutCustomAttributes_ReturnsCorrectParameters()
        {
            //prepare
#pragma warning disable 612,618
            _queryContext.Query<QueryTranslatorTestClass>().ListAttributes();
#pragma warning restore 612,618
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Filter.Should().Be.Null();
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ListAttributesQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_ListAttributesClauseWithCustomAttributes_ReturnsCorrectParameters()
        {
            //prepare
            var attributes = new Dictionary<string, string> { { "one", "one" }, { "two", "two" } };
#pragma warning disable 612,618
            _queryContext.Query<QueryTranslatorTestClass>().ListAttributes(attributes.Keys.ToArray());
#pragma warning restore 612,618
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Filter.Should().Be.Null();
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ListAttributesQueryCommandOptions>();
            _mockFacotry.Options.AttributesToLoad.Should().Have.SameSequenceAs(attributes);
        }

        [TestMethod]
        public void Translate_WhereAndListAttributesClause_ReturnsCorrectParameters()
        {
            //prepare
            _mockFacotry.QueryCommandToReturn = _command.Object;
#pragma warning disable 612,618
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").ListAttributes();
#pragma warning restore 612,618
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ListAttributesQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndListAttributesAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(t => t.Property1 == "a")
                .Select(t => t)
#pragma warning disable 612,618
                .ListAttributes();
#pragma warning restore 612,618
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ListAttributesQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_CountClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Count(t => t.Property1 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.CountCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndCountClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Count(t => t.Property2 == "b");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.CountCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(x=a)(y=b))");
        }

        [TestMethod]
        public void Translate_SelectClause_With_Cast_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Select(t => new { ((QueryTranslatorTestClass)t).Property1, (t as QueryTranslatorTestClass).Property2 });
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_WhereAndCountAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").Select(t => t)
                .Count();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.CountCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
        }

        [TestMethod]
        public void Translate_FilterWithAndSelectClauseWithoutParens_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().FilterWith("some filter")
                .Select(t => t);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(some filter)");
        }

        [TestMethod]
        public void Translate_MultipleFilterWithAndSelectClauseWithoutParens_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().FilterWith("some filter")
                .FilterWith("another filter")
                .Select(t => t);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(some filter)(another filter))");
        }

        [TestMethod]
        public void Translate_FilterWithAndSelectClauseWithParens_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().FilterWith("(some filter)")
                .Select(t => t);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(some filter)");
        }

        [TestMethod]
        public void Translate_WhereAndFilterWithAndSelectClause_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "a").FilterWith("some filter")
                .Select(t => t);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(some filter)(x=a))");
        }

        [TestMethod]
        public void Translate_WhereAndFilterWithAndFilterWithObjectCategoryAndSelectClauseWithObjectCategory_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(t => t.Property1 == "a")
                .FilterWith("some filter")
                .Select(t => t);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(some filter)(&(objectCategory=oc)(x=a)))");
        }

        [TestMethod]
        public void Translate_WhereAndFirstAndFilterWithAndSelectClauseWithObjectCategory_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);

            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(t => t.Property1 == "a")
                .FilterWith("some filter")
                .Select(t => t)
                .First(t => t.Property2 == "a");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.FirstCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(some filter)(&(objectCategory=oc)(x=a)(y=a)))");
        }

        [TestMethod]
        public void Translate_WhereAndIgnroeOCWithObjectCategory_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");

            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(t => t.Property1 == "a")
                .IgnoreOC();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(x=a)");
        }

        [TestMethod]
        public void Translate_WhereAndIgnroeOCWithObjectClass_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new List<string> { "oc" });

            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(t => t.Property1 == "a")
                .IgnoreOC();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(x=a)");
        }

        [TestMethod]
        public void Translate_FliterWithObjectCateogry_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(objectCategory=oc)");
        }

        [TestMethod]
        public void Translate_FliterWithObjectClass_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new List<string> { "cl", "c2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);

            _queryContext.Query<QueryTranslatorTestClass>();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(objectClass=cl)(objectClass=c2))");
        }

        [TestMethod]
        public void Translate_WhereAndFilterWithObjectCategory_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(t => t.Property1 == "some");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(objectCategory=oc)(x=some))");
        }

        [TestMethod]
        public void Translate_WhereAndFliterWithObjectClass_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new List<string> { "cl", "c2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);

            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(t => t.Property1 == "some");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(&(objectClass=cl)(objectClass=c2)(x=some))");
        }

        [TestMethod]
        public void Translate_OrderByDynamic_DirectoryAttributes_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query()
                .OrderBy(x => x.Get("test"))
                .ThenBy("other", "rule");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.SortingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.SortingOptions.Keys.Should().Have.Count.EqualTo(2);
            _mockFacotry.Options.SortingOptions.Keys[0].ReverseOrder.Should().Be.False();
            _mockFacotry.Options.SortingOptions.Keys[0].AttributeName.Should().Be.EqualTo("test");
            _mockFacotry.Options.SortingOptions.Keys[0].MatchingRule.Should().Be.Null();
            _mockFacotry.Options.SortingOptions.Keys[1].ReverseOrder.Should().Be.False();
            _mockFacotry.Options.SortingOptions.Keys[1].AttributeName.Should().Be.EqualTo("other");
            _mockFacotry.Options.SortingOptions.Keys[1].MatchingRule.Should().Be.EqualTo("rule");
        }

        [TestMethod]
        public void Translate_OrderByDynamic_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query()
                .OrderBy("test")
                .ThenBy("other", "rule");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.SortingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.SortingOptions.Keys.Should().Have.Count.EqualTo(2);
            _mockFacotry.Options.SortingOptions.Keys[0].ReverseOrder.Should().Be.False();
            _mockFacotry.Options.SortingOptions.Keys[0].AttributeName.Should().Be.EqualTo("test");
            _mockFacotry.Options.SortingOptions.Keys[0].MatchingRule.Should().Be.Null();
            _mockFacotry.Options.SortingOptions.Keys[1].ReverseOrder.Should().Be.False();
            _mockFacotry.Options.SortingOptions.Keys[1].AttributeName.Should().Be.EqualTo("other");
            _mockFacotry.Options.SortingOptions.Keys[1].MatchingRule.Should().Be.EqualTo("rule");
        }

        [TestMethod]
        public void Translate_OrderBy_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .OrderBy(t => t.Property1);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.SortingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.SortingOptions.Keys.Should().Have.Count.EqualTo(1);
            _mockFacotry.Options.SortingOptions.Keys[0].ReverseOrder.Should().Be.False();
            _mockFacotry.Options.SortingOptions.Keys[0].AttributeName.Should().Be.EqualTo("x");
            _mockFacotry.Options.SortingOptions.Keys[0].MatchingRule.Should().Be.Null();
        }

        [TestMethod]
        public void Translate_OrderByWithMatchingRule_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .OrderBy(t => t.Property1, "mr");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.SortingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.SortingOptions.Keys.Should().Have.Count.EqualTo(1);
            _mockFacotry.Options.SortingOptions.Keys[0].ReverseOrder.Should().Be.False();
            _mockFacotry.Options.SortingOptions.Keys[0].AttributeName.Should().Be.EqualTo("x");
            _mockFacotry.Options.SortingOptions.Keys[0].MatchingRule.Should().Be.EqualTo("mr");
        }

        [TestMethod]
        public void Translate_OrderByDescendingDynamic_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query()
                .OrderByDescending("test")
                .ThenByDescending("other", "rule");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.SortingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.SortingOptions.Keys.Should().Have.Count.EqualTo(2);
            _mockFacotry.Options.SortingOptions.Keys[0].ReverseOrder.Should().Be.True();
            _mockFacotry.Options.SortingOptions.Keys[0].AttributeName.Should().Be.EqualTo("test");
            _mockFacotry.Options.SortingOptions.Keys[0].MatchingRule.Should().Be.Null();
            _mockFacotry.Options.SortingOptions.Keys[1].ReverseOrder.Should().Be.True();
            _mockFacotry.Options.SortingOptions.Keys[1].AttributeName.Should().Be.EqualTo("other");
            _mockFacotry.Options.SortingOptions.Keys[1].MatchingRule.Should().Be.EqualTo("rule");
        }

        [TestMethod]
        public void Translate_OrderByDescending_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .OrderByDescending(t => t.Property1);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.SortingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.SortingOptions.Keys.Should().Have.Count.EqualTo(1);
            _mockFacotry.Options.SortingOptions.Keys[0].ReverseOrder.Should().Be.True();
            _mockFacotry.Options.SortingOptions.Keys[0].AttributeName.Should().Be.EqualTo("x");
            _mockFacotry.Options.SortingOptions.Keys[0].MatchingRule.Should().Be.Null();
        }

        [TestMethod]
        public void Translate_OrderByDescendingWithMatchingRule_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .OrderByDescending(t => t.Property1, "mr");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.SortingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.SortingOptions.Keys.Should().Have.Count.EqualTo(1);
            _mockFacotry.Options.SortingOptions.Keys[0].ReverseOrder.Should().Be.True();
            _mockFacotry.Options.SortingOptions.Keys[0].AttributeName.Should().Be.EqualTo("x");
            _mockFacotry.Options.SortingOptions.Keys[0].MatchingRule.Should().Be.EqualTo("mr");
        }

        [TestMethod]
        public void Translate_Take_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Take(10);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.Null();
            _mockFacotry.Options.PagingOptions.Should().Be.Null();
            _mockFacotry.Options.TakeSize.Should().Be.EqualTo(10);
            _mockFacotry.Options.PageSize.Should().Not.Have.Value();
        }

        [TestMethod]
        public void Translate_InPagesOf_ReturnsCorrectParameters()
        {
            //prepare

            _queryContext.Query<QueryTranslatorTestClass>().InPagesOf(2);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.PagingOptions.Should().Be.Null();
            _mockFacotry.Options.PageSize.Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void Translate_TakeAndInPagesOf_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Take(10)
                .InPagesOf(5);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.PagingOptions.Should().Be.Null();
            _mockFacotry.Options.TakeSize.Should().Be.EqualTo(10);
            _mockFacotry.Options.PageSize.Should().Be.EqualTo(5);
        }

        [TestMethod]
        public void Translate_ToList_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .ToList();
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.PagingOptions.Should().Be.Null();
            _mockFacotry.Options.TakeSize.Should().Not.Have.Value();
            _mockFacotry.Options.PageSize.Should().Not.Have.Value();
        }

        [TestMethod]
        public void Translate_ToPageWithPageSize_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .ToPage(10);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(10);
        }

        [TestMethod]
        public void Translate_ToPageWithPageSizeAndNextPage_ReturnsCorrectParameters()
        {
            //prepare
            var cookie = new byte[] { 1, 2, 3, 4 };
            _queryContext.Query<QueryTranslatorTestClass>()
                .ToPage(10, cookie);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo(null);
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(10);
            _mockFacotry.Options.PagingOptions.NextPage.Should().Have.SameSequenceAs(cookie);
        }

        [TestMethod]
        public void Translate_ToPageWithPageSizeAndNextPageAndFilter_ReturnsCorrectParameters()
        {
            //prepare
            var cookie = new byte[] { 1, 2, 3, 4 };
            _queryContext.Query<QueryTranslatorTestClass>()
                .ToPage(10, cookie, "some filter");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("some filter");
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(10);
            _mockFacotry.Options.PagingOptions.NextPage.Should().Have.SameSequenceAs(cookie);
        }

        [TestMethod]
        public void Translate_WhereAndSelectAndToPageWithPageSize_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(u => u.Property1 == "p")
                .Select(u => u.Property1)
                .ToPage(100);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<ProjectionQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Not.Be.NullOrEmpty();
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(100);
        }

        [TestMethod]
        public void Translate_WhereAndToPage_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(u => u.Property1 == "p")
                .ToPage(100);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Not.Be.NullOrEmpty();
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(100);
        }

        [TestMethod]
        public void Translate_WhereAndOcAndToPage_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(u => u.Property1 == "p")
                .ToPage(100);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Not.Be.NullOrEmpty();
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(100);
        }

        [TestMethod]
        public void Translate_WhereAndCustomFilterAndOcAndToPage_ReturnsCorrectParameters()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(u => u.Property1 == "p")
                .Where("a=b")
                .ToPage(100);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Not.Be.NullOrEmpty();
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(100);
        }

        [TestMethod]
        public void Translate_WhereAndCustomFilterAndToPage_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Where(u => u.Property1 == "p")
                .Where("a=b")
                .ToPage(100);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Not.Be.NullOrEmpty();
            _mockFacotry.Options.PagingOptions.Should().Not.Be.Null();
            _mockFacotry.Options.PagingOptions.PageSize.Should().Be.EqualTo(100);
        }

        [TestMethod]
        public void Translate_DynamicQueryWithSelect_ReturnsCorrectParameters()
        {
            //prepare
            var strings = new[] { "one", "two" };
            _queryContext.Query<IDirectoryAttributes>()
                .Select(strings);
            var expression = _queryContext.ActiveProvider.CurrentExpression;
            _translator.IsDynamic = true;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<DynamicQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.NullOrEmpty();
            _mockFacotry.Options.AttributesToLoad.Should().Have.SameSequenceAs(strings.ToDictionary(s => s));
        }

        [TestMethod]
        public void Translate_DynamicQueryFilterWithAndSelect_ReturnsCorrectParameters()
        {
            //prepare
            var strings = new[] { "one", "two" };
            _queryContext.Query<IDirectoryAttributes>()
                .FilterWith("some filter")
                .Select(strings);
            var expression = _queryContext.ActiveProvider.CurrentExpression;
            _translator.IsDynamic = true;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<DynamicQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(some filter)");
            _mockFacotry.Options.AttributesToLoad.Should().Have.SameSequenceAs(strings.ToDictionary(s => s));
        }

        [TestMethod]
        public void Translate_DynamicQueryFilterWithAndLambdaSelect_ReturnsCorrectParameters()
        {
            //prepare
            var strings = new[] { "one", "two", "three", "four", "DistinguishedName" };
            _queryContext.Query<IDirectoryAttributes>()
                         .FilterWith("some filter")
                         .Select(
                             da => new
                             {
                                 What = da.GetBoolean("one"),
                                 Huh = da.GetString("two").Length.CompareTo("test"),
                                 Yeah = da.GetStrings("three").ToArray(),
                                 Ok = da.Get("four"),
                                 Ignore = da.Set("ignore", "ignore"),
                                 Dn = da.DistinguishedName,
                                 Entry = da.Entry
                             });
            var expression = _queryContext.ActiveProvider.CurrentExpression;
            _translator.IsDynamic = true;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<DynamicQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(some filter)");
            _mockFacotry.Options.AttributesToLoad.Should().Have.SameSequenceAs(strings.ToDictionary(s => s));
        }

        [TestMethod]
        public void Translate_DynamicQueryFilterWithAndLambdaSelectValue_ReturnsCorrectParameters()
        {
            //prepare
            var strings = new[] { "one", "two" };
            _queryContext.Query<IDirectoryAttributes>()
                .FilterWith("some filter")
                .Select(da => new { What = da.GetInt("one").Value, Huh = da.GetInt("two").Value });
            var expression = _queryContext.ActiveProvider.CurrentExpression;
            _translator.IsDynamic = true;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<DynamicQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(some filter)");
            _mockFacotry.Options.AttributesToLoad.Should().Have.SameSequenceAs(strings.ToDictionary(s => s));
        }

        [TestMethod]
        public void Translate_MultipleExpressionSelects_ThrowsFilterException()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Select(u => new { u.Property1 })
                .Select(u => u.Property1);
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            Executing.This(() => _translator.Translate(expression))
                .Should().Throw<FilterException>().And.Exception.Message
                .Should().Contain("Cannot have multiple Select projections.");
        }

        [TestMethod]
        public void Translate_MultipleDynamicSelects_ThrowsFilterException()
        {
            //prepare
            _queryContext.Query<IDirectoryAttributes>()
                .Select("one")
                .Select("two");
            var expression = _queryContext.ActiveProvider.CurrentExpression;
            _translator.IsDynamic = true;

            //act
            Executing.This(() => _translator.Translate(expression))
                .Should().Throw<FilterException>().And.Exception.Message
                .Should().Contain("Cannot have multiple Select projections.");
        }

        [TestMethod]
        public void Translate_DynamicSelectAgainstNonDynamicQuery_ThrowsFilterException()
        {
            //prepare
            _queryContext.Query<IDirectoryAttributes>()
                .Select("one");
            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            Executing.This(() => _translator.Translate(expression))
                .Should().Throw<FilterException>().And.Exception.Message
                .Should().Contain("Cannot use a string attribute projection with a static type.");
        }

        [TestMethod]
        public void Translate_DynamicQueryFilterWithAndWithControls_ReturnsCorrectParameters()
        {
            //prepare
            var controls = new DirectoryControl[]
                               {
                                   new PageResultRequestControl(1),
                                   new SortRequestControl(new[] {new SortKey("test", null, true),})
                               };

            var strings = new[] { "one", "two" };
            _queryContext.Query<IDirectoryAttributes>()
                .Where("some filter")
                .Select(strings)
                .WithControls(controls);
            var expression = _queryContext.ActiveProvider.CurrentExpression;
            _translator.IsDynamic = true;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<DynamicQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(some filter)");
            _mockFacotry.Options.AttributesToLoad.Should().Have.SameSequenceAs(strings.ToDictionary(s => s));
            _mockFacotry.Options.Controls.Should().Have.SameSequenceAs(controls);
        }

        [TestMethod]
        public void Translate_WhereClauseAndWithoutPaging_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .WithoutPaging()
                .Where(c => c.Property1 == "test");

            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.WithoutPaging.Should().Be.True();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(x=test)");
        }

        [TestMethod]
        public void Translate_WhereClauseAndSkip_ReturnsCorrectParameters()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Skip(1)
                .Where(c => c.Property1 == "test");

            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //act
            var command = _translator.Translate(expression);

            //assert
            command.Should().Be.SameInstanceAs(_command.Object);
            _mockFacotry.Mapping.Should().Be.SameInstanceAs(_mapping.Object);
            _mockFacotry.Type.Should().Be.EqualTo(QueryCommandType.StandardCommand);
            _mockFacotry.Options.Should().Be.InstanceOf<StandardQueryCommandOptions>();
            _mockFacotry.Options.Filter.Should().Be.EqualTo("(x=test)");
            _mockFacotry.Options.SkipSize.Should().Be.EqualTo(1);
        }

        [TestMethod]
        public void Translate_NegativeSkip_ThrowsArgumentException()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Skip(-1)
                .Where(c => c.Property1 == "test");

            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //assert
            Executing.This(() => _translator.Translate(expression))
                .Should().Throw<ArgumentException>()
                .And.Exception.Message.Should().Be.EqualTo("Skip value must be greater than zero.");
        }

        [TestMethod]
        public void Translate_SkipWithToPage_ThrowsInvalidOperationException()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Skip(1)
                .ToPage(100);

            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //assert
            Executing.This(() => _translator.Translate(expression))
                .Should().Throw<InvalidOperationException>()
                .And.Exception.Message.Should().Be.EqualTo("Skip relies on Virtual List Views and cannot be used with simple LDAP paging. Please use one method for paging.");
        }

        [TestMethod]
        public void Translate_SkipWithCustomPaging_ThrowsInvalidOperationException()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>()
                .Skip(1)
                .WithControls(new[] { new PageResultRequestControl(1) });

            var expression = _queryContext.ActiveProvider.CurrentExpression;

            //assert
            Executing.This(() => _translator.Translate(expression))
                .Should().Throw<InvalidOperationException>()
                .And.Exception.Message.Should().Be.EqualTo("Skip relies on Virtual List Views and cannot be used with simple LDAP paging. Please use one method for paging.");
        }
    }
}