using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace StricklandPropane.Migrations
{
    public partial class ApplicationUserCurrentBasket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HomeState",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "GrillingPreference",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AddColumn<long>(
                name: "CurrentBasketId",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentBasketId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<byte>(
                name: "HomeState",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<byte>(
                name: "GrillingPreference",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
