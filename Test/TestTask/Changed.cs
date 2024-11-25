using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestTask
{
    public partial class Changed : Form
    {
        public Changed()
        {
            InitializeComponent();
        }
        public Changed(Book chBook)
        {
            InitializeComponent();
            this.Text = "Изменить книгу";
            Book chgBook = chBook;
            textBox1.Text = chgBook.Title;
            textBox2.Text = chgBook.Author;
            textBox3.Text = chgBook.Ganre;
            textBox4.Text = chgBook.Description;
        }



        public event Action<Book> TransfBook;
        public event Action<bool> FormCls;
        bool formCls = true;
        private void button2_Click(object sender, EventArgs e) => this.Close();

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "")
            {
                string tit = textBox1.Text.ToString();
                string aut = textBox2.Text.ToString();
                string gen = textBox3.Text.ToString();
                string des = textBox4.Text.ToString();
                var book = new Book(tit, aut, gen, des);
                TransfBook?.Invoke(book);
                formCls = false;
                FormCls?.Invoke(formCls);

                this.Close();
            }
            else {
                MessageBox.Show("Введите значения!");
            }
        }
        
        private void Changed_Load(object sender, EventArgs e)
        {

        }

        private void Changed_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormCls?.Invoke(formCls);
        }
    }
}
