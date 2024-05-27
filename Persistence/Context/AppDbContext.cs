using Domain.Entities.Orders;
using Domain.Entities.Books;
using Domain.Entities.Stocks;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;


namespace AmxBookstore.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Order> Orders { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.Deleted);
            modelBuilder.Entity<Order>().HasQueryFilter(u => !u.Deleted);
            modelBuilder.Entity<Stock>().HasQueryFilter(u => !u.Deleted);
            modelBuilder.Entity<Book>().HasQueryFilter(u => !u.Deleted);
            modelBuilder.Entity<Order>(entity =>
            {
                entity.OwnsMany(o => o.Products);
            });

            modelBuilder.Entity<Book>().HasData(
                new Book("The Catcher in the Rye", "A novel by J.D. Salinger", 214, "J.D. Salinger", 9.99M),
                new Book("To Kill a Mockingbird", "A novel by Harper Lee", 281, "Harper Lee", 7.99M),
                new Book("1984", "A novel by George Orwell", 328, "George Orwell", 8.99M),
                new Book("Pride and Prejudice", "A novel by Jane Austen", 279, "Jane Austen", 6.99M),
                new Book("The Great Gatsby", "A novel by F. Scott Fitzgerald", 180, "F. Scott Fitzgerald", 10.99M),
                new Book("Moby-Dick", "A novel by Herman Melville", 635, "Herman Melville", 11.99M),
                new Book("War and Peace", "A novel by Leo Tolstoy", 1225, "Leo Tolstoy", 14.99M),
                new Book("The Odyssey", "An epic poem attributed to Homer", 541, "Homer", 12.99M),
                new Book("Crime and Punishment", "A novel by Fyodor Dostoevsky", 671, "Fyodor Dostoevsky", 9.99M),
                new Book("The Brothers Karamazov", "A novel by Fyodor Dostoevsky", 824, "Fyodor Dostoevsky", 13.99M),
                new Book("Brave New World", "A novel by Aldous Huxley", 311, "Aldous Huxley", 9.99M),
                new Book("Jane Eyre", "A novel by Charlotte Brontë", 500, "Charlotte Brontë", 7.99M),
                new Book("Wuthering Heights", "A novel by Emily Brontë", 416, "Emily Brontë", 8.99M),
                new Book("The Hobbit", "A novel by J.R.R. Tolkien", 310, "J.R.R. Tolkien", 10.99M),
                new Book("Fahrenheit 451", "A novel by Ray Bradbury", 194, "Ray Bradbury", 6.99M),
                new Book("Anna Karenina", "A novel by Leo Tolstoy", 864, "Leo Tolstoy", 13.99M),
                new Book("Lord of the Flies", "A novel by William Golding", 224, "William Golding", 7.99M),
                new Book("Gone with the Wind", "A novel by Margaret Mitchell", 1037, "Margaret Mitchell", 12.99M),
                new Book("Great Expectations", "A novel by Charles Dickens", 505, "Charles Dickens", 8.99M),
                new Book("The Scarlet Letter", "A novel by Nathaniel Hawthorne", 272, "Nathaniel Hawthorne", 6.99M),
                new Book("Frankenstein", "A novel by Mary Shelley", 280, "Mary Shelley", 5.99M),
                new Book("Dracula", "A novel by Bram Stoker", 418, "Bram Stoker", 7.99M),
                new Book("The Adventures of Huckleberry Finn", "A novel by Mark Twain", 366, "Mark Twain", 6.99M),
                new Book("The Iliad", "An epic poem attributed to Homer", 704, "Homer", 14.99M),
                new Book("The Divine Comedy", "An epic poem by Dante Alighieri", 798, "Dante Alighieri", 15.99M)
            );


            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
