using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace TestTask
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadBooks();
            
        }
        private string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Гаврюша\Desktop\Test\TestTask\bin\Debug\Biblioteka.mdb";

        Book ChBook;
        string selBook = "*";

        private void LoadBooks() // обновить таблицу
        {

            DataTable dataTable = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM books";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                {
                    adapter.Fill(dataTable); 
                }
            }
            dataGridView1.DataSource = dataTable;
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.Columns[3].Visible = false;
            dataGridView1.Columns[5].Visible = false;
            dataGridView1.Columns[1].Width = 250;
            dataGridView1.Columns[4].Width = 142;
            dataGridView1.Columns[1].HeaderText = "Наименование книги:";
            dataGridView1.Columns[2].HeaderText = "Жанр:";
            dataGridView1.Columns[4].HeaderText = "Авторы:";

        }

        private List<string> LoadBooks1(string titleBook) // поиск авторов
        {
            string title = titleBook;
            List<string> data = new List<string>();
                        
            string query = $"SELECT authors.FIO FROM books INNER JOIN(authors INNER JOIN Svyaz ON authors.ID = Svyaz.authorID) ON books.ID = Svyaz.bookID WHERE(((books.[Title]) = @title)); ";
            
            using (var connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("@title", title);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    
                                    string author = reader.GetString(0); 
                                    data.Add(author);
                                }
                            }
                            else
                            {
                                data.Add("Нет данных.");
                                //richTextBox1.AppendText("Нет данных.\n");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
            return data;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
         
        }
        private int GetOrCreatenewId(string authorName, OleDbConnection connection)
        {
            var command = new OleDbCommand("SELECT ID FROM authors WHERE FIO = @FIO", connection);
            command.Parameters.AddWithValue("@FIO", authorName);
            var result = command.ExecuteScalar();
            if (result != null)
            {
                return (int)result;
            }
            else
            {
                var insertCommand = new OleDbCommand("INSERT INTO authors (FIO) VALUES (@FIO)", connection);
                insertCommand.Parameters.AddWithValue("@FIO", authorName);
                insertCommand.ExecuteNonQuery();
                return (int)new OleDbCommand("SELECT @@IDENTITY", connection).ExecuteScalar();
            }
        }

        private void Svyaz(string authorName, string titl, OleDbConnection connection)
        {
            var command = new OleDbCommand("SELECT ID FROM authors WHERE FIO = @FIO", connection);
            command.Parameters.AddWithValue("@FIO", authorName);
            var result = command.ExecuteScalar();
            var command2 = new OleDbCommand("SELECT ID FROM books WHERE Title = @title", connection);
            command2.Parameters.AddWithValue("title", titl);
            var result2 = command2.ExecuteScalar();
            var insertCommand = new OleDbCommand("INSERT INTO Svyaz (authorID, bookID) VALUES (@result, @result2)", connection);
            insertCommand.Parameters.AddWithValue("@result", result);
            insertCommand.Parameters.AddWithValue("@result2", result2);
            insertCommand.ExecuteNonQuery();
        }
        public int GetCount()
        {
            int count = 0;

            int maxValue = 0; 
            int minValue = 2;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    if (int.TryParse(row.Cells["ID"].Value?.ToString(), out int cellValue))
                    {                      
                        if (cellValue > maxValue)
                        {
                            maxValue = cellValue;
                        }
                        if (cellValue < minValue)
                        {
                            minValue = cellValue;
                        }
                    }
                }
            }
            minValue--;
            maxValue++;
            if (minValue >= 1) { count = minValue; }
            else { count = maxValue; }
            return count;
        }


        private int Exemplar(string title)
        {
            int count = 0;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[1].Value.ToString() == title)
                {
                    count = int.Parse(dataGridView1.Rows[i].Cells[5].Value.ToString()) +1;
                }              
             }

            return count;
        }
        private void button4_Click(object sender, EventArgs e) // добавить книгу
        {   
            Changed frmChg = new Changed();
            frmChg.TransfBook += GettingBook;
            frmChg.FormCls += GettingBool;
            frmChg.ShowDialog();
            
            if (!clsFrm)
            {
                if (Exemplar(ChBook.Title) == 0)
                {
                    try
                    {
                        using (var connection = new OleDbConnection(connectionString))
                        {
                            connection.Open();
                            var newId = GetOrCreatenewId(ChBook.Author, connection);
                            var command = new OleDbCommand("INSERT INTO books (ID, Title, Author, Descr, Ganre, Exemplar) VALUES (@ID, @Title, @Author, @Descr, @Ganre, @Exem)", connection);
                            command.Parameters.AddWithValue("@ID", GetCount());
                            command.Parameters.AddWithValue("@Title", ChBook.Title);
                            command.Parameters.AddWithValue("@Author", ChBook.Author);
                            command.Parameters.AddWithValue("@Descr", ChBook.Description);
                            command.Parameters.AddWithValue("@Ganre", ChBook.Ganre);
                            command.Parameters.AddWithValue("@Exem", 1);
                            command.ExecuteNonQuery();
                            Svyaz(ChBook.Author, ChBook.Title, connection);
                            connection.Close();
                            
                        }
                        LoadBooks();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        using (var connection = new OleDbConnection(connectionString))
                        {
                            connection.Open();
                            int exCoun = Exemplar(ChBook.Title);
                            var command = new OleDbCommand("UPDATE books SET Exemplar = ? WHERE Title = ?", connection);
                            command.Parameters.AddWithValue("Exemplar", exCoun);
                            command.Parameters.AddWithValue("Title", ChBook.Title);
                            
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Запись успешно обновлена.");
                            }
                            else
                            {
                                Console.WriteLine("Запись не найдена.");
                            }
                            connection.Close();
                        }
                        LoadBooks();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Книга не добавлена!");
            }

                
        }
        private void GettingBook(Book book)
        {
            ChBook = new Book(book.Title, book.Author, book.Ganre, book.Description);
        }


        bool clsFrm = true;
        private void GettingBool(bool frmCls)
        {
            clsFrm = frmCls;
        }

        private void button5_Click(object sender, EventArgs e) // описание книги
        {
            int numrow = dataGridView1.SelectedCells[0].RowIndex;

            richTextBox1.Text = "Книга: " + dataGridView1.Rows[numrow].Cells[1].Value.ToString() + "\r" + "Автор: ";
            List<string> auth = LoadBooks1(dataGridView1.Rows[numrow].Cells[1].Value.ToString());
            for (int i = 0; i < auth.Count - 1; i++) {
                richTextBox1.Text += auth[i] + ", ";
            }
            richTextBox1.Text += auth[auth.Count-1];
            richTextBox1.Text += "\r" + "Жанр: " + dataGridView1.Rows[numrow].Cells[2].Value.ToString();
            richTextBox1.Text += "\r" + "Описание: " + dataGridView1.Rows[numrow].Cells[3].Value.ToString();
        }

        private void button3_Click(object sender, EventArgs e) // удалить книгу
        {

            DialogResult dialogResult = MessageBox.Show("Вы хотите удалить выбранную книгу?", "Удаление", MessageBoxButtons.YesNo);
            if(dialogResult == System.Windows.Forms.DialogResult.Yes)
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {

                    int id = dataGridView1.SelectedCells[0].RowIndex;
                    int DelId = int.Parse(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[0].Value.ToString());                 
                    connection.Open();
                    string query = $"DELETE FROM books WHERE ID = @id";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", DelId);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                LoadBooks();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e) // изменить книгу
        {
            Book chgBook = new Book(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[1].Value.ToString(), dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[4].Value.ToString(),
                dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[2].Value.ToString(), dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[3].Value.ToString());
           ///////////////////////////////////////
            Changed frmChg = new Changed(chgBook);
            frmChg.TransfBook += GettingBook;
            frmChg.FormCls += GettingBool;
            frmChg.ShowDialog();
            richTextBox1.Text = clsFrm.ToString();
            if (!clsFrm) { 
           
                try
                {

                    using (var connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();
                        var command = new OleDbCommand("UPDATE books SET Title = ?, Author = ?, Descr = ?, Ganre = ? WHERE ID = ?", connection);
                        command.Parameters.AddWithValue("Title", ChBook.Title);
                        command.Parameters.AddWithValue("Author", ChBook.Author);
                        command.Parameters.AddWithValue("Descr", ChBook.Description);
                        command.Parameters.AddWithValue("Ganre", ChBook.Ganre);
                        command.Parameters.AddWithValue("ID", dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[0].Value);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
        }
        else {
                MessageBox.Show("Изменения не сохранены!");
            }
        }

        private void button1_Click(object sender, EventArgs e) // найти книгу
        {           
            DataTable dataTable = new DataTable();

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string query = $"SELECT * FROM books WHERE Title = @Title OR Ganre = @Ganre OR Author = @Author";                   
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Title", selBook);
                        command.Parameters.AddWithValue("@Author", selBook);                      
                        command.Parameters.AddWithValue("@Ganre", selBook);
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }

                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }

        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            selBook = textBox1.Text;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                MessageBox.Show("Введите данные!");
            }
            else
            {
                richTextBox1.Text = "";
                LoadBooks();
                int nn = int.Parse(textBox2.Text);
                dataGridView1.Sort(dataGridView1.Columns[5], System.ComponentModel.ListSortDirection.Descending);

                for (int i = 0; i < nn; i++)
                {
                    richTextBox1.Text += (i + 1).ToString() + ". " + "Книга: " + dataGridView1.Rows[i].Cells[1].Value.ToString() + " " + "Автор: " + dataGridView1.Rows[i].Cells[4].Value.ToString()+ " ("+dataGridView1.Rows[i].Cells[5].Value.ToString() + " шт.)" +"\r";
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "")
            {
                MessageBox.Show("Введите данные!");
            }
            else { 
            richTextBox1.Text = "";
            LoadBooks();
            string auth = textBox3.Text;
            int numB = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[4].Value.ToString() == auth)
                {
                    numB++;
                }
            }
            if(numB%10 == 1) richTextBox1.Text = "У автора " + auth + " " + numB + " книга.";
            else if(numB % 10 < 5) richTextBox1.Text = "У автора " + auth + " " + numB + " книги.";
            else richTextBox1.Text = "У автора " + auth + " " + numB + " книг.";
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "")
            {
                MessageBox.Show("Введите данные!");
            }
            else
            {
                richTextBox1.Text = "";
                LoadBooks();
                string auth = textBox3.Text;
                int numB = 0;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells[4].Value.ToString() == auth)
                    {
                        numB += int.Parse(dataGridView1.Rows[i].Cells[5].Value.ToString());
                    }
                }

                if (numB % 10 == 1) richTextBox1.Text = "У автора " + auth + " " + numB + " экземпляр книги.";
                else if (numB % 10 < 5) richTextBox1.Text = "У автора " + auth + " " + numB + " экземпляра книги.";
                else richTextBox1.Text = "У автора " + auth + " " + numB + " экземпляров книг.";
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
        }
    }
}
