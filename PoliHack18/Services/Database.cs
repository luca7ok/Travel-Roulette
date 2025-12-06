using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PoliHack18.Services
{
    public static class Database
    {
        private static NpgsqlDataSource date;

        public static void InitializareConexiune(string conexiune)
        {
            date = NpgsqlDataSource.Create(conexiune);
        }
        public static NpgsqlDataSource ReturneazaDataBase
        {
            get
            {
                if (date == null)

                {
                    throw new InvalidOperationException("Baza de date nu a fost legata");
                }
                return date;
            }
        }
        private static void AddParameters(NpgsqlCommand command, Dictionary<string, object?>? parameters)
        {
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    // Npgsql uses the syntax @name for parameters
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }
        }

        /// Functie pentru a returna o singura valoare
        /*
        public static T ExecutaScalar<T>(string query)
        {
            using (var comanda = Database.ReturneazaDataBase.CreateCommand(query))
            {
                object? rezultat = comanda.ExecuteScalar();
                if (rezultat != null && rezultat != DBNull.Value) return (T)rezultat;
                else return default!;
            }
        }*/
        public static T ExecutaScalar<T>(string query, Dictionary<string, object?>? parameters = null)
        {
            using (var comanda = Database.ReturneazaDataBase.CreateCommand(query))
            {
                // Add parameters here to prevent SQL Injection
                AddParameters(comanda, parameters);

                object? rezultat = comanda.ExecuteScalar();
                if (rezultat != null && rezultat != DBNull.Value) return (T)rezultat;
                else return default!;
            }
        }
        /// Functie pentru INSERT,DELETE,UPDATE
        /*
        public static int ExecutaNonQuery(string query)
        {
            using (var comanda = Database.ReturneazaDataBase.CreateCommand(query))
            {
                return comanda.ExecuteNonQuery();
            }
        }*/
        public static int ExecutaNonQuery(string query, Dictionary<string, object?>? parameters = null)
        {
            using (var comanda = Database.ReturneazaDataBase.CreateCommand(query))
            {
                // Add parameters here to prevent SQL Injection
                AddParameters(comanda, parameters);

                return comanda.ExecuteNonQuery();
            }
        }
        /// Functie pentru mai multe instante
        /*
        public static DataTable ExecutaQuery(string query)
        {
            using (var comanda = Database.ReturneazaDataBase.CreateCommand(query))
            {
                NpgsqlDataReader reader = comanda.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }
        */
        public static DataTable ExecutaQuery(string query, Dictionary<string, object?>? parameters = null)
        {
            using (var comanda = Database.ReturneazaDataBase.CreateCommand(query))
            {
                // Add parameters here to prevent SQL Injection
                AddParameters(comanda, parameters);

                NpgsqlDataReader reader = comanda.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }
        
    }
}
