using System;
using Microsoft.Data.Sqlite;

var dbPath = @"C:\Users\stepa\OneDrive\Desktop\Курсовой Кпияп\GeekTour.Web\geektour.db";
using var conn = new SqliteConnection($"Data Source={dbPath}");
conn.Open();
using var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT Id, Email, PasswordHash, Role FROM Users";
using var reader = cmd.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"Id={reader[0]} | Email={reader[1]} | Hash={reader[2]} | Role={reader[3]}");
}
