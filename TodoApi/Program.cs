using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt =>
        opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//agregando un Map Group
var todoItems = app.MapGroup("/todoitems");

//todoItems => Obtener todos los todos
todoItems.MapGet("/", async (TodoDb db) =>
    await db.Todos.ToListAsync());

//todoItems/complete => Obtener todos los todos terminados
todoItems.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

//todoItems/{id} => Obtener todo especifico
todoItems.MapGet("/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());


//POST: todosItems => Agregar un todo
todoItems.MapPost("/", async (Todo todo, TodoDb db) => {
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});



//PUT: todoItems => Modificar un todo
todoItems.MapPut("/{id}", async (int id, Todo inputTodo, TodoDb db) => {
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.name = inputTodo.name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

//DELETE: todoItems => Modificar un todo
todoItems.MapDelete("/{id}", async (int id, TodoDb db) => {

    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();
