using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleBank.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountTypeToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
            name: "Type",
            table: "Accounts",
            type: "int",
            nullable: false,
            defaultValue: 0);

        }
    }
}