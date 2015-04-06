using System;
using System.Collections;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Interface;
using ZB_MVC.Models.Repository.Entity;

namespace ZB_MVC.Models.Repository.Implement
{
    public class PowerClassRepos : IPowerClassRepos
    {
        private DB_MVCDataContext cx = new DB_MVCDataContext();
        public IList GetAll()
        {
            return cx.PowerClasses.ToList();
        }
    }
}