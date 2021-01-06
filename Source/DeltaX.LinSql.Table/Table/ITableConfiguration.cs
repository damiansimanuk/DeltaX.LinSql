namespace DeltaX.LinSql.Table
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    public interface ITableConfiguration
    {
        public string Name { get; }
        public string Schema { get; set; }
        public string Identifier { get; set; }
        public IEnumerable<ColumnConfiguration> Columns { get; }
        public ColumnConfiguration GetIdentityColumn();
        public IEnumerable<ColumnConfiguration> GetPrimaryKeysColumn();
        public IEnumerable<ColumnConfiguration> GetSelectColumns();
        public IEnumerable<ColumnConfiguration> GetSelectColumnsList();
        public IEnumerable<ColumnConfiguration> GetInsertColumns();
        public IEnumerable<ColumnConfiguration> GetUpdateColumns();
        void InvalidatePk();
        void AddColumn(ColumnConfiguration config);
        void AddColumn(string dtoFieldName, string dbColumnName = null, bool isIdentity = false, bool isPrimaryKey = false, Action<ColumnConfiguration> configColumn = null);
        void AddColumn(string dtoFieldName, Action<ColumnConfiguration> configColumn);
    }

    public interface ITableConfiguration<TTable> : ITableConfiguration
            where TTable : class
    {
        void AddColumn<TProperty>(Expression<Func<TTable, TProperty>> property, string dbColumnName = null, bool isIdentity = false, bool isPrimaryKey = false, Action<ColumnConfiguration> configColumn = null);
        void AddColumn<TProperty>(Expression<Func<TTable, TProperty>> property, Action<ColumnConfiguration> configColumn);
        void SetIdentity<TProperty>(Expression<Func<TTable, TProperty>> property);
    }
}