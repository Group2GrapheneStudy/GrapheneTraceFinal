using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrapheneTrace.Migrations
{
    /// <inheritdoc />
    public partial class InicialMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_PressureFrames_PressureFrameFrameId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId1",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Clinicians_ClinicianId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Patients_PatientId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_UserAccounts_CreatedByUserId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_UserAccounts_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DataFiles_UserAccounts_UploadedByUserId",
                table: "DataFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Clinicians_ClinicianId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_DataFiles_DataFileId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Patients_PatientId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Clinicians_ClinicianId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_UserAccounts_CreatedByUserId",
                table: "Prescriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment");

            migrationBuilder.RenameTable(
                name: "Appointment",
                newName: "Appointments");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_PatientId",
                table: "Appointments",
                newName: "IX_Appointments_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_CreatedByUserId",
                table: "Appointments",
                newName: "IX_Appointments_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_ClinicianId",
                table: "Appointments",
                newName: "IX_Appointments_ClinicianId");

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Feedbacks",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_PressureFrames_PressureFrameFrameId",
                table: "Alerts",
                column: "PressureFrameFrameId",
                principalTable: "PressureFrames",
                principalColumn: "FrameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId",
                table: "Alerts",
                column: "UserAccountUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId1",
                table: "Alerts",
                column: "UserAccountUserId1",
                principalTable: "UserAccounts",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Clinicians_ClinicianId",
                table: "Appointments",
                column: "ClinicianId",
                principalTable: "Clinicians",
                principalColumn: "ClinicianId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_UserAccounts_CreatedByUserId",
                table: "Appointments",
                column: "CreatedByUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_UserAccounts_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataFiles_UserAccounts_UploadedByUserId",
                table: "DataFiles",
                column: "UploadedByUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Clinicians_ClinicianId",
                table: "Feedbacks",
                column: "ClinicianId",
                principalTable: "Clinicians",
                principalColumn: "ClinicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_DataFiles_DataFileId",
                table: "Feedbacks",
                column: "DataFileId",
                principalTable: "DataFiles",
                principalColumn: "DataFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Patients_PatientId",
                table: "Feedbacks",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_PressureFrames_PressureFrameFrameId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId1",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Clinicians_ClinicianId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_UserAccounts_CreatedByUserId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_UserAccounts_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DataFiles_UserAccounts_UploadedByUserId",
                table: "DataFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Clinicians_ClinicianId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_DataFiles_DataFileId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Patients_PatientId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Clinicians_ClinicianId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_UserAccounts_CreatedByUserId",
                table: "Prescriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments");

            migrationBuilder.RenameTable(
                name: "Appointments",
                newName: "Appointment");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointment",
                newName: "IX_Appointment_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_CreatedByUserId",
                table: "Appointment",
                newName: "IX_Appointment_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_ClinicianId",
                table: "Appointment",
                newName: "IX_Appointment_ClinicianId");

            migrationBuilder.AlterColumn<string>(
                name: "Rating",
                table: "Feedbacks",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_PressureFrames_PressureFrameFrameId",
                table: "Alerts",
                column: "PressureFrameFrameId",
                principalTable: "PressureFrames",
                principalColumn: "FrameId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId",
                table: "Alerts",
                column: "UserAccountUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_UserAccounts_UserAccountUserId1",
                table: "Alerts",
                column: "UserAccountUserId1",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Clinicians_ClinicianId",
                table: "Appointment",
                column: "ClinicianId",
                principalTable: "Clinicians",
                principalColumn: "ClinicianId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Patients_PatientId",
                table: "Appointment",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_UserAccounts_CreatedByUserId",
                table: "Appointment",
                column: "CreatedByUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_UserAccounts_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DataFiles_UserAccounts_UploadedByUserId",
                table: "DataFiles",
                column: "UploadedByUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Clinicians_ClinicianId",
                table: "Feedbacks",
                column: "ClinicianId",
                principalTable: "Clinicians",
                principalColumn: "ClinicianId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_DataFiles_DataFileId",
                table: "Feedbacks",
                column: "DataFileId",
                principalTable: "DataFiles",
                principalColumn: "DataFileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Patients_PatientId",
                table: "Feedbacks",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Prescriptions_UserAccounts_CreatedByUserId",
                table: "Prescriptions",
                column: "CreatedByUserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
