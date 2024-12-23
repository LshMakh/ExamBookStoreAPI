using Exam.Models;
using Exam.Packages;
using Oracle.ManagedDataAccess.Client;
using System.Data;

public interface IPKG_LSH_BOOKS
{
    Task<List<Book>> GetBooks();
    Task<Book> GetBookById(int id);
    Task<(string result, Book book)> AddOrUpdateBook(string title, string author, int quantity, decimal price);
    Task<string> UpdateBook(int bookId, string title, string author, decimal price);
    Task<string> DeleteBook(int bookId);
}


public class PKG_LSH_BOOKS : PKG_BASE, IPKG_LSH_BOOKS
{
    public async Task<List<Book>> GetBooks()
    {
        var books = new List<Book>();
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.get_all_books";
                cmd.CommandType = CommandType.StoredProcedure;

                var cursorParameter = new OracleParameter
                {
                    ParameterName = "p_books_cursor",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                };
                cmd.Parameters.Add(cursorParameter);

                cmd.Parameters.Add(new OracleParameter
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Varchar2,
                    Size = 1000
                });

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        books.Add(new Book
                        {
                            BookId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            Title = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Author = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Quantity = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            Price = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4)
                        });
                    }
                }
            }
        }
        return books;
    }
    public async Task<Book> GetBookById(int id)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.get_book_by_id";
                cmd.CommandType = CommandType.StoredProcedure;

               
                cmd.Parameters.Add(new OracleParameter("p_book_id", id));

                var cursorParameter = new OracleParameter
                {
                    ParameterName = "p_book_cursor",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                };
                cmd.Parameters.Add(cursorParameter);

                cmd.Parameters.Add(new OracleParameter
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Varchar2,
                    Size = 1000
                });

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Book
                        {
                            BookId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Author = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            Price = reader.GetDecimal(4)
                        };
                    }
                }
            }
        }
        return null;
    }
    public async Task<(string result, Book book)> AddOrUpdateBook(string title, string author, int quantity, decimal price)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.add_update_book";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_title", OracleDbType.Varchar2).Value = title;
                cmd.Parameters.Add("p_author", OracleDbType.Varchar2).Value = author;
                cmd.Parameters.Add("p_quantity", OracleDbType.Int32).Value = quantity;
                cmd.Parameters.Add("p_price", OracleDbType.Decimal).Value = price;
                cmd.Parameters.Add("p_result", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                var result = cmd.Parameters["p_result"].Value.ToString();

                var book = await GetBookByDetails(conn, title, author, price);
                return (result, book);
            }
        }
    }

    private async Task<Book> GetBookByDetails(OracleConnection conn, string title, string author, decimal price)
    {
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT book_id, title, author, quantity, price FROM lsh_books " +
                            "WHERE UPPER(title) = UPPER(:title) AND UPPER(author) = UPPER(:author) AND price = :price";
            cmd.Parameters.Add(new OracleParameter("title", title));
            cmd.Parameters.Add(new OracleParameter("author", author));
            cmd.Parameters.Add(new OracleParameter("price", price));

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new Book
                    {
                        BookId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Quantity = reader.GetInt32(3),
                        Price = reader.GetDecimal(4)
                    };
                }
            }
        }
        return null;
    }

    public async Task<string> UpdateBook(int bookId, string title, string author, decimal price)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.update_book";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_book_id", OracleDbType.Int32).Value = bookId;
                cmd.Parameters.Add("p_title", OracleDbType.Varchar2).Value = title;
                cmd.Parameters.Add("p_author", OracleDbType.Varchar2).Value = author;
                cmd.Parameters.Add("p_price", OracleDbType.Decimal).Value = price;
                cmd.Parameters.Add("p_result", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                return cmd.Parameters["p_result"].Value.ToString();
            }
        }
    }

    public async Task<string> DeleteBook(int bookId)
    {
        using (var conn = new OracleConnection(ConnStr))
        {
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "pkg_lsh_books.delete_book";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_book_id", OracleDbType.Int32).Value = bookId;
                cmd.Parameters.Add("p_result", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                return cmd.Parameters["p_result"].Value.ToString();
            }
        }
    }
}