using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Interface;
using ZB_MVC.Models.Repository.Entity;

namespace ZB_MVC.Models.Repository.Implement
{
    public class AnalogHistoryRepos : IAnalogHistoryRepos
    {
        DB_MVCDataContext cx = new DB_MVCDataContext();
        public IDictionary<string, string> GetTwoEndpointVal(int analogNo, DateTime inputDateTime)
        {
            if (cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time == inputDateTime).Count() > 0)
            {
                return new Dictionary<string, string>();
            }
            IDictionary<string, string> dic = new Dictionary<string, string>();
            var lessObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time < inputDateTime).OrderByDescending(x => x.AH_Time).FirstOrDefault();
            if (lessObj != null)
            {
                dic["min"] = lessObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["min"] = "";
            }
            var greatObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > inputDateTime).OrderBy(x => x.AH_Time).FirstOrDefault();
            if (greatObj != null)
            {
                dic["max"] = greatObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["max"] = "";
            }
            return dic;
        }

        public Boolean AddEnergyHistory(int analogNo, DateTime time, double value)
        {
            try
            {
                var nextTimeVal = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > time).OrderBy(x => x.AH_Time).Select(x => x.AH_Value).FirstOrDefault();
                var preTimeVal = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time < time).OrderByDescending(x => x.AH_Time).Select(x => x.AH_Value).FirstOrDefault();
                if (preTimeVal > 0 && value <= preTimeVal)
                {
                    return false;
                }
                if (nextTimeVal > 0 && value >= nextTimeVal)
                {
                    return false;
                }
                var newItem = new AnalogHistory
                {
                    AH_AnalogNo = analogNo,
                    AH_Time = time,
                    AH_Value = value
                };
                cx.AnalogHistories.InsertOnSubmit(newItem);
                cx.SubmitChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public IQueryable<EnergyEntity> GetEnergyHistory(int analogNo, DateTime startTime, DateTime endTime)
        {
            var list = from ah in cx.AnalogHistories
                       where ah.AH_AnalogNo == analogNo && ah.AH_Time >= startTime && ah.AH_Time < endTime
                       select new EnergyEntity
                       {
                           PNO = ah.AH_AnalogNo,
                           STime = ah.AH_Time,
                           RealTime = ah.AH_Time,
                           Val = ah.AH_Value
                       };
            return list;
        }

        public Boolean ModifyByTimePeriod(int analogNo, DateTime startTime, DateTime endTime, double value)
        {
            IQueryable<AnalogHistory> allAIInTimePeriod = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time >= startTime && x.AH_Time <= endTime);
            if (allAIInTimePeriod.Count() == 0) return false;
            foreach (var item in allAIInTimePeriod)
            {
                item.AH_Value += value;
            }
            cx.SubmitChanges();
            return true;
        }

        public int AICountByTimePeriod(int analogNo, DateTime startTime, DateTime endTime)
        {
            int aiCount = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > startTime && x.AH_Time < endTime).Count();
            return aiCount;
        }

        public Boolean DeleteByTimePeriod(int analogNo, DateTime startTime, DateTime endTime)
        {
            IQueryable<AnalogHistory> allAIInTimePeriod = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > startTime && x.AH_Time < endTime);
            if (allAIInTimePeriod.Count() == 0) return false;
            cx.AnalogHistories.DeleteAllOnSubmit(allAIInTimePeriod);
            cx.SubmitChanges();
            return true;
        }

        public IDictionary<string, string> GetTwoEndpointValAlt(int analogNo, DateTime inputDateTime)
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();
            var lessObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time < inputDateTime).OrderByDescending(x => x.AH_Time).FirstOrDefault();
            if (lessObj != null)
            {
                dic["min"] = lessObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["min"] = "";
            }
            var greatObj = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time > inputDateTime).OrderBy(x => x.AH_Time).FirstOrDefault();
            if (greatObj != null)
            {
                dic["max"] = greatObj.AH_Value.ToString("f1");
            }
            else
            {
                dic["max"] = "";
            }

            return dic;
        }

        public bool Modify(int analogNo, DateTime time, double modifyVal)
        {
            try
            {
                var item = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time == time).SingleOrDefault();
                if (item != null)
                {
                    item.AH_Value = modifyVal;
                    cx.SubmitChanges();
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Delete(int analogNo, DateTime time)
        {
            try
            {
                var item = cx.AnalogHistories.Where(x => x.AH_AnalogNo == analogNo && x.AH_Time == time).SingleOrDefault();
                if (item != null)
                {
                    cx.AnalogHistories.DeleteOnSubmit(item);
                    cx.SubmitChanges();
                }
                else
                {
                    return false;
                }
                cx.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}