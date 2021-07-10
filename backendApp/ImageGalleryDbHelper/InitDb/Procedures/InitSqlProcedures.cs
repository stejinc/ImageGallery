using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGalleryDbHelper.InitDb.Procedures
{
    class InitSqlProcedures
    {
        private static SqlConnection conn = null;
        private bool getConnectionToSql()
        {
            try
            {
                //conn = new SqlConnection("data source=ms-sql-server; database = ImageGallery; User ID = SA; Password=Test@123; Trusted_Connection=false; MultipleActiveResultSets=true;");
                conn = new SqlConnection("data source=(localdb)\\MSSQLLocalDB; database = ImageGallery;  Trusted_Connection=true; MultipleActiveResultSets=true;");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to sql server");
                return false;
            }
            return true;
        }

        public bool CreatePaginationProcedure()
        {
            if (getConnectionToSql())
            {
                using (conn)
                {
                    SqlCommand cmd = new SqlCommand(
                        @"Create Proc PaginatedResult
                          @pagesize int,
                          @pageoffset int,
                          @username nvarchar(50)
                          as
                          Begin
                          Select image, imageid, description
                          from userimages
                          where username = @username
                          order by imageid Desc
                          Offset @pagesize * @pageoffset Rows Fetch Next @pagesize Rows Only
                          End"
                        , conn);
                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0)
                            return true;
                    }catch(Exception ex)
                    {
                        Console.WriteLine("Exception occured while creating paginated procedure");
                        Console.WriteLine("Exception message:"+ ex);
                    }
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Internal server error.Try after sometime");
                return false;
            }
        }
    }
}
