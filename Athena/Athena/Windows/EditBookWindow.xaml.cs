using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Athena.Data.Books;
using Athena.EventManagers;
using Athena.Messages;
using Microsoft.EntityFrameworkCore;
using Hub = MessageHub.MessageHub;

namespace Athena.Windows {
    public partial class EditBookWindow {
        public EditBookWindow(Book book) {
            InitializeComponent();
            var bookControl = new BookFormControl("Edytuj książkę", "Zapisz", book);
            bookControl.ButtonCommand = new EditBookCommand();
            bookControl.ConfirmButton.Click += (sender, args) => {
                var bookView = bookControl.BookView;
                Book book = Mapper.Instance.Map<Book>(bookView);
                BookEdited?.Invoke(this, new EntityEventArgs<Book>{Entity = book});
                Hub.Instance.Publish(new EditBookMessage{BookView = bookView});
            };
            Content = bookControl;

        }

        public event EventHandler<EntityEventArgs<Book>> BookEdited;

        public void BookEditedInvoke(Book book) {
            BookEdited?.Invoke(this, new EntityEventArgs<Book>{Entity = book});
        }
    }

    public class EditBookCommand : ICommand {
        public bool CanExecute(object parameter) {
            var validator = new BookViewValidator();
            var result = validator.Validate(parameter as BookView);
            return result.IsValid;
            ;
        }

        public void Execute(object book) {
            Book bookModel = Mapper.Instance.Map<Book>(book);
            ContextTracker.AttackBookRelatedEntries(bookModel);
            var bookFromDb = ApplicationDbContext.Instance.Books
                .Include(a => a.Authors)
                .Include(a => a.Categories)
                .Include(a => a.PublishingHouse)
                .Include(a => a.Series)
                .Include(a => a.StoragePlace)
                .Single(a => a.Id == bookModel.Id);
            Mapper.Instance.Map(bookModel, bookFromDb);
            
            ApplicationDbContext.Instance.SaveChanges();
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        
    }
}