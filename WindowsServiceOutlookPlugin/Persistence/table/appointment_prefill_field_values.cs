namespace OutlookSql
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class appointment_prefill_field_values
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("appointment")]
        public int? appointment_id { get; set; }

        [StringLength(255)]
        public string workflow_state { get; set; }

        public int? version { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public int field_id { get; set; }

        public string field_value { get; set; }

        public virtual appointments appointment { get; set; }
    }
}
