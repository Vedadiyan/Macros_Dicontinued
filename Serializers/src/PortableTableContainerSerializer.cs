using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Macros.Serializers
{
    internal delegate void WriteLine(string line);
    internal delegate string ReadLine();
    public class PortableTableContainerSerializer
    {
        internal static Task SerializeAsync(DataTable dataTable, WriteLine writeLine)
        {
            Task serializeTask = Task.Run(() =>
            {
                for (int c = 0; c < dataTable.Columns.Count; c++)
                {
                    var reference = dataTable.Columns[c];
                    writeLine(reference.ColumnName);
                    writeLine(((int)Type.GetTypeCode(reference.DataType)).ToString());
                    writeLine((reference.AllowDBNull ? 1 : 0).ToString());
                }
                writeLine(String.Empty);
                for (int r = 0; r < dataTable.Rows.Count; r++)
                {
                    for (int c = 0; c < dataTable.Columns.Count; c++)
                    {
                        object value = dataTable.Rows[r][c];
                        if (value == DBNull.Value)
                        {
                            writeLine("\t");
                        }
                        else if (value is string)
                        {
                            var tmp = value.ToString().Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");
                            if (String.IsNullOrEmpty(tmp))
                            {
                                writeLine("\t\t");
                            }
                            else
                            {
                                writeLine(tmp);
                            }
                        }
                        else if (value is DateTime dateTime)
                        {
                            writeLine(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            writeLine(value.ToString());
                        }
                    }
                    writeLine(String.Empty);
                }
            });
            return serializeTask;
        }
        public static async Task<Stream> SerializeToStreamAsync(DataTable dataTable)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter sw = new StreamWriter(memoryStream);
            await SerializeAsync(dataTable, sw.WriteLine);
            await sw.FlushAsync();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            return (Stream)sw.BaseStream;
        }
        public static async Task<string> SerializeToStringAsync(DataTable dataTable)
        {
            StringWriter sw = new StringWriter();
            await SerializeAsync(dataTable, sw.WriteLine);
            await sw.FlushAsync();
            return sw.ToString();
        }
        public static Task<DataTable> DeserializeFromStreamAsync(Stream serializedTableStream)
        {
            return DeserializeFromStreamAsync(serializedTableStream, null);
        }
        public static Task<DataTable> DeserializeFromStreamAsync(Stream serializedTableStream, Func<string, object, bool> filter)
        {
            StreamReader sr = new StreamReader(serializedTableStream);
            return DeserializeAsync(filter, sr.ReadLine);
        }
        public static Task<DataTable> DeserializeFromStringAsync(string serializedTable)
        {
            return DeserializeFromStringAsync(serializedTable);
        }
        public static Task<DataTable> DeserializeFromStringAsync(string serializedTable, Func<string, object, bool> filter)
        {
            StringReader sr = new StringReader(serializedTable);
            return DeserializeAsync(filter, sr.ReadLine);
        }
        private static Task<DataTable> DeserializeAsync(Func<string, object, bool> filter, ReadLine readLine)
        {
            Task<DataTable> deserializeTask = Task.Run(() =>
            {
                DataTable dataTable = new DataTable();
                string line = null;
                DataColumn dataColumn = new DataColumn();
                for (int i = 1; !String.IsNullOrEmpty(line = readLine()); i++)
                {
                    switch (i)
                    {
                        case 1:
                            dataColumn.ColumnName = line;
                            break;
                        case 2:
                            switch (int.Parse(line))
                            {
                                case 0:
                                    break;
                                case 1:
                                    dataColumn.DataType = typeof(Object);
                                    break;
                                case 2:
                                    dataColumn.DataType = typeof(DBNull);
                                    break;
                                case 3:
                                    dataColumn.DataType = typeof(Boolean);
                                    break;
                                case 4:
                                    dataColumn.DataType = typeof(Char);
                                    break;
                                case 5:
                                    dataColumn.DataType = typeof(SByte);
                                    break;
                                case 6:
                                    dataColumn.DataType = typeof(Byte);
                                    break;
                                case 7:
                                    dataColumn.DataType = typeof(Int16);
                                    break;
                                case 8:
                                    dataColumn.DataType = typeof(UInt16);
                                    break;
                                case 9:
                                    dataColumn.DataType = typeof(Int32);
                                    break;
                                case 10:
                                    dataColumn.DataType = typeof(UInt32);
                                    break;
                                case 11:
                                    dataColumn.DataType = typeof(Int64);
                                    break;
                                case 12:
                                    dataColumn.DataType = typeof(UInt64);
                                    break;
                                case 13:
                                    dataColumn.DataType = typeof(Single);
                                    break;
                                case 14:
                                    dataColumn.DataType = typeof(Double);
                                    break;
                                case 15:
                                    dataColumn.DataType = typeof(Decimal);
                                    break;
                                case 16:
                                    dataColumn.DataType = typeof(DateTime);
                                    break;
                                case 17:
                                    break;
                                case 18:
                                    dataColumn.DataType = typeof(String);
                                    break;

                            }
                            break;
                        case 3:
                            dataColumn.AllowDBNull = int.Parse(line) == 1;
                            dataTable.Columns.Add(dataColumn);
                            dataColumn = new DataColumn();
                            i = 0;
                            break;
                    }
                }
                for (; ; )
                {
                    object[] values = new object[dataTable.Columns.Count];
                    bool searchResult = false;
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if ((line = readLine()) == null)
                        {
                            return dataTable;
                        }
                        if (line == "\t")
                        {
                            values[i] = DBNull.Value;
                        }
                        else if (line == "\t\t")
                        {
                            values[i] = "";
                        }
                        else
                        {
                            values[i] = Convert.ChangeType(line.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"), dataTable.Columns[i].DataType);
                        }
                        if (filter != null && !searchResult)
                        {
                            searchResult = filter(dataTable.Columns[i].ColumnName, values[i]);
                        }
                    }
                    readLine();

                }
            });
            return deserializeTask;
        }
    }
}