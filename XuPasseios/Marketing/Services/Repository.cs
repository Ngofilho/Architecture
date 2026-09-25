using Marketing.Dtos;
using Microsoft.Data.Sqlite;

namespace Marketing.Services
{
    public class Repository : IRepository
    {
        public void Add(SaleDTO saleDTO)
        {
            try
            {
                using var sqlConn = new SqliteConnection("Data Source=Marketing.db");
                SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
                sqlConn.Open();
                using var createCommand = sqlConn.CreateCommand();
                createCommand.CommandText = @"
                INSERT INTO SALE (IdCampaign, IdOrder, IdCustomer) VALUES ($IdCampaign, $IdOrder, $IdCustomer)";
                createCommand.Parameters.AddWithValue("$IdCampaign", 1);
                createCommand.Parameters.AddWithValue("$IdOrder", 1);
                createCommand.Parameters.AddWithValue("$IdCustomer", 1);
                createCommand.ExecuteNonQuery();
                sqlConn.Close();
                sqlConn.Dispose();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
