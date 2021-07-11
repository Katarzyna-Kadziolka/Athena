using Athena.Data;
using Athena.Data.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Athena
{
    public static class BuilderExtensions
    {
        public static void Configure(this EntityTypeBuilder<Book> entity) {
            entity.HasKey(a => a.Id);
            entity
                .HasMany(a => a.Authors)
                .WithMany(a => a.Books);
                //.UsingEntity(a => a.ToTable("BooksAuthors"));
            entity
                .HasOne(a => a.Series)
                .WithMany(a => a.Books);
            entity
                .HasOne(a => a.PublishingHouse)
                .WithMany(a => a.Books);
            entity
                .HasOne(a => a.StoragePlace)
                .WithMany(a => a.Books);
            entity
                .HasMany(a => a.Borrowings)
                .WithOne(a => a.Book)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            entity
                .HasMany(a => a.Categories)
                .WithMany(a => a.Books)
                .UsingEntity(a => a.ToTable("BooksCategories"));
        }
    }
}
