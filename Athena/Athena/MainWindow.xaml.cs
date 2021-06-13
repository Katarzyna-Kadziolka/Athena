using Athena.Data.Books;
using Athena.Data.CategoriesFolder;
using Athena.Import;
using Athena.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Athena.MessageBoxes;

namespace Athena {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public ObservableCollection<BookInListView> Books { get; set; }

        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
            ApplicationDbContext.Instance.Books
                .Include(b => b.Series)
                .Include(b => b.PublishingHouse)
                .Include(b => b.StoragePlace)
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .Include(b => b.Borrowing.OrderByDescending(b => b.BorrowDate))
                .Load();
            Books = Mapper.Instance.Map<ObservableCollection<BookInListView>>(ApplicationDbContext.Instance.Books.Local
                .ToObservableCollection());

            ApplicationDbContext.Instance.ChangeTracker.StateChanged += (sender, e) => {
                if (e.Entry.Entity is Book book && e.NewState == EntityState.Modified) {
                    var bookInList = Books.Single(b => b.Id == book.Id);
                    Mapper.Instance.Map(book, bookInList);
                }
            };
            ApplicationDbContext.Instance.Books.Local.CollectionChanged += (sender, e) => {
                if (e.NewItems != null) {
                    foreach (Book item in e?.NewItems) {
                        if (Books.All(b => b.Id != item.Id)) {
                            Application.Current.Dispatcher.Invoke(()
                                => Books.Add(Mapper.Instance.Map<BookInListView>(item)));
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove) {
                    var book = (Book) e.OldItems[0];
                    var bookInList = Books.First(b => b.Id == book.Id);
                    Books.Remove(bookInList);
                }
            };
            ApplicationDbContext.Instance.Books.Local.CollectionChanged += (sender, e) => {
                if (Books.Count > 0) {
                    Application.Current.Dispatcher.Invoke(() => ImportButton.Visibility = Visibility.Hidden);
                }
                else {
                    Application.Current.Dispatcher.Invoke(() => ImportButton.Visibility = Visibility.Visible);
                }
            };
            this.Closed += (sender, args) => Application.Current.Shutdown();
        }

        public static RoutedUICommand MenuItemBorrow_Click =
            new RoutedUICommand("MenuItemBorrow_Click", "MenuItemBorrow_Click", typeof(MainWindow));

        public void ResizeGridViewColumns(GridView gridView) {
            foreach (GridViewColumn column in gridView.Columns) {
                if (double.IsNaN(column.Width)) {
                    column.Width = column.ActualWidth;
                }

                column.Width = double.NaN;
            }
        }

        private void MenuItemBorrow_Executed(object sender, RoutedEventArgs e) {
            Book book = ApplicationDbContext.Instance.Books.Single(b
                => b.Id == ((BookInListView) BookList.SelectedItem).Id);
            BorrowForm borrowForm = new BorrowForm(book);
            borrowForm.Show();
        }

        private void MenuItemBorrow_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            if (BookList.SelectedItem == null) {
                return;
            }

            var borrowings = ApplicationDbContext.Instance.Borrowings
                .Include(b => b.Book)
                .Where(b => b.Book.Id == ((BookInListView) BookList.SelectedItem).Id)
                .ToList();
            if (borrowings.Any(b => b.ReturnDate == null)) {
                e.CanExecute = false;
            }
            else {
                e.CanExecute = true;
            }
        }

        private void MenuItemEdit_Click(object sender, System.Windows.RoutedEventArgs e) {
            Book book = ApplicationDbContext.Instance.Books
                .Include(a => a.Categories)
                .Include(b => b.Series)
                .Include(b => b.PublishingHouse)
                .Include(b => b.StoragePlace)
                .Include(b => b.Authors)
                .Single(b
                    => b.Id == ((BookInListView) BookList.SelectedItem).Id);
            EditBookWindow editBook = new EditBookWindow(book);
            editBook.BookEdited += (o, e) => {
                var book = Books.First(a => a.Id == e.Entity.Id);
                Mapper.Instance.Map(e.Entity, book);
            };

            editBook.Show();
        }

