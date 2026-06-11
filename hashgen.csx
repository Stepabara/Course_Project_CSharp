using System.Security.Cryptography;
using System.Text;

using var sha = SHA256.Create();
var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("123123" + "GeekTourSalt2024"));
Console.WriteLine(Convert.ToBase64String(bytes));
