using eFormShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookSql
{
    public class Appointment
    {
        #region var/pop
        public string GlobalId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Duration { get; set; }
        public int? Id { get; set; }
        public string Subject { get; set; }
        public string ProcessingState { get; set; }
        public string Body { get; set; }
        public bool Completed { get; set; }

        public int TemplateId { get; set; }
        public List<AppoinntmentSite> AppointmentSites { get; set; }
        public List<AppointmentPrefillFieldValue> AppointmentPrefillFieldValues { get; set; }
        public bool Connected { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
        public List<string> Replacements { get; set; }
        public int Expire { get; set; }
        public bool ColorRule { get; set; }
        public string MicrotingUId { get; set; }

        Tools t = new Tools();
        #endregion

        #region con
        public Appointment()
        {

        }

        public Appointment(string globalId, DateTime start, int duration, string subject, string processingState, string body, bool colorRule, int? id)
        {
            Id = id;
            GlobalId = globalId;
            Start = start;
            Duration = duration;
            End = start.AddMinutes(duration);
            Subject = subject;
            ProcessingState = processingState;
            Body = body;

            TemplateId = -1;
            AppointmentSites = new List<AppoinntmentSite>();
            AppointmentPrefillFieldValues = new List<AppointmentPrefillFieldValue>();
            Connected = false;
            Title = "";
            Description = "";
            Info = "";
            Replacements = new List<string>();
            Expire = 2;
            ColorRule = colorRule;
            MicrotingUId = "";

        }
        #endregion

        #region public
        public override string ToString()
        {
            string globalId = "";
            string start = "";
            string title = "";
            string location = "";

            if (GlobalId != null)
                globalId = GlobalId;

            if (Start != null)
                start = Start.ToString();

            if (Title != null)
                title = Title;

            if (ProcessingState != null)
                location = ProcessingState;

            return "GlobalId:" + globalId + " / Start:" + start + " / Title:" + title + " / Location:" + location;
        }

        public void ParseBodyContent(eFormCore.Core sdkCore)
        {

            FindCheckListId();
            FindSites();
            FindFieldPreFillValues(sdkCore);


        }
        #endregion

        #region private
        private void FindCheckListId()
        {
            TemplateId = int.Parse(FindValue("template#"));
        }

        private void FindSites()
        {
            string[] lines = Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string check = "sites#";
            foreach (var line in lines)
            {
                string input = line.ToLower();

                if (input.Contains(check))
                {
                    // Extracts the content to the right of the searchkey.
                    string lineNoComma = line.Remove(0, check.Length).Trim();
                    lineNoComma = lineNoComma.Replace(",", "|");

                    foreach (var item in t.TextLst(lineNoComma))
                    {
                        AppoinntmentSite appointmentSite = new AppoinntmentSite(null, int.Parse(item), ProcessingStateOptions.Processed.ToString(), null);
                        AppointmentSites.Add(appointmentSite);
                    }

                    AppointmentSites = AppointmentSites.Distinct().ToList();

                    continue;
                }
            }
        }

        private string FindValue(string searchKey)
        {
            string[] lines = Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string returnValue = "";
            foreach (var line in lines)
            {
                string input = line.ToLower();
                if (input.Trim() == "")
                    continue;
                if (input.Contains(searchKey.ToLower()))
                {
                    // Extracts the content to the right of the searchkey.
                    string itemStr = line.Remove(0, searchKey.Length).Trim();

                    returnValue = itemStr;

                    continue;
                }
            }
            return returnValue;
        }
        
        private void FindFieldPreFillValues(eFormCore.Core sdkCore)
        {
            List<Field_Dto> fieldDtos = sdkCore.Advanced_TemplateFieldReadAll(TemplateId);

            foreach (Field_Dto fieldDto in fieldDtos)
            {
                string value = FindValue($"F{fieldDto.Id}#");

                if (!string.IsNullOrEmpty(value))
                {
                    AppointmentPrefillFieldValue appointmentPrefillFieldValue = new AppointmentPrefillFieldValue(fieldDto.Id, value);
                    AppointmentPrefillFieldValues.Add(appointmentPrefillFieldValue);
                }
            }
        }
        #endregion

    }

    public enum ProcessingStateOptions
    {
        //Appointment locations options / ProcessingState options
        Pre_created,
        Planned,
        Processed,
        Created,
        Sent,
        Retrived,
        Completed,
        Canceled,
        Revoked,
        ParsingFailed,
        Exception,
        Unknown_location
    }
}