        private void MenuItemDelete_Click(object sender, System.Windows.RoutedEventArgs e) {
            var messageBoxGenerator = new ConfirmBookDeleteMessageBox();
            var decision = messageBoxGenerator.Show();
            if (decision) {
                var book = ApplicationDbContext.Instance.Books
                    .Include(a => a.Borrowing)
                    .Single(b
                        => b.Id == ((BookInListView) BookList.SelectedItem).Id);
                book.Borrowing = ApplicationDbContext.Instance.Borrowings
                    .Include(a => a.Book)
                    .Where(a => a.Book.Id == book.Id)
                    .ToList();
                ApplicationDbContext.Instance.Books.Remove(book);
                ApplicationDbContext.Instance.SaveChanges();
                SearchTextBox.Text = string.Empty;
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void OpenBorrowedBookList_Click(object sender, RoutedEventArgs e) {
            BorrowedBooksListWindow borrowedBooks = new BorrowedBooksListWindow();
            borrowedBooks.Show();
        }

        private void AddBook_Click(object sender, System.Windows.RoutedEventArgs e) {
            AddBookWindow addBook = new AddBookWindow();
            addBook.Show();
        }

        private void ImportData(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            var fileName = openFileDialog.FileName;
            if (fileName == "") {
                return;
            }

            MainGrid.Visibility = Visibility.Collapsed;
            BackgroundWorker worker = new BackgroundWorker {WorkerReportsProgress = true};
            ImportButton.Visibility = Visibility.Hidden;
            ImportGrid.Visibility = Visibility.Visible;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += (o, args) => {
                if (args.Result != null) {
                    if (args.Result.GetType() == typeof(ImportException)) {
                        var messageBox = new RemoveDataBaseMessageBox();
                        var answer = messageBox.Show();
                        if (answer) {
                            ResetDatabase();
                        }
                        ImportButton.Visibility = Visibility.Visible;
                    }
                }
                ImportGrid.Visibility = Visibility.Hidden;
                MainGrid.Visibility = Visibility.Visible;
                ResizeGridViewColumns(BooksGridView);
            };
            worker.RunWorkerAsync(argument: fileName);
        }

        private void ResetDatabase() {
            ApplicationDbContext.Instance.Database.EnsureDeleted();
            ApplicationDbContext.Instance.Database.EnsureCreated();
        }


        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            ProgressBarStatus.Value = e.ProgressPercentage;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e) {
            var fileName = (string) e.Argument;
            try {
                var dataImporter = new DatabaseImporter();
                dataImporter.ImportFromSpreadsheet(fileName);
            }
            catch (ImportException exception) {
                e.Result = exception;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var text = SearchTextBox.Text;
            if (text.Length == 0)
                BookList.ItemsSource = Books;
            if (text.Length < 3)
                return;
            var fillteredBooks = Books.Where(b
                => b.Title.Contains(text, StringComparison.CurrentCultureIgnoreCase) ||
                   (b.Series?.SeriesName != null &&
                    b.Series.SeriesName.Contains(text, StringComparison.CurrentCultureIgnoreCase)) ||
                   (b.PublishingHouse?.PublisherName != null &&
                    b.PublishingHouse.PublisherName.Contains(text, StringComparison.CurrentCultureIgnoreCase)) ||
                   (b.Authors.Any(a => a.ToString().Contains(text, StringComparison.CurrentCultureIgnoreCase))) ||
                   (b.Categories.Any(c
                       => c.GetDescription().Contains(text, StringComparison.CurrentCultureIgnoreCase)))
            );

            BookList.ItemsSource = fillteredBooks;
        }

        private void BookList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Book book = Mapper.Instance.Map<Book>(BookList.SelectedItem);
            EditBookWindow editBook = new EditBookWindow(book);
            editBook.Show();
        }

        private void DeleteDatabase_Click(object sender, RoutedEventArgs e)
        {
            // poka¿ messagebox "czy na pewno chcesz?"
            // po klikniêciu tak - informacja ¿e baza danych zosta³a usuniête
            // zamkniêcie tych okien
        }
    }
}