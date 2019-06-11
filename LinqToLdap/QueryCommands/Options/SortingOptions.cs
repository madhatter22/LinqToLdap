using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.QueryCommands.Options
{
    internal class SortingOptions : ISortingOptions
    {
        private readonly List<SortKey> _sortKeys;
        private SortKey _sortKey;
        public SortingOptions()
        {
            _sortKeys = new List<SortKey>();
        }

        public void AddSort(string attributeName, bool descending)
        {
            _sortKey = new SortKey { AttributeName = attributeName, ReverseOrder = descending };
            _sortKeys.Add(_sortKey);
        }

        public void SetMatchingRule(string matchingRule)
        {
            _sortKey.MatchingRule = matchingRule;
        }

        public SortKey[] Keys { get { return _sortKeys.ToArray(); } }
    }
}