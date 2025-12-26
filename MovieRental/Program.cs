using Microsoft.EntityFrameworkCore;
using MovieRental.Customer;
using MovieRental.Data;
using MovieRental.Movie;
using MovieRental.PaymentProviders;
using MovieRental.Rental;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEntityFrameworkSqlite()
    .AddDbContext<MovieRentalDbContext>();

builder.Services.AddScoped<ICustomerFeatures, CustomerFeatures>();
builder.Services.AddScoped<IMovieFeatures, MovieFeatures>();
builder.Services.AddScoped<IRentalFeatures, RentalFeatures>();
//builder.Services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();
//builder.Services.AddScoped<IPaymentProvider, PayPalProvider>();
//builder.Services.AddScoped<IPaymentProvider, MbWayProvider>();

builder.Services.AddKeyedScoped<IPaymentProvider, PayPalProvider>("PayPal");
builder.Services.AddKeyedScoped<IPaymentProvider, MbWayProvider>("MbWay");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var ctx = new MovieRentalDbContext(new DbContextOptionsBuilder<MovieRentalDbContext>().Options))
{
	ctx.Database.EnsureCreated();
}

app.Run();
