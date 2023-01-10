using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace ETL.Tools
{
    public static class DataTableExtension
    {
        public static void AddColumnsFromClass(this DataTable table, Type theClass)
        {
            if (theClass == null)
                throw new ArgumentNullException(nameof(theClass), "The class must not be null");

            var properties = theClass.GetProperties();
            foreach(var property in properties)
            {
                table.Columns.Add(property.Name, property.PropertyType.IsEnum ? typeof(string) : property.PropertyType);
            }
        }

        public static void AddRowFromObject(this DataTable table, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "The object must not be null");

            var row = table.NewRow();
            foreach(DataColumn column in table.Columns)
            {
                var prop = obj.GetType().GetProperty(column.ColumnName);

                row[column.ColumnName] = prop.PropertyType.IsEnum ?
                        Enum.GetNames(prop.PropertyType)[(int)prop.GetValue(obj)]
                        : prop.GetValue(obj);
            }
            table.Rows.Add(row);
        }
    }
}
