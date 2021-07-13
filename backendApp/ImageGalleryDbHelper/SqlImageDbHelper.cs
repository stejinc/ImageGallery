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

        public ErrorObjects StoreUserImage(string userName, byte[] image, byte[] thumbnail, string shareOption, string description)
        {
            ErrorObjects err = new ErrorObjects();
            int insertedCount = 0;
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand command = new SqlCommand(
                        "INSERT INTO userimages (username, image, " +
                        " description, shareoption) " +
                        "Values(@username, @image, @description, @shareoption)", conn);

                        command.Parameters.Add("@username",
                           SqlDbType.NVarChar, 50).Value = userName;
                        command.Parameters.Add("@description",
                            SqlDbType.NVarChar, 50).Value = description ?? "";
                        command.Parameters.Add("@image",
                            SqlDbType.VarBinary, image.Length).Value = image;
                        command.Parameters.Add("@shareoption",
                            SqlDbType.VarChar, 8).Value = shareOption;
                        // command.Parameters.Add("@thumbnail",
                        //     SqlDbType.VarBinary, thumbnail.Length).Value = thumbnail;
                        conn.Open();
                        insertedCount = command.ExecuteNonQuery();
                        Console.WriteLine("Inserted count:" + insertedCount);
                    }
                    if (insertedCount > 0)
                    {
                        err.status = true;
                        err.message = "Image uploaded successfully";
                        Console.WriteLine("Image uploaded succesffully");
                        return err;
                    }
                    else
                    {
                        err.message = "Image upload failed";
                        return err;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured while pushing data to sql server");
                    Console.WriteLine("Error exception: " + ex.Message);
                }
            }
            return err;


        }

        public ErrorObjects LoginUser(string userName, string password)
        {
            ErrorObjects err = new ErrorObjects();
            err.status = false;
            err.message = "Internal server error";
            if (getConnectionToSql())
            {
                try
                {
                    byte[] passwordHash = getHashedPassword(password);

                    if (passwordHash == null)
                        return err;

                    using (conn)
                    {
                        SqlCommand command = new SqlCommand("Select Top 1 password from usersdetails where username=@username;", conn);

                        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = userName;
                        conn.Open();

                        if (command.ExecuteNonQuery() == 0)
                        {
                            err.message = "User not registered";
                            return err;
                        }

                        byte[] passwordFromDb = (byte[])command.ExecuteScalar();

                        if (passwordFromDb == null)
                        {
                            err.message = "User not registered";
                            return err;
                        }

                        if (passwordHash.SequenceEqual(passwordFromDb))
                        {
                            err.status = true;
                            err.message = "Login successful";
                            return err;
                        }
                        else
                        {
                            err.message = "Incorrect username or password";
                            return err;
                        }
                    }

                }
                catch (Exception ex)
                {
                    err.message = "Internal server error";
                    Console.WriteLine("Error exception: " + ex.Message);
                    return err;
                }
            }
            else
            {
                err.message = "Internal issue. Please try after sometime";
                Console.WriteLine("Sql connection error");
                return err;
            }
        }

        private int? GetTotalImageCountPerUser(string userName, string sharedoption = null)
        {
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand cmd = cmd = new SqlCommand("Select Count(username) from userimages where username = @username", conn);
                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = userName;

                        conn.Open();
                        int Count = (int)cmd.ExecuteScalar();
                        return Count;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured while updating password");
                }
                return null;
            }
            return null;
        }

        private int? GetTotalSharedImageCount(string sharedoption)
        {
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand cmd;
                        if (sharedoption == "internal")
                            cmd = new SqlCommand("Select Count(imageid) from userimages where shareoption = 'internal' or shareoption = 'public'", conn);
                        else
                            cmd = new SqlCommand("Select Count(imageid) from userimages where shareoption = 'public'", conn);

                        conn.Open();
                        int Count = (int)cmd.ExecuteScalar();
                        return Count;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured while updating password");
                }
                return null;
            }
            return null;
        }

        public ImageContentDTO RetrieveSharedPaginatedImages(int pageSize, int pageOffset, string sharedOption)
        {
            List<ImageContent> usersAllImages = new List<ImageContent>();
            ImageContentDTO contentDTO = new ImageContentDTO();
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        //SqlCommand command = new SqlCommand(
                        //"Select imageid, image, description from userimages where username=@username order by imageid desc", conn);

                        //Use procedure to get paginated results
                        SqlCommand command;
                        if (sharedOption == "public")
                            command = new SqlCommand("[SharedPublicPaginatedResult]", conn);
                        else
                            command = new SqlCommand("[SharedInternalPaginatedResult]", conn);

                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pagesize", pageSize);
                        command.Parameters.AddWithValue("@pageoffset", pageOffset);
                        conn.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            byte[] imageData = (byte[])reader["image"];
                            ImageContent content = new ImageContent()
                            {
                                imageId = (long)reader["imageid"],
                                image = Convert.ToBase64String(imageData, 0, imageData.Length),
                                description = (string)reader["description"],
                                sharedOption = (string)reader["shareoption"]
                            };

                            usersAllImages.Add(content);
                        }
                        contentDTO.imageContents = usersAllImages;
                        int CurrentRetrievedCount = ++pageOffset * pageSize;
                        int? Count = GetTotalSharedImageCount(sharedOption);
                        if (Count == null || (Count - CurrentRetrievedCount < 0))
                            return contentDTO;

                        if (Count - CurrentRetrievedCount > 0)
                        {
                            contentDTO.next = true;
                            return contentDTO;
                        }
                        return contentDTO;
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

        public ErrorObjects UpdateProfilePic(string usernameClaim, byte[] fileBytes)
        {
            ErrorObjects err = new ErrorObjects();
            err.status = false;
            err.message = "Internal server error";
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand command = new SqlCommand("Update usersdetails set profilepic = @profilevalue where username = @username",conn);
                        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = usernameClaim;
                        command.Parameters.Add("@profilevalue", SqlDbType.VarBinary, fileBytes.Length).Value = fileBytes;

                        conn.Open();
                        int inserted = (int)command.ExecuteNonQuery();

                        if(inserted == 1)
                        {
                            Console.WriteLine("Updated profile pic of user " + usernameClaim);
                            err.status = true;
                            err.message = "Profile pic updated successfully";
                            return err;
                        }
                        Console.WriteLine("Error updating profile pic of user: " + usernameClaim);
                        Console.WriteLine("Number of rows affected after update: " + inserted);
                        err.message = "Profile image updation failed";
                        return err;
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine("Exception occured while updating profile pic: " + usernameClaim);
                }
                return err;
            }
            err.message = "Connection to database failed";
            return err;
        }

        public ErrorObjects UpdatePassword(string username, string newpassword)
        {
            ErrorObjects err = new ErrorObjects();
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        SqlCommand cmd = new SqlCommand("Update usersdetails set password = @password where username = @username", conn);
                        byte[] hashPassword = getHashedPassword(newpassword);
                        cmd.Parameters.Add("@password", SqlDbType.VarBinary, hashPassword.Length).Value = hashPassword;
                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;

                        conn.Open();
                        int updated = cmd.ExecuteNonQuery();
                        if(updated == 1)
                        {
                            Console.WriteLine("Password updated succesfully");
                            err.status = true;
                            err.message = "Password updated";
                            return err;
                        }
                        Console.WriteLine("Error while updating password");
                        err.message = "Failed to update password";
                        return err;
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine("Exception occured while updating password");
                }
                err.message = "Failed to update password";
                return err;
            }
            err.message = "Connection to db failed";
            return err;
        }

        public UserDetailsDb getUserDetails(string usernameClaim)
        {
            if (getConnectionToSql()){
                try
                {
                    using (conn)
                    {
                        SqlCommand command = new SqlCommand("Select username, firstname, lastname, dateofbirth, profilepic, gender from usersdetails where username = @username", conn);

                        command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = usernameClaim;

                        conn.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (!reader.HasRows)
                            return null;

                        if (reader.Read())
                        {
                            byte[] imageData = reader["profilepic"] == DBNull.Value ? null : (byte[])reader["profilepic"];
                            return new UserDetailsDb()
                            {
                                userName = (string)reader["username"],
                                firstName = (string?)reader["firstName"],
                                lastName = (string?)reader["lastname"],
                                profilePic = imageData == null ? null : Convert.ToBase64String(imageData, 0, imageData.Length),
                                gender = (int)reader["gender"],
                                dateOfBirth = reader["dateofbirth"].ToString()
                            };
                        }
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine("Exception occured while fetching userdetails from db");
                    Console.WriteLine("Error msg :", ex.Message);
                    return null;
                }
            }
            Console.WriteLine("Error connecting to db while fetching userdetails");
            return null;
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

        public ErrorObjects DeleteImage(long imageId)
        {
            ErrorObjects err = new ErrorObjects();
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
                        if (deleteCount == 1)
                        {
                            err.message = "Image deleted";
                            err.status = true;
                            return err;
                        }
                        err.message = "Failed to delete image";
                        return err;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete image failed");
                    Console.WriteLine("Error exception: " + ex.Message);
                    return err;
                }
            }
            else
            {
                err.message = "Connection to db failed";
                return err;
            }
        }

        public ImageContentDTO RetrievePaginatedImages(string userName, int pageSize = 8, int pageOffset = 0)
        {
            List<ImageContent> usersAllImages = new List<ImageContent>();
            ImageContentDTO contentDTO = new ImageContentDTO();
            if (getConnectionToSql())
            {
                try
                {
                    using (conn)
                    {
                        //SqlCommand command = new SqlCommand(
                        //"Select imageid, image, description from userimages where username=@username order by imageid desc", conn);

                        //Use procedure to get paginated results
                        SqlCommand command = new SqlCommand("PaginatedResult", conn);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pagesize", pageSize);
                        command.Parameters.AddWithValue("@pageoffset", pageOffset);
                        command.Parameters.AddWithValue("@username", userName);
                        conn.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            byte[] imageData = (byte[])reader["image"];
                            ImageContent content = new ImageContent()
                            {
                                imageId = (long)reader["imageid"],
                                image = Convert.ToBase64String(imageData, 0, imageData.Length),
                                description = (string)reader["description"],
                                sharedOption = (string)reader["shareoption"]
                            };

                            usersAllImages.Add(content);
                        }
                        contentDTO.imageContents = usersAllImages;
                        int CurrentRetrievedCount = ++pageOffset * pageSize;
                        int? Count = GetTotalImageCountPerUser(userName);
                        if (Count == null || (Count - CurrentRetrievedCount < 0))
                            return contentDTO;

                        if(Count - CurrentRetrievedCount > 0)
                        {
                            contentDTO.next = true;
                            return contentDTO;
                        }
                        return contentDTO;
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

        public ErrorObjects CreateUser(string userName, string password, string firstName, string lastName, int gender, string dateOfBirth, byte[] image = null)
        {
            ErrorObjects err = new ErrorObjects();
            if (getConnectionToSql())
            {
                try
                {
                    byte[] passwordHash = getHashedPassword(password);

                    if (passwordHash == null)
                    {
                        err.status = false;
                        err.message = $"Failed to generate passwrod hash for user : {userName}";
                        Console.WriteLine(err.message);
                        return err;
                    }

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
                        command.Parameters.Add("@profilepic", SqlDbType.VarBinary, image == null ? 0 : image.Length).Value = (image == null ? DBNull.Value : image);

                        conn.Open();
                        if (command.ExecuteNonQuery() == 1)
                        {
                            err.status = true;
                            err.message = $"User {userName} created successfully";
                            Console.WriteLine(err.message);
                        }
                        else
                        {
                            err.status = false;
                            err.message = $"Failed to create user {userName}";
                            Console.WriteLine(err.message);
                        }
                        return err;
                    }
                }
                catch (Exception ex)
                {
                    if(ex.Message.ToLower().Contains("violation of primary key"))
                    {
                        err.status = false;
                        err.message = $"Username {userName} already taken";
                        Console.WriteLine(err.message);
                        return err;
                    }                    
                    Console.WriteLine("Exception message : " + ex.Message);
                    return null;
                }
            }
            else
            {
                err.status = false;
                err.message = $"Connection failure. Try after sometime";
                Console.WriteLine(err.message);
                return err;
            }
        }

        private byte[] getHashedPassword(string password)
        {
            if(password == null)
            {
                Console.WriteLine("Password is null. Cannot proceed.");
                return null;
            }
            using (SHA256 hash = SHA256.Create())
            {
                try
                {
                    Encoding enc = Encoding.UTF8;
                    return hash.ComputeHash(Encoding.Default.GetBytes(password));
                }catch(Exception ex)
                {
                    Console.WriteLine("Exception : Error while generating hash for password: " + ex.Message);
                    return null;
                }
            }
        }
    }
}
