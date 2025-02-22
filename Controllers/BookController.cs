using BookAPI.Data;
using BookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Reflection.Metadata.BlobBuilder;

namespace BookAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class BookController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public BookController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult GetAllItems()
		{
			return Ok(_context.Books);
		}

		[HttpGet]
		[Route("{id}")]
		public IActionResult GetItemById(int id)
		{
			var task = _context.Books.Find(id);
			if (task == null)
				return NotFound();
			return Ok(task);
		}

		[HttpPost]
		public IActionResult AddNewItem(Book book)
		{
			_context.Books.Add(book);
			_context.SaveChanges();
			return CreatedAtAction("GetAllItems", book);
		}

		[HttpPut]
		[Route("{id}")]
		public IActionResult UpdateItem(Book book)
		{
			var dbBook = _context.Books.Find(book.Id);
			if (dbBook == null)
				return NotFound();

			dbBook.Title = book.Title;
			dbBook.Author = book.Author;
			dbBook.PublicationYear = book.PublicationYear;
			dbBook.ViewsCount = book.ViewsCount;

			_context.SaveChanges();

			return Ok(_context.Books);
		}

		[HttpDelete]
		public IActionResult DeleteItem(int id)
		{
			var dbBook = _context.Books.Find(id);
			if (dbBook == null)
				return NotFound();

			_context.Books.Remove(dbBook);
			_context.SaveChanges();

			return Ok(_context.Books);
		}
	}
}
