using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MiniValidation;
using RestSharp;
using System.Text.Json.Nodes;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/consultaCep/{cep}", async (int cep) =>
{
    var client = new RestClient("http://viacep.com.br/");
    var request = new RestRequest($"ws/{cep}/json/");
    var response = await client.ExecuteGetAsync(request);
    var data = JsonSerializer.Deserialize<JsonNode>(response.Content!)!;

    return JsonSerializer.Deserialize<Cep>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
})
    .Produces<Cep>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetCep")
    .WithTags("Cep");

app.MapGet("/conta", async (MinimalContextDb context) => await context.Contas.ToListAsync())
    .WithName("GetConta")
    .WithTags("Conta");

app.MapGet(
        "/conta/{id}",
        async (int id, MinimalContextDb context) =>
            await context.Contas.FindAsync(id) is Conta conta
                ? Results.Ok(conta)
                : Results.NotFound()
    )
    .Produces<Conta>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetContaId")
    .WithTags("Conta");

app.MapPost("/conta", async (
    MinimalContextDb context,
    Conta conta) =>
{
    if (!MiniValidator.TryValidate(conta, out var errors))
        return Results.ValidationProblem(errors);

    context.Contas.Add(conta);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.CreatedAtRoute("GetContaId", new { id = conta.Id }, conta)
    : Results.BadRequest("Houve um problema ao salvar o registro");
}).ProducesValidationProblem()
    .Produces<Conta>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PostConta")
    .WithTags("Conta");

app.MapPut("/conta/{id}", async (
    int id,
    MinimalContextDb context,
    Conta conta) =>
{
    var contaBD = await context.Contas.FindAsync(id);
    if (contaBD == null) return Results.NotFound();

    contaBD.Description = conta.Description;
    contaBD.Name = conta.Name;

    if (!MiniValidator.TryValidate(conta, out var errors))
        return Results.ValidationProblem(errors);

    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("Houve um problema ao salvar o registro");

}).ProducesValidationProblem()
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PutConta")
.WithTags("Conta");

app.MapDelete("/conta/{id}", async (
    int id,
    MinimalContextDb context) =>
{
    var conta = await context.Contas.FindAsync(id);
    if (conta == null) return Results.NotFound();

    context.Contas.Remove(conta);
    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("Houve um problema ao apagar o registro");

}).Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithName("DeleteConta")
.WithTags("Conta");

app.Run();
