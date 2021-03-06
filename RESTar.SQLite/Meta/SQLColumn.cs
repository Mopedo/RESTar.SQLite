﻿using System;
using System.Data;
using RESTar.Resources;
using static System.StringComparison;

namespace RESTar.SQLite.Meta
{
    /// <summary>
    /// Represents a column in a SQL table
    /// </summary>
    public class SQLColumn
    {
        private ColumnMapping Mapping { get; set; }

        /// <summary>
        /// The name of the column
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the column, as defined in SQL
        /// </summary>
        public SQLDataType Type { get; }

        /// <summary>
        /// The type of the column, as defined in System.Data
        /// </summary>
        internal DbType? DbType { get; }

        /// <summary>
        /// Does this instance represent the RowId SQLite column?
        /// </summary>
        [RESTarMember(ignore: true)] public bool IsRowId { get; }

        /// <summary>
        /// Creates a new SQLColumn instance
        /// </summary>
        public SQLColumn(string name, SQLDataType type)
        {
            Name = name;
            IsRowId = name.EqualsNoCase("rowid");
            Type = type;
            DbType = type.ToDbTypeCode();
        }

        internal void SetMapping(ColumnMapping mapping) => Mapping = mapping;

        internal void Push()
        {
            if (Mapping == null)
                throw new InvalidOperationException($"Cannot push the unmapped SQL column '{Name}' to the database");
            foreach (var column in Mapping.TableMapping.GetSQLColumns())
            {
                if (column.Equals(this)) return;
                if (string.Equals(Name, column.Name, OrdinalIgnoreCase))
                    throw new SQLiteException($"Cannot push column '{Name}' to SQLite table '{Mapping.TableMapping.TableName}'. " +
                                              $"The table already contained a column definition '({column.ToSQL()})'.");
            }
            Database.Query($"BEGIN TRANSACTION;ALTER TABLE {Mapping.TableMapping.TableName} ADD COLUMN {ToSQL()};COMMIT;");
        }

        internal string ToSQL() => $"{Name.Fnuttify()} {Type}";

        /// <inheritdoc />
        public override string ToString() => ToSQL();

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is SQLColumn col
                                                   && string.Equals(Name, col.Name, OrdinalIgnoreCase)
                                                   && Type == col.Type;

        /// <inheritdoc />
        public override int GetHashCode() => (Name.ToUpperInvariant(), Type).GetHashCode();
    }
}