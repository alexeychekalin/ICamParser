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
            _answer = "PAPA303000011013005006005007000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002000000000000000000000000000001001000000000000000000000000031014000000000000000000000000000000000000000000000000000000023045014000000000000000000000023000000000001031014002000000082}290007A/";
            richTextBox1.Text = _answer;
            var status = _answer.Substring(7, 3);
            label5.Text = status == "000" ? "Рабоает" : "Стоит";

            log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Отправлен запрос PAPA303/";
            log.Text += Environment.NewLine + DateTime.Now.ToString("H:mm:ss dd-MM-yy") + " Получен ответ, Status = " + status + "(" + label5.Text + ")";
            toolStripStatusLabel1.Text = " Status = " + status + " (" + label5.Text + ")" ;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var sentences = Split(_answer.Substring(7, _answer.Length-16), 3).ToList();
            cej = Convert.ToInt32(sentences[82]) + Convert.ToInt32(sentences[83]) + Convert.ToInt32(sentences[84]) + Convert.ToInt32(sentences[91]);

            var cpa_seq = _answer.Substring(_answer.Length - 9, 3);
            int cpa_symbol = cpa_seq[0];
            cpa = Convert.ToInt32((cpa_symbol - 48).ToString() + cpa_seq[1] + cpa_seq[2]);

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
