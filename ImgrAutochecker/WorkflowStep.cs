using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgrAutochecker
{
    public class WorkflowStepData
    {
        public int Id { get; set; }
        public long StepId { get; set; }
        public long TaskId { get; set; }
        public string Workflow { get; set; }
        public string FromStep { get; set; }
        public string ToStep { get; set; }
        public Dictionary<string, string> Attribute = new Dictionary<string, string>(); 
        public string ErrorMessage { get; set; }
        public string Error { get; set; }
    }

    class AttrProcessingResult
    {
        private List<Tuple<string,string>> attr = new List<Tuple<string, string>>();

        public WorkflowStepData stepData { get; set; }

        public string Error { get; set; }
        public string ErrorMessage { get; set; }

        public List<Tuple<string, string>> Attr
        {
            get { return attr; }
            set { attr = value; }
        }
    }
}
