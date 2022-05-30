using System.Data;
using System.Linq.Expressions;
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
using var connection = new NpgsqlConnection(builder.Configuration.GetConnectionString("KursachDb"));//могут быть проблемы с лямбдами

app.Map("/api", appBuilder =>
{
    appBuilder.Map("/patients", Patients);
    appBuilder.Map("/kdh", Kdh);
    appBuilder.Map("/criteriaforinclusion", CritForInc);
    appBuilder.Map("/criteriaforexceprion", CritForExc);
    appBuilder.Map("/visit", Visit);
    /*appBuilder.Map("/html", Html);
    appBuilder.Map("/images", Images);
    appBuilder.Map("/css", Css);
    appBuilder.Map("/js", Js);
    appBuilder.Map("/data", Data);*/
});



/*app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;

    string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    var pathValues = path.Value?.Split("/"); 

    //await WriteToDB("User ID=postgres;Password=Vhhvze05042002;Host=localhost;Port=5432;Database=Kursach;", response, request);
    
    //using var connection = new NpgsqlConnection(builder.Configuration.GetConnectionString("KursachDb"));

    //await GetCriteriaForInclusion(response, connection, 3, 0);
    

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
    else if (path == "/api/patients" && request.Method == "GET")
    {
        await GetAllPatients(response, connection);
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
});*/

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;

    switch (path.Value?.Split("/").Length)
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

void Visit(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        try
        {
            var visitPriority = path.Value?.Split("/")[1];
            if (visitPriority is "0" or "1" or "2" or "3")
            {
                switch (request.Method)
                {
                    case "POST":
                        await GetVisit(response, request, connection, Convert.ToInt32(visitPriority));
                        break;
                    case "DELETE":

                        break;
                }
            }
            else
            {
                throw new Exception("Неверный адрес!");
            }
        }
        catch(Exception)
        {
            response.StatusCode = 404;
            await response.WriteAsJsonAsync(new { message = "Неверный адрес!" });
        }
    });
}

void CritForExc(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async(context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        switch (request.Method)
        {
            case "GET":
               
                break;
                
            case "PUT":
                    
                break;
                
            case "DELETE":
                    
                break;
                
            case "POST":
                break;
        
        }
    });
}

void Html(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        await SendHtml(response, path.Value?.Split("/")[2]);
    });
}

void Images(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        await SendImage(response, path.Value?.Split("/")[2]);
    });
}

void Css(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        await SendCss(response, path.Value?.Split("/")[2]);
    });
}

void Js(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        await SendJs(response, path.Value?.Split("/")[2]);
    });
}

void Data(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        await SendData(response, path.Value?.Split("/")[2]);
    });
}

void CritForInc(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async(context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        switch (request.Method)
        {
            case "GET":
               
                break;
                
            case "PUT":
                    
                break;
                
            case "DELETE":
                    
                break;
                
            case "POST":
                break;
        
        }
    });
}

void Patients(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        if (path.Value?.Split("/").Length == 4)
        {
            switch (request.Method)
            {
                case "GET":
                    await GetPatient(response, connection, Convert.ToInt32(path.Value?.Split("/")[3]));
                    break;
                
                case "PUT":
                    
                    break;
                
                case "DELETE":
                    
                    break;
                
                case "POST":
                    break;
            }
        }
        else
        {
            switch (request.Method)
            {
                case "GET":
                    await GetAllPatients(response, connection);
                    break;
                case "PUT":
                    await CreatePatient(response,request,connection);
                    break;
                case "POST":
                    await EditPatient(response, request, connection);
                    break;
                case "DELETE":
                    await DeletePatient(response, request, connection);
                    break;
            }
        }
    });
}

void Kdh(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        switch (request.Method)
        {
            case "GET":
                await GetKDH(response, request, connection);
                break;
                
            case "PUT":
                    
                break;
            
                
            case "POST":
                break;
        
        }
    });
}

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

async Task GetAllPatients(HttpResponse response, NpgsqlConnection connection)
{
    var patients = connection.Query<User>("SELECT * FROM \"user\" WHERE \"Group\" LIKE 'patient'");
    await response.WriteAsJsonAsync(patients);
}

async Task GetPatient(HttpResponse response, NpgsqlConnection connection, int id)
{
    
    var query = "SELECT * FROM \"user\" WHERE \"Id\" = @Id";

    var param = new DynamicParameters();
    param.Add("@Id", id);
    
    var patient = connection.Query<User>(query,param);
    await response.WriteAsJsonAsync(patient);
}

