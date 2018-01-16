using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using YuanTu.Consts.Models;

namespace YuanTu.Consts.EventModels
{
    public class ModulesChangeEvent : PubSubEvent<ModulesChangeEvent>
    {
        public Action ModulesChangeAction;
        public Action ResetAction;
        public Stack<List<ChoiceButtonInfo>> ButtonStack;
    }
}
