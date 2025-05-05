using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagementSystem.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly LibraryDbContext _context;
    private IDbContextTransaction _transaction;
    private bool _disposed;
    
    public IUserRepository Users { get; }
    public IBookRepository Books { get; }
    public ICategoryRepository Categories { get; }
    public IBookBorrowingRequestRepository BookBorrowingRequests { get; }
    public IBookBorrowingRequestDetailRepository BookBorrowingRequestDetails { get; }
    public IUserActivityLogRepository UserActivityLogs { get; }
    
    public UnitOfWork(
        LibraryDbContext context,
        IUserRepository userRepository,
        IBookRepository bookRepository,
        ICategoryRepository categoryRepository,
        IBookBorrowingRequestRepository borrowingRequestRepository,
        IBookBorrowingRequestDetailRepository borrowingRequestDetailRepository,
        IUserActivityLogRepository userActivityLogRepository)
    {
        _context = context;
        Users = userRepository;
        Books = bookRepository;
        Categories = categoryRepository;
        BookBorrowingRequests = borrowingRequestRepository;
        BookBorrowingRequestDetails = borrowingRequestDetailRepository;
        UserActivityLogs = userActivityLogRepository;
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            return; // Transaction already begun
        }
        
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction == null)
            {
                return; // No transaction to commit
            }
            
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_transaction == null)
            {
                return; // No transaction to rollback
            }
            
            await _transaction.RollbackAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            
            _disposed = true;
        }
    }
} 