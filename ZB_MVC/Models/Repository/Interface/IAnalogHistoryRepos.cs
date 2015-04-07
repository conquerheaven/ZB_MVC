using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Entity;

namespace ZB_MVC.Models.Repository.Interface
{
    public interface IAnalogHistoryRepos
    {
        IDictionary<string, string> GetTwoEndpointVal(int analogNo, DateTime inputDateTime);
        Boolean AddEnergyHistory(int analogNo, DateTime time, double value);
        IQueryable<EnergyEntity> GetEnergyHistory(int analogNo, DateTime startTime, DateTime endTime);
        Boolean ModifyByTimePeriod(int analogNo, DateTime startTime, DateTime endTime, double value);
        int AICountByTimePeriod(int analogNo, DateTime startTime, DateTime endTime);
        Boolean DeleteByTimePeriod(int analogNo, DateTime startTime, DateTime endTime);
        IDictionary<string, string> GetTwoEndpointValAlt(int analogNo, DateTime inputDateTime);
        bool Modify(int analogNo, DateTime time, double modifyVal);
        bool Delete(int analogNo, DateTime time);
    }
}
