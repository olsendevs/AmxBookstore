using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using AmxBookstore.Infrastructure.Persistence;
using AmxBookstore.Infrastructure.Repositories;
using Domain.Entities.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BookRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public BookRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDb")
            .Options;
    }

    private async Task SeedDatabase(AppDbContext context)
    {
        var books = new List<Book>
        {
            new Book("Book 1", "Description 1", 100, "Author 1", 9.99m),
            new Book("Book 2", "Description 2", 150, "Author 2", 14.99m)
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBook_WhenBookExists()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new BookRepository(context);
            var bookId = context.Books.First().Id;

            var book = await repository.GetByIdAsync(bookId);

            Assert.NotNull(book);
            Assert.Equal(bookId, book.Id);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBooks()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new BookRepository(context);

            var books = await repository.GetAllAsync();

            Assert.NotNull(books);
            Assert.NotEmpty(books);
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddBook()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            var repository = new BookRepository(context);
            var newBook = new Book("New Book", "New Description", 200, "New Author", 19.99m);

            await repository.AddAsync(newBook);

            var book = await context.Books.FindAsync(newBook.Id);
            Assert.NotNull(book);
            Assert.Equal(newBook.Title, book.Title);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBook()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new BookRepository(context);
            var book = context.Books.First();
            book.UpdateTitle("Updated Title");

            await repository.UpdateAsync(book);

            var updatedBook = await context.Books.FindAsync(book.Id);
            Assert.NotNull(updatedBook);
            Assert.Equal("Updated Title", updatedBook.Title);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkBookAsDeleted()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new BookRepository(context);
            var bookId = context.Books.First().Id;

            await repository.DeleteAsync(bookId);

            var book = await context.Books.FindAsync(bookId);
            Assert.True(book.Deleted);
        }
    }

    [Fact]
    public async Task GetPagedBooksAsync_ShouldReturnPagedBooks()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new BookRepository(context);

            var books = await repository.GetPagedBooksAsync(1, 1);

            Assert.NotNull(books);
            Assert.Single(books);
        }
    }
}
