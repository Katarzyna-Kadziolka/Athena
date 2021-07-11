using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Athena.Data.Books;
using Athena.Data.Borrowings;
using Athena.EventManagers;
using Athena.Messages;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Hub = MessageHub.MessageHub;

namespace Athena {
    /// <summary>
    /// Interaction logic for BorrowForm.xaml
    /// </summary>
    public partial class BorrowForm {

        public BorrowingView BorrowingView { get; set; }

        public BorrowForm(Book book) {
            InitializeComponent();
            this.DataContext = this;
            Title.Text = book.Title;
            var authors = ToAuthorsNames(book);
            if (!authors.IsNullOrEmpty()) {
                Author.Text = authors;
            }

            BorrowingView = new BorrowingView();
            BorrowingView.Book = book;
            Calendar.SelectedDate = DateTime.Today;
            Calendar.BlackoutDates.Add(new CalendarDateRange(DateTime.Today.AddDays(1), DateTime.Today.AddDays(1).AddYears(1000)));
        }

        public string ToAuthorsNames(Book book) {
            StringBuilder builder = new StringBuilder();
            if (book?.Authors != null && book?.Authors.Count > 0) {
                for (int i = 0; i < book.Authors.Count; i++) { 
                    var authorsList = book.Authors.ToList();
                    if (i == 0) {
                        builder.Append($"{authorsList[i].FirstName} {authorsList[i].LastName}");
                        continue;
                    }
                    builder.Append($"\n {authorsList[i].FirstName} {authorsList[i].LastName}");
                }
            }

            return builder.ToString();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !Validation.GetHasError(FirstName);
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e) {
            BorrowingView.Id = Guid.NewGuid();
            BorrowingView.BorrowDate = Calendar.SelectedDate.Value;
            var borrowing = Mapper.Instance.Map<Borrowing>(BorrowingView);
            ApplicationDbContext.Instance.Entry(borrowing).State = EntityState.Added;
            ApplicationDbContext.Instance.SaveChanges();
            Hub.Instance.Publish(new BorrowBookMessage{Borrowing = borrowing});
            this.Close();
        }
    }
}