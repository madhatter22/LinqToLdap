using LinqToLdap;
using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands;
using LinqToLdap.QueryCommands.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToLdap.Visitors
{
    internal class QueryTranslator : ExpressionVisitor
    {
        private static readonly Dictionary<Type, int> QueryableDeclaringTypes = new Dictionary<Type, int>
            {
                { typeof(Queryable), 0},
                { typeof(QueryableExtensions), 0},
                { typeof(PredicateBuilder), 0},
#if (!NET35 && !NET40)
                { typeof(Async.QueryableAsyncExtensions), 0},
#endif
            };

        private StringBuilder _sb;
        private bool _not;
        private Stack<ExpressionType> _expressionStack;
        private int _exclusiveWhereCount;
        private bool _dnFilter;
        private bool _shouldCleanFilter;
        private readonly IObjectMapping _mapping;
        private bool _binaryExpression;
        private OC? _ignoreOc;
        private int _customFiltersCount;
        private StringBuilder _sbCustomFilter;
        private PagingOptions _pagingOptions;
        private SortingOptions _sortingOptions;
        private int? _toListPageSize;
        private int? _takeSize;
        private int? _skipSize;
        private bool _withoutPaging;
        private IPropertyMapping _currentProperty;
        private List<DirectoryControl> _controls;
        private bool _falseEncountered;
        private OC? _includeOc;
        private string _pagingFilter;
        private bool _isLongCount;
        private PartialResultProcessing _asyncProcessing;

        private QueryCommandType _commandType = QueryCommandType.StandardCommand;
        private QueryCommandOptions _commandOptions;

        public QueryTranslator(IObjectMapping mapping)
        {
            _mapping = mapping;
        }

        public bool IsDynamic { get; set; }

#pragma warning disable 649
        private IQueryCommandFactory _factory;
#pragma warning restore 649
        private IQueryCommandFactory QueryCommandFactory => _factory ?? new QueryCommandFactory();

        public IQueryCommand Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            _sb = new StringBuilder();
            _sbCustomFilter = new StringBuilder();
            _expressionStack = new Stack<ExpressionType>();
            Visit(expression);

            if (_commandOptions == null)
            {
                if (IsDynamic)
                {
                    _commandOptions = new DynamicQueryCommandOptions();
                }
                else
                {
                    _commandOptions = new StandardQueryCommandOptions(_mapping, _mapping.Properties);
                }
            }

            if ((_falseEncountered && _sb.Length == 0) == false)
            {
                string filter;
                if (_pagingFilter == null)
                {
                    string ocFilter = GetOcFilter();
                    if (_sb.Length == 0)
                    {
                        filter = ocFilter;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ocFilter))
                        {
                            filter = $"(&{ocFilter}{_sb})";
                        }
                        else
                        {
                            filter = _exclusiveWhereCount > 1
                                ? $"(&{_sb})"
                                : $"{_sb}";
                        }
                    }

                    if (_customFiltersCount > 0)
                    {
                        var customFilter = _customFiltersCount > 1
                            ? $"(&{_sbCustomFilter})"
                            : _sbCustomFilter.ToString();

                        filter = filter.IsNullOrEmpty()
                            ? customFilter
                            : $"(&{customFilter}{filter})";
                    }
                }
                else
                {
                    filter = _pagingFilter;
                }

                _commandOptions.Filter = filter;
            }
            else
            {
                _commandOptions.YieldNoResults = true;
            }

            if ((_pagingOptions != null || (_controls?.Find(x => x is PageResultRequestControl) != null)) && _skipSize.HasValue)
            {
                throw new InvalidOperationException("Skip relies on Virtual List Views and cannot be used with simple LDAP paging. Please use one method for paging.");
            }

            _commandOptions.WithoutPaging = _withoutPaging;
            _commandOptions.PagingOptions = _pagingOptions;
            _commandOptions.SortingOptions = _sortingOptions;
            _commandOptions.PageSize = _toListPageSize;
            _commandOptions.TakeSize = _takeSize;
            _commandOptions.SkipSize = _skipSize;
            _commandOptions.Controls = _controls;
            _commandOptions.IsLongCount = _isLongCount;
            _commandOptions.AsyncProcessing = _asyncProcessing;

            var queryCommand = QueryCommandFactory.GetCommand(_commandType, _commandOptions, _mapping);
            return queryCommand;
        }

        private string GetOcFilter()
        {
            string category = GetObjectCategory();
            string classes = GetObjectClasses();

            if (_sb.Length == 0 && _customFiltersCount == 0)
            {
                if (string.IsNullOrEmpty(classes))
                {
                    return category;
                }
                if (_mapping.ObjectClasses.Count() > 1)
                {
                    return $"(&{category}{classes})";
                }

                return category == null
                    ? classes
                    : $"(&{category}{classes})";
            }

            return category + classes;
        }

        private string GetObjectCategory()
        {
            if (_ignoreOc != OC.Both && _ignoreOc != OC.ObjectCategory && (_mapping.IncludeObjectCategory || _includeOc == OC.Both || _includeOc == OC.ObjectCategory))
            {
                if (!_mapping.ObjectCategory.IsNullOrEmpty())
                {
                    return $"(objectCategory={_mapping.ObjectCategory})";
                }
            }

            return null;
        }

        private string GetObjectClasses()
        {
            if (_ignoreOc != OC.Both && _ignoreOc != OC.ObjectClasses && (_mapping.IncludeObjectClasses || _includeOc == OC.Both || _includeOc == OC.ObjectClasses))
            {
                if (_mapping.ObjectClasses != null)
                {
#if NET35
                    return string.Join(string.Empty,
                                _mapping.ObjectClasses.Select(oc => string.Format("(objectClass={0})", oc)).ToArray());
#else
                    return string.Join(string.Empty,
                                _mapping.ObjectClasses.Select(oc => $"(objectClass={oc})"));
#endif
                }
            }

            return null;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (QueryableDeclaringTypes.ContainsKey(m.Method.DeclaringType))
            {
                VisitQueryableMethods(m);
            }
            else if (m.Method.DeclaringType == typeof(Filter))
            {
                VisitFilterMethods(m);
            }
            else if (m.Method.DeclaringType == typeof(string))
            {
                VisitStringMethods(m);
            }
            else if (m.Method.DeclaringType == typeof(Enumerable) ||
                     (m.Method.DeclaringType?.GetInterface("IEnumerable`1") != null))
            {
                VisitEnumerableMethods(m);
            }
            else
            {
                string message = m.Method.DeclaringType == null
                                     ? $"Method  {m.Method.Name} could not be translated."
                                     : $"Method {m.Method.Name} on type {m.Method.DeclaringType.FullName} could not be translated.";

                throw new NotSupportedException(message);
            }
            return m;
        }

        private void VisitQueryableMethods(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Where":
                    _exclusiveWhereCount++;
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.StandardCommand;
                    break;

                case "WithoutPaging":
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.StandardCommand;
                    _withoutPaging = true;
                    break;

                case "GetRequest":
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.GetRequestCommand;
                    break;

                case "Any":
                case "AnyAsync":
                    if (m.Arguments.Count > 1) _exclusiveWhereCount++;
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.AnyCommand;
                    break;

                case "FirstOrDefault":
                case "FirstOrDefaultAsync":
                    if (m.Arguments.Count > 1) _exclusiveWhereCount++;
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.FirstOrDefaultCommand;
                    break;

                case "First":
                case "FirstAsync":
                    if (m.Arguments.Count > 1) _exclusiveWhereCount++;
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.FirstCommand;
                    break;

                case "SingleOrDefault":
                case "SingleOrDefaultAsync":
                    if (m.Arguments.Count > 1) _exclusiveWhereCount++;
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.SingleOrDefaultCommand;
                    break;

                case "Single":
                case "SingleAsync":
                    if (m.Arguments.Count > 1) _exclusiveWhereCount++;
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.SingleCommand;
                    break;

                case "LongCount":
                case "Count":
                case "LongCountAsync":
                case "CountAsync":
                    _isLongCount = m.Method.Name == "LongCount" || m.Method.Name == "LongCountAsync";
                    if (m.Arguments.Count > 1) _exclusiveWhereCount++;
                    foreach (Expression t in m.Arguments)
                    {
                        var x = Visit(t);
                    }
                    _commandType = QueryCommandType.CountCommand;
                    break;

                case "ListAttributes":
                case "ListAttributesAsync":
                    Dictionary<string, string> attributes = null;
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(string[]))
                        {
                            attributes = (((ConstantExpression)t).Value as string[]).ToDictionary(s => s);
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _commandOptions = new ListAttributesQueryCommandOptions(attributes);
                    _commandType = QueryCommandType.StandardCommand;
                    break;

                case "FilterWith":
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(string))
                        {
                            _sbCustomFilter.Append(((ConstantExpression)t).Value.ToString());
                            _customFiltersCount++;
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _commandType = QueryCommandType.StandardCommand;
                    break;

                case "Select":
                    foreach (Expression t in m.Arguments)
                    {
                        if (StripQuotes(t) is LambdaExpression lambda)
                        {
                            if (_commandOptions != null) throw new FilterException("Cannot have multiple Select projections.");

                            if (IsDynamic)
                            {
                                var projection = new DynamicSelectProjector(_mapping.Properties)
                                    .ProjectProperties(lambda);

                                _commandOptions = new DynamicQueryCommandOptions(projection);
                            }
                            else
                            {
                                var projection = new SelectProjector(_mapping.Properties)
                                    .ProjectProperties(lambda);
                                _commandOptions = new ProjectionQueryCommandOptions(_mapping, projection);
                            }
                        }
                        else if (t.Type == typeof(string[]))
                        {
                            if (!IsDynamic)
                                throw new FilterException(
                                    "Cannot use a string attribute projection with a static type.");
                            if (_commandOptions != null) throw new FilterException("Cannot have multiple Select projections.");
                            _commandOptions =
                                new DynamicQueryCommandOptions(((ConstantExpression)t).Value as string[]);
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _commandType = QueryCommandType.StandardCommand;
                    break;

                case "ToPage":
                    int pageSize = 0;
                    byte[] nextPage = null;
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(int))
                        {
                            pageSize = (int)((ConstantExpression)t).Value;
                        }
                        else if (t.Type == typeof(byte[]))
                        {
                            nextPage = (byte[])((ConstantExpression)t).Value;
                        }
                        else if (t.Type == typeof(string))
                        {
                            _pagingFilter = (string)((ConstantExpression)t).Value;
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _pagingOptions = new PagingOptions(pageSize, nextPage);
                    break;

                case "InPagesOf":
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(int))
                        {
                            _toListPageSize = (int)((ConstantExpression)t).Value;
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _commandType = QueryCommandType.StandardCommand;
                    break;

                case "ToList":
                case "ToListAsync":
                    foreach (Expression t in m.Arguments)
                    {
                        Visit(t);
                    }
                    _commandType = QueryCommandType.StandardCommand;
                    break;

                case "Take":
                    int takeAmount = 0;
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(int))
                        {
                            takeAmount = (int)((ConstantExpression)t).Value;
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _takeSize = takeAmount;
                    break;

                case "Skip":
                    int skipAmount = 0;
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(int))
                        {
                            skipAmount = (int)((ConstantExpression)t).Value;
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    if (skipAmount < 0) throw new ArgumentException("Skip value must be greater than zero.");
                    _skipSize = skipAmount;
                    break;

                case "WithControls":
                    if (_controls == null) _controls = new List<DirectoryControl>();
                    foreach (Expression t in m.Arguments)
                    {
                        if (t is ConstantExpression && ((ConstantExpression)t).Value is IEnumerable<DirectoryControl> controls)
                        {
                            _controls.AddRange(controls);
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    break;

                case "OrderByDescending":
                case "ThenByDescending":
                    string descRule = null;
                    if (_sortingOptions == null)
                    {
                        _sortingOptions = new SortingOptions();
                    }
                    foreach (Expression t in m.Arguments)
                    {
                        if (StripQuotes(t) is LambdaExpression lambda)
                        {
                            string attribute = null;
                            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                            {
                                attribute = GetMemberName((MemberExpression)lambda.Body);
                            }
                            else if (lambda.Body.NodeType == ExpressionType.Call)
                            {
                                var call = (MethodCallExpression)lambda.Body;
                                if (typeof(IDirectoryAttributes).IsAssignableFrom(call.Method.DeclaringType) && call.Method.Name == "Get")
                                {
                                    attribute = ((ConstantExpression)call.Arguments[0]).Value.ToString();
                                }
                            }
                            _sortingOptions.AddSort(attribute, true);
                        }
                        else if (t.Type == typeof(string))
                        {
                            if (m.Arguments.IndexOf(t) == 1)
                            {
                                _sortingOptions.AddSort(((ConstantExpression)t).Value.ToString(), true);
                            }
                            else
                            {
                                descRule = ((ConstantExpression)t).Value.ToString();
                            }
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _sortingOptions.SetMatchingRule(descRule);
                    break;

                case "OrderBy":
                case "ThenBy":
                    if (_sortingOptions == null)
                    {
                        _sortingOptions = new SortingOptions();
                    }
                    string ascRule = null;
                    foreach (Expression t in m.Arguments)
                    {
                        if (StripQuotes(t) is LambdaExpression lambda)
                        {
                            string attribute = null;
                            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                            {
                                attribute = GetMemberName((MemberExpression)lambda.Body);
                            }
                            else if (lambda.Body.NodeType == ExpressionType.Call)
                            {
                                var call = (MethodCallExpression)lambda.Body;
                                if (typeof(IDirectoryAttributes).IsAssignableFrom(call.Method.DeclaringType) && call.Method.Name == "Get")
                                {
                                    attribute = ((ConstantExpression)call.Arguments[0]).Value.ToString();
                                }
                            }
                            _sortingOptions.AddSort(attribute, false);
                        }
                        else if (t.Type == typeof(string))
                        {
                            if (m.Arguments.IndexOf(t) == 1)
                            {
                                _sortingOptions.AddSort(((ConstantExpression)t).Value.ToString(), false);
                            }
                            else
                            {
                                ascRule = ((ConstantExpression)t).Value.ToString();
                            }
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    _sortingOptions.SetMatchingRule(ascRule);
                    break;

                case "IgnoreOC":
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(OC))
                        {
                            _ignoreOc = (OC)((ConstantExpression)t).Value;
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    break;

                case "IncludeOC":
                    foreach (Expression t in m.Arguments)
                    {
                        if (t.Type == typeof(OC))
                        {
                            _includeOc = (OC)((ConstantExpression)t).Value;
                        }
                        else
                        {
                            Visit(t);
                        }
                    }
                    break;
            }
        }

        private void VisitFilterMethods(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Approximately":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append("~=");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[2]);
                    _shouldCleanFilter = _dnFilter = false;
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "Equal":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append("=");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[2]);
                    _shouldCleanFilter = _dnFilter = false;
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "MatchingRule":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append(":");
                    Visit(m.Arguments[2]);
                    _sb.Append(":=");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[3]);
                    _shouldCleanFilter = _dnFilter = false;
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "EqualAnything":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append("=");
                    _sb.Append("*");
                    _dnFilter = false;
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "EqualAny":

                    if (!(((ConstantExpression)m.Arguments[2]).Value is IEnumerable<string> values) || !values.Any())
                    {
                        throw new FilterException("Cannot create an EqualAny filter with null or empty values.");
                    }
                    int count = values.Count();
                    if (count > 1)
                    {
                        _sb.Append("(|");
                    }
                    _dnFilter = true;
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    foreach (var value in values)
                    {
                        _sb.Append("(");
                        if (_not) _sb.Append("!(");
                        Visit(m.Arguments[1]);
                        _sb.Append("=");
                        Visit(Expression.Constant(value));
                        if (_not) _sb.Append(")");
                        _sb.Append(")");
                    }
                    _shouldCleanFilter = _dnFilter = false;
                    if (count > 1)
                    {
                        _sb.Append(")");
                    }
                    break;

                case "StartsWith":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append("=");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[2]);
                    _sb.Append("*");
                    _shouldCleanFilter = _dnFilter = false;
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "EndsWith":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append("=");
                    _sb.Append("*");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[2]);
                    _shouldCleanFilter = _dnFilter = false;
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "Like":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append("=");
                    _sb.Append("*");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[2]);
                    _sb.Append("*");
                    _shouldCleanFilter = _dnFilter = false;
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "GreaterThanOrEqual":
                    _sb.Append("(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append(_not ? "<=" : ">=");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[2]);
                    _shouldCleanFilter = _dnFilter = false;
                    _sb.Append(")");
                    break;

                case "GreaterThan":
                    if (_not)
                    {
                        _sb.Append("(");
                        _dnFilter = true;
                        Visit(m.Arguments[1]);
                        _sb.Append("<=");
                        _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                        Visit(m.Arguments[2]);
                        _shouldCleanFilter = _dnFilter = false;
                        _sb.Append(")");
                    }
                    else
                    {
                        bool shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                        _sb.Append("(&(");
                        _dnFilter = true;
                        Visit(m.Arguments[1]);
                        _shouldCleanFilter = shouldCleanFilter;
                        _sb.Append(">=");
                        Visit(m.Arguments[2]);
                        _sb.Append(")(!(");
                        _shouldCleanFilter = false;
                        Visit(m.Arguments[1]);
                        _sb.Append("=");
                        _shouldCleanFilter = shouldCleanFilter;
                        Visit(m.Arguments[2]);
                        _shouldCleanFilter = _dnFilter = false;
                        _sb.Append(")))");
                    }
                    break;

                case "LessThanOrEqual":
                    _sb.Append("(");
                    _dnFilter = true;
                    Visit(m.Arguments[1]);
                    _sb.Append(_not ? ">=" : "<=");
                    _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                    Visit(m.Arguments[2]);
                    _shouldCleanFilter = _dnFilter = false;
                    _sb.Append(")");
                    break;

                case "LessThan":
                    if (_not)
                    {
                        _sb.Append("(");
                        _dnFilter = true;
                        Visit(m.Arguments[1]);
                        _sb.Append(">=");
                        _shouldCleanFilter = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                        Visit(m.Arguments[2]);
                        _shouldCleanFilter = _dnFilter = false;
                        _sb.Append(")");
                    }
                    else
                    {
                        bool shouldCleanValue = (bool)((ConstantExpression)m.Arguments.Last()).Value;
                        _sb.Append("(&(");
                        _dnFilter = true;
                        Visit(m.Arguments[1]);
                        _sb.Append("<=");
                        _shouldCleanFilter = shouldCleanValue;
                        Visit(m.Arguments[2]);
                        _sb.Append(")(!(");
                        _shouldCleanFilter = false;
                        Visit(m.Arguments[1]);
                        _sb.Append("=");
                        _shouldCleanFilter = shouldCleanValue;
                        Visit(m.Arguments[2]);
                        _shouldCleanFilter = _dnFilter = false;
                        _sb.Append(")))");
                    }
                    break;
            }
        }

        private void VisitStringMethods(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "ToUpper":
                case "ToUpperInvariant":
                case "ToLower":
                case "ToLowerInvariant":
                    Visit(m.Object);
                    break;

                case "Contains":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    Visit(m.Object);
                    _sb.Append("=*");
                    Visit(m.Arguments[0]);
                    _sb.Append("*");
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "StartsWith":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    Visit(m.Object);
                    _sb.Append("=");
                    Visit(m.Arguments[0]);
                    _sb.Append("*");
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "EndsWith":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    Visit(m.Object);
                    _sb.Append("=*");
                    Visit(m.Arguments[0]);
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                case "Equals":
                    _sb.Append("(");
                    if (_not) _sb.Append("!(");
                    Visit(m.Object);
                    _sb.Append("=");
                    Visit(m.Arguments[0]);
                    if (_not) _sb.Append(")");
                    _sb.Append(")");
                    break;

                default:
                    throw new NotSupportedException("String method " + m.Method.Name + " is not supported.");
            }
        }

        private void VisitEnumerableMethods(MethodCallExpression m)
        {
            if (m.Method.Name.Equals("Contains"))
            {
                if (m.Arguments.Count == 1)
                {
                    if (m.Object is MemberExpression)
                    {
                        _sb.Append("(");
                        if (_not) _sb.Append("!(");
                        Visit(m.Object);
                        _sb.Append("=");
                        Visit(m.Arguments[0]);
                        if (_not) _sb.Append(")");
                        _sb.Append(")");
                    }
                    else
                    {
                        VisitArgumentsForContains(m);
                    }
                }
                else
                {
                    if (m.Arguments[0] is MemberExpression)
                    {
                        _sb.Append("(");
                        if (_not) _sb.Append("!(");
                        Visit(m.Arguments[0]);
                        _sb.Append("=");
                        Visit(m.Arguments[1]);
                        if (_not) _sb.Append(")");
                        _sb.Append(")");
                    }
                    else
                    {
                        VisitArgumentsForContains(m);
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Collection method " + m.Method.Name + " is not supported.");
            }
        }

        private void VisitArgumentsForContains(MethodCallExpression m)
        {
            IEnumerable list;
            Expression member;
            var enumerableVisitor = new EnumerableVisitor();
            var memberVisitor = new MemberVisitor();

            if (m.Arguments.Count == 1)
            {
                list = enumerableVisitor.GetList(m.Object);
                member = memberVisitor.GetMember(m.Arguments[0]);
            }
            else
            {
                list = enumerableVisitor.GetList(m.Arguments[0]);
                member = memberVisitor.GetMember(m.Arguments[1]);
            }

            if (list == null || (member as MemberExpression) == null)
            {
                throw new FilterException("Could not translate Contains.");
            }

            _sb.Append("(|");
            foreach (object value in list)
            {
                Visit(Expression.Equal(member, Expression.Constant(value)));
            }
            _sb.Append(")");
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    _not = true;
                    Visit(u.Operand);
                    _not = false;
                    break;

                case ExpressionType.Quote:
                    Visit(!_dnFilter ? StripQuotes(u) : u.Operand);
                    break;

                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported",
                                                                  u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            bool rightIsNull = new NullVisitor().IsNull(b.Right);
            _binaryExpression = true;
            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                    if (_expressionStack.Count == 0 || _expressionStack.Peek() != ExpressionType.AndAlso) _sb.Append("(&");
                    _expressionStack.Push(ExpressionType.AndAlso);
                    Visit(b.Left);
                    Visit(b.Right);
                    _expressionStack.Pop();
                    if (_expressionStack.Count == 0 || _expressionStack.Peek() != ExpressionType.AndAlso) _sb.Append(")");
                    break;

                case ExpressionType.OrElse:
                    if (_expressionStack.Count == 0 || _expressionStack.Peek() != ExpressionType.OrElse) _sb.Append("(|");
                    _expressionStack.Push(ExpressionType.OrElse);
                    Visit(b.Left);
                    Visit(b.Right);
                    _expressionStack.Pop();
                    if (_expressionStack.Count == 0 || _expressionStack.Peek() != ExpressionType.OrElse) _sb.Append(")");
                    break;

                case ExpressionType.Equal:
                    _sb.Append("(");
                    if (rightIsNull)
                    {
                        if (!_not) _sb.Append("!(");
                    }
                    else
                    {
                        if (_not) _sb.Append("!(");
                    }
                    Visit(b.Left);
                    _sb.Append("=");
                    Visit(b.Right);
                    if (rightIsNull)
                    {
                        if (!_not) _sb.Append(")");
                    }
                    else
                    {
                        if (_not) _sb.Append(")");
                    }
                    _sb.Append(")");
                    break;

                case ExpressionType.NotEqual:
                    _sb.Append("(");
                    if (rightIsNull)
                    {
                        if (_not) _sb.Append("!(");
                    }
                    else
                    {
                        if (!_not) _sb.Append("!(");
                    }
                    Visit(b.Left);
                    _sb.Append("=");
                    Visit(b.Right);
                    if (rightIsNull)
                    {
                        if (_not) _sb.Append(")");
                    }
                    else
                    {
                        if (!_not) _sb.Append(")");
                    }
                    _sb.Append(")");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _sb.Append("(");
                    Visit(b.Left);
                    _sb.Append(_not ? "<=" : ">=");
                    Visit(b.Right);
                    _sb.Append(")");
                    break;

                case ExpressionType.GreaterThan:
                    Expression greaterThanLeft = Expression.GreaterThanOrEqual(b.Left, b.Right);
                    Expression greaterThanRight = Expression.Not(Expression.Equal(b.Left, b.Right));
                    Visit(Expression.AndAlso(greaterThanLeft, greaterThanRight));
                    break;

                case ExpressionType.LessThanOrEqual:
                    _sb.Append("(");
                    Visit(b.Left);
                    _sb.Append(_not ? ">=" : "<=");
                    Visit(b.Right);
                    _sb.Append(")");
                    break;

                case ExpressionType.LessThan:
                    Expression lessThanLeft = Expression.LessThanOrEqual(b.Left, b.Right);
                    Expression lessThanRight = Expression.Not(Expression.Equal(b.Left, b.Right));
                    Visit(Expression.AndAlso(lessThanLeft, lessThanRight));
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }
            _binaryExpression = false;
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;
            if (q != null)
            {
            }
            else if (c.Value == null || string.Empty.Equals(c.Value))
            {
                _sb.Append(_dnFilter ? "" : "*");
            }
            else if (c.Value is Expression)
            {
                Visit(c.Value as Expression);
            }
            else if (_dnFilter)
            {
                _sb.Append(_shouldCleanFilter ? (c.Value as string).CleanFilterValue() : (c.Value as string));
            }
            else if (c.Type == typeof(PartialResultProcessing))
            {
                _asyncProcessing = (PartialResultProcessing)c.Value;
            }
            else if (_currentProperty != null)
            {
                var str = _currentProperty.FormatValueToFilter(c.Value);

                if (str.IsNullOrEmpty())
                {
                    throw new FilterException("Cannot use white spaces for a value in a filter.");
                }

                _sb.Append(str);
            }
            else if (!_binaryExpression && false.Equals(c.Value))
            {
                _falseEncountered = true;
            }
            return c;
        }

        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            // ReSharper disable PossibleNullReferenceException
            return Visit((iv.Expression as LambdaExpression).Body);
            // ReSharper restore PossibleNullReferenceException
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var isHasValue = false;
            MemberExpression copy;
            if (m.Expression.NodeType != ExpressionType.Parameter)
            {
                copy = m.Expression as MemberExpression;
                if (copy == null || copy.Expression.NodeType != ExpressionType.Parameter)
                {
                    copy = m;
                }
                else
                {
                    isHasValue = m.Member.Name == "HasValue" && m.Member.DeclaringType.Name == "Nullable`1";
                }
            }
            else
            {
                copy = m;
            }

            if (copy.Expression.NodeType == ExpressionType.Parameter)
            {
                if (!_binaryExpression && isHasValue)
                {
                    Visit(Expression.NotEqual(copy, Expression.Constant(null)));
                }
                else if ((!_binaryExpression && copy.Type == typeof(bool)))
                {
                    ConstantExpression constat = _not ? Expression.Constant(false) : Expression.Constant(true);
                    bool reverseNegation = false;
                    if (_not)
                    {
                        reverseNegation = true;
                        _not = false;
                    }
                    Visit(Expression.Equal(copy, constat));
                    if (reverseNegation)
                    {
                        _not = true;
                    }
                }
                else
                {
                    _currentProperty = _mapping.GetPropertyMapping(copy.Member.Name);
                    _sb.Append(_currentProperty.AttributeName);
                }
                return m;
            }
            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }

        private string GetMemberName(MemberExpression m)
        {
            if (!_mapping.Properties.TryGetValue(m.Member.Name, out string attributeName))
            {
                throw new MappingException(
                    $"{m.Member.Name} was not found as a mapped property on {_mapping.Type.FullName}");
            }

            return attributeName;
        }
    }
}