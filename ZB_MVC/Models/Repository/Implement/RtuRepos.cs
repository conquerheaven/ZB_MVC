using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Interface;

namespace ZB_MVC.Models.Repository.Implement
{
    public class RtuRepos : IRtuRepos
    {
        private DB_MVCDataContext cx = new DB_MVCDataContext();
        public IQueryable<RTU> GetAll()
        {
            var Rtu = from rtu in cx.RTUs
                      select rtu;
            return Rtu;
        }
    }
}