using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Parquet;
using Parquet.Data;
using DataColumn = System.Data.DataColumn;
using DBNull = System.DBNull;
using ETL.File.InputTypes;

namespace ETL.ParquetUtil
{
    public static class ParquetNet
    {
        public static void DtToParquet(DataTable dataTable, String OutputFilePath, int RowGroupSize = 0)
        {
            using (var stream = System.IO.File.Open(OutputFilePath, FileMode.Create, FileAccess.Write))
            {
                DtToParquet(dataTable, stream, RowGroupSize);
            }
        }

        public static void DtToParquet(DataTable dt, Stream stream, int RowGroupSize = 0)
        {
            var fields = GenerateSchema(dt);

            if (RowGroupSize == 0) RowGroupSize = dt.Rows.Count;

            // Open the output file for writing
            using (stream)
            {
                using (var writer = new ParquetWriter(new Schema(fields), stream))
                {
                    var startRow = 0;
                    //var table = new List<>[]();

                    // Keep on creating row groups until we run out of data
                    while (startRow < dt.Rows.Count)
                    {
                        using (var rgw = writer.CreateRowGroup())
                        {
                            // Data is written to the row group column by column
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                var columnIndex = i;

                                // Determine the target data type for the column
                                var targetType = dt.Columns[columnIndex].DataType;
                                if (targetType == typeof(DateTime)) targetType = typeof(DateTimeOffset);
                                if (targetType == typeof(Char)) targetType = typeof(String);

                                // Generate the value type, this is to ensure it can handle null values
                                var valueType = targetType.IsClass
                                    ? targetType
                                    : typeof(Nullable<>).MakeGenericType(targetType);

                                // Create a list to hold values of the required type for the column
                                var list = (IList)typeof(List<>)
                                    .MakeGenericType(valueType)
                                    .GetConstructor(Type.EmptyTypes)
                                    .Invoke(null);

                                // Get the data to be written to the parquet stream
                                foreach (var row in dt.AsEnumerable().Skip(startRow).Take(RowGroupSize))
                                {
                                    // Check if value is null, if so then add a null value
                                    if (row[columnIndex] == null || row[columnIndex] == DBNull.Value)
                                    {
                                        list.Add(null);
                                    }
                                    else
                                    {
                                        var val = dt.Columns[columnIndex];
                                        if(val.DataType ==  typeof(DateTime)) {
                                            list.Add(new DateTimeOffset((DateTime)row[columnIndex]));

                                        }
                                         else if(val.DataType == typeof(Char)) {
                                            list.Add(row[columnIndex] as String);

                                        }
                                         else {
                                            list.Add(row[columnIndex]);
                                        }
                                        // Add the value to the list, but if it's a DateTime then create it as a DateTimeOffset first
                                        // list.Add(dt.Columns[columnIndex].DataType == typeof(DateTime)
                                        //     ? new DateTimeOffset((DateTime)row[columnIndex])
                                        //     : row[columnIndex]);
                                    }
                                }

                                // Copy the list values to an array of the same type as the WriteColumn method expects
                                // and Array
                                var valuesArray = Array.CreateInstance(valueType, list.Count);
                                list.CopyTo(valuesArray, 0);

                                // Write the column
                                rgw.WriteColumn(new Parquet.Data.DataColumn(fields[i], valuesArray));

                            }
                        }

                        startRow += RowGroupSize;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a collection of Parquet fields from the <see cref="System.Data.DataTable"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static List<DataField> GenerateSchema(DataTable dt)
        {
            var fields = new List<DataField>(dt.Columns.Count);

            foreach (DataColumn column in dt.Columns)
            {
                // Attempt to parse the type of column to a parquet data type
                var success = Enum.TryParse<DataType>(column.DataType.Name, true, out var type);

                // If the parse was not successful and it's source is a DateTime then use a DateTimeOffset, otherwise default to a string
                if (!success && column.DataType == typeof(DateTime))
                {
                    type = DataType.DateTimeOffset;
                }
                else if (!success && column.DataType == typeof(Char))
                {
                    type = DataType.String;
                }
                else if (!success)
                {
                    type = DataType.String;
                }

                fields.Add(new DataField(column.ColumnName, type));
            }

            return fields;
        }


    }
}