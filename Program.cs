using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Demo1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter \nEmail:");
            string Email=Console.ReadLine();
            Console.WriteLine("\nPassword:");
            string password=Console.ReadLine();
            if (Insert.validate_user(Email,password))
            {
                Console.WriteLine("Welcome aboard!!!");
                Program.AddMenu();
            }
            else 
            {
                Console.WriteLine("Invalid Credentials!!!");
                
            }

            
            Console.ReadKey();
        }
        public static void AddMenu() 
        {
            User1 user = new User1();
            Console.WriteLine("Enter \nName:");
            user.Name = Console.ReadLine();
            Console.WriteLine("Email:");
            user.Email = Console.ReadLine();
            Console.WriteLine("Phone Number:");
            user.Mobile = Convert.ToInt64(Console.ReadLine());

            Insert.InsertUsers(user);

        }
    }
}
