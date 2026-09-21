var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var userDb = postgres.AddDatabase("userdb");
var busDb = postgres.AddDatabase("busdb");
var bookingDb = postgres.AddDatabase("bookingdb");

var userService = builder.AddProject<Projects.Voyago_UserService>("user-service")
    .WithReference(userDb)
    .WaitFor(userDb);

var busService = builder.AddProject<Projects.Voyago_BusService>("bus-service")
    .WithReference(busDb)
    .WaitFor(busDb);

var bookingService = builder.AddProject<Projects.Voyago_BookingService>("booking-service")
    .WithReference(bookingDb)
    .WaitFor(bookingDb);

builder.Build().Run();
