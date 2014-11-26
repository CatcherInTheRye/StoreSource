using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Web.Caching;
using DataRepository.DataContracts;
using DataRepository.Mappers;
using Ninject;
using Ninject.Modules;
using PCSMvc.Mappers;
using PCS.DataRepository.DataContracts;
using System.Globalization;

namespace DataRepository
{
    //public class RepositoryNinjectModule : NinjectModule
    //{
    //    public override void Load()
    //    {
    //        this.Bind<IRepository>().To<Repository>().InSingletonScope()
    //            .WithConstructorArgument("connectionString", ConnectionString.ModelConnectionString);
    //    }
    //}

    public class Repository : IRepository
    {
        #region init

        [Inject]
        public StoreEntities Db { get; set; }


        [Inject]
        public IMapper ModelMapper { get; set; }

        //SubmitChanges
        private EfStatus SubmitChanges()
        {
            var status = new EfStatus ();
            try
            {
                Db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                return status.SetErrors(ex.EntityValidationErrors);
            }
            catch (DbUpdateException ex)
            {
                var decodedErrors = status.TryDecodeDbUpdateException(ex);
                if (decodedErrors == null)
                    throw; //it isn't something we understand so rethrow
                return status.SetErrors(decodedErrors);
            }
            //else it isn't an exception we understand so it throws in the normal way
            return status;
        }

        #endregion init


        #region Users

        public IQueryable<User> Users
        {
            get
            {
                return Db.Users;
            }
        }

        public List<UserForm> UserForms
        {
            //TODO: Tolik. First ToList() may be redundant. Check SQL Server
            get { return Users.ToList().Select(u => (UserForm)ModelMapper.Map(u, typeof(User), typeof(UserForm))).ToList(); }
        }

        public User UserGetByLogin(string userName)
        {
            return Users.FirstOrDefault(p => string.Compare(p.Phone, userName, true) == 0);
        }

        public User Login(string login, string password)
        {
            return Users.FirstOrDefault(p => string.Compare(p.Login, login, true) == 0 && p.Password == password);
        }

        public UserForm UserUpdate(UserForm userForm, int userId)
        {
            throw new NotImplementedException();
        }

        public void UserRemove(int id)
        {
            User user = Users.FirstOrDefault(t => t.Id == id);
            if (user == null) return;
            Db.Users.Remove(user);
            SubmitChanges();
        }

        #endregion Users

    }
}