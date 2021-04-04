namespace DeltaX.LinSql.Table
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DialectQuery
    {
        public string EncapsulationSql { get; private set; }
        public string IdentityQueryFormatSql { get; private set; }
        public string PagedListQueryFormatSql { get; private set; }
        public string SingleQueryFormatSql { get; private set; }
        public string InsertQueryFormatSql { get; private set; }
        public string DeleteQueryFormatSql { get; private set; }
        public string UpdateQueryFormatSql { get; private set; }
        public string CountQueryFormatSql { get; private set; }
        public string LimitFormatSql { get; private set; }
        public DialectType Dialect { get; private set; }

        public DialectQuery(DialectType dialect)
        {
            DefaultInitialization();
            Dialect = dialect;

            switch (dialect)
            {
                case DialectType.PostgreSQL:
                    IdentityQueryFormatSql = "SELECT LASTVAL() AS id";
                    break;
                case DialectType.SQLite:
                    IdentityQueryFormatSql = "SELECT LAST_INSERT_ROWID() AS id";
                    break;
                case DialectType.MySQL:
                    EncapsulationSql = "`{0}`";
                    IdentityQueryFormatSql = "SELECT LAST_INSERT_ID() AS id";
                    PagedListQueryFormatSql = "SELECT \n" +
                        "\t{SelectColumns} \n" +
                        "FROM {TableName} \n" +
                        "{WhereClause} \n" +
                        "{OrderByClause} \n" +
                        "LIMIT {SkipCount},{RowsPerPage} ";
                    break;
                case DialectType.SQLServer:
                    EncapsulationSql = "[{0}]";
                    IdentityQueryFormatSql = "SELECT SCOPE_IDENTITY() AS [id]";
                    LimitFormatSql = "OFFSET {SkipCount} ROWS FETCH FIRST {RowsPerPage} ROWS ONLY ";
                    PagedListQueryFormatSql = "SELECT \n" +
                        "\t{SelectColumns} \n" +
                        "FROM {TableName} \n" +
                        "{WhereClause} \n" +
                        "{OrderByClause} \n" +
                        "OFFSET {SkipCount} ROWS FETCH FIRST {RowsPerPage} ROWS ONLY ";
                    break;
            }
        }

        private void DefaultInitialization()
        {
            EncapsulationSql = "\"{0}\"";
            IdentityQueryFormatSql = "SELECT LAST_INSERT_ROWID() AS id";
            LimitFormatSql = "LIMIT {RowsPerPage} OFFSET {SkipCount} ";
            PagedListQueryFormatSql = "SELECT \n" +
                "\t{SelectColumns} \n" +
                "FROM {TableName} \n" +
                "{WhereClause} \n" +
                "{OrderByClause} \n" +
                "LIMIT {RowsPerPage} OFFSET {SkipCount} ";
            SingleQueryFormatSql = "SELECT \n" +
                "\t{SelectColumns} \n" +
                "FROM {TableName} \n" +
                "{WhereClause} \n";
            InsertQueryFormatSql = "INSERT INTO {TableName} \n" +
                "\t({InsertColumns}) " +
                "VALUES\n" +
                "\t({InsertValues}) ";
            DeleteQueryFormatSql = "DELETE FROM {TableName} \n{WhereClause}";
            UpdateQueryFormatSql = "UPDATE {TableName} SET\n\t {SetColumns} \n{WhereClause}";
            CountQueryFormatSql = "SELECT count(*) as Count FROM {TableName} \n{WhereClause}";
        }

        public string GetTableName(ITableConfiguration table, string tableAlias = null)
        {
            string tableName = string.IsNullOrEmpty(table.Schema) ? table.Name : $"{table.Schema}.{table.Name}";

            return string.IsNullOrEmpty(tableAlias) ? tableName : $"{tableName} {tableAlias}";
        }

        public string Encapsulation(string dbWord, string tableAlias = null)
        {
            return string.IsNullOrEmpty(tableAlias)
                ? string.Format(EncapsulationSql, dbWord)
                : $"{tableAlias}." + string.Format(EncapsulationSql, dbWord);
        }

        public string GetWhereClausePK(ITableConfiguration table, string tableAlias = null)
        {
            var pks = table.GetPrimaryKeysColumn();
            return "WHERE " + string.Join(" AND ", pks.Select(c => $"{Encapsulation(c.DbColumnName, tableAlias)} = @{c.DtoFieldName}"));
        }

        public string GetColumnFormated(ColumnConfiguration column, string tableAlias = null)
        {
            return column.DbAlias == null
                 ? Encapsulation(column.DbColumnName, tableAlias)
                 : $"{Encapsulation(column.DbColumnName, tableAlias)} as {Encapsulation(column.DbAlias)}";
        }

        public string GetSelectColumns(ITableConfiguration table, string tableAlias = null)
        {
            var columns = table.GetSelectColumns();
            return string.Join("\n\t, ", columns.Select(c => GetColumnFormated(c, tableAlias)));
        }

        public string GetSelectColumnsList(ITableConfiguration table, string tableAlias = null)
        {
            var columns = table.GetSelectColumnsList();
            return string.Join("\n\t, ", columns.Select(c => GetColumnFormated(c, tableAlias)));
        }

        public string GetInsertColumns(
            ITableConfiguration table,
            IEnumerable<string> fieldsToInsert = null)
        {
            var columns = table.GetInsertColumns();

            if (fieldsToInsert != null && fieldsToInsert.Any())
            {
                columns = columns.Where(c => fieldsToInsert.Contains(c.DtoFieldName) || fieldsToInsert.Contains(c.DbColumnName));
            }

            return string.Join(", ", columns.Select(c => Encapsulation(c.DbColumnName)));
        }

        public string GetInsertValues(
            ITableConfiguration table,
            IEnumerable<string> fieldsToInsert = null)
        {
            var columns = table.GetInsertColumns();

            if (fieldsToInsert != null && fieldsToInsert.Any())
            {
                columns = columns.Where(c => fieldsToInsert.Contains(c.DtoFieldName) || fieldsToInsert.Contains(c.DbColumnName));
            }

            return string.Join(", ", columns.Select(c => $"@{c.DtoFieldName}"));
        }

        public string GetSetColumns(
            ITableConfiguration table,
            IEnumerable<string> fieldsToSet = null,
            string tableAlias = null)
        {
            var columns = table.GetUpdateColumns();

            if (fieldsToSet != null && fieldsToSet.Any())
            {
                columns = table.Columns.Where(c => fieldsToSet.Contains(c.DtoFieldName) || fieldsToSet.Contains(c.DbColumnName));
            }

            return string.Join("\n\t, ", columns.Select(c => $"{Encapsulation(c.DbColumnName, tableAlias)} = @{c.DtoFieldName}"));
        }


        public string GetPagedListQuery(
            ITableConfiguration table,
            int skipCount = 0,
            int rowsPerPage = 1000,
            string whereClause = null,
            string orderByClause = null)
        {
            var query = PagedListQueryFormatSql
                .Replace("{SelectColumns}", GetSelectColumnsList(table, table.Identifier))
                .Replace("{TableName}", GetTableName(table, table.Identifier))
                .Replace("{WhereClause}", whereClause)
                .Replace("{OrderByClause}", orderByClause)
                .Replace("{SkipCount}", skipCount.ToString())
                .Replace("{RowsPerPage}", rowsPerPage.ToString());

            return query;
        }

        public string GetSingleQuery(
            ITableConfiguration table,
            string whereClause = null)
        {
            if (string.IsNullOrEmpty(whereClause))
            {
                whereClause = GetWhereClausePK(table, table.Identifier);
            }

            var query = SingleQueryFormatSql
                .Replace("{SelectColumns}", GetSelectColumns(table, table.Identifier))
                .Replace("{TableName}", GetTableName(table, table.Identifier))
                .Replace("{WhereClause}", whereClause);

            return query;
        }

        public string GetInsertQuery(
            ITableConfiguration table,
            IEnumerable<string> fieldsToInsert = null)
        {
            var query = InsertQueryFormatSql
                .Replace("{TableName}", GetTableName(table))
                .Replace("{InsertColumns}", GetInsertColumns(table, fieldsToInsert))
                .Replace("{InsertValues}", GetInsertValues(table, fieldsToInsert));

            return query;
        }

        public string GetDeleteQuery(
            ITableConfiguration table,
            string whereClause = null)
        {
            if (string.IsNullOrEmpty(whereClause))
            {
                if (!table.GetPrimaryKeysColumn().Any())
                {
                    throw new Exception("Can not detected Primary key for delete clause!");
                }
                whereClause = GetWhereClausePK(table);
            }

            var query = DeleteQueryFormatSql
                .Replace("{TableName}", GetTableName(table))
                .Replace("{WhereClause}", whereClause);

            return query;
        }

        public string GetUpdateQuery(
            ITableConfiguration table,
            string whereClause = null,
            IEnumerable<string> fieldsToSet = null)
        {
            if (string.IsNullOrEmpty(whereClause))
            {
                if (!table.GetPrimaryKeysColumn().Any())
                {
                    throw new Exception("Can not detected Primary key for update clause!");
                }
                whereClause = GetWhereClausePK(table);
            }

            var query = UpdateQueryFormatSql
                .Replace("{TableName}", GetTableName(table))
                .Replace("{SetColumns}", GetSetColumns(table, fieldsToSet))
                .Replace("{WhereClause}", whereClause);

            return query;
        }

        public string GetCountQuery(
            ITableConfiguration table,
            string whereClause = null)
        {
            var query = CountQueryFormatSql
                .Replace("{TableName}", GetTableName(table))
                .Replace("{WhereClause}", whereClause);

            return query;
        } 
    }
}
