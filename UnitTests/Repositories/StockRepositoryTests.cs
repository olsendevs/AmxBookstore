using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using AmxBookstore.Infrastructure.Persistence;
using AmxBookstore.Infrastructure.Repositories;
using Domain.Entities.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StockRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public StockRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDb")
            .Options;
    }

    private async Task SeedDatabase(AppDbContext context)
    {
        var stocks = new List<Stock>
        {
            new Stock(Guid.NewGuid(), 10),
            new Stock(Guid.NewGuid(), 5)
        };

        context.Stocks.AddRange(stocks);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnStock_WhenStockExists()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new StockRepository(context);
            var stockId = context.Stocks.First().Id;

            var stock = await repository.GetByIdAsync(stockId);

            Assert.NotNull(stock);
            Assert.Equal(stockId, stock.Id);
        }
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnStock_WhenStockExists()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new StockRepository(context);
            var productId = context.Stocks.First().BookId;

            var stock = await repository.GetByProductIdAsync(productId);

            Assert.NotNull(stock);
            Assert.Equal(productId, stock.BookId);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllStocks()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new StockRepository(context);

            var stocks = await repository.GetAllAsync();

            Assert.NotNull(stocks);
            Assert.Equal(2, stocks.Count());
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddStock()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            var repository = new StockRepository(context);
            var newStock = new Stock(Guid.NewGuid(), 20);

            await repository.AddAsync(newStock);

            var stock = await context.Stocks.FindAsync(newStock.Id);
            Assert.NotNull(stock);
            Assert.Equal(newStock.Quantity, stock.Quantity);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStock()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new StockRepository(context);
            var stock = context.Stocks.First();
            stock.UpdateQuantity(30);

            await repository.UpdateAsync(stock);

            var updatedStock = await context.Stocks.FindAsync(stock.Id);
            Assert.NotNull(updatedStock);
            Assert.Equal(30, updatedStock.Quantity);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkStockAsDeleted()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new StockRepository(context);
            var stockId = context.Stocks.First().Id;

            await repository.DeleteAsync(stockId);

            var stock = await context.Stocks.FindAsync(stockId);
            Assert.True(stock.Deleted);
        }
    }

    [Fact]
    public async Task GetPagedStocksAsync_ShouldReturnPagedStocks()
    {
        using (var context = new AppDbContext(_dbContextOptions))
        {
            await SeedDatabase(context);
            var repository = new StockRepository(context);

            var stocks = await repository.GetPagedStocksAsync(1, 1);

            Assert.NotNull(stocks);
            Assert.Single(stocks);
        }
    }
}
