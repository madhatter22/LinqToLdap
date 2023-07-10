using System.Linq;
using System.Reflection;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.ClassMapAssembly;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;

namespace LinqToLdap.Tests.Mapping
{
    [TestClass]
    public class DirectoryMapperTest
    {
        private Mock<IClassMap> _classMap;
        private DirectoryMapper _mapper;

        public string Property1 { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            _classMap = new Mock<IClassMap>();
            _mapper = new DirectoryMapper();
        }

        [TestMethod]
        public void Map_NonMappedStandardClass_CachesObjectMapping()
        {
            //prepare
            var objectMapping = new Mock<IObjectMapping>();
            objectMapping.Setup(m => m.Type)
                .Returns(typeof(DirectoryMapperTest));
            _classMap.Setup(m => m.ToObjectMapping())
                .Returns(objectMapping.Object);
            _classMap.Setup(m => m.PerformMapping("nc", "oc", false, new[] { "oc" }, false))
                .Returns(_classMap.Object);
            _classMap.Setup(m => m.Type)
                .Returns(typeof(DirectoryMapperTest));

            //act
            _mapper.Map(_classMap.Object, "nc", new[] { "oc" }, false, "oc", false);

            //assert
            _mapper.GetMappings().Should().HaveCount(1);
            _mapper.GetMappings()[GetType()].Should().Be(objectMapping.Object);
            _classMap.Verify(c => c.PerformMapping("nc", "oc", false, new[] { "oc" }, false));
            _classMap.Verify(c => c.Validate());
        }

        [TestMethod]
        public void Map_NonPremappedType_CreatesAutoClassMap()
        {
            //act
            var mapping = _mapper.Map<DirectoryMapperTest>("context");

            //assert
            _mapper.GetMappings().Should().HaveCount(1);
            mapping.Type.Should().Be(GetType());
        }

        [TestMethod]
        public void Map_NonPremappedTypeAndCustomAutoClassMapper_CareatesAutoClassMap()
        {
            //prepare

            var objectMapping = new Mock<IObjectMapping>();
            objectMapping.Setup(m => m.Type)
                .Returns(typeof(DirectoryMapperTest));
            _classMap.Setup(m => m.ToObjectMapping())
                .Returns(objectMapping.Object);
            _classMap.Setup(m => m.PerformMapping("nc", "oc", true, new[] { "oc" }, true))
                .Returns(_classMap.Object);
            _classMap.Setup(m => m.Type)
                .Returns(typeof(DirectoryMapperTest));
            _mapper.AutoMapWith(t => _classMap.Object);

            //act
            var mapping = _mapper.Map<DirectoryMapperTest>("nc", objectCategory: "oc", objectClasses: new[] { "oc" });

            //assert
            _mapper.GetMappings().Should().HaveCount(1);
            mapping.Type.Should().Be(GetType());
            mapping.Should().BeSameAs(objectMapping.Object);
            _classMap.Verify(c => c.PerformMapping("nc", "oc", true, new[] { "oc" }, true));
            _classMap.Verify(c => c.Validate());
        }

        [TestMethod]
        public void Map_NonPremappedTypeAndCustomAttributeClassMapper_CareatesAutoClassMap()
        {
            //prepare
            var objectMapping = new Mock<IObjectMapping>();
            objectMapping.Setup(m => m.Type)
                .Returns(typeof(AttributeClass));
            _classMap.Setup(m => m.ToObjectMapping())
                .Returns(objectMapping.Object);
            _classMap.Setup(m => m.PerformMapping("nc", "oc", true, new[] { "oc" }, true))
                .Returns(_classMap.Object);
            _classMap.Setup(m => m.Type)
                .Returns(typeof(AttributeClass));
            _mapper.AttributeMapWith(t => _classMap.Object);

            //act
            var mapping = _mapper.Map<AttributeClass>("nc", objectCategory: "oc", objectClasses: new[] { "oc" });

            //assert
            _mapper.GetMappings().Should().HaveCount(1);
            mapping.Type.Should().Be(typeof(AttributeClass));
            mapping.Should().BeSameAs(objectMapping.Object);
            _classMap.Verify(c => c.PerformMapping("nc", "oc", true, new[] { "oc" }, true));
            _classMap.Verify(c => c.Validate());
        }

        [TestMethod]
        public void Map_ObjectClassAndObjectClasses_ThrowsArgumentException()
        {
            //act
            Executing.This(() => _mapper.Map<DirectoryMapperTest>("context", objectClass: "oc", objectClasses: new[] { "oc" }))
                .Should().Throw<ArgumentException>().And.Message.Should().Be("objectClass and objectClasses cannot both have a value.");
        }

