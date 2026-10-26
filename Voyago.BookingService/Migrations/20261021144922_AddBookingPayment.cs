﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Invoices",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Issued");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Bookings");
        }
    }
}
