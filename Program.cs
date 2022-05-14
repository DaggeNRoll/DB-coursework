using System.Text.RegularExpressions;
using Dapper;
using Npgsql;


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

    var pathValues = path.Value?.Split("/"); 

    //await WriteToDB("User ID=postgres;Password=Vhhvze05042002;Host=localhost;Port=5432;Database=Kursach;", response, request);
    

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
        await UpdatePerson(response,request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeletePerson(id,response,request);
    }
    else switch (pathValues?.Length)
    {
       
        case 3 when path.Value?.Split("/")[1] == "images":
            await SendImage(response, path.Value?.Split("/")[2]);
            break;
        case 3 when path.Value?.Split("/")[1] == "css":
            await SendCss(response, path.Value?.Split("/")[2]);
            break;
        case 3 when path.Value?.Split("/")[1]=="html":
            await SendHtml(response, path.Value?.Split("/")[2]);
            break;
        case 3 when path.Value?.Split("/")[1]=="js":
            await SendJs(response, path.Value?.Split("/")[2]);
            break;
        case 3 when path.Value?.Split("/")[1]=="data":
            await SendData(response, path.Value?.Split("/")[2]);
            break;
        default:
            response.ContentType="text/html; charset=utf-8";
            await response.SendFileAsync("html/PatientsList-2.html");
            break;
    }
});

app.Run();

async Task SendData(HttpResponse response, string? dataFilePath)
{
    await response.SendFileAsync("data/" + dataFilePath);
}
async Task SendCss(HttpResponse response, string? cssFilePath)
{
    await response.SendFileAsync("css/" + cssFilePath);
}

async Task SendImage(HttpResponse response, string? imageFilePath)
{
    await response.SendFileAsync("images/" + imageFilePath);
}

async Task SendHtml(HttpResponse response, string? htmlFilePath)
{
    response.ContentType = "text/html; charset=utf-8";
    await response.SendFileAsync("html/" + htmlFilePath);
}

async Task SendJs(HttpResponse response, string? jsFilePath)
{
    await response.SendFileAsync("js/" + jsFilePath);
}

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

async Task UpdatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        Person? userData = await request.ReadFromJsonAsync<Person>();
        if(userData != null)
        {
            var user = users.FirstOrDefault((u) => u.Id == userData.Id);
            if(user != null)
            {
                user.Age = userData.Age;
                user.Name = userData.Name;
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode=404;
                await response.WriteAsJsonAsync(new { message = "Пользователь не найден!"});
            }
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode=404;
        await response.WriteAsJsonAsync(new { message ="Некорректные данные!"});
    }
}

async Task WriteToDB(string conString, HttpResponse response, HttpRequest request)
{
    using var connection = new NpgsqlConnection(conString);
    await connection.QueryAsync("INSERT INTO kdh VALUES(1, 1, 0, 32, true, true, 'abc', 0, 0, 0, true, 'abc', 1, 1, true, 0)");
}

async Task DeletePerson(string? id, HttpResponse response, HttpRequest request)
{
    Person? user = users.FirstOrDefault((u) => u.Id == id);

    if( user != null)
    {
        users.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode=404;
        await response.WriteAsJsonAsync(new {message = "Пользователь не найден"});
    }
}




public class Person
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string Group { get; set; } = "";
    public string AC { get; set; } = "";
    public int Age { get; set; }
}

public class Visit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public int Priority { get; set; }
}

public class Authorization
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Password { get; set; } = "";
    public string Login { get; set; } = "";
}

public class KDH
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public int Gender { get; set; }
    public int? LengthOfMenopause { get; set; }
    public int AggravatedHeredity { get; set; }
    public bool LiveWithFamily { get; set; }
    public string FamilyStatus { get; set; } = "";
    public int Children { get; set; }
    public int PhysicalActivity { get; set; }
    public int WorkStatus { get; set; }
    public bool HasOccupationalHazards { get; set; }
    public string? OccupationalHazards { get; set; }
    public bool Smoking { get; set; }
    public int? NumberOfCigarettes { get; set; }
    public bool Dislipidemia { get; set; }
    public int Hypertension { get; set; }
}

public class CriteriaForException
{
    public bool SymptomaticAG { get; set; }
    public bool Cardiomyopathy { get; set; }
    public bool HeartValvePathology { get; set; }
    public bool HeartRateAndConductancePathology { get; set; }
    public bool EndocrineDisease { get; set; }
    public bool ChronicLiverRenalFailure { get; set; }
    public bool OncoHemoDisease { get; set; }
    public bool CollagenOutbreak { get; set; }
    public bool MorbideObesity { get; set; }
    public bool InflammatoryBowelDisease { get; set; }
    public bool OOP { get; set; }
    public bool OperationAntibioticAntiInflamatoryTherapy { get; set; }
    public bool PsychotropicDrug { get; set; }
    public int CriteriaForExceptionId { get; set; }
    public bool? RASBlockers { get; set; }
    public int VisitId { get; set; }
    
}
