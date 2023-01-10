using System;
using System.Linq;
using System.Data;

using System.Data.Common;

namespace ETL.SQL {
public class DtLoader : IDisposable {

private IDataReader r;
public String[] Columns {get; private set;}
public Type[] Types {get; private set;}
public Int32 FieldCount {get; private set;}
public DataTable dt {get; private set;}
public bool HasData {get; private set;}

public DtLoader (IDataReader reader) {
 
  r = reader;
  Columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
  Types = Enumerable.Range(0, reader.FieldCount).Select(reader.GetFieldType).ToArray();
  FieldCount = r.FieldCount;
  dt = new DataTable();
  for ( int i = 0; i < FieldCount; i++) {
   dt.Columns.Add(Columns[i], Types[i]);
  }
  HasData = true;

}

public static Object[] SelectColumn (DataTable dt, String columnName) {
   
   return dt.AsEnumerable().Select(row => row.Field<Object>(columnName)).ToArray();
   
} 

public Object[] GetColumnValues (String columnName) {
  return this.dt.AsEnumerable().Select(row => row.Field<Object>(columnName)).ToArray();
}

public Int32 ReadRows( Int32 rowCount)  {
  
  Int32 rowsRetrieved = 0;
  this.dt.Clear();

  if(!this.HasData) { return rowsRetrieved; }

  while(rowCount > 0) { 

        if(this.r.Read()) {
       
        var rowValues = new Object[this.FieldCount];
        this.r.GetValues(rowValues);
        this.dt.Rows.Add(rowValues);
        rowsRetrieved++;
        rowCount--;
       
       } else {  
         this.HasData = false;
         this.r.Close();
         this.r.Dispose();
         break;
       } 

   }
   return rowsRetrieved;
  
 }

  public void Dispose() { 
      this.r.Close();
      this.r.Dispose();
     } 
 
 }  

}
