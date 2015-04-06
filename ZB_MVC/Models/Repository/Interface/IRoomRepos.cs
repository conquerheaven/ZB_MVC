using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZB_MVC.Models.Repository.Interface
{
    public interface IRoomRepos
    {
        IQueryable<SchoolInfo> GetAllSchool();
        IQueryable<SchoolAreaInfo> GetAreaBySchoolID(int schoolID);
        IQueryable<BuildingBriefInfo> GetBuildingByAreaID(int areaID);
    }
}
