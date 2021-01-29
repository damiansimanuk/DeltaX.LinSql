﻿namespace DeltaX.LinSql.Table
{
    using System;
    using System.Reflection;

    public class ColumnConfiguration
    {
        private PropertyInfo propertyInfo;

        public string DtoFieldName { get; private set; }
        public string DbColumnName { get; set; }
        public string DbAlias => DtoFieldName == DbColumnName ? null : DtoFieldName;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsIdentity { get; set; } = false;
        public bool IgnoreSelect { get; set; } = false;
        public bool IgnoreSelectList { get; set; } = false;
        public bool IgnoreInsert { get; set; } = false;
        public bool IgnoreUpdate { get; set; } = false;
        public object TableDto { get; internal set; }

        protected void Initialice(string dtoFieldName, string dbColumnName = null, bool isIdentity = false, bool isPrimaryKey = false)
        {
            DtoFieldName = dtoFieldName ?? throw new ArgumentException("Column 'dtoFieldName' cannot be null", nameof(dtoFieldName));
            DbColumnName = dbColumnName ?? dtoFieldName;
            IsIdentity = isIdentity;
            IsPrimaryKey = isPrimaryKey;
        }

        public ColumnConfiguration(string dtoFieldName, string dbColumnName = null, bool isIdentity = false, bool isPrimaryKey = false)
        {
            Initialice(dtoFieldName, dbColumnName, isIdentity, isPrimaryKey);
        }

        public ColumnConfiguration(object tableDto, string dtoFieldName, string dbColumnName = null, bool isIdentity = false, bool isPrimaryKey = false)
        {
            TableDto = tableDto;
            Initialice(dtoFieldName, dbColumnName, isIdentity, isPrimaryKey);
            GetPropertyInfo();
        }

        public PropertyInfo GetPropertyInfo()
        {
            if (propertyInfo == null)
            {
                Type tableType = TableDto.GetType();
                propertyInfo = tableType.GetProperty(DtoFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return propertyInfo;
        }
    }
}
