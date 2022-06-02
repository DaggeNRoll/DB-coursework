using System.Data;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Dapper;
using Npgsql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using Azure;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/acessdenied";
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

using var connection =
    new NpgsqlConnection(builder.Configuration.GetConnectionString("KursachDb")); //могут быть проблемы с лямбдами

app.Map("/login", Login);

app.MapGet("/accessdenied", async (context) =>
{
    context.Response.StatusCode = 403;
    await context.Response.WriteAsync("Access Denied");
});




app.MapPost("/accessdenied", async (HttpContext context) =>
{
    await context.Response.SendFileAsync("html/rejected.html");
});

app.Map("/patient", Measurements);

    
   



app.Map("/api", [Authorize(Roles = "doctor")](appBuilder) =>
{
    appBuilder.Map("/patients", Patients);
    appBuilder.Map("/kdh", Kdh);
    appBuilder.Map("/criteriaforinclusion", CritForInc);
    appBuilder.Map("/criteriaforexception", CritForExc);
    appBuilder.Map("/visit", Visit);
    appBuilder.Map("/home", HomePage);

    /*appBuilder.Map("/html", Html);
    appBuilder.Map("/images", Images);
    appBuilder.Map("/css", Css);
    appBuilder.Map("/js", Js);
    appBuilder.Map("/data", Data);*/
});

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
        case 3 when path.Value?.Split("/")[1] == "html":
            await SendHtml(response, path.Value?.Split("/")[2]);
            break;
        case 3 when path.Value?.Split("/")[1] == "js":
            await SendJs(response, path.Value?.Split("/")[2]);
            break;
        case 3 when path.Value?.Split("/")[1] == "data":
            await SendData(response, path.Value?.Split("/")[2]);
            break;
        default:
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync("html/signin.html");
            break;
    }
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return "Данные удалены";
});

app.Run();

void HomePage(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("/html/PatientList-2.html");
    });
}

void Measurements(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        switch (request.Method)
        {
            case "POST":
                try
                {
                    var login = path.Value?.Split("/")[1];

                    if (login == null)
                    {
                        throw new Exception("Некорректный логин");
                    }

                    var queryUser = "select \"UserId\" from \"authorization\" where \"Login\" = @Login";

                    var param = new DynamicParameters();
                    param.Add("@Login", login);

                    var userId = connection.Query<int>(queryUser, param);

                    if (!userId.Any())
                    {
                        throw new Exception("Пользователь не найден");
                    }

                    var queryMeasurements = "select * from \"measurement\" where \"UserId\" = @UserId";
                    param.Add("UserId", userId.ToList()[0]);

                    var measurements = connection.Query<Measurement>(queryMeasurements, param);

                    await response.WriteAsJsonAsync(measurements);

                }
                catch (Exception)
                {
                    response.StatusCode = 400;
                    await response.WriteAsJsonAsync("Произошла ошибка");
                }
                break;
            
            case "PUT":
                try
                {
                    var login = path.Value?.Split("/")[1];
                    
                    var measurement = await request.ReadFromJsonAsync<Measurement>();

                    if (measurement is null)
                    {
                        throw new Exception("Некорректные данные");
                    }

                    var querySelect = "select * from \"measurement\" where \"Date\" = @Date";
                    var queryUser = "select \"UserId\" from \"authorization\" where \"Login\" = @Login";
                    
                    var param = new DynamicParameters();
                    param.Add("@Date", measurement.Date);
                    param.Add("@BloodPressureMorning", measurement.BloodPressureMorning);
                    param.Add("@BloodPressureEvening", measurement.BloodPressureEvening);
                    param.Add("@HeartRateMorning", measurement.HeartRateMorning);
                    param.Add("@HeartRateEvening", measurement.HeartRateEvening);
                    param.Add("@Login", login);
                    var userId = connection.Query<int>(queryUser, param);
                    param.Add("@UserId", userId.ToList()[0]);

                    var measurementDb = connection.Query<Measurement>(querySelect, param);

                    if (measurementDb.Any())
                    {
                        var queryUpdate =
                            "update \"measurement\" set \"BloodPressureMorning\" = @BloodPressureMorning, " +
                            "\"BloodPressureEvening\" = @BloodPressureEvening, \"HeartRateMorning\" = @HeartRateMorning, " +
                            "\"HeartRateEvening\" = @HeartRateEvening where \"Date\" = @Date";

                        connection.Query(queryUpdate, param);
                        
                        await response.WriteAsJsonAsync(measurement);
                    }
                    else
                    {
                        var queryInsert = "insert into \"measurement\"  (\"Date\", \"BloodPressureMorning\", " +
                                          "\"BloodPressureEvening\", \"HeartRateMorning\", \"HeartRateEvening\", " +
                                          "\"UserId\") " +
                                          "values (@Date, @BloodPressureMorning, @BloodPressureEvening, " +
                                          "@HeartRateMorning, @HeartRateEvening, @UserId)";
                        connection.Query(queryInsert, param);
                        await response.WriteAsJsonAsync(measurement);
                    }

                }
                catch (Exception)
                {
                    response.StatusCode = 400;
                    await response.WriteAsJsonAsync(new { message = "Произошла ошибка" });
                }
                
                
                break;
            case "DELETE":
                try
                {
                    var measurements = await request.ReadFromJsonAsync<List<Measurement>>();
                    

                    if (!measurements.Any())
                    {
                        throw new Exception("Некорректные данные");
                    }

                    var measurement = measurements[0];

                    var queryDelete = "delete from \"measurement\" where \"Date\" = @Date";
                    var param = new DynamicParameters();
                    param.Add("@Date", measurement.Date);

                    connection.Query(queryDelete, param);
                }
                catch (Exception)
                {
                    response.StatusCode = 400;
                    await response.WriteAsJsonAsync(new { message = "Произошла ошибка" });
                }
                break;
        }
    });
}