        [TestMethod]
        public void AddMappingFromAssembly_Assembly_AddsMappings()
        {
            //prepare
            var assembly = Assembly.GetAssembly(typeof(AssemblyTestClass));

            //act
            _mapper.AddMappingsFrom(assembly);

            //assert
            var mappings = _mapper.GetMappings();
            mappings.Should().HaveCount(3);
            mappings.ContainsKey(typeof(AssemblyTestClass)).Should().BeTrue();
            mappings.ContainsKey(typeof(AssenblyTestClass2)).Should().BeTrue();
            mappings.ContainsKey(typeof(AssemblyTestClassSub)).Should().BeTrue();
        }

        [TestMethod]
        public void AddMappingFromAssembly_AssemblyNameWithoutDllEnd_AddsMappings()
        {
            //act
            _mapper.AddMappingsFrom("LinqToLdap.Tests.ClassMapAssembly");

            //assert
            var mappings = _mapper.GetMappings();
            mappings.Should().HaveCount(3);
            mappings.Keys.Select(k => k.ToString()).Should().Contain(typeof(AssemblyTestClass).ToString());
            mappings.Keys.Select(k => k.ToString()).Should().Contain(typeof(AssenblyTestClass2).ToString());
            mappings.Keys.Select(k => k.ToString()).Should().Contain(typeof(AssemblyTestClassSub).ToString());
        }

        [TestMethod]
        public void GetMapping_Auto_Maps_AttributeClassMap()
        {
            //act
            _mapper.GetMapping<AttributeClass>();
            var mappings = _mapper.GetMappings();
            mappings.Should().HaveCount(1);
            mappings.Keys.Select(k => k.ToString()).Should().Contain(typeof(AttributeClass).ToString());
        }

        [TestMethod]
        public void GetMapping_Class_Without_DirectorySchemaAttribute_Throws_Exception()
        {
            //prepare
            Action action = () => _mapper.GetMapping(typeof(object));

            //act
            action.Should().Throw<MappingException>()
                .And.GetBaseException().Message.Should()
                .Be(string.Format("Mapping not found for '{0}'", typeof(object).FullName));
        }

        [TestMethod]
        public void GetMapping_Base_Type_Mapping_Added_First_Associates_Sub_Type_With_Base_Type()
        {
            //act
            var baseType = _mapper.GetMapping<AttributeClass>();
            var subType = _mapper.GetMapping<SubAttributeClass>();

            //assert
            baseType.SubTypeMappings.Should().Contain(subType).And.HaveCount(1);
        }

        [TestMethod]
        public void GetMapping_Sub_Type_Mapping_Added_First_Associates_Sub_Type_With_Base_Type()
        {
            //act
            var subType = _mapper.GetMapping<SubAttributeClass>();
            var baseType = _mapper.GetMapping<AttributeClass>();

            //assert
            baseType.SubTypeMappings.Should().Contain(subType).And.HaveCount(1);
        }

        [TestMethod]
        public void GetMapping_Multiple_Sub_Types_Added_Ad_Hoc_Assoications_Sub_Types_With_Base_Type()
        {
            //act
            var subType = _mapper.GetMapping<SubAttributeClass>();
            var subType3 = _mapper.GetMapping<Sub3AttributeClass>();
            var subType2 = _mapper.GetMapping<Sub2AttributeClass>();
            var baseType = _mapper.GetMapping<AttributeClass>();
            var subType2Same = _mapper.GetMapping<Sub2SameAttributeClass>();

            //assert
            subType2Same.SubTypeMappings.Should().BeEmpty();
            subType3.SubTypeMappings.Should().BeEmpty();
            subType2.SubTypeMappings.Should().Contain(subType3).And.HaveCount(1);
            subType2.CastTo<ObjectMapping>()
                .SubTypeMappingsObjectClassDictionary.All(x => x.Key == "sub3" && x.Value == subType3)
                .Should()
                .BeTrue();
            subType2.CastTo<ObjectMapping>()
                .SubTypeMappingsTypeDictionary.All(x => x.Key == subType3.Type && x.Value == subType3)
                .Should()
                .BeTrue();
            subType.SubTypeMappings.Should().Contain(subType2).And.Contain(subType3).And.Contain(subType2Same).And.HaveCount(3);
            subType.CastTo<ObjectMapping>()
                .SubTypeMappingsObjectClassDictionary.All(x => (x.Key == "sub2" && x.Value == subType2) || (x.Key == "sub2same" && x.Value == subType2Same) || (x.Key == "sub3" && x.Value == subType3))
                .Should()
                .BeTrue();
            subType.CastTo<ObjectMapping>()
                .SubTypeMappingsTypeDictionary.All(x => (x.Key == subType2.Type && x.Value == subType2) || (x.Key == subType2Same.Type && x.Value == subType2Same) || (x.Key == subType3.Type && x.Value == subType3))
                .Should()
                .BeTrue();
            baseType.SubTypeMappings.Should()
                .Contain(subType)
                .And.Contain(subType2)
                .And.Contain(subType2Same)
                .And.Contain(subType3)
                .And.HaveCount(4);
            baseType.CastTo<ObjectMapping>()
                .SubTypeMappingsObjectClassDictionary.All(x => (x.Key == "sub2" && x.Value == subType2) ||
                                                    (x.Key == "sub3" && x.Value == subType3) ||
                                                    (x.Key == "sub2same" && x.Value == subType2Same) ||
                                                    (x.Key == "sub" && x.Value == subType))
                .Should()
                .BeTrue();
            baseType.CastTo<ObjectMapping>()
                .SubTypeMappingsTypeDictionary.All(x => (x.Key == subType2.Type && x.Value == subType2) ||
                                                    (x.Key == subType3.Type && x.Value == subType3) ||
                                                    (x.Key == subType2Same.Type && x.Value == subType2Same) ||
                                                    (x.Key == subType.Type && x.Value == subType))
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidateObjectClasses_Base_Type_Missing_ObjectClasses_Should_Throw_InvalidOperationException()
        {
            //arrange
            var baseTypeMapping = new Mock<IObjectMapping>();
            baseTypeMapping.SetupGet(x => x.ObjectClasses)
                .Returns(default(IEnumerable<string>));
            baseTypeMapping.Setup(x => x.Type)
                .Returns(typeof(AttributeClass));
            var subTypeMapping = new Mock<IObjectMapping>();
            subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubAttributeClass));

