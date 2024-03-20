using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Demo1
{
    internal static class Insert
    {
        public static async void InsertUsers(User1 user)
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
                    Dictionary<string,string> Credentials= new Dictionary<string,string>();

                    Random random = new Random();
                    int length = random.Next(8, 13);
                    var tempPassword= GeneratePassword(alphaSet,length);
                    
                    Console.WriteLine(tempPassword);
                    Credentials["Email"] = user.Email;
                    Credentials[user.Email] = tempPassword;
                    Credentials["Name"]=user.Name;
                    login.Password = ToHashSHA1(tempPassword);
                    command.Parameters.AddWithValue("@Password", login.Password);

                    int result=command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        Console.WriteLine("Inserted Successfully");
                        Dictionary<string, string> mailContents = await SendMessage(Credentials);
                        bool Sent = await SendEmail(mailContents);
                        if (Sent)
                        {
                            await Console.Out.WriteLineAsync("Mail is Sent");
                        }
                        else
                        {
                            await Console.Out.WriteLineAsync("Error in senting the Mail");
                        }
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
        public static async Task<Dictionary<string,string>> SendMessage(Dictionary<string,string> Credentials)
        {
            Dictionary<string, string> EmailContent = new Dictionary<string, string>();
            await Task.Run(() =>
            {
                 string Subject = "Welcome Aboard! Verify Your Email Address for Full Access to Job Portal";
                 string Message = $"Dear {Credentials["Name"]},\r\n\r\nWelcome to JobPortal! We're thrilled to have you on board. To ensure the security of your account, we've generated a temporary password for you to sign in. Here are your temporary login details:\r\n\r\nUsername/Email:{Credentials["Email"]} \r\nTemporary Password: {Credentials[Credentials["Email"]]}" +
                     "\r\nPlease sign in using the provided credentials to access your account. Once signed in, we highly recommend that you update your password to something more memorable and secure. You can change your password by following these simple steps:\r\n\r\n" +
                     "1.Sign in to your account using the temporary credentials provided above.\r\n" +
                     "Navigate to your account settings or profile settings.\r\n" +
                     "Select the option to change your password.\r\n" +
                     "Enter your current temporary password and create a new, secure password.\r\n" +
                     "Save your changes.\r\nRemember, choosing a strong password is crucial for the security of your account. " +
                     "Ensure your new password is unique, includes a mix of letters, numbers, and special characters, and is not easily guessable." +
                     "Thank you for choosing Job Portal. We look forward to assisting you in your job search journey.\r\n\r\nBest regards,";
            
                EmailContent.Add("Subject", Subject);
                EmailContent.Add("Message", Message);
                EmailContent.Add("Email", Credentials["Email"]);

            });
            
            await Task.Delay(1000);
            return EmailContent;
        }
        public static async Task<bool> SendEmail(Dictionary<string,string> MailContent)
        {
            var mail = "demoJobPortal@outlook.com";
            var pwd = "demo@123!";
            var client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pwd)
            };
            try
            {
                client.SendMailAsync(
                new MailMessage(
                    from: mail,
                    to: MailContent["Email"], MailContent["Subject"], MailContent["Message"]));
                return true;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync( "Error:in Sending mail:"+ex.Message);
                return false;
            }

              
        }
    }
}
