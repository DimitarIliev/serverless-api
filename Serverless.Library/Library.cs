using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;

namespace Serverless.Library
{
    public static class Library
    {
        [FunctionName("GetAll")]
        public static async Task<IActionResult> GetAllBooks(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "books")] HttpRequest req,
            [CosmosDB(databaseName: "Library", collectionName: "books", SqlQuery = "SELECT * FROM books", ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<dynamic> books,
            ILogger log)
        {
            log.LogInformation("Get All Books called!");

            return new OkObjectResult(books);
        }

        [FunctionName("Get")]
        public static async Task<IActionResult> GetBook(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "books/{id}")] HttpRequest req,
            [CosmosDB(databaseName: "Library", collectionName: "books", Id = "{id}", PartitionKey = "{id}", ConnectionStringSetting = "CosmosDbConnectionString")] dynamic book,
           string id,
           ILogger log)
        {
            log.LogInformation($"Get Book called with id {id}!");

            return new OkObjectResult(book);
        }

        [FunctionName("Post")]
        public static async Task<IActionResult> AddBook(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "books")] HttpRequest req,
              [CosmosDB(databaseName: "Library", collectionName: "books", ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<dynamic> documentsOut,
           ILogger log)
        {
            log.LogInformation("Add Book called!");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            await documentsOut.AddAsync(new
            {
                id = Guid.NewGuid().ToString(),
                name = data?.name,
                author = data?.author
            });

            return new OkResult();
        }

        [FunctionName("Put")]
        public static async Task<IActionResult> UpdateBook(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "books/{id}")] HttpRequest req,
              [CosmosDB(databaseName: "Library", collectionName: "books", Id = "{id}", PartitionKey = "{id}", ConnectionStringSetting = "CosmosDbConnectionString")] dynamic book,
           string id,
           ILogger log)
        {
            log.LogInformation($"Update Book called with id {id}!");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            book.name = data?.name;
            book.author = data?.author;

            return new OkResult();
        }

        [FunctionName("Delete")]
        public static async Task<IActionResult> DeleteBook(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "books/{id}")] HttpRequest req,
             [CosmosDB(databaseName: "Library", collectionName: "books", Id = "{id}", PartitionKey = "{id}", ConnectionStringSetting = "CosmosDbConnectionString")] dynamic book,
             [CosmosDB(databaseName: "Library", collectionName: "books", ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
           string id,
           ILogger log)
        {
            log.LogInformation($"Delete Book called called with id {id}!");

            await client.DeleteDocumentAsync(book.SelfLink, new Microsoft.Azure.Documents.Client.RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(id) });

            return new OkResult();
        }
    }
}