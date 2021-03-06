﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Athena.Annotations;
using Athena.Data.Borrowings;

namespace Athena.Data.Books {
    public class BookView : IDataErrorInfo, INotifyPropertyChanged {
        public Guid Id { get; set; }

        public string Title {
            get => _title;
            set {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public int? PublishmentYear {
            get => _year;
            set {
                _year = value;
                OnPropertyChanged(nameof(PublishmentYear));
            }
        }

        public Language Language { get; set; }
        public string ISBN { get; set; }
        public string Comment { get; set; }
        public int? VolumeNumber { get; set; }

        public virtual ICollection<Author> Authors { get; set; }
        public virtual Series.Series Series { get; set; }
        public virtual PublishingHouse PublishingHouse { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual StoragePlace StoragePlace { get; set; }
        public virtual ICollection<Borrowing> Borrowing { get; set; }

        private BookViewValidator _bookViewValidator;
        private string _title;
        private int? _year;

        public BookView() {
            _bookViewValidator = new BookViewValidator();
        }

        public string Error {
            get {
                if (_bookViewValidator != null) {
                    var results = _bookViewValidator.Validate(this);
                    if (results != null && results.Errors.Any()) {
                        var errors = string.Join(Environment.NewLine,
                            results.Errors.Select(x => x.ErrorMessage).ToArray());
                        return errors;
                    }
                }

                return string.Empty;
            }
        }

        public string this[string columnName] {
            get {
                var firstOrDefault = _bookViewValidator.Validate(this).Errors
                    .FirstOrDefault(a => a.PropertyName == columnName);
                if (firstOrDefault != null) {
                    return _bookViewValidator != null ? firstOrDefault.ErrorMessage : null;
                }

                return null;
            }
        }

<<<<<<< HEAD
        public Book ToBook() {
            return new Book {
                Id = Id,
                Title = Title,
                Authors = Authors.ToList(),
                Series = Series,
                PublishingHouse = PublishingHouse,
                PublishmentYear = PublishmentYear,
                VolumeNumber = VolumeNumber,
                Language = Language,
                ISBN = ISBN,
                StoragePlace = StoragePlace,
                Categories = Categories,
                Comment = Comment,
                Borrowing = Borrowing
            };
        }

=======
>>>>>>> 69d9c17518b8a2d462dc6274e8a2264147131c39
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}