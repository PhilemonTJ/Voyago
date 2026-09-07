var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume();

var userDb = postgres.AddDatabase("userdb");

var userService = builder.AddProject<Projects.Voyago_UserService>("user-service")
    .WithReference(userDb)
    .WaitFor(userDb);

var busService = builder.AddProject<Projects.Voyago_BusService>("bus-service");

var bookingService = builder.AddProject<Projects.Voyago_BookingService>("booking-service");

builder.Build().Run();
