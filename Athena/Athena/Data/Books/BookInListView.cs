using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Athena.Annotations;
using Athena.Data.Borrowings;
using Athena.Data.PublishingHouses;

namespace Athena.Data.Books
{
    public class BookInListView : INotifyPropertyChanged
    {
        private Guid id;
        private string title;
        private Series.Series series;
        private PublishingHouse publishingHouse;
        private int? publishmentYear;
        private Language language;
        private StoragePlace storagePlace;
        private ObservableCollection<Borrowing> _borrowings;

        public IList<Category> Categories { get; set; }
        public Guid Id
        {
            get => id; set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        public string Title
        {
            get => title; set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        public ObservableCollection<Author> Authors { get; set; }
        public Series.Series Series
        {
            get => series; set
            {
                series = value;
                OnPropertyChanged(nameof(Series));
            }
        }
        public PublishingHouse PublishingHouse
        {
            get => publishingHouse; set
            {
                publishingHouse = value;
                OnPropertyChanged(nameof(PublishingHouse));
            }
        }
        public Language Language
        {
            get => language; set
            {
                language = value;
                OnPropertyChanged(nameof(Language));
            }
        }
        public StoragePlace StoragePlace
        {
            get => storagePlace; set
            {
                storagePlace = value;
                OnPropertyChanged(nameof(StoragePlace));
            }
        }
        public int? PublishmentYear
        {
            get => publishmentYear;
            set
            {
                publishmentYear = value;
                OnPropertyChanged(nameof(PublishmentYear));
            }
        }

        public ObservableCollection<Borrowing> Borrowings {
            get => _borrowings;
            set {
                _borrowings = value;
                _borrowings.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(LastBorrowName));
            } 
        }

        public string LastBorrowName => this.Borrowings.Count < 1 ? string.Empty : this.Borrowings.Last().ReturnDate != null ? string.Empty : $"{this.Borrowings.Last().FirstName} {this.Borrowings.Last().LastName}";

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
