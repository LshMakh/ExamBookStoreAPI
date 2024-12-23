using Exam.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IPKG_LSH_BOOKS _bookPackage;

    public BookController(IPKG_LSH_BOOKS bookPackage)
    {
        _bookPackage = bookPackage;
    }

    [HttpGet]
    public async Task<ActionResult<List<Book>>> GetBooks()
    {
        var books = await _bookPackage.GetBooks();
        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _bookPackage.GetBookById(id);
        if (book == null)
            return NotFound();
        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<Book>> AddBook(Book book)
    {
        var (result, newBook) = await _bookPackage.AddOrUpdateBook(
            book.Title,
            book.Author,
            book.Quantity,
            book.Price
        );

        if (result.StartsWith("ERROR"))
            return BadRequest(result);

        return Ok(newBook);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<string>> UpdateBook(int id, Book book)
    {
        var result = await _bookPackage.UpdateBook(
            id,
            book.Title,
            book.Author,
            book.Price
        );

        if (result == "SUCCESS")
            return Ok(result);
        else if (result == "BOOK_NOT_FOUND")
            return NotFound();
        else
            return BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<string>> DeleteBook(int id)
    {
        var result = await _bookPackage.DeleteBook(id);

        if (result == "SUCCESS")
            return Ok(result);
        else if (result == "BOOK_NOT_FOUND")
            return NotFound();
        else
            return BadRequest(result);
    }
}