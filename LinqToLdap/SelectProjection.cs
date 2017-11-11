/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToLdap
{
    internal class SelectProjection
    {
        public SelectProjection(IDictionary<string, string> selectedProperties, LambdaExpression projection)
        {
            SelectedProperties = selectedProperties;
            Projection = projection.Compile();
#if NET35
            ReturnType = projection.Body.Type;
#else
            ReturnType = projection.ReturnType;
#endif
        }

        public IDictionary<string, string> SelectedProperties { get; private set; }
        public Delegate Projection { get; private set; }
        public Type ReturnType { get; private set; }
    }
}
