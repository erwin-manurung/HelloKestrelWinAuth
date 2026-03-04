using Allegro.Core.Criteria;
using Allegro.Diagnostics;
using System.Data;

namespace Allegro.BuildTasks.UseCases
{
    public class MainWS
    {
        public string GetVersion()
        {
            return "24.0.1";
        }

        public DataSet RetrieveData(DataSet dsRetrieve, string[] tableNames, SelectCriteria criteria)
        {
            DataSet ret = new DataSet();
            DataTable dataTable = new DataTable("dummy");
            dataTable.Columns.Add("dummy");
            dataTable.Rows.Add("ok");
            dataTable.AcceptChanges();
            ret.Tables.Add(dataTable);
            return ret;
        }
        public DataSet AnotherRetrieveData(DataSet dsRetrieve, string[] tableNames, SelectCriteria criteria)
        {
            DataSet ret = new DataSet();
            DataTable dataTable = new DataTable("dummy");
            dataTable.Columns.Add("dummy");
            dataTable.Rows.Add("ok");
            dataTable.AcceptChanges();
            ret.Tables.Add(dataTable);
            return ret;
        }

    }
}
