using ImageGalleryDbHelper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ImageGalleryDbHelper
{
    public class SqlImageDbHelper
    {
        private static SqlConnection conn = null;

        private bool getConnectionToSql()
        {
            try
            {
                conn = new SqlConnection("data source=ms-sql-server; database = ImageGallery; User ID = SA; Password=Test@123; Trusted_Connection=false; MultipleActiveResultSets=true;");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to sql server");
                return false;
            }
            return true;
        }

        public bool StoreUserImage(string userName, byte[] image, byte[] thumbnail, string description)
        {
            int insertedCount = 0;
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand command = new SqlCommand(
                        "INSERT INTO userimages (username, image, " +
                        " description) " +
                        "Values(@username, @image, @description)", conn);

                        command.Parameters.Add("@username",
                           SqlDbType.NVarChar, 50).Value = userName;
                        command.Parameters.Add("@description",
                            SqlDbType.NVarChar, 50).Value = description ?? "";
                        command.Parameters.Add("@image",
                            SqlDbType.VarBinary, image.Length).Value = image;
                        // command.Parameters.Add("@thumbnail",
                        //     SqlDbType.VarBinary, thumbnail.Length).Value = thumbnail;
                        conn.Open();
                        insertedCount = command.ExecuteNonQuery();
                        Console.WriteLine("Inserted count:"+ insertedCount);
                    }
                    if (insertedCount > 0)
                        return true;
                    else
                        return false;
                }catch(Exception ex)
                {
                    Console.WriteLine("Exception occured while pushing data to sql server");
                    Console.WriteLine("Error exception: " + ex.Message);
                    return false;
                }
            }
            else{
                Console.WriteLine("Store image: Sql Connection issue");
                return false;
            }


        }

        public async Task<bool> LoginUser(string userName, string password)
        {
            if (getConnectionToSql())
            {
                try
                {
                    byte[] passwordHash = getHashedPassword(password);

                    if (passwordHash == null)
                        return false;

                    using (conn)
                    {
                        SqlCommand command = new SqlCommand("Select Top 1 password from usersdetails where username=@username;", conn);

                        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = userName;
                        conn.Open();

                        if (command.ExecuteNonQuery() == 0)
                            return false;

                        byte[] passwordFromDb =  (byte[]) await command.ExecuteScalarAsync();

                        if (passwordFromDb == null)
                        {
                            Console.WriteLine("No users available with username : " + userName);
                            return false;
                        }

                        if (passwordHash.SequenceEqual(passwordFromDb))
                            return true;
                        else
                        {
                            Console.WriteLine("Incorrect password");
                            return false;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Login validation failed");
                    Console.WriteLine("Error exception: " + ex.Message);
                    return false;
                }
            }
            else
                return false;
        }

        public async Task<ImageContent> RetrieveImage(string userName, long imageId)
        {
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand command = new SqlCommand(
                        "Select imageid, image, description from userimages where username=@username and imageid=@imageid", conn);

                        command.Parameters.Add("@username",
                           SqlDbType.NVarChar, 50).Value = userName;
                        command.Parameters.Add("@imageid",
                           SqlDbType.BigInt).Value = imageId;
                        conn.Open();
                        SqlDataReader reader = await command.ExecuteReaderAsync();

                        if (reader.Read())
                        {
                            byte[] imageData = (byte[])reader["image"];
                            return new ImageContent()
                            {
                                imageId = (long)reader["imageid"],
                                image = Convert.ToBase64String(imageData, 0, imageData.Length),
                                description = (string)reader["description"]
                            };
                        }
                        else
                            return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured while retriving image from sql server");
                    Console.WriteLine("Error exception: " + ex.Message);
                    return null;
                }
            }
            else
                return null;


        }

        public bool DeleteImage(long imageId)
        {
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand command = new SqlCommand("Delete from userimages where imageid=@imageid;", conn);

                        command.Parameters.Add("@imageid", SqlDbType.BigInt).Value = imageId;
                        conn.Open();

                        int deleteCount = command.ExecuteNonQuery();
                        if (deleteCount != 1)
                            return false;

                        return true;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete image failed");
                    Console.WriteLine("Error exception: " + ex.Message);
                    return false;
                }
            }
            else
                return false;
        }

        public async Task<List<ImageContent>> RetrieveAllImages(string userName)
        {
            List<ImageContent> usersAllImages = new List<ImageContent>();
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand command = new SqlCommand(
                        "Select imageid, image, description from userimages where username=@username", conn);

                        command.Parameters.Add("@username",
                           SqlDbType.NVarChar, 50).Value = userName;
                        conn.Open();
                        SqlDataReader reader = await command.ExecuteReaderAsync();

                        while (reader.Read())
                        {
                            byte[] imageData = (byte[])reader["image"];
                            ImageContent content = new ImageContent()
                            {
                                imageId = (long)reader["imageid"],
                                image = Convert.ToBase64String(imageData, 0, imageData.Length),
                                description = (string)reader["description"]
                            };

                            usersAllImages.Add(content);
                        }
                    }

                    return usersAllImages;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured while retriving image from sql server");
                    Console.WriteLine("Error exception: " + ex.Message);
                    return null;
                }
            }
            else
                return null;


        }

        public bool CreateUser(string userName, string password, string firstName, string lastName, int gender, string dateOfBirth, byte[] image = null)
        {
            if (getConnectionToSql())
            {
                try
                {
                    byte[] passwordHash = getHashedPassword(password);
                    
                    if (passwordHash == null)
                        return false;

                    using (conn)
                    {
                        SqlCommand command = new SqlCommand("Insert into usersdetails(username, password, firstname, lastname," +
                            "dateOfBirth, gender, profilepic) Values(@username, @password, @firstname, @lastname, " +
                            "@dateOfBirth, @gender, @profilepic);", conn);

                        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = userName;
                        command.Parameters.Add("@password", SqlDbType.VarBinary, passwordHash.Length).Value = passwordHash;
                        command.Parameters.Add("@firstname", SqlDbType.NVarChar, 20).Value = firstName;
                        command.Parameters.Add("@lastname", SqlDbType.NVarChar, 20).Value = (lastName == null ? DBNull.Value : lastName);
                        command.Parameters.Add("@gender", SqlDbType.Int).Value = gender;
                        command.Parameters.Add("@dateOfBirth", SqlDbType.Date, 10).Value = dateOfBirth;
                        command.Parameters.Add("@profilepic", SqlDbType.VarBinary, image == null ? 0: image.Length).Value = (image == null ? DBNull.Value : image);

                        conn.Open();
                        if (command.ExecuteNonQuery() > 0)
                            return true;

                        return false;
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine("Error occured while creating new user");
                    Console.WriteLine("Exception message : " + ex.Message);
                    return false;
                }
            }
            else
                return false;
        }

        private byte[] getHashedPassword(string password)
        {
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                return hash.ComputeHash(Encoding.Default.GetBytes(password));
            }
        }
    }
}
