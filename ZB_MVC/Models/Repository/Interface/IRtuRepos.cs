using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZB_MVC.Models.Repository.Interface
{
    public interface IRtuRepos
    {
        IQueryable<RTU> GetAll();
    }
}
