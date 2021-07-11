using Athena.Data.Books;
using Athena.Data.Borrowings;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Athena.Windows
{
    public partial class ReturnWindow
    {
        public Borrowing Borrowing{ get; set; }
        public ReturnWindow(Book book)
        {
            InitializeComponent();
            this.DataContext = this;
            Title.Text = book.Title;
            var authors = ToAuthorsNames(book);
            if (!authors.IsNullOrEmpty())
            {
                Author.Text = authors;
            }

            Borrowing = book.Borrowings.Last();
            Calendar.SelectedDate = DateTime.Today;
            Calendar.BlackoutDates.Add(new CalendarDateRange(DateTime.Today.AddDays(1), DateTime.Today.AddDays(1).AddYears(1000)));
            Calendar.BlackoutDates.Add(new CalendarDateRange(DateTime.Today.AddDays(-1).AddYears(-1000), (book.Borrowings.Last().BorrowDate).AddDays(-1)));
        }


        public string ToAuthorsNames(Book book)
        {
            StringBuilder builder = new StringBuilder();
            if (book.Authors.Count > 0)
            {
                for (int i = 0; i < book.Authors.Count; i++)
                {
                    var authorsList = book.Authors.ToList();
                    if (i == 0)
                    {
                        builder.Append($"{authorsList[i].FirstName} {authorsList[i].LastName}");
                        continue;
                    }
                    builder.Append($"\n {authorsList[i].FirstName} {authorsList[i].LastName}");
                }
            }

            return builder.ToString();
        }

        private async void ReturnBorrowedBook_Click(object sender, RoutedEventArgs e)
        {
            var borrowing = await ApplicationDbContext.Instance.Borrowings.FindAsync(Borrowing.Id);
            borrowing.ReturnDate = Calendar.SelectedDate;
            await ApplicationDbContext.Instance.SaveChangesAsync();
            BookReturned?.Invoke(this, new BookReturnedEventArgs(borrowing));
            this.Close();
        }
        public class BookReturnedEventArgs : EventArgs
        {
            public Borrowing Value { get; private set; }
            public BookReturnedEventArgs(Borrowing val)
            {
                Value = val;
            }
        }
        public event EventHandler<BookReturnedEventArgs> BookReturned;
    }
}
