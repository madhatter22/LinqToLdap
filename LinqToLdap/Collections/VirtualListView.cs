/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.Collections.Generic;

namespace LinqToLdap.Collections
{
    internal class VirtualListView<T> : List<T>, IVirtualListView<T>
    {
        public VirtualListView(int contentCount, byte[] contextId, int targetPosition, IEnumerable<T> view)
            : base(view)
        {
            ContentCount = contentCount;
            ContextId = contextId;
            TargetPosition = targetPosition;
        }

        public int ContentCount { get; private set; }
        public byte[] ContextId { get; private set; }
        public int TargetPosition { get; private set; }
    }
}
