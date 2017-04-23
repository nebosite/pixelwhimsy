
namespace PixelWhimsy
{
#if DEBUG
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data.SqlClient;
    using System.Web;
    using System.Collections.Specialized;
    using System.Threading;

    class webscripts
    {
        #region Experiments
        /// --------------------------------------------------------------
        /// <summary>
        /// PageData is a test method to pull data off of a test table
        /// </summary>
        /// --------------------------------------------------------------
        void PageData()
        {
            SqlDataReader dataReader = null;
            SqlCommand command = null;
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(
                    "server=mssql02.1and1.com; " +
                    "initial catalog=db213009860;" +
                    "uid=dbo213009860;" +
                    "pwd=GqMhYBh2");
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = "INSERT into TestTable (Name) values('Toby" + DateTime.Now.ToString() + "')";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM TestTable";
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Response.Write(dataReader["ID"] + ", " + dataReader["Name"]);
                    Response.Write("<BR/>");
                }

            }
            catch (Exception e)
            {
                Response.Write("Error: " + e.Message);
            }
            finally
            {
                dataReader = null;
                command = null;
                if (connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }
        #endregion

        #region Merchant receiver
        /// --------------------------------------------------------------
        /// <summary>
        /// This method processes incoming purchase data
        /// </summary>
        /// --------------------------------------------------------------
        void SwRegHandler()
        {
            NameValueCollection query = Request.QueryString;

            string lastName = "";
            string firstName = "";
            string email = "";
            string realEmail = "";
            string address = "";
            string purchaseDate = DateTime.Now.ToShortDateString();
            int licenses = 1;
            int maxLicenses = 2;
            string notes = "";
            string variation = "";
            int quantity = 1;

            foreach (string key in query.Keys)
            {
                // Response.Write(key + ": " + query[key] + "\n");
                switch (key)
                {
                    case "name": lastName += GetDbSafeValue(query[key]); break;
                    case "initals": firstName = GetDbSafeValue(query[key]); break;
                    case "email": email = GetDbSafeValue(query[key]); realEmail = query[key]; break;
                    case "add1": address = GetDbSafeValue(query[key]); break;
                    case "add2": if (query[key] != "") address += "\n" + GetDbSafeValue(query[key]); break;
                    case "add3": if (query[key] != "") address += "\n" + GetDbSafeValue(query[key]); break;
                    case "add4": if (query[key] != "") address += "\n" + GetDbSafeValue(query[key]); break;
                    case "add5": if (query[key] != "") address += "\n" + GetDbSafeValue(query[key]); break;
                    case "add6": if (query[key] != "") address += "\n" + GetDbSafeValue(query[key]); break;
                    case "var":
                        variation = query[key].ToLower();
                        switch (variation)
                        {
                            case "single user":
                                licenses = 1;
                                break;
                            case "5 user classroom pack":
                                licenses = 5;
                                break;
                            case "25 user lab pack":
                                licenses = 25;
                                break;
                            default:
                                notes += "\nUnknown variation: " + variation + "\n";
                                licenses = 1;
                                break;
                        }
                        break;
                    case "qty":
                        string keyValue = query[key];
                        if (keyValue == null || keyValue == "") keyValue = "1";
                        quantity = Int32.Parse(keyValue);
                        break;
                    case "user_text": notes += GetDbSafeValue(query[key]); break;
                }
            }

            maxLicenses = licenses * (quantity + 2);
            licenses *= quantity;

            // Make a seed from the last name
            long seed = 1;
            for (int i = 0; i < lastName.Length; i++)
            {
                seed *= (int)lastName[i];
            }
            for (int i = 0; i < firstName.Length; i++)
            {
                seed *= (int)firstName[i];
            }

            string registrationCode = MakeNewLicense((int)seed);

            string insertionQuery =
                "INSERT INTO  [dbo213009860].[Licenses] ([RegistrationCode], [Name], [email], [Address], [PurchaseType], [PurchaseDate], [LicencesGranted], [MaxLicenses], [Notes])  " +
                "VALUES ('" +
                registrationCode + "', '" +
                firstName + " " + lastName + "', '" +
                email + "', '" +
                address + "', '" +
                variation + "', '" +
                purchaseDate + "', " +
                licenses + ", " +
                maxLicenses + ", '" +
                notes + "')";

            if (firstName.ToLower().Contains("showquery"))
            {
                Response.Write("For testing purposes: " + insertionQuery);
            }

            try
            {
                ExecuteCommand(insertionQuery);
                Response.Write("\n\n<softshop>" +
                    registrationCode.Substring(0, 4) + " " +
                    registrationCode.Substring(4, 4) + " " +
                    registrationCode.Substring(8, 4) + " " +
                    registrationCode.Substring(12, 4) +
                    "</softshop>");
            }
            catch (Exception e)
            {
                Response.Write("ERROR: " + e.ToString());
            }

        }


        /// <summary>
        /// Execute a command on the database
        /// </summary>
        void ExecuteCommand(string commandText)
        {
            //SqlDataReader dataReader = null;
            SqlCommand command = null;
            SqlConnection connection = null;

            try {
                connection = new SqlConnection(
                    "server=mssql02.1and1.com; " +
                    "initial catalog=db213009860;" +
                    "uid=dbo213009860;" +
                    "pwd=GqMhYBh2");
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
            finally
            {
                //dataReader = null;
                command = null;
                if (connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        /// <summary>
        /// Make a new license key
        /// </summary>
        string MakeNewLicense(int seed)
        {
            char[] validChars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789".ToCharArray();
            Random rand1 = new Random(DateTime.Now.Millisecond);
            Random rand2 = new Random(DateTime.Now.Second);
            Random rand3 = new Random(DateTime.Now.Minute);
            Random rand4 = new Random(DateTime.Now.Hour);
            Random rand5 = new Random(DateTime.Now.Day);
            Random rand6 = new Random(DateTime.Now.Month);
            Random rand7 = new Random(DateTime.Now.Year);
            Random rand8 = new Random(seed);

            StringBuilder newLicense = new StringBuilder();
            newLicense.Append(validChars[rand1.Next(validChars.Length)]);
            newLicense.Append(validChars[rand2.Next(validChars.Length)]);
            newLicense.Append(validChars[rand3.Next(validChars.Length)]);
            newLicense.Append(validChars[rand4.Next(validChars.Length)]);
            newLicense.Append(validChars[rand5.Next(validChars.Length)]);
            newLicense.Append(validChars[rand6.Next(validChars.Length)]);
            newLicense.Append(validChars[rand7.Next(validChars.Length)]);
            newLicense.Append(validChars[rand8.Next(validChars.Length)]);
            newLicense.Append(validChars[rand1.Next(validChars.Length)]);
            newLicense.Append(validChars[rand2.Next(validChars.Length)]);
            newLicense.Append(validChars[rand3.Next(validChars.Length)]);
            newLicense.Append(validChars[rand4.Next(validChars.Length)]);
            newLicense.Append(validChars[rand5.Next(validChars.Length)]);
            newLicense.Append(validChars[rand6.Next(validChars.Length)]);
            newLicense.Append(validChars[rand7.Next(validChars.Length)]);
            newLicense.Append(validChars[rand8.Next(validChars.Length)]);

            return newLicense.ToString();
        }


        /// --------------------------------------------------------------
        /// <summary>
        /// Convert a string to something safe for the database
        /// </summary>
        /// --------------------------------------------------------------
        string GetDbSafeValue(string value)
        {
            StringBuilder newValue = new StringBuilder();

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) || c == '.' || c == ' ' || c == '@' || c == '-')
                {
                    newValue.Append(c);
                }
                else
                {
                    newValue.Append('_');   
                }

            }

            return newValue.ToString();
        }
        #endregion

        #region registration handler
        /// --------------------------------------------------------------
        /// <summary>
        /// PageData is a test method to pull data off of a test table
        /// </summary>
        /// --------------------------------------------------------------
        void RegistrationHandler()
        {
            NameValueCollection query = Request.QueryString;

            string productCode = GetDbSafeValue(query["pc"]).Replace(" ", "").ToUpper();
            string version = GetDbSafeValue(query["ver"]);
            string computerId = GetDbSafeValue(query["id"]);
            uint licenseId = 0;
            int currentLicenseCount = 0;
            int licensesGranted = 0;
            int maxLicenseCount = 0;

            //Response.Write("code: " + productCode + "<br>");
            //Response.Write("version: " + version + "<br>");
            //Response.Write("id: " + computerId + "<br>");

            SqlDataReader dataReader = null;
            SqlCommand command = null;
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(
                    "server=mssql02.1and1.com; " +
                    "initial catalog=db213009860;" +
                    "uid=dbo213009860;" +
                    "pwd=GqMhYBh2");
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;

                // Find a matching license
                command.CommandText = "SELECT * FROM Licenses WHERE RegistrationCode = '" + productCode + "'";
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    licenseId = uint.Parse(dataReader["ID"].ToString());
                    licensesGranted = int.Parse(dataReader["LicencesGranted"].ToString());
                    maxLicenseCount = int.Parse(dataReader["MaxLicenses"].ToString());
                    break;
                }
                dataReader.Close();

                // If not valid, we are done
                if (licenseId == 0)
                {
                    RegRespond(false, "The registration code was invalid.", false);
                    return;
                }

                // If valid, we need the current license count
                command.CommandText = "SELECT count(*) FROM Registrations WHERE LicenseID = " + licenseId;
                currentLicenseCount = (int)command.ExecuteScalar();

                // We also need to know if it was already registered
                bool alreadyRegisteredOnThisMachine = CheckIfAlreadyRegistered(computerId, licenseId, command);

                if (alreadyRegisteredOnThisMachine)
                {
                    RegRespond(true, "Already registered", false);
                    return;
                }
                else
                {
                    RegRespond(true, "New Registration", currentLicenseCount > maxLicenseCount);
                }

                // Record the registration
                command.CommandText = "INSERT into Registrations (Date, LicenseID, Version, computerID) " +
                    "values('" + DateTime.Now.ToString() + "', " +
                    "" + licenseId + ", " +
                    "'" + version + "', " +
                    "'" + computerId + "')";
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Response.Write("<PRE>Error: " + e.ToString() + "</PRE>");
            }
            finally
            {
                dataReader = null;
                command = null;
                if (connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        /// <summary>
        /// Check to see if this computer is already registered against this license
        /// </summary>
        private bool CheckIfAlreadyRegistered(string computerId, uint licenseId, SqlCommand command)
        {
            SqlDataReader dataReader = null;
            bool registered = false;

            if (computerId.Length > 8)
            {
                command.CommandText = "SELECT * FROM Registrations WHERE LicenseID = " + licenseId + " AND computerID = '" + computerId + "'";
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    registered = true;
                    break;
                }
                dataReader.Close();
            }

            return registered;
        }

        /// <summary>
        /// print out what happened
        /// </summary>
        public void RegRespond(bool succeeded, string reason, bool overMaxCount)
        {
            string responseContents = "RegistrationResult " +
                "succeeded=\"" + succeeded + "\" " +
                "reason=\"" + reason + "\" " +
                "overmaxcount=\"" + overMaxCount + "\"";
            //Response.Write("<br>&lt;" + responseContents + "&gt;");
            Response.Write("<" + responseContents + " />");
        }

        #endregion
        // END OF CLASS
    }

    #region Supporting classes
    /// <summary>
    /// Dummy class for mimicing an HttpResponse thing
    /// </summary>
    class Response 
    {
        public static void Write(string text) {}
    }

    class Request
    {
        public static NameValueCollection QueryString { get { return null; } }
        public static NameValueCollection Headers { get { return null; } }
        public static NameValueCollection Params { get { return null; } }
    }
    #endregion

#endif // DEBUG
}
