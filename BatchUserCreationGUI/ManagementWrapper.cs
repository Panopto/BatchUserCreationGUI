using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatchUserCreationGUI.PanoptoAccessManagement;
using BatchUserCreationGUI.PanoptoUserManagement;
using BatchUserCreationGUI.PanoptoSessionManagement;

/// <summary>
/// Sample C# that uses the Panopto PublicAPI
/// This sample shows how to creates an user with/without a group and grant the user access to a folder with the Panopto PublicAPI
/// </summary>
namespace BatchUserCreationGUI
{

    public class ManagementWrapper
    {
        private static string errorMessage = "";
        private static bool hasError = false;

        /// <summary>
        /// Creates a new user and their folder and adds them as creator to their folder
        /// </summary>
        /// <param name="authUserKey">User with admin access to create the new user</param>
        /// <param name="authPassword">Admin user's password</param>
        /// <param name="data">argument containing information of new user</param>
        /// <returns>String containing error message, null if no error</returns>
        public static string FullyCreateUser(string authUserKey, string authPassword, string data)
        {
            // Set Panopto server authentication details
            string userKey = authUserKey;
            string password = authPassword;

            // Parse data
            string[] parsedData = data.Split(',');

            // The Id of the Panopto folder to the new user will be granted access
            string folderName = parsedData[1] + " " + parsedData[2] + "'s Folder";

            // Setup error message
            errorMessage = "\nUser: " + parsedData[0];
            hasError = false;
                        
            // Permissions for user management
            PanoptoUserManagement.AuthenticationInfo userAuth = new PanoptoUserManagement.AuthenticationInfo()
            {
                UserKey = userKey,
                Password = password
            };

            // Permissions for access management
            PanoptoAccessManagement.AuthenticationInfo accessAuthInfo = new PanoptoAccessManagement.AuthenticationInfo()
            {
                UserKey = userKey,
                Password = password
            };

            // Permissions for session management
            PanoptoSessionManagement.AuthenticationInfo sessionAuthInfo = new PanoptoSessionManagement.AuthenticationInfo()
            {
                UserKey = userKey,
                Password = password
            };

            Guid panoptoFolderId = Guid.Empty;

            if (!String.IsNullOrWhiteSpace(folderName))
            {
                panoptoFolderId = CreateFolder(sessionAuthInfo, folderName);
            }

            if (panoptoFolderId != Guid.Empty)
            {
                // Creates a new user
                Guid idUser = CreateUser(userAuth, parsedData[0], parsedData[1], parsedData[2], parsedData[3]);

                // Sets creator privilege to the created group over the folder
                if (idUser != Guid.Empty)
                {
                    SetPrivilegesForFolder(accessAuthInfo, panoptoFolderId, idUser, AccessRole.Creator);
                }
            }

            errorMessage += "\n==========";
            if (hasError)
            {
                return errorMessage;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a folder 
        /// </summary>
        /// <param name="auth">Authorization token</param>
        /// <param name="folderName">folder name</param>
        /// <returns>Guid of folder created</returns>
        private static Guid CreateFolder(PanoptoSessionManagement.AuthenticationInfo auth, string folderName)
        {
            Guid folderId = Guid.Empty;
            ISessionManagement sessionMgmt = new SessionManagementClient();

            try
            {
                Folder folder = sessionMgmt.AddFolder(auth, folderName, null, false);
                folderId = folder.Id;
            }
            catch (Exception e)
            {
                hasError = true;
                errorMessage += "\n\tUnable to create a folder with folder name: " + folderName + "; " + e.Message;
            }

            return folderId;
        }

        /// <summary>
        /// Creates a new user in Panopto server.
        /// </summary>
        /// <param name="userAuth">Authentification info.</param>
        /// <param name="userKey">User name</param>
        /// <param name="firstName">User first name.</param>
        /// <param name="lastName">User last name.</param>
        /// <param name="email">User email.</param>
        /// <returns>Guid associated to the created user.</returns>
        private static Guid CreateUser(PanoptoUserManagement.AuthenticationInfo userAuth, string userKey, string firstName, string lastName, string email)
        {
            Guid userID = Guid.Empty;
            IUserManagement userMgr = new UserManagementClient();
            
            try
            {   
                // Tries to get the user by key
                User panUser = userMgr.GetUserByKey(userAuth, userKey);
                
                //Checks if the user exist
                if (panUser != null && !panUser.UserId.Equals(Guid.Empty))
                {
                    userID = panUser.UserId;
                    errorMessage += "\n\tThe user " + userID.ToString() + " already exists";
                    hasError = true;
                }
                else
                {
                    // Checks new user data
                    if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName) ||
                        String.IsNullOrEmpty(email))
                    {
                        errorMessage += "\n\t" + userKey + "doesn't have enough details to make a user account";
                        hasError = true;
                    }
                    else
                    {
                        // Creates a new user
                        panUser = new User()
                        {
                            UserKey = userKey,
                            FirstName = firstName,
                            LastName = lastName,
                            SystemRole = BatchUserCreationGUI.PanoptoUserManagement.SystemRole.None,
                            UserBio = String.Empty,
                            Email = email,
                            EmailSessionNotifications = false
                        };

                        // Creates the user in Panopto Server
                        userID = userMgr.CreateUser(userAuth, panUser, String.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage += "\n\tError creating user: " + e.Message;
                hasError = true;
            }

            return userID;
        }

        /// <summary>
        /// Sets an access role privilege to a group for an specific folder.
        /// </summary>
        /// <param name="accessAuthInfo">Authentification info.</param>
        /// <param name="folderId">Folder Guid.</param>
        /// <param name="userId">User Guid.</param>
        /// <param name="accessRole">Access role type.</param>
        private static void SetPrivilegesForFolder(PanoptoAccessManagement.AuthenticationInfo accessAuthInfo, Guid folderId, Guid userId, AccessRole accessRole)
        {
            IAccessManagement accessMgr = new AccessManagementClient();

            try
            {
                // Grant access to the folder
                accessMgr.GrantUsersAccessToFolder(accessAuthInfo, folderId, new Guid[] { userId }, accessRole);
            }
            catch (Exception e)
            {
                errorMessage += "\n\tUnable to adding user " + userId + " as " + accessRole.ToString() + " of folder " + folderId + "; " + e.Message;
                hasError = true;
            }
        }
    }
}
