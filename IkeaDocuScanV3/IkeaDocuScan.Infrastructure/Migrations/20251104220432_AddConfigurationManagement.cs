using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkeaDocuScan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditTrail",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    User = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Action = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Details = table.Column<string>(type: "varchar(2500)", unicode: false, maxLength: 2500, nullable: true),
                    BarCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditTrail__4AB81AF0", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    CountryCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Country__5D9B0D2D34531A97", x => x.CountryCode);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Currency__408426BEA95ECF99", x => x.CurrencyCode);
                });

            migrationBuilder.CreateTable(
                name: "DocumentFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "varchar(900)", unicode: false, nullable: false),
                    FileType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: ".pdf"),
                    Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("DOCUMENTFILE_PK", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentType",
                columns: table => new
                {
                    DT_ID = table.Column<int>(type: "int", nullable: false),
                    DT_Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    BarCode = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    CounterParty = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    DateOfContract = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    Comment = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "O"),
                    ReceivingDate = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    DispatchDate = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    Fax = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    OriginalReceived = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    DocumentNo = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    AssociatedToPUA = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    VersionNo = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    AssociatedToAppendix = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    ValidUntil = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    Currency = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    Amount = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    Authorisation = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    BankConfirmation = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "N"),
                    TranslatedVersionReceived = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    ActionDate = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "O"),
                    ActionDescription = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "O"),
                    ReminderGroup = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "O"),
                    Confidential = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    IsAppendix = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CounterPartyAlpha = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "M"),
                    SendingOutDate = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "O"),
                    ForwardedToSignatoriesDate = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "O")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Document__148CEA33753529D2", x => x.DT_ID);
                });

            migrationBuilder.CreateTable(
                name: "DocuScanUser",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    UserIdentifier = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    LastLogon = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsSuperUser = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("DOCUSCANUSER_PK", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "EmailRecipientGroup",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRecipientGroup", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplate",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlainTextBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceholderDefinitions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplate", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfiguration",
                columns: table => new
                {
                    ConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConfigSection = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOverride = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfiguration", x => x.ConfigurationId);
                    table.CheckConstraint("CK_SystemConfiguration_Section", "ConfigSection IN ('Email', 'ActionReminderService', 'General', 'System')");
                });

            migrationBuilder.CreateTable(
                name: "CounterParty",
                columns: table => new
                {
                    CounterPartyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    Since = table.Column<DateTime>(type: "datetime", nullable: false),
                    Address = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    DisplayAtCheckIn = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Comments = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Country = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    City = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    AffiliatedTo = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    CounterPartyNoAlpha = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CounterParty__3B75D760", x => x.CounterPartyId);
                    table.ForeignKey(
                        name: "FK__CounterPa__Count__3D5E1FD2",
                        column: x => x.Country,
                        principalTable: "Country",
                        principalColumn: "CountryCode");
                });

            migrationBuilder.CreateTable(
                name: "DocumentName",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DocumentName__47DBAE45", x => x.ID);
                    table.ForeignKey(
                        name: "FK__DocumentN__Docum__48CFD27E",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentType",
                        principalColumn: "DT_ID");
                });

            migrationBuilder.CreateTable(
                name: "EmailRecipient",
                columns: table => new
                {
                    RecipientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRecipient", x => x.RecipientId);
                    table.ForeignKey(
                        name: "FK_EmailRecipient_Group",
                        column: x => x.GroupId,
                        principalTable: "EmailRecipientGroup",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurationAudit",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationId = table.Column<int>(type: "int", nullable: false),
                    ConfigKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangeReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurationAudit", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_ConfigAudit_Config",
                        column: x => x.ConfigurationId,
                        principalTable: "SystemConfiguration",
                        principalColumn: "ConfigurationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: true),
                    CounterPartyId = table.Column<int>(type: "int", nullable: true),
                    CountryCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("USERACCOUNT_PK", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_DocuScanUser",
                        column: x => x.UserId,
                        principalTable: "DocuScanUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__UserPermi__Count__693CA210",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "CountryCode");
                    table.ForeignKey(
                        name: "FK__UserPermi__Count__6A30C649",
                        column: x => x.CounterPartyId,
                        principalTable: "CounterParty",
                        principalColumn: "CounterPartyId");
                    table.ForeignKey(
                        name: "FK__UserPermi__Docum__6A30C649",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentType",
                        principalColumn: "DT_ID");
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    BarCode = table.Column<int>(type: "int", nullable: false),
                    DT_ID = table.Column<int>(type: "int", nullable: true),
                    CounterPartyId = table.Column<int>(type: "int", nullable: true),
                    DocumentNameId = table.Column<int>(type: "int", nullable: true),
                    FileId = table.Column<int>(type: "int", nullable: true),
                    DateOfContract = table.Column<DateTime>(type: "datetime", nullable: true),
                    Comment = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ReceivingDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    DispatchDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Fax = table.Column<bool>(type: "bit", nullable: true),
                    OriginalReceived = table.Column<bool>(type: "bit", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ActionDescription = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ReminderGroup = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    DocumentNo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    AssociatedToPUA = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    VersionNo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    AssociatedToAppendix = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime", nullable: true),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    Authorisation = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    BankConfirmation = table.Column<bool>(type: "bit", nullable: true),
                    TranslatedVersionReceived = table.Column<bool>(type: "bit", nullable: true),
                    Confidential = table.Column<bool>(type: "bit", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ThirdParty = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ThirdPartyId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    SendingOutDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ForwardedToSignatoriesDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("DOCUMENT_PK", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Document__Counte__5EBF139D",
                        column: x => x.CounterPartyId,
                        principalTable: "CounterParty",
                        principalColumn: "CounterPartyId");
                    table.ForeignKey(
                        name: "FK__Document__Curren__619B8048",
                        column: x => x.CurrencyCode,
                        principalTable: "Currency",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK__Document__DT_ID__6383C8BA",
                        column: x => x.DT_ID,
                        principalTable: "DocumentType",
                        principalColumn: "DT_ID");
                    table.ForeignKey(
                        name: "FK__Document__Docume__5FB337D6",
                        column: x => x.DocumentNameId,
                        principalTable: "DocumentName",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK__Document__FileId__5AEE82B9",
                        column: x => x.FileId,
                        principalTable: "DocumentFile",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "COUNTERPARTY_NOA_IDX",
                table: "CounterParty",
                column: "CounterPartyNoAlpha");

            migrationBuilder.CreateIndex(
                name: "COUNTERPARTY_SNC_IDX",
                table: "CounterParty",
                column: "Since");

            migrationBuilder.CreateIndex(
                name: "IX_CounterParty_Country",
                table: "CounterParty",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Document_CounterPartyId",
                table: "Document",
                column: "CounterPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_CurrencyCode",
                table: "Document",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentNameId",
                table: "Document",
                column: "DocumentNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DT_ID",
                table: "Document",
                column: "DT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Document_FileId",
                table: "Document",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "uk_document_barcode",
                table: "Document",
                column: "BarCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_file_filename",
                table: "DocumentFile",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentName_DocumentTypeId",
                table: "DocumentName",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocuScanUser_IsSuperUser",
                table: "DocuScanUser",
                column: "IsSuperUser");

            migrationBuilder.CreateIndex(
                name: "IX_DocuScanUser_LastLogon",
                table: "DocuScanUser",
                column: "LastLogon");

            migrationBuilder.CreateIndex(
                name: "UK_DocuScanUser_AccountName",
                table: "DocuScanUser",
                column: "AccountName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_DocuScanUser_UserIdentifier",
                table: "DocuScanUser",
                column: "UserIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipient_Group_Active",
                table: "EmailRecipient",
                columns: new[] { "GroupId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipient_Group_Email",
                table: "EmailRecipient",
                columns: new[] { "GroupId", "EmailAddress" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipientGroup_Key",
                table: "EmailRecipientGroup",
                column: "GroupKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipientGroup_Name",
                table: "EmailRecipientGroup",
                column: "GroupName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplate_Key",
                table: "EmailTemplate",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplate_Key_Active",
                table: "EmailTemplate",
                columns: new[] { "TemplateKey", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplate_Name",
                table: "EmailTemplate",
                column: "TemplateName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfiguration_ConfigKey",
                table: "SystemConfiguration",
                column: "ConfigKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfiguration_Section_Active",
                table: "SystemConfiguration",
                columns: new[] { "ConfigSection", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurationAudit_ChangedDate",
                table: "SystemConfigurationAudit",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurationAudit_ConfigId",
                table: "SystemConfigurationAudit",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_CounterPartyId",
                table: "UserPermissions",
                column: "CounterPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_CountryCode",
                table: "UserPermissions",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_DocumentTypeId",
                table: "UserPermissions",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrail");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "EmailRecipient");

            migrationBuilder.DropTable(
                name: "EmailTemplate");

            migrationBuilder.DropTable(
                name: "SystemConfigurationAudit");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "DocumentName");

            migrationBuilder.DropTable(
                name: "DocumentFile");

            migrationBuilder.DropTable(
                name: "EmailRecipientGroup");

            migrationBuilder.DropTable(
                name: "SystemConfiguration");

            migrationBuilder.DropTable(
                name: "DocuScanUser");

            migrationBuilder.DropTable(
                name: "CounterParty");

            migrationBuilder.DropTable(
                name: "DocumentType");

            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
