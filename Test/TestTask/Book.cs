using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Ganre { get; set; }
        public string Description { get; set; }

        public Book(string title, string author, string ganre, string description)
        {
            Title = title;
            Author = author;
            Ganre = ganre;
            Description = description;
            
        }

        public override string ToString()
        {
            return "\t\tTitle: " + Title + "\r\n" +
                "\t\tAuthor: " + Author + "\r\n" +
                "\t}";
        }

    }
}
