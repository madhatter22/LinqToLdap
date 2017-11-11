using System;

namespace LinqToLdap.Tests
{
    public enum QueryTranslatorEnum
    {
        Value = 0,
        Value2 = 1
    }

    public class QueryTranslatorTestClass
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public int Property3 { get; set; }
        public DateTime? Property4 { get; set; }
        public bool Property5 { get; set; }
        public Guid Property6 { get; set; }
        public QueryTranslatorEnum Property7 { get; set; }
    }
}