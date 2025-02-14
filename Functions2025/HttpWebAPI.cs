using System.Linq;
using System.Net;
using Functions2025.Models.School;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SchoolLibrary;

namespace Snoopy.Function;

public class HttpWebAPI
{
    private readonly ILogger<HttpWebAPI> _logger;
    private readonly SchoolContext _context;

    public HttpWebAPI(ILogger<HttpWebAPI> logger, SchoolContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("HttpWebAPI")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }

    [Function("GetStudents")]
    public HttpResponseData GetStudents(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP GET/posts trigger function processed a request in GetStudents().");

        var students = _context.Students.ToArray();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        response.WriteStringAsync(JsonConvert.SerializeObject(students));

        return response;
    }

    [Function("GetStudentById")]
    public HttpResponseData GetStudentById
    (
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students/{id}")] HttpRequestData req,
        int id
    )
    {
        _logger.LogInformation("C# HTTP GET/posts trigger function processed a request.");
        var student = _context.Students.FindAsync(id).Result;
        if (student == null)
        {
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteStringAsync("Not Found");
            return response;
        }
        var response2 = req.CreateResponse(HttpStatusCode.OK);
        response2.Headers.Add("Content-Type", "application/json");
        response2.WriteStringAsync(JsonConvert.SerializeObject(student));
        return response2;
    }

    [Function("GetStudentBySchool")]
    public async Task<HttpResponseData> GetStudentBySchool
    (
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "student/{school}")] HttpRequestData req,
        string school
    )
    {
        _logger.LogInformation($"C# HTTP GET trigger function processed a request for school: {school}");

        var students = await _context.Students
            .Where(s => s.School == school)
            .ToListAsync();

        if (students == null || students.Count == 0)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            notFoundResponse.Headers.Add("Content-Type", "application/json");
            await notFoundResponse.WriteStringAsync("{\"message\": \"No students found for this school\"}");
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonConvert.SerializeObject(students));
        
        return response;
    }


    [Function("GetNumberOfStudents")]
    public HttpResponseData GetNumberofStudents
    (
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students/count-by-school")] HttpRequestData req
    )
    {
        _logger.LogInformation("C# HTTP GET/posts trigger function processed a request.");
        var students = _context.Students.ToArray();
        var schools = new Dictionary<string, int>();
        foreach(Student s in students){
            if (!schools.ContainsKey(s.School))
            {
                schools.Add(s.School, 1);
            }
            else
            {
                schools[s.School]++;
            }
        }

        if (schools == null)
        {
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteStringAsync("Not Found");
            return response;
        }

        var response2 = req.CreateResponse(HttpStatusCode.OK);
        response2.Headers.Add("Content-Type", "application/json");
        response2.WriteStringAsync(JsonConvert.SerializeObject(schools.OrderByDescending(kv => kv.Value)));
        return response2;
    }

    [Function("CreateStudent")]
    public HttpResponseData CreateStudent
    (
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "students")] HttpRequestData req
    )
    {
        _logger.LogInformation("C# HTTP POST/posts trigger function processed a request.");
        var student = JsonConvert.DeserializeObject<Student>(req.ReadAsStringAsync().Result);
        _context.Students.Add(student);
        _context.SaveChanges();
        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteStringAsync(JsonConvert.SerializeObject(student));
        return response;
    }

    [Function("UpdateStudent")]
    public HttpResponseData UpdateStudent
    (
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "students/{id}")] HttpRequestData req,
        int id
    )
    {
        _logger.LogInformation("C# HTTP PUT/posts trigger function processed a request.");
        var student = _context.Students.FindAsync(id).Result;
        if (student == null)
        {
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteStringAsync("Not Found");
            return response;
        }
        var student2 = JsonConvert.DeserializeObject<Student>(req.ReadAsStringAsync().Result);
        student.FirstName = student2.FirstName;
        student.LastName = student2.LastName;
        student.School = student2.School;
        _context.SaveChanges();
        var response2 = req.CreateResponse(HttpStatusCode.OK);
        response2.Headers.Add("Content-Type", "application/json");
        response2.WriteStringAsync(JsonConvert.SerializeObject(student));
        return response2;
    }

    [Function("DeleteStudent")]
    public HttpResponseData DeleteStudent
    (
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "students/{id}")] HttpRequestData req,
      int id
    )
    {
        _logger.LogInformation("C# HTTP DELETE/posts trigger function processed a request.");
        var student = _context.Students.FindAsync(id).Result;
        if (student == null)
        {
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteStringAsync("Not Found");
            return response;
        }
        _context.Students.Remove(student);
        _context.SaveChanges();
        var response2 = req.CreateResponse(HttpStatusCode.OK);
        response2.Headers.Add("Content-Type", "application/json");
        response2.WriteStringAsync(JsonConvert.SerializeObject(student));
        return response2;
    }

}

