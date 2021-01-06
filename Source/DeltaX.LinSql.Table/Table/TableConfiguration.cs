namespace DeltaX.LinSql.Table
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class TableConfiguration<TTable> : ITableConfiguration<TTable>
        where TTable : class
    {

        private List<ColumnConfiguration> columns;

        public TableConfiguration(string name, string schema, string identifier = null)
        {
            Name = name;
            Schema = schema;
            Table = Activator.CreateInstance<TTable>();
            columns = new List<ColumnConfiguration>();
            Identifier = identifier ?? TableConfigurationIdentifierCreator.GetIdentifier();
        }


        public TTable Table { get; private set; }
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Identifier { get; set; }
        public IEnumerable<ColumnConfiguration> Columns => columns;

        public void AddColumn(ColumnConfiguration config)
        {
            config.TableDto ??= Table;
            columns.Add(config);
        }

        public void AddColumn(string dtoFieldName, string dbColumnName = null,
            bool isIdentity = false, bool isPrimaryKey = false,
            Action<ColumnConfiguration> configColumn = null)
        {
            var config = new ColumnConfiguration(Table, dtoFieldName, dbColumnName, isIdentity, isPrimaryKey);
            configColumn?.Invoke(config);
            AddColumn(config);
        }

        public void AddColumn(string dtoFieldName, Action<ColumnConfiguration> configColumn)
        {
            AddColumn(dtoFieldName, null, false, false, configColumn);
        }

        public void AddColumn<TProperty>(Expression<Func<TTable, TProperty>> property,
            string dbColumnName = null, bool isIdentity = false, bool isPrimaryKey = false,
            Action<ColumnConfiguration> configColumn = null)
        {
            PropertyInfo propInfo = GetPropertyInfo(property);
            var dtoFieldName = propInfo?.Name;

            AddColumn(dtoFieldName, dbColumnName, isIdentity, isPrimaryKey, configColumn);
        }

        public void AddColumn<TProperty>(Expression<Func<TTable, TProperty>> property, Action<ColumnConfiguration> configColumn)
        {
            AddColumn(property, null, false, false, configColumn);
        }

        public void SetIdentity<TProperty>(Expression<Func<TTable, TProperty>> property)
        {
            PropertyInfo propInfo = GetPropertyInfo(property);
            var dtoFieldName = propInfo?.Name;
            var type = propInfo.PropertyType;

            columns.ForEach(c => c.IsIdentity = false);
            var column = columns.FirstOrDefault(c => c.DtoFieldName == dtoFieldName);
            _ = column ?? throw new ArgumentException("Column 'dtoFieldName' cannot be null", nameof(property));
            column.IsIdentity = true;
        }


        public void InvalidatePk()
        {
            var pks = columns.Where(c => c.IsPrimaryKey || c.IsIdentity);
            if (pks.Any())
            {
                return;
            }

            pks = columns.Where(c => c.DtoFieldName == "Id" || $"{Name}Id" == c.DbColumnName);
            foreach (var pk in pks)
            {
                pk.IsPrimaryKey = true;
            }
        }

        public ColumnConfiguration GetIdentityColumn()
        {
            return Columns.FirstOrDefault(c => c.IsIdentity);
        }

        public IEnumerable<ColumnConfiguration> GetPrimaryKeysColumn()
        {
            return Columns.Where(c => c.IsIdentity || c.IsPrimaryKey).ToArray();
        }

        public IEnumerable<ColumnConfiguration> GetSelectColumns()
        {
            return Columns.Where(c => !c.IgnoreSelect).ToArray();
        }

        public IEnumerable<ColumnConfiguration> GetSelectColumnsList()
        {
            return Columns.Where(c => !c.IgnoreSelect && !c.IgnoreSelectList).ToArray();
        }

        public IEnumerable<ColumnConfiguration> GetInsertColumns()
        {
            return Columns.Where(c => !c.IgnoreInsert && !c.IsIdentity).ToArray();
        }

        public IEnumerable<ColumnConfiguration> GetUpdateColumns()
        {
            return Columns.Where(c => !c.IgnoreUpdate && !c.IsIdentity && !c.IsPrimaryKey).ToArray();
        }

        private PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TTable, TProperty>> property)
        {
            MemberExpression member = property?.Body as MemberExpression;
            return member?.Member as PropertyInfo;
        }

        public static TableConfiguration<TTable> AutoConfigure() 
        {
            var type = typeof(TTable);

            var tableAttrib = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            var table = new TableConfiguration<TTable>(
                name: (tableAttrib != null ? tableAttrib.Name : type.Name),
                schema: (tableAttrib != null ? tableAttrib.Schema : null));

            
            foreach(var x in type.GetProperties() )
            {
                var isKey = x.GetCustomAttribute(typeof(KeyAttribute)) != null
                       || x.Name.ToUpper() == "ID"
                       || x.Name == table.Name + "Id";
                var alias = ((ColumnAttribute)x.GetCustomAttribute(typeof(ColumnAttribute)))?.Name;
                var allowMapped = x.GetCustomAttribute(typeof(NotMappedAttribute)) == null;
                var isAutoGenerated = x.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) != null;
                var notEditable = ((EditableAttribute)x.GetCustomAttribute(typeof(EditableAttribute)))?.AllowEdit == false;

                if (allowMapped)
                {
                    var column = new ColumnConfiguration(x.Name, alias, isKey, isKey);
                    column.IgnoreInsert = isAutoGenerated;
                    column.IgnoreUpdate = isAutoGenerated || notEditable;
                    table.AddColumn(column);
                }
            }

            return table;
        }
    }
}