async Task CreatePatient(HttpResponse response, HttpRequest request, NpgsqlConnection connection)
{
    try
    {
        
        var patient = await request.ReadFromJsonAsync<User>();
        if (patient != null)
        {
            var query = "INSERT INTO \"user\" (\"Name\", \"PhoneNumber\", \"Email\", \"Group\"," +
                        " \"AC\", \"Age\") VALUES (@Name, @PhoneNumber, @Email, @Group, @Ac, @Age);";

            var param = new DynamicParameters();
            param.Add("@Name", patient.Name);
            param.Add("@PhoneNumber", patient.PhoneNumber);
            param.Add("@Email", patient.Email);
            param.Add("@Group", patient.Group);
            param.Add("@Ac", patient.AC);
            param.Add("@Age", patient.Age);

            connection.Query(query, param);
            await response.WriteAsJsonAsync(patient);
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

async Task EditPatient(HttpResponse response, HttpRequest request, NpgsqlConnection connection)
{
    try
    {
        var beingEditedPatient = await request.ReadFromJsonAsync<User>();
        if (beingEditedPatient != null)
        {
            var query = "UPDATE \"user\" SET \"Name\"=@Name, \"PhoneNumber\"=@PhoneNumber, \"Email\"=@Email," +
                        " \"Group\"=@Group, \"AC\"=@Ac, \"Age\"=@Age WHERE \"Id\"=@Id;";

            var param = new DynamicParameters();
            param.Add("@Name", beingEditedPatient.Name);
            param.Add("@PhoneNumber", beingEditedPatient.PhoneNumber);
            param.Add("@Email", beingEditedPatient.Email);
            param.Add("@Group", beingEditedPatient.Group);
            param.Add("@Ac", beingEditedPatient.AC);
            param.Add("@Age", beingEditedPatient.Age);
            param.Add("@Id", beingEditedPatient.Id);

            connection.Query(query, param);
            await response.WriteAsJsonAsync(beingEditedPatient);
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

async Task DeletePatient(HttpResponse response, HttpRequest request, NpgsqlConnection connection)
{
    try
    {
        
        var patients = await request.ReadFromJsonAsync<List<User>>();
        var patient = patients[0];
        if (patient != null)
        {
            var query = "DELETE FROM \"user\" WHERE \"Id\"=@Id";

            var param = new DynamicParameters();
            param.Add("@Id", patient.Id);
            connection.Query(query, param);
            await response.WriteAsJsonAsync(patient);
        }
        else
        {
            throw new Exception("Неверные данные!");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные!" });
    }
}

async Task GetKDH(HttpResponse response, HttpRequest request, NpgsqlConnection connection)
{
    try
    {
        var visit = await request.ReadFromJsonAsync<Visit>();

        if (visit != null)
        {
            var query = "select * from \"kdh\" where \"VisitId\" = @Id";
            var param = new DynamicParameters();
            param.Add("@Id", visit.Id);

            var kdh = connection.Query<KDH>(query, param);

            if (kdh.Any())
            {
                await response.WriteAsJsonAsync(kdh);
            }
            else
            {
                query = "insert into \"kdh\" (\"VisitId\") values (@Id)";
                connection.Query(query, param);

                query = "select * from \"kdh\" where \"VisitId\" = @Id";
                kdh = connection.Query<KDH>(query, param);

                await response.WriteAsJsonAsync(kdh);
            }
        }
        else
        {
            throw new Exception("Где-то ошибка!");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new
            { message = "Где-то снова ошибка. Можно уже закончить наконец-то этот курсач?" });
    }
}

async Task GetVisit(HttpResponse response, HttpRequest request, NpgsqlConnection connection, int priority)
{
    try
    {
        var patient = await request.ReadFromJsonAsync<User>();
        if (patient != null)
        {
            var query = "select * from \"visit\" where \"UserId\" = @Id and \"Priority\" = @Priority";
            var param = new DynamicParameters();
            param.Add("@Id", patient.Id);
            param.Add("@Priority", priority);
            var visit = connection.Query<Visit>(query, param);

            if (visit.Any())
            {
                await response.WriteAsJsonAsync(visit);
            }
            else
            {
                query = "insert into \"visit\" (\"UserId\", \"Date\", \"Priority\") values (@Id, @Date::date, @Priority)";
                
                param.Add("@Date", DateTime.Today);

                connection.Query(query, param);

                query = "select * from \"visit\" where \"UserId\" = @Id";
                visit = connection.Query<Visit>(query, param);

                await response.WriteAsJsonAsync(visit);
            }
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

async Task DeleteVisit(HttpResponse response, HttpRequest request, NpgsqlConnection connection)
{
    try
    {
        var visit = await request.ReadFromJsonAsync<Visit>();

        if (visit != null)
        {
            var query = "delete from \"visit\" where \"Id\"=@Id";
            var param = new DynamicParameters();
            param.Add("@Id", visit.Id);

            connection.Query(query, param);
            await response.WriteAsJsonAsync(visit);
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

async Task GetCriteriaForException(HttpResponse response, NpgsqlConnection connection, int id, int visitPriority)
{
    var query = "SELECT * FROM \"criteriaForException\" WHERE \"criteriaForException\".\"VisitId\" = (SELECT \"Id\"" +
                " FROM \"visit\" WHERE \"visit\".\"UserId\" = @Id AND \"visit\".\"Priority\"" +
                "= @VisitPriority)";

    var param = new DynamicParameters();
    param.Add("@Id",id);
    param.Add("@VisitPriority", visitPriority);

    var criteriaForException = connection.Query<CriteriaForException>(query, param);

    await response.WriteAsJsonAsync(criteriaForException);
}

async Task GetCriteriaForInclusion(HttpResponse response, HttpRequest request, NpgsqlConnection connection)
{
    try
    {
        var visit = await request.ReadFromJsonAsync<Visit>();

        if (visit != null)
        {
            var querySelect = "select * from \"criteriaForInclusion\" where \"VisitId\" = @Id";
            var param = new DynamicParameters();
            param.Add("@Id", visit.Id);

            var criteriaForInclusion = connection.Query(querySelect, param);

            if (criteriaForInclusion.Any())
            {
                await response.WriteAsJsonAsync(criteriaForInclusion);
            }
            else
            {
                var queryInsert = "insert into \"criteriaForInclusion\" (\"VisitId\") values (@Id)";
                connection.Query(queryInsert, param);

                criteriaForInclusion = connection.Query<CriteriaForInclusion>(querySelect, param);
                await response.WriteAsJsonAsync(criteriaForInclusion);
            }
        }
        else
        {
            throw new Exception("Ошибка! Опять вкалывать!");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new
        {
            message = "Ошибка опять снова какая-то. Надеюсь искренне, что это сообщение" +
                      "никогда не отправится"
        });
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
    public string? Email { get; set; } = "";
    public string Group { get; set; } = "";
    public string AC { get; set; } = "";
    public int Age { get; set; }
}

public class Visit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public int Priority { get; set; }
    
    public Visit(){}

    public Visit(int id, int userId, DateTime date, int priority)
    {
        this.Id = id;
        this.UserId = userId;
        this.Date = date;
        this.Priority = priority;
    }
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
    public int? Gender { get; set; }
    public int? LengthOfMenopause { get; set; }
    public int? AggravatedHeredity { get; set; }
    public bool? LiveWithFamily { get; set; }
    public string? FamilyStatus { get; set; } = "";
    public int? Children { get; set; }
    public int? PhysicalActivity { get; set; }
    public int? WorkStatus { get; set; }
    public bool? HasOccupationalHazards { get; set; }
    public string? OccupationalHazards { get; set; }
    public bool? Smoking { get; set; }
    public int? NumberOfCigarettes { get; set; }
    public bool? Dislipidemia { get; set; }
    public int? Hypertension { get; set; }
}

public class CriteriaForException
{
    public bool? SymptomaticAG { get; set; }
    public bool? Cardiomyopathy { get; set; }
    public bool? HeartValvePathology { get; set; }
    public bool? HeartRateAndConductancePathology { get; set; }
    public bool? EndocrineDisease { get; set; }
    public bool? ChronicLiverRenalFailure { get; set; }
    public bool? OncoHemoDisease { get; set; }
    public bool? CollagenOutbreak { get; set; }
    public bool? MorbideObesity { get; set; }
    public bool? InflammatoryBowelDisease { get; set; }
    public bool? OOP { get; set; }
    public bool? OperationAntibioticAntiInflamatoryTherapy { get; set; }
    public bool? PsychotropicDrug { get; set; }
    public int CriteriaForExceptionId { get; set; }
    public bool? RASBlockers { get; set; }
    public int VisitId { get; set; }
    
}

public class CriteriaForInclusion
{
    public int Id { get; set; }
    public bool? AgeBetween40_65 { get; set; }
    public bool? LowAndModerateRiskOfCardiovascularComplications { get; set; }
    public bool? ParticipationAgreement { get; set; }
    public bool? Hypertension { get; set; }
    public bool? SRBOrDOrA { get; set; }
    public int VisitId { get; set; }
}
