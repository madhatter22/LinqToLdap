using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using LinqToLdap.TestSupport;
using LinqToLdap.Visitors;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Visitors
{
    [TestClass]
    public class QueryTranslatorFilterTest
    {
        private Mock<IObjectMapping> _mapping;
        private QueryTranslator _translator;
        private MockQueryCommandFactory _factory;
        private MockQueryContext _queryContext;
        private Expression<Func<QueryTranslatorTestClass, bool>> _expression;
        private Mock<IPropertyMapping> _property1Mapping;
        private Mock<IPropertyMapping> _property2Mapping;
        private Mock<IPropertyMapping> _property3Mapping;
        private Mock<IPropertyMapping> _property4Mapping;
        private Mock<IPropertyMapping> _property5Mapping;
        private Mock<IPropertyMapping> _property6Mapping;
        private Mock<IPropertyMapping> _property7Mapping;

        [TestInitialize]
        public void SetUp()
        {
            _queryContext = new MockQueryContext();
            _factory = new MockQueryCommandFactory();
            _mapping = new Mock<IObjectMapping>();

            _property1Mapping = new Mock<IPropertyMapping>();
            _property2Mapping = new Mock<IPropertyMapping>();
            _property3Mapping = new Mock<IPropertyMapping>();
            _property4Mapping = new Mock<IPropertyMapping>();
            _property5Mapping = new Mock<IPropertyMapping>();
            _property6Mapping = new Mock<IPropertyMapping>();
            _property7Mapping = new Mock<IPropertyMapping>();

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
            _property3Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<int>()))
                .Returns((int s) => s.ToString(CultureInfo.InvariantCulture));
            _property4Mapping.Setup(p => p.AttributeName)
                .Returns("a");
            _property4Mapping.Setup(p => p.PropertyName)
                .Returns("Property4");
            _property4Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<DateTime>()))
                .Returns((DateTime s) => s.ToFileTime().ToString(CultureInfo.InvariantCulture));
            _property5Mapping.Setup(p => p.AttributeName)
                .Returns("b");
            _property5Mapping.Setup(p => p.PropertyName)
                .Returns("Property5");
            _property5Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<bool>()))
                .Returns((bool s) => s ? "TRUE" : "FALSE");
            _property6Mapping.Setup(p => p.AttributeName)
                .Returns("c");
            _property6Mapping.Setup(p => p.PropertyName)
                .Returns("Property6");
            _property6Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<Guid>()))
                .Returns((Guid s) => s.ToStringOctet());
            _property7Mapping.Setup(p => p.AttributeName)
                .Returns("d");
            _property7Mapping.Setup(p => p.PropertyName)
                .Returns("Property7");

            _mapping.Setup(m => m.GetPropertyMapping("Property1", null))
                .Returns(_property1Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property2", null))
                .Returns(_property2Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property3", null))
                .Returns(_property3Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property4", null))
                .Returns(_property4Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property5", null))
                .Returns(_property5Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property6", null))
                .Returns(_property6Mapping.Object);
            _mapping.Setup(m => m.GetPropertyMapping("Property7", null))
                .Returns(_property7Mapping.Object);
            
            var dictionary = new Dictionary<string, string>
                              {
                                  {"Property1", "x"},
                                  {"Property2", "y"},
                                  {"Property3", "z"},
                                  {"Property4", "a"},
                                  {"Property5", "b"},
                                  {"Property6", "c"},
                                  {"Property7", "d"}
                              }.ToReadOnlyDictionary();
            _mapping.Setup(m => m.Properties)
                .Returns(dictionary);

            _translator = new QueryTranslator(_mapping.Object);

            _translator.SetFieldValue("_factory", _factory);
        }

        [TestCleanup]
        public void TearDown()
        {
            Console.WriteLine(_factory.Filter);
            _expression = null;
        }

        [TestMethod]
        public void Translate_NullObjectCategory_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns(default(string));
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);

            _expression = s => s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        #region OC Tests

        [TestMethod]
        public void Translate_ObjectCategory_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _expression = s => s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(objectCategory=oc)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryNotInclude_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(false);
            _expression = s => s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_ObjectCategoryIncludeWithIgnoreBoth_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IgnoreOC(OC.Both);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_ObjectCategoryIncludeWithIgnoreObjectCategory_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IgnoreOC(OC.ObjectCategory);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_ObjectCategoryNotIncludeWithIncludeObjectCategory_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(false);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IncludeOC(OC.ObjectCategory);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectCategory=oc)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryNotIncludeWithIncludeBoth_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(false);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IncludeOC(OC.Both);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectCategory=oc)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectClasses_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new []{"oc1"});
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);
            _expression = s => s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(objectClass=oc1)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectClassesNotInclude_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(false);
            _expression = s => s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_ObjectClassesIncludeWithIgnoreBoth_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IgnoreOC(OC.Both);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_ObjectClassesIncludeWithIgnoreObjectClasses_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IgnoreOC(OC.ObjectClasses);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_ObjectClassesNotIncludeWithIncludeObjectClasses_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(false);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IncludeOC(OC.ObjectClasses);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectClass=oc1)(objectClass=oc2)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectClassesNotIncludeWithIncludeBoth_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(false);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IncludeOC(OC.Both);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectClass=oc1)(objectClass=oc2)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryAndObjectClasses_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);
            _expression = s => s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(objectCategory=oc)(objectClass=oc1)(objectClass=oc2)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryAndObjectClassesNotIncludeWithIncludeBoth_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(false);
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(false);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IncludeOC(OC.Both);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectCategory=oc)(objectClass=oc1)(objectClass=oc2)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryAndObjectCLassesNotIncludeWithIncludeObjectCategory_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(false);
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(false);
            var query = _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IncludeOC(OC.ObjectCategory);

            //act
            _translator.Translate(query.Provider.CastTo<MockQueryProvider>().CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectCategory=oc)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryAndObjectCLassesNotIncludeWithIncludeObjectClasses_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(false);
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(false);
            var query = _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IncludeOC(OC.ObjectClasses);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectClass=oc1)(objectClass=oc2)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectClassesAndObjectCategoryIncludeWithIgnoreObjectClasses_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IgnoreOC(OC.ObjectClasses);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectCategory=oc)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectClassesAndObjectCategoryIncludeWithIgnoreObjectCategory_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IgnoreOC(OC.ObjectCategory);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(&(objectClass=oc1)(objectClass=oc2)(x=assd))");
        }

        [TestMethod]
        public void Translate_ObjectClassesAndObjectCategoryIncludeWithIgnoreBoth_BuildsFilter()
        {
            //prepare
            _mapping.Setup(m => m.ObjectCategory)
                .Returns("oc");
            _mapping.Setup(m => m.IncludeObjectCategory)
                .Returns(true);
            _mapping.Setup(m => m.ObjectClasses)
                .Returns(new[] { "oc1", "oc2" });
            _mapping.Setup(m => m.IncludeObjectClasses)
                .Returns(true);
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "assd").IgnoreOC(OC.Both);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        #endregion OC Tests

        [TestMethod]
        public void Translate_WhereClauseAndAnyClause_BuildsCorrectFilter()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "s").Any(t => t.Property2 == "a");

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            _factory.Filter.Should().Be("(&(x=s)(y=a))");
        }

        [TestMethod]
        public void Translate_WhereClauseAndAnyClauseWitObjectCategory_BuildsCorrectFilter()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "s").Any(t => t.Property5);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            _factory.Filter.Should().Be("(&(x=s)(b=TRUE))");
        }

        [TestMethod]
        public void Translate_MultipleWhereClauses_BuildsCorrectFilter()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "s").Where(t => t.Property5 == false);

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            _factory.Filter.Should().Be("(&(x=s)(b=FALSE))");
        }

        [TestMethod]
        public void Translate_MultipleWhereClausesWitObjectCategory_BuildsCorrectFilter()
        {
            //prepare
            _queryContext.Query<QueryTranslatorTestClass>().Where(t => t.Property1 == "s").Where(t => t.Property2 == "a");

            //act
            _translator.Translate(_queryContext.ActiveProvider.CurrentExpression);

            _factory.Filter.Should().Be("(&(x=s)(y=a))");
        }

        [TestMethod]
        public void Translate_NullObjectCategoryWithMultipleProperties_BuildsFilter()
        {
            //prepare
            _expression = s => s.Property1 == "assd" && s.Property2 == "as" && s.Property3 == 2;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x=assd)(y=as)(z=2))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryWithMultipleProperties_BuildsFilter()
        {
            //prepare
            _expression = s => s.Property1 == "assd" && s.Property2 == "as" && s.Property3 == 2;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x=assd)(y=as)(z=2))");
        }

        [TestMethod]
        public void Translate_PropertyEqual_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_StringPropertyEqualWithEqualsMethod_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1.Equals("assd");

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_PropertyEqualToUpper_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1.ToUpper() == "assd".ToUpper();

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=ASSD)");
        }

        [TestMethod]
        public void Translate_PropertyEqualToUpperInvariant_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1.ToUpperInvariant() == "assd".ToUpperInvariant();

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=ASSD)");
        }

        [TestMethod]
        public void Translate_PropertyEqualToLower_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1.ToLower() == "ASSD".ToLower();

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_PropertyEqualToLowerInvariant_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1.ToLowerInvariant() == "ASSD".ToLowerInvariant();

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd)");
        }

        [TestMethod]
        public void Translate_FilterPropertyEqual_BuildsEqualFilter()
        {
            //prepare
            _expression = s => Filter.Equal(s, "x", "assd~", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=assd\\7e)");
        }

        [TestMethod]
        public void Translate_MultipleFilterPropertyEqual_BuildsEqualFilter()
        {
            //prepare
            _expression = s => Filter.Equal(s, "x", "assd~", false) && Filter.Equal(s, "y", "blah", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x=assd~)(y=blah))");
        }

        [TestMethod]
        public void Translate_MultipleMixedFilterPropertyEqual_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1 == "p1" && Filter.Equal(s, "x", "assd", true) && Filter.Equal(s, "y", "blah", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x=p1)(x=assd)(y=blah))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryMultipleMixedFilterPropertyEqual_BuildsEqualFilter()
        {
            //prepare
            _expression = s => s.Property1 == "p1" && Filter.Equal(s, "x", "assd", true) && Filter.Equal(s, "y", "blah", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x=p1)(x=assd)(y=blah))");
        }

        [TestMethod]
        public void Translate_NegateMultiplePropertyEqual_BuildsNotEqualFilter()
        {
            //prepare
            _expression = s => !(s.Property1 == "assd" && s.Property2 == "as");

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(!(x=assd))(!(y=as)))");
        }

        [TestMethod]
        public void Translate_PropertyEqualNull_BuildsNullFilter()
        {
            //prepare
            _expression = s => s.Property1 == null;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=*))");
        }

        [TestMethod]
        public void Translate_PropertyEqualEmpty_BuildsNullFilter()
        {
            //prepare
            _expression = s => s.Property1 == "";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=*))");
        }

        [TestMethod]
        public void Translate_PropertyEqualWhiteSpace_ThrowsFilterException()
        {
            //prepare
            _expression = s => s.Property1 == "   ";

            //assert
            Executing.This(() => _translator.Translate(_expression))
                .Should().Throw<FilterException>();
        }

        [TestMethod]
        public void Translate_PropertyNotEqualNull_BuildsNotNullFilter()
        {
            //prepare
            _expression = s => s.Property1 != null;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=*)");
        }

        [TestMethod]
        public void Translate_NotPropertyNotEqualNull_BuildsNullFilter()
        {
            //prepare
            _expression = s => !(s.Property1 != null);
            
            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=*))");
        }

        [TestMethod]
        public void Translate_NotPropertyEqualNull_BuildsNullFilter()
        {
            //prepare
            _expression = s => !(s.Property1 == null);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=*)");
        }

        [TestMethod]
        public void Translate_OrChain_BuildsSimplifiedOrElseFilter()
        {
            //prepare
            _expression = s => s.Property1 == "1" || s.Property1 == "2" || s.Property1 == "3";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(|(x=1)(x=2)(x=3))");
        }

        [TestMethod]
        public void Translate_ObjectCategoryOrChainWithAnd_BuildsSimplifiedOrElseWithAndFilter()
        {
            //prepare
            _expression = s => (s.Property1 == "1" || s.Property1 == "2" || s.Property1 == "3") && s.Property1 == "4";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(|(x=1)(x=2)(x=3))(x=4))");
        }

        [TestMethod]
        public void Translate_OrChainWithAnd_BuildsSimplifiedOrElseWithAndFilter()
        {
            //prepare
            _expression = s => (s.Property1 == "1" || s.Property1 == "2" || s.Property1 == "3") && s.Property1 == "4";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(|(x=1)(x=2)(x=3))(x=4))");
        }

        [TestMethod]
        public void Translate_AndChain_BuildsSimplifiedAndElseFilter()
        {
            //prepare
            _expression = s => s.Property1 == "1" && s.Property1 == "2" && s.Property1 == "3";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x=1)(x=2)(x=3))");
        }

        [TestMethod]
        public void Translate_AndChainWithOr_BuildsSimplifiedAndAlsoWithOrFilter()
        {
            //prepare
            _expression = s => s.Property1 == "1" && (s.Property1 == "2" || s.Property1 == "3" || s.Property1 == "4");

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x=1)(|(x=2)(x=3)(x=4)))");
        }

        [TestMethod]
        public void Translate_MethodIEnumberableContainsMember_BuildsOrElseFilter()
        {
            //prepare
            _expression = s => GetStrings().Contains(s.Property1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(|(x=string1)(x=string2)(x=string3))");
        }

        [TestMethod]
        public void Translate_MethodListContainsMember_BuildsOrElseFilter()
        {
            //prepare
            _expression = s => GetStrings().ToList().Contains(s.Property1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(|(x=string1)(x=string2)(x=string3))");
        }

        [TestMethod]
        public void Translate_MethodListContainsMemberWithAnd_BuildsAndWithOrElseFilter()
        {
            //prepare
            _expression = s => GetStrings().ToList().Contains(s.Property1) && s.Property1 == "assd";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(|(x=string1)(x=string2)(x=string3))(x=assd))");
        }

        [TestMethod]
        public void Translate_LocalListContainsMember_BuildsOrElseFilter()
        {
            //prepare
            var local = GetStrings().ToList();
            _expression = s => local.Contains(s.Property1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(|(x=string1)(x=string2)(x=string3))");
        }

        [TestMethod]
        public void Translate_NegateLocalListContainsMember_BuildsOrElseFilter()
        {
            //prepare
            var local = GetStrings().ToList();
            _expression = s => !local.Contains(s.Property1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(|(!(x=string1))(!(x=string2))(!(x=string3)))");
        }

        [TestMethod]
        public void Translate_GreaterThan_BuildsGreaterThanOrEqualAndNotEqual()
        {
            //prepare
            _expression = s => s.Property3 > 1;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(z>=1)(!(z=1)))");
        }

        [TestMethod]
        public void Translate_NotGreaterThan_BuildsLessThanOrEqualAndNotEqual()
        {
            //prepare
            _expression = s => !(s.Property3 > 1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(z<=1)(!(z=1)))");
        }

        [TestMethod]
        public void Translate_GreaterThanOrEqual_BuildsGreaterThanOrEqual()
        {
            //prepare
            _expression = s => s.Property3 >= 1;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(z>=1)");
        }

        [TestMethod]
        public void Translate_NotGreaterThanOrEqual_BuildsLessThanOrEqual()
        {
            //prepare
            _expression = s => !(s.Property3 >= 1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(z<=1)");
        }

        [TestMethod]
        public void Translate_LessThan_BuildsLessThanOrEqualAndNotEqual()
        {
            //prepare
            _expression = s => s.Property3 < 1;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(z<=1)(!(z=1)))");
        }

        [TestMethod]
        public void Translate_NotLessThan_BuildsGreaterThanOrEqualAndNotEqual()
        {
            //prepare
            _expression = s => !(s.Property3 < 1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(z>=1)(!(z=1)))");
        }

        [TestMethod]
        public void Translate_LessThanOrEqual_BuildsLessThanOrEqual()
        {
            //prepare
            _expression = s => s.Property3 <= 1;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(z<=1)");
        }

        [TestMethod]
        public void Translate_NotLessThanOrEqual_BuildsGreaterThanOrEqual()
        {
            //prepare
            _expression = s => !(s.Property3 <= 1);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(z>=1)");
        }

        [TestMethod]
        public void Translate_StringArrayMemberContains_BuildsPropertyEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(string[]), Property2 = "" };
            var expression = example.CreateExpression(e => e.Property1.Contains("anon") && e.Property2.Contains("anon"));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(&(x=anon)(y=*anon*))");
        }

        [TestMethod]
        public void Translate_DynamicStringArrayMemberContains_BuildsPropertyEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(string[]), Property2 = "" };
            var expression = example.CreateExpression(e => e.Property1.Contains("anon") && e.Property2.Contains("anon"));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(&(x=anon)(y=*anon*))");
        }

        [TestMethod]
        public void Translate_StringListMemberContains_BuildsPropertyEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(List<string>) };
            var expression = example.CreateExpression(e => e.Property1.Contains("anon"));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(x=anon)");
        }

        [TestMethod]
        public void Translate_StringListMemberContainsAndFilterEqual_BuildsPropertyEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(List<string>) };
            var expression = example.CreateExpression(e => e.Property1.Contains("anon") && Filter.Equal(e, "x", "anon2", true));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(&(x=anon)(x=anon2))");
        }

        [TestMethod]
        public void Translate_StringIEnumerableMemberContains_BuildsPropertyNotEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(IEnumerable<string>) };
            var expression = example.CreateExpression(e => e.Property1.Contains("anon"));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(x=anon)");
        }

        [TestMethod]
        public void Translate_NotStringArrayMemberContains_BuildsPropertyNotEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(string[]) };
            var expression = example.CreateExpression(e => !e.Property1.Contains("anon"));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(!(x=anon))");
        }

        [TestMethod]
        public void Translate_NotStringListMemberContains_BuildsPropertyNotEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(List<string>) };
            var expression = example.CreateExpression(e => !e.Property1.Contains("anon"));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(!(x=anon))");
        }

        [TestMethod]
        public void Translate_NotStringIEnumerableMemberContains_BuildsPropertyNotEqualFilter()
        {
            //prepare
            var example = new { Property1 = default(IEnumerable<string>) };
            var expression = example.CreateExpression(e => !e.Property1.Contains("anon"));

            //act
            _translator.Translate(expression);

            //assert
            _factory.Filter.Should().Be("(!(x=anon))");
        }

        [TestMethod]
        public void Translate_Guid_BuildsStringOctetFilter()
        {
            //prepare
            var guid = Guid.NewGuid();
            _expression = e => e.Property6 == guid;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be(string.Format("(c={0})", guid.ToStringOctet()));
        }

        [TestMethod]
        public void Translate_NullDateTime_BuildsNullFilter()
        {
            //prepare
            DateTime? now = null;
            _expression = e => e.Property4 == now;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(a=*))");
        }
        
        [TestMethod]
        public void Translate_TrueBoolean_BuildsTrueString()
        {
            //prepare
            _expression = e => e.Property5 == true;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(b=TRUE)");
        }

        [TestMethod]
        public void Translate_TrueBooleanWithoutValue_BuildsTrueString()
        {
            //prepare
            _expression = e => e.Property5;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(b=TRUE)");
        }

        [TestMethod]
        public void Translate_FalseBoolean_BuildsFalseString()
        {
            //prepare
            _expression = e => e.Property5 == false;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(b=FALSE)");
        }

        [TestMethod]
        public void Translate_FalseBooleanWithoutValue_BuildsFalseString()
        {
            //prepare
            _expression = e => !e.Property5;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(b=FALSE)");
        }
        
        [TestMethod]
        public void Translate_FilterApproximateExpression_BuildsApproximateFilter()
        {
            //prepare
            _expression = e => Filter.Approximately(e, e.Property1, "some value", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x~=some value)");
        }

        [TestMethod]
        public void Translate_FilterApproximateValue_BuildsApproximateFilter()
        {
            //prepare
            _expression = e => Filter.Approximately(e, "prop", "some value~", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(prop~=some value\\7e)");
        }

        [TestMethod]
        public void Translate_FilterNotApproximateExpression_BuildsApproximateFilter()
        {
            //prepare
            _expression = e => !Filter.Approximately(e, e.Property1, "some value~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x~=some value~))");
        }

        [TestMethod]
        public void Translate_FilterNotApproximateValue_BuildsApproximateFilter()
        {
            //prepare
            _expression = e => !Filter.Approximately(e, "prop", "some value", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(prop~=some value))");
        }

        [TestMethod]
        public void Translate_FilterEqual_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.Equal(e, e.Property1, "some value", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=some value)");
        }

        [TestMethod]
        public void Translate_NotFilterEqual_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.Equal(e, e.Property1, "some value", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=some value))");
        }

        [TestMethod]
        public void Translate_FilterEqualAndFilterEqualAny_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.Equal(e, "a", "test", true) && Filter.EqualAny(e, "x", new[] { "a", "b", "c", "~" }, true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(a=test)(|(x=a)(x=b)(x=c)(x=\\7e)))");
        }

        [TestMethod]
        public void Translate_NotFilterEqualAny_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.EqualAny(e, e.Property1, new[]{"~"}, false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=~))");
        }

        [TestMethod]
        public void Translate_FilterStartsWith_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.StartsWith(e, "x", "some value*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=some value\\2a*)");
        }

        [TestMethod]
        public void Translate_NotFilterStartsWith_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.StartsWith(e, "x", "some value~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=some value~*))");
        }

        [TestMethod]
        public void Translate_NotFilterStartsWithMemberExpression_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.StartsWith(e, e.Property1, "some value", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=some value*))");
        }

        [TestMethod]
        public void Translate_FilterEndsWith_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.EndsWith(e, "x", "some value*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=*some value\\2a)");
        }

        [TestMethod]
        public void Translate_NotFilterEndsWith_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.EndsWith(e, "x", "some value*", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=*some value*))");
        }

        [TestMethod]
        public void Translate_NotFilterEndsWithMemberExpression_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.EndsWith(e, e.Property1, "some value", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=*some value))");
        }

        [TestMethod]
        public void Translate_FilterLike_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.Like(e, "x", "some value*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=*some value\\2a*)");
        }

        [TestMethod]
        public void Translate_NotFilterLike_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.Like(e, "x", "some value~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=*some value~*))");
        }

        [TestMethod]
        public void Translate_NotFilterLikeMemberExpression_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.Like(e, e.Property1, "some value", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(x=*some value*))");
        }

        [TestMethod]
        public void Translate_FilterGreaterThanOrEqual_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.GreaterThanOrEqual(e, "x", "1*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x>=1\\2a)");
        }

        [TestMethod]
        public void Translate_NotFilterGreaterThanOrEqual_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.GreaterThanOrEqual(e, "x", "1~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x<=1~)");
        }

        [TestMethod]
        public void Translate_NotFilterGreaterThanOrEqualMemberExpression_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.GreaterThanOrEqual(e, e.Property1, "1", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x<=1)");
        }

        [TestMethod]
        public void Translate_FilterLessThanOrEqual_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.LessThanOrEqual(e, "x", "1*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x<=1\\2a)");
        }

        [TestMethod]
        public void Translate_NotFilterLessThanOrEqual_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.LessThanOrEqual(e, "x", "1~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x>=1~)");
        }

        [TestMethod]
        public void Translate_NotFilterLessThanOrEqualMemberExpression_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.LessThanOrEqual(e, e.Property1, "1", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x>=1)");
        }

        [TestMethod]
        public void Translate_FilterLessThan_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.LessThan(e, "x", "1*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x<=1\\2a)(!(x=1\\2a)))");
        }

        [TestMethod]
        public void Translate_NotFilterLessThan_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.LessThan(e, "x", "1~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x>=1~)");
        }

        [TestMethod]
        public void Translate_NotFilterLessThanMemberExpression_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.LessThan(e, e.Property1, "1", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x>=1)");
        }

        [TestMethod]
        public void Translate_FilterGreaterThan_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.GreaterThan(e, "x", "1*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(&(x>=1\\2a)(!(x=1\\2a)))");
        }

        [TestMethod]
        public void Translate_NotFilterGreaterThan_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.GreaterThan(e, "x", "1~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x<=1~)");
        }

        [TestMethod]
        public void Translate_NotFilterGreaterThanMemberExpression_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.GreaterThan(e, e.Property1, "1", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x<=1)");
        }

        [TestMethod]
        public void Translate_FilterEnumWithInt_BuildsIntFilter()
        {
            //prepare
            _property7Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<int>()))
                .Returns((QueryTranslatorEnum s) => ((int)s).ToString(CultureInfo.InvariantCulture));
            _expression = e => e.Property7 == QueryTranslatorEnum.Value2;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(d=1)");
        }

        [TestMethod]
        public void Translate_FilterEnumString_BuildsFilter()
        {
            //prepare
            _property7Mapping.Setup(p => p.FormatValueToFilter(It.IsAny<int>()))
                .Returns((QueryTranslatorEnum s) => s.ToString());
            _expression = e => e.Property7 == QueryTranslatorEnum.Value2;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(d=Value2)");
        }

        [TestMethod]
        public void Translate_FilterComplexMemberExpressionAndOr_BuildsFilter()
        {
            //prepare
            _expression = _expression.Or(u => u.Property1 == "ab" || u.Property1 == "ab");
            _expression =
                _expression.Or(
                    u =>
                    u.Property1.StartsWith("a") &&
                    (u.Property1.StartsWith("b") ||
                     u.Property1.StartsWith("b")));

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(|(x=ab)(x=ab)(&(x=a*)(|(x=b*)(x=b*))))");
        }

        [TestMethod]
        public void Translate_FilterComplexAndOr_BuildsFilter()
        {
            //prepare
            _expression = _expression.Or(u => Filter.Equal(u, u.Property1, "ab", true) || Filter.Equal(u, u.Property1, "ab", true));
            _expression =
                _expression.Or(
                    u =>
                    Filter.EqualAny(u, "x", new[]{"a", "b", "c"}, true) &&
                    (Filter.StartsWith(u, u.Property1, "b", true) ||
                     Filter.StartsWith(u, u.Property1, "b", true)));

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(|(x=ab)(x=ab)(&(|(x=a)(x=b)(x=c))(|(x=b*)(x=b*))))");
        }

        [TestMethod]
        public void Translate_UnnecessaryBooleans_IgnoresAndBuildsFilter()
        {
            //prepare
            _expression = e => false && true && e.Property1 == "test";

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().BeNull();
        }

        [TestMethod]
        public void Translate_UnnecessaryConstantsAndProperty_IgnoresConstantsAndBuildsFilter()
        {
            //prepare
            DateTime? now = DateTime.Now;
            _expression = e => now.HasValue && e.Property4 == now.Value;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(a=" + now.Value.ToFileTime() + ")");
        }

        [TestMethod]
        public void Translate_UnnecessaryConstantsWithNullValueAndProperty_IgnoresConstantsAndBuildsFilter()
        {
            //prepare
            DateTime? now = null;
            _expression = e => now.HasValue && e.Property4 == now.Value;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().BeNull();
        }

        [TestMethod]
        public void Translate_NullablePropertyMultipleMemberAccess_BuildsFilter()
        {
            //prepare
            var now = DateTime.Now;
            _expression = e => e.Property4.Value == now;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(a=" + now.ToFileTime() + ")");
        }

        [TestMethod]
        public void Translate_NullablePropertyHasValue_BuildsFilter()
        {
            //prepare
            _expression = e => e.Property4.HasValue;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(a=*)");
        }

        [TestMethod]
        public void Translate_NullablePropertyNotHasValue_BuildsFilter()
        {
            //prepare
            _expression = e => !e.Property4.HasValue;

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(a=*))");
        }

        [TestMethod]
        public void Translate_FilterEqualAnything_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.EqualAnything(e, "prop1");

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(prop1=*)");
        }

        [TestMethod]
        public void Translate_FilterEqualAnythingPropertyExpression_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.EqualAnything(e, x => x.Property4);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(a=*)");
        }

        [TestMethod]
        public void Translate_NotFilterEqualAnything_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.EqualAnything(e, "prop1");

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(prop1=*))");
        }

        [TestMethod]
        public void Translate_MatchingRule_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.MatchingRule(e, "prop1", "1.2.840.113556.1.4.803", "2*", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(prop1:1.2.840.113556.1.4.803:=2\\2a)");
        }

        [TestMethod]
        public void Translate_NotMatchingRule_BuildsFilter()
        {
            //prepare
            _expression = e => !Filter.MatchingRule(e, "prop1", "1.2.840.113556.1.4.803", "2~", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(!(prop1:1.2.840.113556.1.4.803:=2~))");
        }

        [TestMethod]
        public void Translate_MatchingRulePropertyExpression_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.MatchingRule(e, x => x.Property4, "1.2.840.113556.1.4.803", "2", true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(a:1.2.840.113556.1.4.803:=2)");
        }

        [TestMethod]
        public void Translate_FilterEqualWithFuzzyMathing_BuildsFilter()
        {
            //prepare
            _expression = e => Filter.Equal(e, "x", "*something*", false);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=*something*)");
        }

        [TestMethod]
        public void Translate_FilterEqualWithLocalVariable_BuildsFilter()
        {
            //prepare
            const string parameter = "some string";
            _expression = e => Filter.Equal(e, "x", parameter, true);

            //act
            _translator.Translate(_expression);

            //assert
            _factory.Filter.Should().Be("(x=some string)");
        }

        private static IEnumerable<string> GetStrings()
        {
            return new List<string> { "string1", "string2", "string3" };
        }
    }
}
