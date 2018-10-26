using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookSql
{
    public class AppointmentPrefillFieldValue
    {
        #region var/prop
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string FieldValue { get; set; }
        public int AppointmentId { get; set; }

        #endregion

        public AppointmentPrefillFieldValue(int field_id, string field_value, int appointment_id)
        {
            FieldId = field_id;
            FieldValue = field_value;
            AppointmentId = appointment_id;
        }
    }
}