void Login(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var request = context.Request;
    var response = context.Response;
    var path = request.Path;

    switch (request.Method)
    {
        case "GET":
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync("html/signin.html");
            break;
        case "POST":
            var loginData = await request.ReadFromJsonAsync<LoginData>();
            if (loginData is null)
            {
                await response.WriteAsJsonAsync(new{group="Некорректные данные"});
                return;
            }

            if (loginData.Login == null || loginData.Password == null)
            {
                await response.WriteAsJsonAsync(new { group = "Логин или пароль не указан" });
                return;
            }

            try
            {
                var query = "select * from \"authorization\" where \"Login\" = @Login";

                var param = new DynamicParameters();
                param.Add("@Login", loginData.Login);

                var userData = connection.Query<Authorization>(query, param);

                if (!userData.Any())
                {
                    await response.WriteAsJsonAsync(new { group = "Пользователь не найден!" });
                    return;
                }

                var password = HashPassword(loginData.Password, userData.ToList()[0].Salt);

                if (password != userData.ToList()[0].Password)
                {
                    await response.WriteAsJsonAsync(new { group = "Неверный пароль!" });
                    return;;
                }

                query = "select \"Group\" from \"user\" where \"Id\" = @Id";
                param.Add("@Id", userData.ToList()[0].UserId);

                var group = connection.Query<string>(query, param);

                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, loginData.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, group.ToList()[0])
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await context.SignInAsync(claimsPrincipal);
                await response.WriteAsJsonAsync(new { group = $"{group.ToList()[0]}" });
            }
            catch (Exception)
            {
                Results.BadRequest("Что-то пошло не по плану");
            }
            break;
    }
    });
}


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
                        await DeleteVisit(response, request, connection);
                        break;
                }
            }
            else
            {
                throw new Exception("Неверный адрес!");
            }
        }
        catch (Exception)
        {
            response.StatusCode = 404;
            await response.WriteAsJsonAsync(new { message = "Неверный адрес!" });
        }
    });
}


void CritForExc(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        switch (request.Method)
        {
            case "GET":
                await GetCriteriaForException(response, connection, Convert.ToInt32(path.Value?.Split("/")[1]));
                break;

            case "PUT":
                await EditCriteriaForException(response, request, connection,
                    Convert.ToInt32(path.Value?.Split("/")[1]));
                break;

            case "DELETE":

                break;

            case "POST":
                break;
        }
    });
}


