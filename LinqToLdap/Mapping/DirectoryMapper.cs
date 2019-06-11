using LinqToLdap.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Class for storing a retrieving object mappings.
    /// </summary>
    public class DirectoryMapper : IDirectoryMapper
    {
#if (NET35 || NET40)
        private readonly LinqToLdap.Collections.SafeDictionary<Type, IObjectMapping> _mappings = new LinqToLdap.Collections.SafeDictionary<Type, IObjectMapping>();
#else
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IObjectMapping> _mappings = new System.Collections.Concurrent.ConcurrentDictionary<Type, IObjectMapping>();
#endif
        private Func<Type, IClassMap> _autoClassMapper;
        private Func<Type, IClassMap> _attributeClassMapper;

        /// <summary>
        /// Returns all mappings tracked by this object.
        /// </summary>
        /// <returns></returns>
#if (NET35 || NET40)
        public LinqToLdap.Collections.ReadOnlyDictionary<Type, IObjectMapping> GetMappings()
        {
            return _mappings.ToReadOnly();
        }
#else

        public System.Collections.ObjectModel.ReadOnlyDictionary<Type, IObjectMapping> GetMappings()
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<Type, IObjectMapping>(_mappings);
        }

#endif

        /// <summary>
        /// Provide a delegate that takes an object type and returns the class map for it.
        /// </summary>
        /// <param name="autoClassMapBuilder">The delegate.</param>
        /// <returns></returns>
        public IDirectoryMapper AutoMapWith(Func<Type, IClassMap> autoClassMapBuilder)
        {
            _autoClassMapper = autoClassMapBuilder;
            return this;
        }

        /// <summary>
        /// Indicates if a custom AutoMapping delegate has been provided
        /// </summary>
        public bool HasCustomAutoMapping => _autoClassMapper != null;

        /// <summary>
        /// Indicates if a custom AttributeMapping delegate has been provided
        /// </summary>
        public bool HasCustomAttributeMapping => _attributeClassMapper != null;

        /// <summary>
        /// Provide a delegate that takes an object type and returns the class map for it.
        /// </summary>
        /// <param name="attributeClassMapBuilder">The delegate.</param>
        /// <returns></returns>
        public IDirectoryMapper AttributeMapWith(Func<Type, IClassMap> attributeClassMapBuilder)
        {
            _attributeClassMapper = attributeClassMapBuilder;
            return this;
        }

        /// <summary>
        /// Adds all mappings in the assembly.
        /// </summary>
        /// <param name="assemblyName">
        /// The name of the assembly containing the mappings.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="assemblyName"/> is null, empty or white space.
        /// </exception>
        public void AddMappingsFrom(string assemblyName)
        {
            if (assemblyName.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(assemblyName));

            assemblyName = assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                ? assemblyName
                                : assemblyName + ".dll";

            var assembly = Assembly.LoadFrom(assemblyName);

            AddMappingsFrom(assembly);
        }

        /// <summary>
        /// Adds all mappings from <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly containing the mappings.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="assembly"/> is null..
        /// </exception>
        public void AddMappingsFrom(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var type in assembly.GetTypes().Where(t => !t.IsInterface))
            {
                if (type.HasDirectorySchema())
                {
                    IClassMap mapping;
                    if (HasCustomAttributeMapping)
                    {
                        mapping = _attributeClassMapper.Invoke(type);
                    }
                    else
                    {
                        var classMapType = typeof(AttributeClassMap<>).MakeGenericType(type);
                        mapping = (IClassMap)Activator.CreateInstance(classMapType);
                    }

                    Map(mapping);
                }
                else
                {
                    var baseType = type.BaseType;
                    while (baseType != null && baseType != typeof(object))
                    {
                        if (baseType.IsGenericType &&
                            baseType.GetGenericTypeDefinition() == typeof(ClassMap<>))
                        {
                            var mapping = (IClassMap)Activator.CreateInstance(type);

                            Map(mapping);
                            break;
                        }
                        baseType = baseType.BaseType;
                    }
                }
            }
        }

        /// <summary>
        /// Creates or retrieves the <see cref="IObjectMapping"/> from the classMap.
        /// </summary>
        /// <param name="classMap">The mapping.</param>
        /// <param name="objectCategory">The object category for the object.</param>
        /// <param name="includeObjectCategory">
        /// Indicates if the object category should be included in all queries.
        /// </param>
        /// <param name="namingContext">The location of the objects in the directory.</param>
        /// <param name="objectClasses">The object classes for the object.</param>
        /// <param name="includeObjectClasses">Indicates if the object classes should be included in all queries.</param>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is invalid.
        /// </exception>
        /// <returns></returns>
        public IObjectMapping Map(IClassMap classMap, string namingContext = null, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true, string objectCategory = null, bool includeObjectCategory = true)
        {
            if (classMap == null) throw new ArgumentNullException(nameof(classMap));

            return _mappings.GetOrAdd(classMap.Type, t =>
            {
                var mapped = classMap.PerformMapping(namingContext, objectCategory,
                                        includeObjectCategory,
                                        objectClasses, includeObjectClasses);

                mapped.Validate();

                var objectMapping = mapped.ToObjectMapping();
                MapSubTypes(objectMapping);

                return objectMapping;
            });
        }

        /// <summary>
        /// Creates or retrieves the <see cref="IObjectMapping"/> from <typeparam name="T"/>.
        /// </summary>
        /// <param name="namingContext">The optional naming context.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <param name="objectClasses">The optional object classes.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <param name="objectClass">The optional object class.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <param name="objectCategory">The optional object category.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is invalid.
        /// </exception>
        /// <returns></returns>
        public IObjectMapping Map<T>(string namingContext = null, string objectClass = null, IEnumerable<string> objectClasses = null, string objectCategory = null) where T : class
        {
            return _mappings.GetOrAdd(typeof(T), t =>
            {
                IClassMap classMap;
                if (t.HasDirectorySchema())
                {
                    classMap = !HasCustomAttributeMapping
                      ? new AttributeClassMap<T>()
                      : _attributeClassMapper.Invoke(typeof(T));
                }
                else
                {
                    if (objectClass != null)
                    {
                        if (objectClasses != null)
                            throw new ArgumentException("objectClass and objectClasses cannot both have a value.");

                        objectClasses = new[] { objectClass };
                    }
                    classMap = !HasCustomAutoMapping
                        ? new AutoClassMap<T>()
                        : _autoClassMapper.Invoke(typeof(T));
                }

                var mapped = classMap.PerformMapping(namingContext,
                                                     objectCategory: objectCategory,
                                                     objectClasses: objectClasses);

                mapped.Validate();

                var objectMapping = mapped.ToObjectMapping();
                MapSubTypes(objectMapping);

                return objectMapping;
            });
        }

        /// <summary>
        /// Gets the mapping for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for the mapping.</typeparam>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is not found.
        /// </exception>
        /// <returns></returns>
        public IObjectMapping GetMapping<T>() where T : class
        {
            return GetMapping(typeof(T));
        }

        /// <summary>
        /// Gets the mapping for <param name="type"/>.
        /// </summary>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is not found.
        /// </exception>
        /// <returns></returns>
        public IObjectMapping GetMapping(Type type)
        {
            return _mappings.GetOrAdd(type, t =>
            {
                if (t.HasDirectorySchema())
                {
                    var classMap = (IClassMap)(!HasCustomAttributeMapping
                           ? Activator.CreateInstance(typeof(AttributeClassMap<>).MakeGenericType(t))
                           : _attributeClassMapper.Invoke(t));
                    var mapped = classMap.PerformMapping();
                    mapped.Validate();

                    var objectMapping = mapped.ToObjectMapping();
                    MapSubTypes(objectMapping);
                    return objectMapping;
                }

                throw new MappingException($"Mapping not found for '{type.FullName}'");
            });
        }

        private void MapSubTypes(IObjectMapping mapping)
        {
#if (NET35 || NET40)
            var mappings = _mappings.ToReadOnly();
            foreach (var objectMapping in mappings)
#else
            foreach (var objectMapping in _mappings)
#endif
            {
                //check if already mapped instance is in new mappings inheritance chain
                var alreadyMappedBaseType = objectMapping.Key;
                while (alreadyMappedBaseType != null && alreadyMappedBaseType != typeof(object))
                {
                    if (alreadyMappedBaseType == mapping.Type)
                    {
                        ValidateObjectClasses(mapping, objectMapping.Value);
                        mapping.AddSubTypeMapping(objectMapping.Value);
                        break;
                    }
                    alreadyMappedBaseType = alreadyMappedBaseType.BaseType;
                }

                //check if new mapping is in the inheritance chain of an existing mapping
                var newMappedBaseType = mapping.Type;
                while (newMappedBaseType != null && newMappedBaseType != typeof(object))
                {
                    if (newMappedBaseType == objectMapping.Key)
                    {
                        ValidateObjectClasses(objectMapping.Value, mapping);
                        objectMapping.Value.AddSubTypeMapping(mapping);
                        break;
                    }
                    newMappedBaseType = newMappedBaseType.BaseType;
                }
            }
        }

        internal static void ValidateObjectClasses(IObjectMapping baseTypeMapping, IObjectMapping subTypeMapping)
        {
            if (!(baseTypeMapping.ObjectClasses ?? new string[0]).Any())
            {
                throw new InvalidOperationException(
                    $"In order to use subclass mapping {baseTypeMapping.Type.Name} must be mapped with objectClasses");
            }
            if (!(subTypeMapping.ObjectClasses ?? new string[0]).Any())
            {
                throw new InvalidOperationException(
                    $"In order to use subclass mapping {subTypeMapping.Type.Name} must be mapped with objectClasses");
            }

            var currentMappings =
                new[] { baseTypeMapping }.Union(baseTypeMapping.HasSubTypeMappings
                    ? baseTypeMapping.SubTypeMappings
                    : (IList<IObjectMapping>)new List<IObjectMapping>());

            if (currentMappings.Any(objectMapping => objectMapping.ObjectClasses.OrderBy(x => x)
                .SequenceEqual(subTypeMapping.ObjectClasses.OrderBy(x => x),
                    StringComparer.InvariantCultureIgnoreCase)))
            {
                throw new InvalidOperationException($"All sub types of {baseTypeMapping.Type.Name} must have a unique sequence of objectClasses.");
            }
        }
    }
}