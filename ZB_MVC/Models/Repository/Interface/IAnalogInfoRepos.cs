using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZB_MVC.Models.Repository.Interface
{
    public interface IAnalogInfoRepos
    {
        bool ModifyAI(AnalogInfo ai);
        int GetNextAnalogNo();
        int AddAnalogInfo(AnalogInfo ai);
    }
}