void CritForInc(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;

        switch (request.Method)
        {
            case "GET":
                await GetCriteriaForInclusion(response, connection, Convert.ToInt32(path.Value?.Split("/")[1]));
                break;

            case "PUT":
                await EditCriteriaForInclusion(response, request, connection,
                    Convert.ToInt32(path.Value?.Split("/")[1]));
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
                    await CreatePatient(response, request, connection);
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
                await GetKdh(response, connection, Convert.ToInt32(path.Value?.Split("/")[1]));
                break;

            case "PUT":
                await EditKdh(response, request, connection, Convert.ToInt32(path.Value?.Split("/")[1]));
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

    var patient = connection.Query<User>(query, param);
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

        if (patients.Any())
        {
            var patient = patients[0];
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
                await response.WriteAsJsonAsync(visit.ToList()[0]);
            }
            else
            {
                query =
                    "insert into \"visit\" (\"UserId\", \"Date\", \"Priority\") values (@Id, @Date::date, @Priority)";

                param.Add("@Date", DateTime.Today);

                connection.Query(query, param);

                query = "select * from \"visit\" where \"UserId\" = @Id";
                visit = connection.Query<Visit>(query, param);

                await response.WriteAsJsonAsync(visit.ToList()[0]);
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

async Task GetKdh(HttpResponse response, NpgsqlConnection connection, int visitId)
{
    try
    {
        var query = "select * from \"kdh\" where \"VisitId\" = @Id";
        var param = new DynamicParameters();
        param.Add("@Id", visitId);

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
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new
            { message = "Где-то снова ошибка. Можно уже закончить наконец этот курсач?" });
    }
}

async Task EditKdh(HttpResponse response, HttpRequest request, NpgsqlConnection connection, int visitId)
{
    try
    {
        var kdh = await request.ReadFromJsonAsync<KDH>();

        if (kdh != null)
        {
            var query = "update \"kdh\" set \"Gender\" = @Gender, \"LengthOfMenopause\" = @Menopause, " +
                        "\"AggravatedHeredity\" = @Heredity, \"LiveWithFamily\" = @Live, " +
                        "\"FamilyStatus\" = @Family, \"Children\" = @Children, \"PhysicalActivity\" = @PhActivity, " +
                        "\"WorkStatus\" = @Work, \"HasOccupationalHazards\" = @HasHazards, \"OccupationalHazards\" = " +
                        "@Hazards, \"Smoking\" = @Smoking, \"NumberOfCigarettes\" = @Cigarettes, " +
                        "\"Dislipidemia\" = @Dislipidemia, \"Hypertension\" = @Hypertension where \"VisitId\" = @VisitId;";

            var param = new
            {
                Gender = kdh.Gender, Menopause = kdh.LengthOfMenopause, Heredity = kdh.AggravatedHeredity,
                Live = kdh.LiveWithFamily, Family = kdh.FamilyStatus, Children = kdh.Children,
                PhActivity = kdh.PhysicalActivity,
                Work = kdh.WorkStatus, HasHazards = kdh.HasOccupationalHazards, Hazards = kdh.OccupationalHazards,
                Smoking = kdh.Smoking, Cigarettes = kdh.NumberOfCigarettes, Dislipidemia = kdh.Dislipidemia,
                Hypertension = kdh.Hypertension, VisitId = visitId
            };
            connection.Query(query, param);

            await response.WriteAsJsonAsync(kdh);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные!" });
    }
}

async Task GetCriteriaForException(HttpResponse response, NpgsqlConnection connection, int visitId)
{
    try
    {
        var qurySelect = "select * from \"criteriaForException\" where \"VisitId\" = @VisitId;";
        var param = new DynamicParameters();
        param.Add("@VisitId", visitId);

        var criteriaForException = connection.Query<CriteriaForException>(qurySelect, param);

        if (criteriaForException.Any())
        {
            await response.WriteAsJsonAsync(criteriaForException);
        }
        else
        {
            var queryInsert = "insert into \"criteriaForException\" (\"VisitId\") values (@VisitId);";
            connection.Query(queryInsert, param);

            criteriaForException = connection.Query<CriteriaForException>(qurySelect, param);
            await response.WriteAsJsonAsync(criteriaForException);
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Это не баг, а фича!" });
    }
}

async Task EditCriteriaForException(HttpResponse response, HttpRequest request, NpgsqlConnection connection,
    int visitId)
{
    try
    {
        var criteriaForException = await request.ReadFromJsonAsync<CriteriaForException>();

        if (criteriaForException != null)
        {
            var query =
                "update \"criteriaForException\" set \"SymptomaticAG\" = @Ag, \"Сardiomyopathy\" = @Cardiomyopathy, " +
                "\"HeartValvePathology\" = @HeartValve, \"HeartRateAndConductancePathology\" = @HeartRate, " +
                "\"EndocrineDisease\" = @Endocrine, \"ChronicLiverRenalFailure\" = @Liver, " +
                "\"OncoHemoDisease\" = @OncoDisease, \"CollagenOutbreak\" = @Collagen, \"MorbideObesity\" = @Obesity, " +
                "\"InflammatoryBowelDisease\" = @Bowel, \"OPP\" = @Opp, " +
                "\"OperationAntibioticAntiInflamatoryTherapy\" = @Operation, \"PsychotropicDrug\" = @Drug, " +
                "\"RASBlockers\" = @Rasb where \"VisitId\" = @VisitId;";

            var param = new
            {
                Ag = criteriaForException.SymptomaticAG, Cardiomyopathy = criteriaForException.Cardiomyopathy,
                HeartValve = criteriaForException.HeartValvePathology,
                HeartRate = criteriaForException.HeartRateAndConductancePathology,
                Endocrine = criteriaForException.EndocrineDisease,
                Liver = criteriaForException.ChronicLiverRenalFailure,
                OncoDisease = criteriaForException.OncoHemoDisease, Collagen = criteriaForException.CollagenOutbreak,
                Obesity = criteriaForException.MorbideObesity, Bowel = criteriaForException.InflammatoryBowelDisease,
                OPP = criteriaForException.OOP,
                Operation = criteriaForException.OperationAntibioticAntiInflamatoryTherapy,
                Drug = criteriaForException.PsychotropicDrug, Rasb = criteriaForException.RASBlockers, VisitId = visitId
            };
            connection.Query(query, param);

            await response.WriteAsJsonAsync(criteriaForException);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные!" });
    }
}

async Task GetCriteriaForInclusion(HttpResponse response, NpgsqlConnection connection, int visitId)
{
    try
    {
        var qurySelect = "select * from \"criteriaForInclusion\" where \"VisitId\" = @VisitId;";
        var param = new DynamicParameters();
        param.Add("@VisitId", visitId);

        var criteriaForInclusion = connection.Query<CriteriaForInclusion>(qurySelect, param);

        if (criteriaForInclusion.Any())
        {
            await response.WriteAsJsonAsync(criteriaForInclusion);
        }
        else
        {
            var queryInsert = "insert into \"criteriaForInclusion\" (\"VisitId\") values (@VisitId);";
            connection.Query(queryInsert, param);

            criteriaForInclusion = connection.Query<CriteriaForInclusion>(qurySelect, param);
            await response.WriteAsJsonAsync(criteriaForInclusion);
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

async Task EditCriteriaForInclusion(HttpResponse response, HttpRequest request, NpgsqlConnection connection,
    int visitId)
{
    try
    {
        var criteriaForInclusion = await request.ReadFromJsonAsync<CriteriaForInclusion>();

        if (criteriaForInclusion != null)
        {
            var query = "update \"criteriaForInclusion\" set \"AgeBetween40_65\" = @Age, " +
                        "\"LowAndModerateRiskOfCardiovascularComplications\" = @Risk, \"ParticipationAgreement\" = @Agreement," +
                        "\"Hypertension\" = @Hypertension, \"SRBOrDOrA\" = @Srb where \"VisitId\" = @VisitId";

            var param = new
            {
                Age = criteriaForInclusion.AgeBetween40_65,
                Risk = criteriaForInclusion.LowAndModerateRiskOfCardiovascularComplications,
                Agreement = criteriaForInclusion.ParticipationAgreement,
                Hypertension = criteriaForInclusion.Hypertension,
                Srb = criteriaForInclusion.SRBOrDOrA, VisitId = visitId
            };
            connection.Query(query, param);

            await response.WriteAsJsonAsync(criteriaForInclusion);
        }
        else
        {
            throw new Exception("Некорректные данные!");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Что-то пошло не по плану" });
    }
}

string HashPassword(string password, string salt)
{
    return Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: password,
        salt: Convert.FromBase64String(salt),
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 100000,
        numBytesRequested: 256 / 8));
}

string CreateSalt()
{
    byte[] salt = new byte[128 / 8];
    using (var rngCsp = new RNGCryptoServiceProvider())
    {
        rngCsp.GetNonZeroBytes(salt);
    }

    return Convert.ToBase64String(salt);
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

    public Visit()
    {
    }

    public Visit(int id, int userId, DateTime date, int priority)
    {
        this.Id = id;
        this.UserId = userId;
        this.Date = date;
        this.Priority = priority;
    }
}

public class Measurement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Date { get; set; }
    public string? BloodPressureMorning { get; set; }
    public string? BloodPressureEvening { get; set; }
    public int? HeartRateMorning { get; set; }
    public int? HeartRateEvening { get; set; }
}

public class Authorization
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Password { get; set; } = "";
    public string Login { get; set; } = "";
    public string Salt { get; set; } = "";
}

public class LoginData
{
    public string? Login { get; set; }
    public string? Password { get; set; }
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
    public int? Smoking { get; set; }
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