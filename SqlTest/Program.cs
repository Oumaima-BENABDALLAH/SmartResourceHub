using Microsoft.Data.SqlClient;

string connStr = "Server=DESKTOP-4OLR9Q6\\NETSQLDEV025;Database=CatalogDb;User Id=sa;Password=SQLDEV*2025;TrustServerCertificate=True";

try
{
    using var conn = new SqlConnection(connStr);
    conn.Open();
    Console.WriteLine("✅ Connexion réussie !");
    Console.WriteLine($"Server version: {conn.ServerVersion}");
    Console.WriteLine($"Database: {conn.Database}");
}
catch (Exception ex)
{
    Console.WriteLine("❌ Erreur : " + ex.Message);
    Console.WriteLine(ex);
}