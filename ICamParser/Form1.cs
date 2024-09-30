using ICamParser.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICamParser
{
    public partial class Form1 : Form
    {
        string _answer;
        int[] _values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 21, 22, 31, 32, 33, 51, 52, 53, 82, 83, 84, 91, 93, 94, 95, 96, 97, 98 };
        int cej, cpa, ces;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Text = "RHRH303/";
            log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Отправлен запрос AHAH303/";
            log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Получен ответ RHRH303/";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //_answer = "PAPA303000032010081003000007000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000000000000000000000000000000017010000000000000000000000000000000000000000000000000000000129027000000000000000000000000129000000000000017010001000000155Ќ9900084/";
            _answer = richTextBox1.Text;
            var status = _answer.Substring(7, 3);
            label5.Text = status == "000" ? "Рабоает" : "Стоит";

            log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Отправлен запрос PAPA303/";
            log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Получен ответ, Status = " + status + "(" + label5.Text + ")";
            toolStripStatusLabel1.Text = " Status = " + status + " (" + label5.Text + ")" ;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _answer = richTextBox1.Text;
            var sentences = Split(_answer.Substring(7, _answer.Length-16), 3).ToList();
            cej = Convert.ToInt32(sentences[82]) + Convert.ToInt32(sentences[83]) + Convert.ToInt32(sentences[84]) + Convert.ToInt32(sentences[91]);

            byte[] res = System.Text.Encoding.Default.GetBytes(_answer.Substring(_answer.Length - 9, 1));
            int first = (int)res[0] - 48;
            int second = Convert.ToInt32(_answer.Substring(_answer.Length - 8, 2));
            cpa = Math.Abs(first * 100 + second);

            ces = 0;

            log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Данные расшифрованы";


            var sql = CreateInsertSql(sentences, "_Online");

            var conn = DbWalker.GetConnection(Resources.Server, Resources.User, Resources.Password, Resources.secure, "CPS" + Resources.Cech);
            try
            {
                conn.Open();

                var command = new SqlCommand(CreateInsertSql(sentences, "_Online"), conn);
                command.ExecuteNonQuery();
                log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Данные записаны в таблицу [CPS" + Resources.Cech + "].[dbo].[Line_" + Resources.LineControl + "_303_Online]";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка записи в БД: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
        static IEnumerable<int> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => Int32.Parse(str.Substring(i * chunkSize, chunkSize)));
        }

        private string CreateInsertSql(List<int> sequence, string table)
        {
            var sql = "INSERT INTO [CPS"+Resources.Cech+"].[dbo].[Line_"+Resources.LineControl+"_303"+table+"] (Time, ";
            _values.ToList().ForEach(x => sql += "C" + x + ", ");
            sql += "CEJ, CPA, CES)";
            sql += " VALUES ( '" + DateTime.Now.ToString("yyyy-MM-ddTHH:00:00.000") + "', ";
            _values.ToList().ForEach(x => sql += sequence[x] + ", ");
            sql += cej + ", " + cpa + ", " + ces + " )";
            return sql;
        }

        private void ZipTable()
        {
            var sql = "SELECT ";
            _values.ToList().ForEach(x => sql += "SUM(C" + x + ") AS C" + x + ", ");
            sql += "SUM(CEJ) AS CEJ, SUM(CPA) AS CPA, SUM(CES) AS CES FROM [CPS" + Resources.Cech + "].[dbo].[Line_" + Resources.LineControl + "_303_Online]";

            var conn = DbWalker.GetConnection(Resources.Server, Resources.User, Resources.Password, Resources.secure, "CPS" + Resources.Cech);
            try
            {
                conn.Open();

                var command = new SqlCommand(sql, conn);
                var reader = command.ExecuteReader();
                var sqlinsert = "";
                while (reader.Read())
                {
                    sqlinsert = "INSERT INTO [CPS" + Resources.Cech + "].[dbo].[Line_" + Resources.LineControl + "_303] (Time, ";
                    _values.ToList().ForEach(x => sqlinsert += "C" + x + ", ");
                    sqlinsert += "CEJ, CPA, CES)";
                    sqlinsert += " VALUES ( '" + DateTime.Now.ToString("yyyy-MM-ddTHH:00:00.000") + "', ";
                    _values.ToList().ForEach(x => sqlinsert += reader["C"+x] + ", ");
                    sqlinsert += reader["CEJ"] + ", " + reader["CPA"] + ", " + reader["CES"] + " )";
                }

                reader.Close();
                command = new SqlCommand(sqlinsert, conn);
                command.ExecuteNonQuery();

                log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Данные за час сгруппированы и записаны в таблицу [CPS" + Resources.Cech + "].[dbo].[Line_" + Resources.LineControl + "_303]";

                //Чистим временную таблицу
                sql = "TRUNCATE TABLE [CPS" + Resources.Cech + "].[dbo].[Line_" + Resources.LineControl + "_303]";
                new SqlCommand(sql, conn).ExecuteNonQuery();

                log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Таблица [CPS" + Resources.Cech + "].[dbo].[Line_" + Resources.LineControl + "_303_Online] очищена";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка записи в БД: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
