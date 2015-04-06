using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZB_MVC.Models.Repository.Interface;
using ZB_MVC.Models.Repository.Entity;

namespace ZB_MVC.Models.Repository.Implement
{
    public class AnalogInfoRepos : IAnalogInfoRepos
    {
        private DB_MVCDataContext cx = new DB_MVCDataContext();
        public bool ModifyAI(AnalogInfo ai)
        {
            try
            {
                AnalogInfo oldAI = cx.AnalogInfos.SingleOrDefault(x => x.AI_No == ai.AI_No);

                if (oldAI.RTU_No != ai.RTU_No) oldAI.RTU_No = ai.RTU_No;
                if (oldAI.AI_Serial != ai.AI_Serial) oldAI.AI_Serial = ai.AI_Serial;
                if (oldAI.AI_Base != ai.AI_Base) oldAI.AI_Base = ai.AI_Base;
                if (oldAI.AI_Rate != ai.AI_Rate) oldAI.AI_Rate = ai.AI_Rate;
                if (oldAI.AI_Name != ai.AI_Name) oldAI.AI_Name = ai.AI_Name;
                cx.SubmitChanges();
                return true;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}