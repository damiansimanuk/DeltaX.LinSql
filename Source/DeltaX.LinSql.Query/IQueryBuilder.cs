using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaX.LinSql.Query
{
    public interface IQueryBuilder { /*public TableQueryBuilder Builder { get; }*/ }
    public interface IQueryBuilder<T1> : IQueryBuilder { }
    public interface IQueryBuilder<T1, T2> : IQueryBuilder<T1> { }
    public interface IQueryBuilder<T1, T2, T3> : IQueryBuilder<T1, T2> { }
    public interface IQueryBuilder<T1, T2, T3, T4> : IQueryBuilder<T1, T2, T3> { }
    public interface IQueryBuilder<T1, T2, T3, T4, T5> : IQueryBuilder<T1, T2, T3, T4> { }
    public interface IQueryBuilder<T1, T2, T3, T4, T5, T6> : IQueryBuilder<T1, T2, T3, T4, T5> { }
    public interface IQueryBuilder<T1, T2, T3, T4, T5, T6, T7> : IQueryBuilder<T1, T2, T3, T4, T5, T6> { }
}
