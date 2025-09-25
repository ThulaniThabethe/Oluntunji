<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data.Entity" %>
<%@ Import Namespace="WebApplication1.Models" %>
<%@ Import Namespace="WebApplication1.Migrations" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Update Database</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .success { color: green; font-weight: bold; }
        .error { color: red; font-weight: bold; }
        .info { color: blue; }
    </style>
</head>
<body>
    <h1>Database Update Utility</h1>
    <form id="form1" runat="server">
        <div>
            <h2>Database Status:</h2>
            <asp:Label ID="lblStatus" runat="server" CssClass="info"></asp:Label>
            <br /><br />
            <asp:Button ID="btnUpdateDatabase" runat="server" Text="Update Database" OnClick="btnUpdateDatabase_Click" />
            <br /><br />
            <asp:Label ID="lblResult" runat="server"></asp:Label>
        </div>
    </form>
</body>
</html>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CheckDatabaseStatus();
        }
    }

    private void CheckDatabaseStatus()
    {
        try
        {
            using (var db = new BookstoreDbContext())
            {
                // Try to query the database
                var userCount = db.Users.Count();
                lblStatus.Text = $"Database is accessible. Current users: {userCount}";
                lblStatus.CssClass = "success";
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Database needs update. Error: {ex.Message}";
            lblStatus.CssClass = "error";
        }
    }

    protected void btnUpdateDatabase_Click(object sender, EventArgs e)
    {
        try
        {
            // Create and run migrations
            var configuration = new Configuration();
            var migrator = new DbMigrator(configuration);
            
            // Get pending migrations
            var pendingMigrations = migrator.GetPendingMigrations().ToList();
            
            if (pendingMigrations.Any())
            {
                lblResult.Text = $"Found {pendingMigrations.Count} pending migrations. Updating...<br/>";
                
                // Update database
                migrator.Update();
                
                lblResult.Text += "<span class='success'>Database updated successfully!</span><br/>";
                lblResult.Text += "Migrations applied: " + string.Join(", ", pendingMigrations);
            }
            else
            {
                lblResult.Text = "<span class='info'>Database is already up to date.</span>";
            }
            
            // Recheck status
            CheckDatabaseStatus();
        }
        catch (Exception ex)
        {
            lblResult.Text = $"<span class='error'>Error updating database: {ex.Message}</span><br/>{ex.StackTrace}";
        }
    }
</script>