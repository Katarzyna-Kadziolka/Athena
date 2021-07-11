using System;
using System.Collections.Generic;
using Athena.Data.Borrowings;
using Athena.Data.PublishingHouses;


namespace Athena.Data.Books {
    public class Book {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int? PublishmentYear { get; set; }
        public Language Language { get; set; }
        public string ISBN { get; set; }
        public string Comment { get; set; }
        public int? VolumeNumber { get; set; }

        public IList<Author> Authors { get; set; }
        public Series.Series Series { get; set; }
        public PublishingHouse PublishingHouse { get; set; }
        public IList<Category> Categories { get; set; }
        public StoragePlace StoragePlace { get; set; }
        public IList<Borrowing> Borrowings { get; set; }
    }
}