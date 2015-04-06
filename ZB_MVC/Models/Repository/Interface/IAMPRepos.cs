using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Entity;

namespace ZB_MVC.Models.Repository.Interface
{
    public interface IAMPRepos
    {
        IQueryable<AMPExtEntity>  GetAllAMP();
        AnalogMeasurePoint GetAMP(int analogNo);
        bool ModifyAMP(AnalogMeasurePoint amp);
        bool IsUsedByObj(int pno);
        bool DeleteAMP(int pno);
    }
}
