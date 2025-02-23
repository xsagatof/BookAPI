using BookAPI.Data;
using BookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Reflection.Metadata.BlobBuilder;

namespace BookAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class BookController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public BookController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<string>>> GetListOfAllBooks(int pageNumber = 1, int pageSize = 10)
		{
			if (pageNumber < 1 || pageSize < 1)
			{
				return BadRequest("Invalid page number or page size.");
			}

			var totalBooks = await _context.Books.CountAsync();
			var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

			if (pageNumber > totalPages && totalBooks > 0)
			{
				return BadRequest("Page number exceeds total pages.");
			}

			var bookTitles = await _context.Books
				.OrderByDescending(book => book.ViewsCount)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.Select(book => book.Title)
				.ToListAsync();

			var response = new
			{
				TotalItems = totalBooks,
				TotalPages = totalPages,
				PageNumber = pageNumber,
				PageSize = pageSize,
				Items = bookTitles
			};

			return Ok(response);
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetBookById(int id)
		{
			var task = await _context.Books.FindAsync(id);
			if (task == null)
				return NotFound();
			return Ok(task);
		}

		[HttpPost]
		public async Task<ActionResult<IEnumerable<Book>>> AddBooks([FromBody] List<Book> books)
		{
			if (books == null || books.Count == 0)
			{
				return BadRequest("No books provided.");
			}

			var existingTitles = await _context.Books.Select(b => b.Title).ToListAsync();

			var booksToAdd = new List<Book>();
			var duplicateTitles = new List<string>();

			foreach (var book in books)
			{
				if (existingTitles.Contains(book.Title))
				{
					duplicateTitles.Add(book.Title);
				}
				else
				{
					booksToAdd.Add(book);
					existingTitles.Add(book.Title);
				}
			}

			if (duplicateTitles.Count > 0)
			{
				return BadRequest($"Books with the following titles already exist: {string.Join(", ", duplicateTitles)}");
			}

			_context.Books.AddRange(booksToAdd);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetListOfAllBooks), booksToAdd);
		}

		[HttpPut]
		[Route("{id}")]
		public async Task<IActionResult> UpdateBook(Book book)
		{
			var dbBook = await _context.Books.FindAsync(book.Id);
			if (dbBook == null)
				return NotFound();

			dbBook.Title = book.Title;
			dbBook.Author = book.Author;
			dbBook.PublicationYear = book.PublicationYear;
			dbBook.ViewsCount = book.ViewsCount;

			await _context.SaveChangesAsync();

			return Ok(_context.Books);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBook(int id)
		{
			var book = await _context.Books.FindAsync(id);
			if (book == null)
			{
				return NotFound();
			}

			_context.Books.Remove(book);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteBooks([FromBody] List<int> ids)
		{
			if (ids == null || ids.Count == 0)
			{
				return BadRequest("No book IDs provided.");
			}

			var booksToDelete = await _context.Books.Where(b => ids.Contains(b.Id)).ToListAsync();
			if (booksToDelete.Count == 0)
			{
				return NotFound("No books found with the provided IDs.");
			}

			_context.Books.RemoveRange(booksToDelete);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}
