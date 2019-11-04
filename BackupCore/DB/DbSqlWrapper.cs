using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupCore.DB
{
    /// <summary>
    /// Wraps functionality around the current SQLEXPRESS DB.
    /// </summary>
    public class DbSqlWrapper
    {
        /// <summary>
        /// Gets ID from the Users DB
        /// </summary>
        /// <returns>Integer identifier from the row containing the name of the current user.</returns>
        public static int GetCurrentUserID() { return GetUserID(Environment.UserName); }

        /// <summary>
        /// Gets ID from the Users DB
        /// </summary>
        /// <returns>Integer identifier from the row containing the name of the current user.</returns>
        public static int GetUserID(string userName)
        {
            using (BackupDBDataContext ctx = new BackupDBDataContext())
            {
                var me = from user in ctx.Users
                         where user.Name == userName
                         select user;
                if (me.Count() == 0)
                {
                    ctx.Users.InsertOnSubmit(new User { Name = Environment.UserName });
                    ctx.SubmitChanges();
                    me = from user in ctx.Users
                         where user.Name == Environment.UserName
                         select user; //Try getting the record again after creating it.
                }
                return me.First().ID;
            }
        }

        /// <summary>
        /// Creates a new run for the specified user and returns the created run ID
        /// </summary>
        /// <param name="userID">ID of the user in the database.</param>
        public static int NewRunID(int userID)
        {
            using (BackupDBDataContext ctx = new BackupDBDataContext())
            {
                Run newRun = new Run { Time = DateTime.Now, UserID = userID };
                ctx.Runs.InsertOnSubmit(newRun);
                ctx.SubmitChanges();
                return newRun.ID;
            }
        }
    }
}
