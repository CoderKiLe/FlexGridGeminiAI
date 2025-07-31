using FlexGridGeminiAI.Interface;
using FlexGridGeminiAI.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FlexGridGeminiAI.Data
{

    /// <summary>
    /// class to provide a datasource either locallyor from the componenton control panel
    /// </summary>
    public class DataSource: IDataSource
    {
        private string _databaseFileName = @"\NORTHWND.db";
        public void SetDataSource(string fileName)
        {
            _databaseFileName = fileName;
        }


        #region "private methods"
        private static List<KeyValuePair<int, string>> paths = new List<KeyValuePair<int, string>>()
            {
                new KeyValuePair<int, string>(1, Environment.CurrentDirectory),
                new KeyValuePair<int, string>(2, Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\ComponentOne Samples\Common")
            };

        /// <summary>
        /// Search for the availibility of the database
        /// </summary>
        /// <returns>string path to the database</returns>
        public string GetPathDb()
        {

            //var existsDb = paths.Select(x => new
            //{
            //    Priority = x.Key,
            //    Path = x.Value + _databaseFileName,
            //    Exists = File.Exists(x.Value + _databaseFileName)
            //}).Where(x => x.Exists)
            //  .OrderBy(x => x.Priority)
            //  .FirstOrDefault()?.Path ?? "";

            var existDb = (
                from x in paths
                let fullPath = x.Value + _databaseFileName
                where File.Exists(fullPath)
                orderby x.Key
                select fullPath
                ).FirstOrDefault() ?? "";

            return existDb;
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        private DbConnection GetOpenConnection()
        {
            //string dbPath = @"C:\Users\kile3\Documents\ComponentOne Samples\Common\C1NWind.db";
            string dbPath = GetPathDb();
            bool checkDb = CheckDatabase();
            if (!checkDb) return default;
            var connectionString = String.Format("Data Source={0}", dbPath);
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            return connection;
        }

        private bool CheckDatabase()
        {
            var existsPathDb = GetPathDb();
            if (string.IsNullOrEmpty(existsPathDb))
            {
                var message = $"File {_databaseFileName} not found! {Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, paths.Select(x => x.Value).ToArray())}";
                MessageBox.Show(message, "Error");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all the Tables inside of the database
        /// </summary>
        public List<string> GetDataTables(string query)
        {
            List<string> titles = new List<string> { };
            var connection = GetOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = query;
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                titles.Add(reader.GetString(0));
            }
            return titles;
        }

        /// <summary>
        /// created the column form the data that was processed above
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="imageColumns"></param>
        /// <returns>IEnumerable after selecting a data</returns>
        private IEnumerable<DataColumn> CreateColumns(SqliteDataReader reader, IEnumerable<string> imageColumns = null)
        {
            var columns = new List<DataColumn>();
            var dateColumnNames = new List<string>()
            { "datetime", "date"};

            if (reader.HasRows)
            {
                // Create base columns 
                var schemaTable = reader.GetSchemaTable();
                columns = (from s in schemaTable.Rows.Cast<DataRow>() select s)
                    .Select(x => new
                    {
                        // Name field
                        ColumnName = x["ColumnName"].ToString(),
                        // Database type
                        DataTypeName = x["DataTypeName"].ToString().ToLower(),
                        // System type
                        SystemType = Type.GetType(x["DataType"].ToString())
                    })
                    .Select(x => new DataColumn()
                    {
                        ColumnName = x.ColumnName,
                        DataType =
                                    // Check type as date
                                    dateColumnNames.Any(y => y == x.DataTypeName) ? typeof(DateTime) :
                                    imageColumns != null ?
                                        // Check type as image
                                        imageColumns.Any(y => y == x.ColumnName) ? typeof(Image) : x.SystemType
                                    : x.SystemType
                    }).ToList();
            }

            return columns;
        }
        #endregion

        /// <summary>
        /// Generates a detailed string representation of a DataTable's schema.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public List<DataColumnSchema> GetDataTableSchema(DataTable dataTable)
        {
            var schemaList = new List<DataColumnSchema>();

            if (dataTable == null || dataTable.Columns.Count == 0)
                return schemaList;

            foreach (DataColumn col in dataTable.Columns)
            {
                schemaList.Add(new DataColumnSchema
                {
                    Name = col.ColumnName,
                    ColumnIndex = col.Ordinal,
                    DataType = col.DataType.FullName
                });
            }

            return schemaList;
        }

        /// <summary>
        ///  Generated the DataTable for the Grid
        /// </summary>
        /// <param name="queryString">Sql query for selection of data from the col</param>
        /// <param name="tableName">Name of the table from which the data is selected</param>
        /// <param name="imageColumns">if there is image in the column</param>
        /// <returns>DataTable</returns>
        public DataTable GetRows(string queryString,
            string tableName = "Result", 
            IEnumerable<string> imageColumns = null)
        {
            var table = new DataTable(tableName);
            var existsPathDb = GetPathDb(); // C:\Users\DELL\Documents\ComponentOne Samples\Common\NORTHWND.db
            if (!CheckDatabase()) return null;

            var connectionString = String.Format("Data Source={0}", existsPathDb);
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                using (SqliteCommand command = new SqliteCommand(queryString, connection))
                {
                    // Open SQLite database
                    connection.Open();
                    var reader = command.ExecuteReader();
                    var columns = CreateColumns(reader, imageColumns);
                    table.Columns.AddRange(columns.ToArray());

                    if (columns.Any())
                    {
                        while (reader.Read())
                        {
                            // Fill table
                            var row = table.NewRow();
                            var arrayColumns = columns.ToArray();
                            Enumerable.Range(0, reader.FieldCount)
                                .ToList()
                                .ForEach(x =>
                                {
                                    var currentColumns = arrayColumns[x];
                                    bool IsImageColumn = imageColumns == null ? false :
                                            imageColumns.Any(y => y == currentColumns.ColumnName) ? true : false;

                                    if (IsImageColumn)
                                        row[x] = ImageConverterUtils.Base64ToImage(reader[x].ToString());
                                    else
                                        row[x] = reader[x];
                                });

                            table.Rows.Add(row);
                        }

                        return table;
                    }
                }
            }

            return null;
        }
    }

    class ImageConverterUtils
    {
        /// <summary>
        /// Converts a Base64-encoded string into an Image object.
        /// </summary>
        /// <param name="base64String">The Base64-encoded image string.</param>
        /// <returns>An Image object.</returns>
        public static Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                return Image.FromStream(ms, true);
            }
        }
    }
}
