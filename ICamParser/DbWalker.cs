using System;
using System.Data.SqlClient;
using System.Windows.Forms;

//Класс подключения к БД

namespace ICamParser
{
    class DbWalker
    {
        public static SqlConnection GetConnection(string server, string user, string password, string security, string database)
        {

            SqlConnection conn;
            try
            {
                conn = new SqlConnection(@"Data Source = " + server + @"; Initial Catalog =" + database + @";" +
                                         @"Integrated Security = " + security + @"; User ID =" + user + @"; Password = " + password);
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Не удалось подключиться к БД." + Environment.NewLine + e.Message);
                return null;
            }
            return conn;
        }
    }
}

