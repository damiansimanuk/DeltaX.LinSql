﻿namespace DeltaX.LinSql.Table
{
    using System;
    using System.Collections.Generic;
    using System.Linq; 

    public class TableQueryFactory
    {
        private readonly Dictionary<Type, ITableConfiguration> tablesConfig;

        public TableQueryFactory(DialectQuery dialectQuery)
        {
            tablesConfig = new Dictionary<Type, ITableConfiguration>();
            DialectQuery = dialectQuery;
            instance = this;
        }

        public TableQueryFactory(DialectType dialect) : this(new DialectQuery(dialect))
        {
        }

        private static TableQueryFactory instance;

        public static TableQueryFactory GetInstance()
        {
            return instance ??= new TableQueryFactory(DialectType.SQLite);
        }

        public DialectQuery DialectQuery { get; private set; }

        public ITableConfiguration GetTable<TTable>()
            where TTable : class
        {
            return GetTable(typeof(TTable));
        }
          
        public ITableConfiguration GetTable(Type type)
        {
            return tablesConfig.GetValueOrDefault(type)
                 ?? throw new ArgumentException($"Table type '{type.Name}' is not configurated!", type.Name);
        }

        public bool IsConfiguredTable<TTable>()
        {
            return tablesConfig.GetValueOrDefault(typeof(TTable)) != default;
        }

        public bool IsConfiguredTable(Type type)
        {
            return tablesConfig.GetValueOrDefault(type) != default;
        }

        public IDictionary<Type, ITableConfiguration> GetConfiguredTables()
        {
            return tablesConfig;
        }

        public void ConfigureTable<TTable>(string tableName, Action<ITableConfiguration<TTable>> configTable)
            where TTable : class
        {
            ConfigureTable(null, tableName, null, configTable);
        }

        public void ConfigureTable<TTable>(string schema, string tableName, Action<ITableConfiguration<TTable>> configTable)
            where TTable : class
        {
            ConfigureTable(schema, tableName, null, configTable);
        }

        public void ConfigureTable<TTable>(string schema, string tableName, string identifier, Action<ITableConfiguration<TTable>> configTable)
           where TTable : class
        {
            var table = new TableConfiguration<TTable>(tableName, schema, identifier);
            configTable.Invoke(table);
            AddTable(table);
        }

        public void AddTable<TTable>()
           where TTable : class
        {
            if (!IsConfiguredTable<TTable>())
            {
                var table = TableConfiguration<TTable>.AutoConfigure();
                AddTable(table);
            }
        }

        public void AddTable<TTable>(TableConfiguration<TTable> table)
           where TTable : class
        {
            table.InvalidatePk();
            tablesConfig.Add(typeof(TTable), table);
        } 


        public string GetPagedListQuery<TTable>(int skipCount = 0, int rowsPerPage = 1000, string whereClause = null, string orderByClause = null)
            where TTable : class
        {
            var table = GetTable<TTable>();

            var query = DialectQuery.PagedListQueryFormatSql;
            query = query.Replace("{SelectColumns}", DialectQuery.GetSelectColumnsList(table, table.Identifier));
            query = query.Replace("{TableName}", DialectQuery.GetTableName(table, table.Identifier));
            query = query.Replace("{WhereClause}", whereClause);
            query = query.Replace("{OrderByClause}", orderByClause);
            query = query.Replace("{SkipCount}", skipCount.ToString());
            query = query.Replace("{RowsPerPage}", rowsPerPage.ToString());
            return query;
        }


        public string GetSingleQuery<TTable>(string whereClause = null)
            where TTable : class
        {
            var table = GetTable<TTable>();

            if (string.IsNullOrEmpty(whereClause))
            {
                whereClause = DialectQuery.GetWhereClausePK(table, table.Identifier);
            }

            var query = DialectQuery.SingleQueryFormatSql;
            query = query.Replace("{SelectColumns}", DialectQuery.GetSelectColumns(table, table.Identifier));
            query = query.Replace("{TableName}", DialectQuery.GetTableName(table, table.Identifier));
            query = query.Replace("{WhereClause}", whereClause);
            return query;
        }

        public string GetInsertQuery<TTable>(IEnumerable<string> fieldsToInsert = null)
            where TTable : class
        {
            var table = GetTable<TTable>();

            var query = DialectQuery.InsertQueryFormatSql;
            query = query.Replace("{TableName}", DialectQuery.GetTableName(table));
            query = query.Replace("{InsertColumns}", DialectQuery.GetInsertColumns(table, fieldsToInsert));
            query = query.Replace("{InsertValues}", DialectQuery.GetInsertValues(table, fieldsToInsert));
            return query;
        }

        public string GetDeleteQuery<TTable>(string whereClause = null)
            where TTable : class
        {
            var table = GetTable<TTable>();

            if (string.IsNullOrEmpty(whereClause))
            {
                if (!table.GetPrimaryKeysColumn().Any())
                {
                    throw new Exception("Can not detected Primary key for delete clause!");
                }
                whereClause = DialectQuery.GetWhereClausePK(table);
            }

            var query = DialectQuery.DeleteQueryFormatSql;
            query = query.Replace("{TableName}", DialectQuery.GetTableName(table));
            query = query.Replace("{WhereClause}", whereClause);
            return query;
        }

        public string GetUpdateQuery<TTable>(string whereClause = null, IEnumerable<string> fieldsToSet = null)
            where TTable : class
        {
            var table = GetTable<TTable>();

            if (string.IsNullOrEmpty(whereClause))
            {
                if (!table.GetPrimaryKeysColumn().Any())
                {
                    throw new Exception("Can not detected Primary key for update clause!");
                }
                whereClause = DialectQuery.GetWhereClausePK(table);
            }

            var query = DialectQuery.UpdateQueryFormatSql;
            query = query.Replace("{TableName}", DialectQuery.GetTableName(table));
            query = query.Replace("{SetColumns}", DialectQuery.GetSetColumns(table, fieldsToSet));
            query = query.Replace("{WhereClause}", whereClause);
            return query;
        }


        public string GetCountQuery<TTable>(string whereClause = null)
            where TTable : class
        {
            var table = GetTable<TTable>();

            var query = DialectQuery.CountQueryFormatSql;
            query = query.Replace("{TableName}", DialectQuery.GetTableName(table));
            query = query.Replace("{WhereClause}", whereClause);

            return query;
        }

        public string GetSelectColumns<TTable>(bool useTableAlias=true)
         where TTable : class
        {
            var table = GetTable<TTable>();

            return DialectQuery.GetSelectColumns(table, useTableAlias ? table.Identifier : null);
        }

        public ColumnConfiguration GetIdentityColumn<TTable>()
           where TTable : class
        {
            return GetTable<TTable>().GetIdentityColumn();
        }

        public IEnumerable<ColumnConfiguration> GetPrimaryKeysColumn<TTable>()
           where TTable : class
        {
            return GetTable<TTable>().GetPrimaryKeysColumn();
        }
    }
}
