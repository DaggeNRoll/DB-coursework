using System.Text.RegularExpressions;

List<Person> users = new List<Person>
{
    new() {Id = Guid.NewGuid().ToString(), Name="Van", Age=69},
    new(){Id = Guid.NewGuid().ToString(), Name="Billy", Age=38},
    new() {Id = Guid.NewGuid().ToString(), Name="Ivan", Age=25}
};

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;

    string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    if(path=="/api/users" && request.Method == "GET")
    {
        await GetAllPeople(response);
    }
    else if(Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        string? id = path.Value?.Split("/")[3];
        await GetPerson(id, response, request);
    }
    else if(path=="/api/users" && request.Method == "POST")
    {
        await CreatePerson(response, request);
    }
    else if(path=="/api/users" && request.Method == "Put")
    {

    }
});

app.Run();

async Task GetAllPeople(HttpResponse response)
{
    await response.WriteAsJsonAsync(users);
}

async Task GetPerson(string? id, HttpResponse response, HttpRequest request)
{
    Person? user = users.FirstOrDefault((u) => u.Id == id);
    if (user != null)
        await response.WriteAsJsonAsync(user);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Пользователь не найден!" });
    }
}

async Task CreatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        var user = await request.ReadFromJsonAsync<Person>();
        if (user != null)
        {
            user.Id = Guid.NewGuid().ToString();
            users.Add(user);
            await response.WriteAsJsonAsync(user); 
        }
        else
        {
            throw new Exception("Некорректные данные!");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные!" });
    }
}


public class Person
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

