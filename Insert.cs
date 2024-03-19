using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Demo1
{
    internal static class Insert
    {
        public static void InsertUsers(User1 user)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Testing"].ToString();
            using (SqlConnection connection=new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command=new SqlCommand("spUserLogin",connection))
                {
                    Login1 login = new Login1();
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Mobile", user.Mobile);

                    string alphaSet=user.Email+user.Mobile;
                    Random random = new Random();
                    int length = random.Next(8, 13);
                    var tempPassword= GeneratePassword(alphaSet,length);
                    Console.WriteLine(tempPassword);
                    login.Password = ToHashSHA1(tempPassword);
                    command.Parameters.AddWithValue("@Password", login.Password);

                    int result=command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        Console.WriteLine("Inserted Successfully");
                    }
                    else
                    {
                        Console.WriteLine("Error in insertion");
                    }
                }
            }
        }
        public static string GeneratePassword(string charSet,int length)
        {
            char[] shuffledcharacters=charSet.ToCharArray();
            Random random = new Random();
            int n = shuffledcharacters.Length;
            while (n>1)
            {
                n--;
                int k=random.Next(n+1);
                var temp = shuffledcharacters[k];
                shuffledcharacters[k] = shuffledcharacters[n];
                shuffledcharacters[n] = temp;  
            }
            return new string(shuffledcharacters.Take(length).ToArray());
        }
        public static string ToHashSHA1(string Password) 
        {   var sha1 = SHA1.Create();
            byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(Password));
            var sb=new StringBuilder();
            foreach (byte b in bytes) 
            {
                sb.Append(b.ToString("x2"));
            }   
            return sb.ToString();
        }

        public static bool validate_user(string Email,string Password)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Testing"].ToString();
            using (SqlConnection connection=new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT LoginID,UserID from Login1 where Email=@Email AND PASSWORD=@Password";
                using (SqlCommand command=new SqlCommand(query,connection))
                {
                    command.Parameters.AddWithValue("@Email", Email);
                    var hashPassword = ToHashSHA1(Password);
                    command.Parameters.AddWithValue("@Password", hashPassword);

                    SqlDataReader dataReader= command.ExecuteReader();


                    return dataReader.HasRows;
                }
            }
        }
    }
}
