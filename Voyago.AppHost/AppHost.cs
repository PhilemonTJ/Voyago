var builder = DistributedApplication.CreateBuilder(args);

var userService = builder.AddProject<Projects.Voyago_UserService>("user-service");

var busService = builder.AddProject<Projects.Voyago_BusService>("bus-service");

var bookingService = builder.AddProject<Projects.Voyago_BookingService>("booking-service");

builder.Build().Run();
