using System;
using ETL.File.InputTypes;
using System.IO;
using System.Data;
using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using FastMember;

namespace ETL
{

    public static class Generic<T> where T : new()
    {

        public static IEnumerable<T> GetRecordsFromCsv(StreamData input)
        {
            var csv = new CsvReader(new StreamReader(input.Stream), CultureInfo.InvariantCulture);
            return csv.GetRecords<T>();
        }

        /// <summary>
        /// converts IDataReader to IEnumerable<T>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<T> ReaderToObjects(IDataReader reader)
        {

            var members = TypeAccessor.Create(typeof(T)).GetMembers();

            while (reader.Read())
            {
                var obj = new T();
                var w = ObjectAccessor.Create(obj);
                foreach (var member in members)
                {
                    w[member.Name] = reader[member.Name] is DBNull ? null : reader[member.Name];
                }

                yield return obj;

            }

            reader.Dispose();

        }


    }


}