using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace KoncertDatabaseOpgae
{
    public class BookBillet
    {
        SqlConnection _connection;

        public BookBillet()
        {

        }

        //Metode til at connecte til databasen
        public void ConnectToDatabase()
        {
            //Connection string

           
           var config = new ConfigurationBuilder()

                .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                .Build();


            string _connectionString = config["ElisabethsSecretString"];

            //Create connection

            _connection = new SqlConnection(_connectionString);

            _connection.Open();

        }

        //metode til at disconnecte fra databasen
        public void DisconnectFromDatabase()
        {
            _connection.Close();
        }



        public string ShowFreeSeats()
        {
            ConnectToDatabase();

            

            string sql = "SELECT * From Billet Where Booked = 0";

            SqlCommand command = new SqlCommand(sql, _connection);

            SqlDataReader reader = command.ExecuteReader();

            Console.WriteLine("Viser alle ledige sæder");

            while (reader.Read())
            {
                int id = (int)reader["BilletId"];
                string koncert = (string)reader["Koncert"];
                DateTime datoFraDatabase = (DateTime)reader["Dato"];
                DateOnly dato = DateOnly.FromDateTime(datoFraDatabase); //Sørger for kun at få datoen og ikke tidspunkt for billettens Date.
                int række = (int)reader["Række"];
                int sæde = (int)reader["Sædenummer"];
                

                return $"Billet-ID: {id}, Koncert: {koncert}, " +
                    $"Dato: {dato:dd-MM-yyyy}, Række: {række}, Sæde: {sæde}";
            }

            DisconnectFromDatabase();

            return "Ingen ledige billetter tilbage";
           
        }


        public void BookTicket(int billetId)
        {
            ConnectToDatabase();

            Console.WriteLine($"Tjekker om billet med billetId {billetId} er ledig");


            SqlTransaction transaction = _connection.BeginTransaction();

            string bookSeat = "UPDATE Billet SET Booked = 1 " +
                "Where BilletId = @BilletID AND Booked = 0";

            SqlCommand command1 = new SqlCommand(bookSeat, _connection);

            command1.Transaction = transaction;

            command1.Parameters.AddWithValue("@BilletId", billetId);
            


            int rowsAffected = command1.ExecuteNonQuery();


            if (rowsAffected == 1)
            {
                Console.WriteLine(
                    $"Billetten med ID {billetId} er ledig og reserveret midlertidigt.");

                Console.WriteLine("Vil du bekræfte bookingen? Skriv ja eller nej:");

                string? svar = Console.ReadLine();

                if (svar?.ToLower() == "ja")
                {
                    transaction.Commit();

                    Console.WriteLine($"Du har nu booket billetten med ID {billetId}.");
                }
                else
                {
                    transaction.Rollback();

                    Console.WriteLine("Bookingen er fortrudt. Billetten er stadig ledig.");
                }
            }

            else
            {
                transaction.Rollback();

                Console.WriteLine($"Billetten med ID {billetId} findes ikke eller er allerede booket.");
            }

            DisconnectFromDatabase();

        }


    }
}
