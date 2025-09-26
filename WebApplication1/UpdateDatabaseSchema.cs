using System;
using System.Data.SqlClient;
using System.IO;

class UpdateDatabaseSchema
{
    static void Main()
    {
        string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=SouthAfricanBookstore;Integrated Security=True;MultipleActiveResultSets=True;AttachDbFilename=|DataDirectory|SouthAfricanBookstore.mdf";
        
        // Replace |DataDirectory| with the actual path
        string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        connectionString = connectionString.Replace("|DataDirectory|", dataDirectory);
        
        string sqlScript = File.ReadAllText("AddNotificationSettingsColumns.sql");
        
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Split the script into individual statements
                string[] statements = sqlScript.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string statement in statements)
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        using (SqlCommand command = new SqlCommand(statement, connection))
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine("Executed: " + statement.Substring(0, Math.Min(50, statement.Length)) + "...");
                        }
                    }
                }
                
                Console.WriteLine("Database schema updated successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}