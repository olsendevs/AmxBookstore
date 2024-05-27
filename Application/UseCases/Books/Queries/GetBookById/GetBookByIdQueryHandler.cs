using MediatR;
using AmxBookstore.Application.DTOs;
using AutoMapper;
using AmxBookstore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AmxBookstore.Application.UseCases.Books.Queries.GetBookById
{
    public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDTO>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GetBookByIdQueryHandler(IBookRepository bookRepository, IMapper mapper, IMemoryCache cache)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<BookDTO> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"Book_{request.Id}";

            if (_cache.TryGetValue(cacheKey, out BookDTO bookDto))
            {
                return bookDto;
            }

            var book = await _bookRepository.GetByIdAsync(request.Id);

            bookDto = _mapper.Map<BookDTO>(book);

      
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), 
                SlidingExpiration = TimeSpan.FromSeconds(15) 
            };

            _cache.Set(cacheKey, bookDto, cacheOptions);

            return bookDto;
        }
    }
}
