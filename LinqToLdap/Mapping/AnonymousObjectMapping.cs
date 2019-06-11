using LinqToLdap.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToLdap.Mapping
{
    internal class AnonymousObjectMapping<T> : ObjectMapping where T : class
    {
        private readonly CtorWithParams<T> _constructor;

        public AnonymousObjectMapping(string namingContext,
            IEnumerable<IPropertyMapping> propertyMappings, string objectCategory, bool includeObjectCategory, IEnumerable<string> objectClass, bool includeObjectClasses)
            : base(namingContext, propertyMappings, objectCategory, includeObjectCategory, objectClass, includeObjectClasses)
        {
            _constructor = DelegateBuilder.BuildCtorWithParams<T>(typeof(T).GetConstructors().First());
        }

        public override bool IsForAnonymousType { get { return true; } }

        public override Type Type => typeof(T);

        public override object Create(object[] parameters = null, object[] objectClasses = null)
        {
            if (objectClasses != null) throw new NotSupportedException("Anonymous objects can't support sub types");

            T instance = _constructor(parameters);
            return instance;
        }

        public override void AddSubTypeMapping(IObjectMapping mapping)
        {
            throw new NotSupportedException("Anonymous objects can't support sub types");
        }
    }
}