            //act
            Executing.This(() => DirectoryMapper.ValidateObjectClasses(baseTypeMapping.Object, subTypeMapping.Object))
                .Should()
                .Throw<InvalidOperationException>()
                .And.Message.Should()
                .Be(
                    $"In order to use subclass mapping {typeof(AttributeClass).Name} must be mapped with objectClasses");
        }

        [TestMethod]
        public void ValidateObjectClasses_Sub_Type_Missing_ObjectClasses_Should_Throw_InvalidOperationException()
        {
            //arrange
            var baseTypeMapping = new Mock<IObjectMapping>();
            baseTypeMapping.SetupGet(x => x.ObjectClasses)
                .Returns(new[] { "base" });
            baseTypeMapping.Setup(x => x.Type)
                .Returns(typeof(AttributeClass));
            var subTypeMapping = new Mock<IObjectMapping>();
            subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubAttributeClass));

            //act
            Executing.This(() => DirectoryMapper.ValidateObjectClasses(baseTypeMapping.Object, subTypeMapping.Object))
                .Should()
                .Throw<InvalidOperationException>()
                .And.Message.Should()
                .Be(
                    $"In order to use subclass mapping {typeof(SubAttributeClass).Name} must be mapped with objectClasses");
        }

        [TestMethod]
        public void ValidateObjectClasses_Sub_Type_Has_Identical_ObjectClasses_To_Base_Type_Should_Throw_InvalidOperationException()
        {
            //arrange
            var baseTypeMapping = new Mock<IObjectMapping>();
            baseTypeMapping.SetupGet(x => x.ObjectClasses)
                .Returns(new[] { "base" });
            baseTypeMapping.SetupGet(x => x.SubTypeMappings)
                .Returns(new ReadOnlyCollection<IObjectMapping>(new List<IObjectMapping>()));
            baseTypeMapping.Setup(x => x.Type)
                .Returns(typeof(AttributeClass));
            var subTypeMapping = new Mock<IObjectMapping>();
            subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubAttributeClass));
            subTypeMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "base" });

            //act
            Executing.This(() => DirectoryMapper.ValidateObjectClasses(baseTypeMapping.Object, subTypeMapping.Object))
                .Should()
                .Throw<InvalidOperationException>()
                .And.Message.Should()
                .Be(
                    $"All sub types of {typeof(AttributeClass).Name} must have a unique sequence of objectClasses.");
        }

        [TestMethod]
        public void ValidateObjectClasses_Sub_Type_Has_Identical_ObjectClasses_To_Other_Sub_Type_Should_Throw_InvalidOperationException()
        {
            //arrange
            var subTypeMapping2 = new Mock<IObjectMapping>();
            subTypeMapping2.Setup(x => x.Type)
                .Returns(typeof(Sub2AttributeClass));
            subTypeMapping2.Setup(x => x.ObjectClasses)
                .Returns(new[] { "Sub", "top" });
            var baseTypeMapping = new Mock<IObjectMapping>();
            baseTypeMapping.SetupGet(x => x.ObjectClasses)
                .Returns(new[] { "base" });
            baseTypeMapping.SetupGet(x => x.SubTypeMappings)
                .Returns(new ReadOnlyCollection<IObjectMapping>(new[] { subTypeMapping2.Object }));
            baseTypeMapping.SetupGet(x => x.HasSubTypeMappings)
                .Returns(true);
            baseTypeMapping.Setup(x => x.Type)
                .Returns(typeof(AttributeClass));
            var subTypeMapping = new Mock<IObjectMapping>();
            subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubAttributeClass));
            subTypeMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "top", "sub" });

            //act
            Executing.This(() => DirectoryMapper.ValidateObjectClasses(baseTypeMapping.Object, subTypeMapping.Object))
                .Should()
                .Throw<InvalidOperationException>()
                .And.Message.Should()
                .Be(
                    $"All sub types of {typeof(AttributeClass).Name} must have a unique sequence of objectClasses.");
        }

        [TestMethod]
        public void ValidateObjectClasses_Sub_Type_Have_Unique_ObjectClasses_Should_Validate()
        {
            //arrange
            var subTypeMapping2 = new Mock<IObjectMapping>();
            subTypeMapping2.Setup(x => x.Type)
                .Returns(typeof(Sub2AttributeClass));
            subTypeMapping2.Setup(x => x.ObjectClasses)
                .Returns(new[] { "sub2" });
            var baseTypeMapping = new Mock<IObjectMapping>();
            baseTypeMapping.SetupGet(x => x.HasSubTypeMappings)
                .Returns(true);
            baseTypeMapping.SetupGet(x => x.ObjectClasses)
                .Returns(new[] { "base" });
            baseTypeMapping.SetupGet(x => x.SubTypeMappings)
                .Returns(new ReadOnlyCollection<IObjectMapping>(new[] { subTypeMapping2.Object }));
            baseTypeMapping.Setup(x => x.Type)
                .Returns(typeof(AttributeClass));
            var subTypeMapping = new Mock<IObjectMapping>();
            subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubAttributeClass));
            subTypeMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "sub" });

            //act
            Executing.This(() => DirectoryMapper.ValidateObjectClasses(baseTypeMapping.Object, subTypeMapping.Object))
                .Should()
                .NotThrow();
        }

        [TestMethod]
        public void GetMapping_types_configured_to_flatten_hiearchy_do_not_map_with_inheritance()
        {
            //act
            var baseType = _mapper.GetMapping<AttributeWithoutHierarchyClass>();
            var subType = _mapper.GetMapping<SubAttributeWithoutHierarchyClass>();

            //assert
            baseType.SubTypeMappings.Should().NotContain(subType).And.HaveCount(0);
            subType.GetPropertyMapping(nameof(SubAttributeWithoutHierarchyClass.Property1)).Should().NotBeNull();
            subType.GetPropertyMapping(nameof(SubAttributeWithoutHierarchyClass.Property2)).Should().NotBeNull();

            var instance = new SubAttributeWithoutHierarchyClass();
            subType.GetPropertyMapping(nameof(SubAttributeWithoutHierarchyClass.Property1)).SetValue(instance, "sub");

            instance.Property1.Should().Be("sub");
        }

        [DirectorySchema("test", ObjectClasses = new[] { "base" })]
        private class AttributeClass
        {
            [DirectoryAttribute]
            public string Property1 { get; set; }
        }

        [DirectorySchema("testsub", ObjectClasses = new[] { "base", "sub" })]
        private class SubAttributeClass : AttributeClass
        {
            [DirectoryAttribute]
            public string Property2 { get; set; }
        }

        [DirectorySchema("testsub2", ObjectClasses = new[] { "base", "sub", "sub2" })]
        private class Sub2AttributeClass : SubAttributeClass
        {
            [DirectoryAttribute]
            public string Property3 { get; set; }
        }

        [DirectorySchema("testsub2same", ObjectClasses = new[] { "base", "sub", "sub2same" })]
        private class Sub2SameAttributeClass : SubAttributeClass
        {
            [DirectoryAttribute]
            public string PropertySame { get; set; }
        }

        [DirectorySchema("testsub3", ObjectClasses = new[] { "base", "sub", "sub2", "sub3" })]
        private class Sub3AttributeClass : Sub2AttributeClass
        {
            [DirectoryAttribute]
            public string Property4 { get; set; }
        }

        [DirectorySchema("test", ObjectClasses = new[] { "base" }, WithoutSubTypeMapping = true)]
        private class AttributeWithoutHierarchyClass
        {
            [DirectoryAttribute]
            public string Property1 { get; set; }
        }

        [DirectorySchema("testsub", ObjectClasses = new[] { "base", "sub" }, WithoutSubTypeMapping = true)]
        private class SubAttributeWithoutHierarchyClass : AttributeWithoutHierarchyClass
        {
            [DirectoryAttribute]
            public string Property2 { get; set; }
        }
    }
}