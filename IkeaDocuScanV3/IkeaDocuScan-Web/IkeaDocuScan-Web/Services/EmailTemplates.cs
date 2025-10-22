namespace IkeaDocuScan_Web.Services;

/// <summary>
/// Email template builder for consistent email formatting
/// </summary>
public static class EmailTemplates
{
    private const string BaseHtmlTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: Arial, Helvetica, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            background-color: #0051A5;
            color: #ffffff;
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px 20px;
            color: #333333;
            line-height: 1.6;
        }}
        .info-box {{
            background-color: #f8f9fa;
            border-left: 4px solid #0051A5;
            padding: 15px;
            margin: 20px 0;
        }}
        .info-box strong {{
            color: #0051A5;
        }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            margin: 20px 0;
            background-color: #0051A5;
            color: #ffffff;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #666666;
            border-top: 1px solid #e0e0e0;
        }}
        .document-list {{
            list-style: none;
            padding: 0;
            margin: 15px 0;
        }}
        .document-list li {{
            padding: 10px;
            margin: 5px 0;
            background-color: #f8f9fa;
            border-radius: 4px;
        }}
        .document-list a {{
            color: #0051A5;
            text-decoration: none;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>IKEA DocuScan</h1>
        </div>
        <div class='content'>
            {0}
        </div>
        <div class='footer'>
            <p>This is an automated message from IKEA DocuScan System.</p>
            <p>Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

    /// <summary>
    /// Build access request notification email for admin
    /// </summary>
    public static (string Html, string PlainText) BuildAccessRequestNotification(
        string username,
        string? reason,
        string applicationUrl)
    {
        var requestDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        var contentHtml = $@"
            <h2>New Access Request</h2>
            <p>A new user has requested access to the IKEA DocuScan system.</p>

            <div class='info-box'>
                <p><strong>Username:</strong> {username}</p>
                <p><strong>Request Date:</strong> {requestDate}</p>
                {(!string.IsNullOrWhiteSpace(reason) ? $"<p><strong>Reason:</strong><br/>{reason}</p>" : "")}
            </div>

            <p>To grant access to this user:</p>
            <ol>
                <li>Add the user to the appropriate Active Directory group (Reader, Publisher, or SuperUser)</li>
                <li>Or assign specific permissions in the database for document types, countries, and counter parties</li>
            </ol>

            <p><a href='{applicationUrl}' class='button'>Open DocuScan System</a></p>

            <p>If you did not expect this request, please contact your IT security team.</p>";

        var htmlBody = string.Format(BaseHtmlTemplate, contentHtml);

        var plainText = $@"
IKEA DocuScan - New Access Request

A new user has requested access to the IKEA DocuScan system.

Username: {username}
Request Date: {requestDate}
{(!string.IsNullOrWhiteSpace(reason) ? $"Reason: {reason}" : "")}

To grant access to this user:
1. Add the user to the appropriate Active Directory group (Reader, Publisher, or SuperUser)
2. Or assign specific permissions in the database for document types, countries, and counter parties

Application URL: {applicationUrl}

---
This is an automated message from IKEA DocuScan System.
Please do not reply to this email.
";

        return (htmlBody, plainText);
    }

    /// <summary>
    /// Build access request confirmation email for user
    /// </summary>
    public static (string Html, string PlainText) BuildAccessRequestConfirmation(
        string username,
        string adminEmail)
    {
        var contentHtml = $@"
            <h2>Access Request Received</h2>
            <p>Dear {username},</p>

            <p>Thank you for your interest in the IKEA DocuScan system. We have received your access request.</p>

            <div class='info-box'>
                <p>Your request is being reviewed by our administrators.</p>
                <p>You will be notified once your access has been granted.</p>
            </div>

            <p>If you have any questions, please contact the administrator at:
               <a href='mailto:{adminEmail}'>{adminEmail}</a></p>

            <p>Thank you for your patience.</p>";

        var htmlBody = string.Format(BaseHtmlTemplate, contentHtml);

        var plainText = $@"
IKEA DocuScan - Access Request Received

Dear {username},

Thank you for your interest in the IKEA DocuScan system. We have received your access request.

Your request is being reviewed by our administrators.
You will be notified once your access has been granted.

If you have any questions, please contact the administrator at: {adminEmail}

Thank you for your patience.

---
This is an automated message from IKEA DocuScan System.
Please do not reply to this email.
";

        return (htmlBody, plainText);
    }

    /// <summary>
    /// Build email for sending document link
    /// </summary>
    public static (string Html, string PlainText) BuildDocumentLink(
        string documentBarCode,
        string documentLink,
        string? message)
    {
        var contentHtml = $@"
            <h2>Document Shared with You</h2>
            <p>A document has been shared with you from the IKEA DocuScan system.</p>

            {(!string.IsNullOrWhiteSpace(message) ? $"<div class='info-box'><p>{message}</p></div>" : "")}

            <div class='info-box'>
                <p><strong>Document Bar Code:</strong> {documentBarCode}</p>
            </div>

            <p><a href='{documentLink}' class='button'>View Document</a></p>

            <p>This link will remain active according to your organization's document retention policy.</p>";

        var htmlBody = string.Format(BaseHtmlTemplate, contentHtml);

        var plainText = $@"
IKEA DocuScan - Document Shared

A document has been shared with you from the IKEA DocuScan system.

{(!string.IsNullOrWhiteSpace(message) ? $"Message: {message}\n" : "")}
Document Bar Code: {documentBarCode}

View Document: {documentLink}

This link will remain active according to your organization's document retention policy.

---
This is an automated message from IKEA DocuScan System.
Please do not reply to this email.
";

        return (htmlBody, plainText);
    }

    /// <summary>
    /// Build email for sending multiple document links
    /// </summary>
    public static (string Html, string PlainText) BuildDocumentLinks(
        IEnumerable<(string BarCode, string Link)> documents,
        string? message)
    {
        var documentList = documents.ToList();
        var documentListHtml = string.Join("", documentList.Select(d =>
            $"<li><strong>{d.BarCode}</strong><br/><a href='{d.Link}'>View Document</a></li>"));

        var contentHtml = $@"
            <h2>Documents Shared with You</h2>
            <p>{documentList.Count} document(s) have been shared with you from the IKEA DocuScan system.</p>

            {(!string.IsNullOrWhiteSpace(message) ? $"<div class='info-box'><p>{message}</p></div>" : "")}

            <ul class='document-list'>
                {documentListHtml}
            </ul>

            <p>These links will remain active according to your organization's document retention policy.</p>";

        var htmlBody = string.Format(BaseHtmlTemplate, contentHtml);

        var documentListPlain = string.Join("\n", documentList.Select(d =>
            $"  - {d.BarCode}\n    {d.Link}"));

        var plainText = $@"
IKEA DocuScan - Documents Shared

{documentList.Count} document(s) have been shared with you from the IKEA DocuScan system.

{(!string.IsNullOrWhiteSpace(message) ? $"Message: {message}\n" : "")}
Documents:
{documentListPlain}

These links will remain active according to your organization's document retention policy.

---
This is an automated message from IKEA DocuScan System.
Please do not reply to this email.
";

        return (htmlBody, plainText);
    }

    /// <summary>
    /// Build email for sending document attachment
    /// </summary>
    public static (string Html, string PlainText) BuildDocumentAttachment(
        string documentBarCode,
        string fileName,
        string? message)
    {
        var contentHtml = $@"
            <h2>Document Attached</h2>
            <p>A document has been sent to you from the IKEA DocuScan system.</p>

            {(!string.IsNullOrWhiteSpace(message) ? $"<div class='info-box'><p>{message}</p></div>" : "")}

            <div class='info-box'>
                <p><strong>Document Bar Code:</strong> {documentBarCode}</p>
                <p><strong>File Name:</strong> {fileName}</p>
            </div>

            <p>The document is attached to this email.</p>";

        var htmlBody = string.Format(BaseHtmlTemplate, contentHtml);

        var plainText = $@"
IKEA DocuScan - Document Attached

A document has been sent to you from the IKEA DocuScan system.

{(!string.IsNullOrWhiteSpace(message) ? $"Message: {message}\n" : "")}
Document Bar Code: {documentBarCode}
File Name: {fileName}

The document is attached to this email.

---
This is an automated message from IKEA DocuScan System.
Please do not reply to this email.
";

        return (htmlBody, plainText);
    }

    /// <summary>
    /// Build email for sending multiple document attachments
    /// </summary>
    public static (string Html, string PlainText) BuildDocumentAttachments(
        IEnumerable<(string BarCode, string FileName)> documents,
        string? message)
    {
        var documentList = documents.ToList();
        var documentListHtml = string.Join("", documentList.Select(d =>
            $"<li><strong>{d.BarCode}</strong> - {d.FileName}</li>"));

        var contentHtml = $@"
            <h2>Documents Attached</h2>
            <p>{documentList.Count} document(s) have been sent to you from the IKEA DocuScan system.</p>

            {(!string.IsNullOrWhiteSpace(message) ? $"<div class='info-box'><p>{message}</p></div>" : "")}

            <ul class='document-list'>
                {documentListHtml}
            </ul>

            <p>The documents are attached to this email.</p>";

        var htmlBody = string.Format(BaseHtmlTemplate, contentHtml);

        var documentListPlain = string.Join("\n", documentList.Select(d =>
            $"  - {d.BarCode} - {d.FileName}"));

        var plainText = $@"
IKEA DocuScan - Documents Attached

{documentList.Count} document(s) have been sent to you from the IKEA DocuScan system.

{(!string.IsNullOrWhiteSpace(message) ? $"Message: {message}\n" : "")}
Documents:
{documentListPlain}

The documents are attached to this email.

---
This is an automated message from IKEA DocuScan System.
Please do not reply to this email.
";

        return (htmlBody, plainText);
    }
}
