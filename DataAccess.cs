using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApp
{
    class DataAccess
    {
        static string connectionString = Helper.CnnVal("ChatAppDB");
        static MySqlConnection conn = null; 
        private static DataAccess dataAccess = null;
        private MySqlCommand command;
        public MySqlDataAdapter adapter;
        

        public static DataAccess GetObj()
        {
            if(dataAccess == null)
            {
                dataAccess = new DataAccess();
            }
            return dataAccess;
        }

        public bool InsertContact(string fName, string lName, string ip, string port)
        {
            try
            {
                string query = "INSERT INTO contacts (first_name, last_name, ip, port) VALUES('" +
                fName + "','" + lName + "','" + ip + "', '" + port + "')";

                command = new MySqlCommand(query, conn);
                command.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            
        }


        public DataSet GetContacts(string bindingName)
        {
            DataSet dataSet = new DataSet();
            if (MakeConnection())
            {
                string query = "select * from contacts";
                command = new MySqlCommand(query, conn);
                adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet, bindingName);
                return dataSet;
            }
            return dataSet;
        }


        

        private bool MakeConnection()
        {
            //Connection code
            if(conn == null)
            {
                try
                {
                    conn = new MySqlConnection(connectionString);
                    conn.Open();
                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
 