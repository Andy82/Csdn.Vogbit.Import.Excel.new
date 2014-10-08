using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using com.imageright.server.workflow;
using imageright.interfaces;

namespace ImgrAutochecker
{
    class ImgrProcessor
    {
        private const string MANUAL_STEP_GUID = "be3a6bcb-1fb3-4973-b798-4827d13de7b6";

        public static void CreateTask(IConnection _conn, long objectId, WorkflowStepData workflowStep)
        {
            TaskCreateData tcd = new TaskCreateData();
            tcd.SetDescription("Automatic Step");
            tcd.SetPriority(5);
            tcd.SetAvailable(DateTime.Now);

            workflowStep.TaskId = _conn.Workflow.CreateTask(objectId, workflowStep.StepId, tcd);
        }

        public static void ReleaseTask(IConnection _conn, WorkflowStepData workflowStep)
        {
            _conn.Workflow.LockTask(workflowStep.TaskId);

            foreach (KeyValuePair<string, string> attributeVal in workflowStep.Attribute)
            {
                _conn.Workflow.SetTaskAttribute(workflowStep.TaskId, attributeVal.Key, attributeVal.Value);
            }

            TaskReleaseStatus taskStatus = _conn.Workflow.ReleaseTask(workflowStep.TaskId, true);

            TaskData task = _conn.Workflow.GetTaskData(workflowStep.TaskId);
            workflowStep.ToStep = _conn.WorkflowMetadata.GetStep(task.stepid, StepRequestFlag.Production).programmaticName;

            if (taskStatus.ErrorMessage == null) return;

            workflowStep.ErrorMessage = taskStatus.ErrorMessage;
            workflowStep.Error = taskStatus.TaskError.ToString();
        }

        public static List<WorkflowStepData> GetFromImgr(IConnection _conn, string WorkflowName, BindingList<AttributeList> attributes)
        {
            List<WorkflowStepData> wfSteps = new List<WorkflowStepData>();
            WorkflowDef workflow = _conn.WorkflowMetadata.GetWorkflow(WorkflowName);
            StepRootDef[] steps = _conn.WorkflowMetadata.Steps(workflow.id, StepRequestFlag.Production);

            foreach (StepRootDef step in steps)
            {
                if (step.typeGuid != MANUAL_STEP_GUID) continue;

                List<List<string>> atrList = getGuidAttributes(_conn, step.id, attributes);
                var atrSortList = FindAllCombinations(atrList);

                //if (atrSortList.Count(IEnumerable<List<string>>) == 0)
                //{
                //    WorkflowStepData wfStep = new WorkflowStepData();
                //    wfStep.Workflow = workflow.programmaticName;
                //    wfStep.StepId = step.id;
                //    wfStep.FromStep = step.programmaticName;
                //    wfSteps.Add(wfStep);
                //}

                foreach (var _val in atrSortList)
                {
                    WorkflowStepData wfStep = new WorkflowStepData();
                    foreach (var _val2 in _val)
                    {
                        wfStep.Workflow = workflow.programmaticName;
                        wfStep.StepId = step.id;
                        wfStep.FromStep = step.programmaticName;

                        string attrName = getAttributeName(attributes, _val.IndexOf(_val2));
                        wfStep.Attribute.Add(attrName, _val2);
                    }
                    wfSteps.Add(wfStep);
                }
            }
            return wfSteps;
        }

        public static void getAttributesList(IConnection _conn, long stepId, BindingList<AttributeList> attributes)
        {
            foreach (AttributeList attrib in attributes)
            {
                    if (attrib.All)
                    {
                        attrib.Attributes = GetStepAttributeRules(_conn, stepId, attrib);
                    }
                    else if (attrib.Value != null)
                    {
                        attrib.Attributes.Add(attrib.Value);
                    }
            }
        }

        public static string getAttributeName(BindingList<AttributeList> attributes, int index)
        {
            return attributes.Where(att => att.all || !string.IsNullOrEmpty(att.Value)).ToArray()[index].Attribute;
        }


        public static List<List<string>> getGuidAttributes(IConnection _conn, long stepId, BindingList<AttributeList> attributes)
        {
            List<List<string>> tmp = new List<List<string>>();
            foreach (AttributeList attrib in attributes)
            {
                    List<string> guidAttribute = new List<string>();
                    if (attrib.All)
                    {
                        guidAttribute = GetStepAttributeRules(_conn, stepId, attrib);
                    }
                    else if (attrib.Value != null)
                    {
                        guidAttribute.Add(attrib.Value);
                    }

                if (guidAttribute.Count != 0)
                {
                    tmp.Add(guidAttribute.ToList());
                }
            }
            return tmp;
        }


        public static IEnumerable<List<T>> FindAllCombinations<T>(List<List<T>> itemsList)
        {

            if (itemsList.Count == 0)
            {
                yield break;
            }
            List<T> iter = itemsList.First();

            var ienumerator = iter.GetEnumerator();

            while (ienumerator.MoveNext())
            {
                var item = ienumerator.Current;

                List<List<T>> range = itemsList.GetRange(1, itemsList.Count - 1).ToList();

                if (range.Count == 0)
                {

                    yield return new List<T> { item };
                    while (ienumerator.MoveNext())
                    {
                        yield return new List<T> { ienumerator.Current };
                    }
                    yield break;

                }

                var res = FindAllCombinations(range).ToList();

                if (!res.Any())
                {
                    yield return new List<T> { item };
                    yield break;
                }

                foreach (var comb in res)
                {

                    var combination = new List<T> { item };

                    combination.AddRange(comb);

                    yield return combination;
                }
            }
        }

        public static List<String> GetStepAttributeRules(IConnection _conn, long stepId, AttributeList attrib)
        {
            StepAttributeRule stepAttr = _conn.WorkflowMetadata.GetStepAttribute(stepId, attrib.Attribute);
            
            //Get Validation rules from step
            var xDoc = XDocument.Parse(stepAttr.validationRules);
            List<string> stepValidationRules =  xDoc.Elements("InputListData").Elements("inputList").Elements("inputlistitem").Elements("Value").Select(e => e.Value).ToList();
            
            if (stepValidationRules.Count == 0)
            {
                //Get default Validation rules
                xDoc = XDocument.Parse(attrib.ValidationRules);
                stepValidationRules = xDoc.Elements("FieldProperties").Elements("combobox").Elements("comboitem").Select(e => e.Value).ToList();
            }
            return stepValidationRules;
        }
    }
}
