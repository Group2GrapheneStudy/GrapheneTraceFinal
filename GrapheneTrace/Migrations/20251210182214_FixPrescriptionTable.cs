using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrapheneTrace.Migrations
{
    /// <inheritdoc />
    public partial class FixPrescriptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Clinicians_ClinicianId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_UserAccounts_CreatedByUserId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_CreatedByUserId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "MedicationName",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Prescriptions",
                newName: "DrugName");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Prescriptions",
                newName: "Frequency");

            migrationBuilder.RenameColumn(
                name: "IssuedDate",
                table: "Prescriptions",
                newName: "DatePrescribed");

            migrationBuilder.RenameColumn(
                name: "DurationDays",
                table: "Prescriptions",
                newName: "UserAccountUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Prescriptions",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "PrescriptionId",
                table: "Prescriptions",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "Prescriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Dosage",
                table: "Prescriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_UserAccountUserId",
                table: "Prescriptions",
                column: "UserAccountUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Clinicians_ClinicianId",
                table: "Prescriptions",
                column: "ClinicianId",
                principalTable: "Clinicians",
                principalColumn: "ClinicianId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_UserAccounts_UserAccountUserId",
                table: "Prescriptions",
                column: "UserAccountUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Clinicians_ClinicianId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_UserAccounts_UserAccountUserId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_UserAccountUserId",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "UserAccountUserId",
                table: "Prescriptions",
                newName: "DurationDays");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Prescriptions",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "Prescriptions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "DrugName",
                table: "Prescriptions",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DatePrescribed",
                table: "Prescriptions",
                newName: "IssuedDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Prescriptions",
                newName: "PrescriptionId");

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "Prescriptions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Dosage",
                table: "Prescriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicationName",
                table: "Prescriptions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Prescriptions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_CreatedByUserId",
                table: "Prescriptions",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Clinicians_ClinicianId",
                table: "Prescriptions",
                column: "ClinicianId",
                principalTable: "Clinicians",
                principalColumn: "ClinicianId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_UserAccounts_CreatedByUserId",
                table: "Prescriptions",
                column: "CreatedByUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
