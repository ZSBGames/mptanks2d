﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using System.Threading.Tasks;
using ZSB.Infrastructure.Apis.Account.Models;

namespace ZSB.Infrastructure.Apis.Account.Database
{
    public class LoginDB
    {
        public LoginDB(Contexts.LoginDatabaseContext ctx)
        {
            DBContext = ctx;
        }
        public Contexts.LoginDatabaseContext DBContext { get; set; }
        public const int MaxActiveLoginCount = 25;
        public const int MaxActiveServerTokenCount = 25;
        public readonly TimeSpan LoginLength = TimeSpan.FromDays(15);
        #region Find users
        internal async Task<UserModel> FindByEmailAddress(string email)
        {
            return await Task.Run(() =>
            {
                return DBContext.Users
                    .Where(a => a.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault()?.With(DBContext);
            });
        }

        internal async Task<UserModel> FindBySessionKey(string sessionKey)
        {
            var sess = await GetSessionFromKey(sessionKey);
            if (sess == null) return null;
            if (DateTime.UtcNow > sess.ExpiryDate)
            {
                //Remove session
                await RemoveSession(sess, sess.Owner);
                return null;
            }
            return sess.Owner;
        }

        internal async Task<UserModel> FindByUniqueId(Guid uid)
        {
            return await Task.Run(() =>
            {
                return DBContext.Users
                    .Where(a => a.UniqueId == uid)
                    .FirstOrDefault()?.With(DBContext);
            });
        }
        internal async Task<UserModel> FindByUsername(string username)
        {
            return await Task.Run(() =>
            {
                return DBContext.Users
                    .Where(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault()?.With(DBContext);
            });
        }
        #endregion

        internal async Task<Guid> ChangeConfirmCode(UserModel user)
        {
            user.UniqueConfirmationCode = Guid.NewGuid();
            await UpdateUser(user);
            return user.UniqueConfirmationCode;
        }

        #region Add, update, and remove users

        internal async Task UpdateUser(UserModel user)
        {
            DBContext.Users.Update(user);
            await Save();
        }

        internal async Task DeleteUser(UserModel user)
        {
            foreach (var sess in (await FindByUniqueId(user.UniqueId)).ActiveSessions)
                DBContext.Sessions.Remove(sess);
            foreach (var tkn in (await FindByUniqueId(user.UniqueId)).ActiveServerTokens)
                DBContext.ServerTokens.Remove(tkn);

            await Save();

            DBContext.Users.Remove(user);
            await Save();
        }

        internal async Task AddUser(UserModel user)
        {
            DBContext.Users.Add(user);
            await Save();
        }

        #endregion

        internal async Task<bool> Validate(Models.AuthenticatedRequestModel model)
        {
            if (await FindBySessionKey(model.SessionKey) == null)
                return false;

            return true;
        }
        internal async Task<bool> ValidateAccount(string email, string password)
        {
            var usr = await FindByEmailAddress(email);
            return usr == null ? false : await Task.Run(() => Backend.PasswordHasher.CompareHash(password, usr.PasswordHashes));
        }

        internal async Task<UserActiveSessionModel> DoLogin(string email, string password)
        {
            if (!await ValidateAccount(email, password)) //Hot Spot : hash generation is too slow
                throw new ArgumentException("username_or_password_incorrect");

            var usr = await FindByEmailAddress(email);
            if (!usr.IsEmailConfirmed)
                throw new ArgumentException("email_not_confirmed");
            //Clean up all of the sessions that have expired
            var removable = new List<UserActiveSessionModel>();
            foreach (var mdl in usr.ActiveSessions)
                if (DateTime.UtcNow > mdl.ExpiryDate)
                    removable.Add(mdl);

            foreach (var m in removable) await RemoveSession(m, usr);
            //Check if we are over the limit
            while (usr.ActiveSessions.Count > MaxActiveLoginCount)
                await RemoveSession(usr.ActiveSessions.First(), usr);

            //And create the login key
            var sess = new UserActiveSessionModel(LoginLength);
            usr.AddSession(sess);
            DBContext.Sessions.Add(sess);
            await Save();
            return sess;
        }

        internal async Task<UserActiveSessionModel> GetSessionFromKey(string sessionKey)
        {
            var sk = DBContext.Sessions.Include(a => a.Owner)
                .Where(a => a.SessionKey == sessionKey)
                .FirstOrDefault();

            sk?.Owner?.With(DBContext);

            if (sk == null) return null;

            //Check if expired
            if (DateTime.UtcNow > sk.ExpiryDate)
            {
                await RemoveSession(sk, sk.Owner);
                return null;
            }

            return sk;
        }

        internal async Task RemoveSession(UserActiveSessionModel session, UserModel owner)
        {
            owner.RemoveSession(session);
            DBContext.Sessions.Remove(session);
            DBContext.Users.Update(owner);
            await Save();
        }

        internal async Task<UserServerTokenModel> ValidateServerToken(string token)
        {
            var tkn = await Task.Run(() => DBContext.ServerTokens
                .Include(a => a.Owner)
                .Where(a => a.ServerToken == token)
                .FirstOrDefault());

            tkn?.Owner?.With(DBContext);
            return tkn;
        }

        internal async Task Save()
        {
            await DBContext.SaveChangesAsync();
        }
    }
}
