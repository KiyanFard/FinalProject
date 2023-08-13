using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using FinalProject.Repository;
using System.Data;
using FinalProject.Dtos;
using FinalProject;
using System.Linq;

namespace FinalProject.DBContext
{

    public class TaxiShopAppDbContext : DbContext, IRepository
    {
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LoginDb;Integrated Security=True";

        public DbSet<Users> Users { get; set; }
        string Message = "قبلا ثبت نام کردید";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DataTable SelectAll()
        {
            string query = "select * From Fields";
            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            DataTable data = new DataTable();
            adapter.Fill(data);
            return data;
        }

        public bool insert(int UserId, string PhoneNumber)
        {
            JwtDto dto = new JwtDto();

            int register = (from p in dto.PhoneNumber where p = PhoneNumber select p).SingleOrDefault();
            SqlConnection connection = new SqlConnection(connectionString);

            if (register != null)
            {
                // farda poresh kon!
            }
            else
            {
                try
                {
                    string query = "Insert Into MyContacts (PhoneNumber) values (@PhoneNumber)";
                    SqlCommand command = new SqlCommand(query, connection);
                    //command.Parameters.AddWithValue("@UserId", UserId);
                    command.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }

    public class Users
    {
        public string NationalCode { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}