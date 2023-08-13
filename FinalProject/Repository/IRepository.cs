using System.Data;

namespace FinalProject.Repository
{
    public interface IRepository
    {
        DataTable SelectAll();
        bool insert(int UserId, string PhoneNumber);
    }
}
