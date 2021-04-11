using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            List<TimeSpan> ls = new List<TimeSpan>();
            Stopwatch sw = new Stopwatch();
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection("Data Source=192.168.147.30;Initial Catalog=FinancialAnalysisDb;User Id=#_R;Password=$++@#$N@V@666!@@#"))
            {
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM [DIT].[Tbl02_CompanyBalanceSheetFormulaAmount] ";
                sw.Start();
                connection.Open();
                dt.Load(cmd.ExecuteReader());
                sw.Stop();
                ls.Add(sw.Elapsed);
                sw.Restart();
            }
            var data = Macros.Serializers.PortableTableContainerSerializer.SerializeToStringAsync(dt).Result;
            System.IO.File.WriteAllText("D:\\sssssssss.dt", data);
            var deserialized = Macros.Serializers.PortableTableContainerSerializer.DeserializeFromStringAsync(data, (column , value)=> column == "ComBS_ID" && (long?)value == 30).Result;
            Console.WriteLine("Hello World!");
        }
    }
}
