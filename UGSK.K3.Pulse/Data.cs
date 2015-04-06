using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UGSK.K3.Pulse
{
    public enum PeriodKind { Daily = 1, Weekly = 2, Monthly = 3 }

    public enum CounterKind { Average = 0, Total = 1 }

    public enum IndexKind { Normal, Super }

    public class SaleSystemNotification
    {
        public int Filial { get; set; }
        public string Product { get; set; }
        public DateTimeOffset ContractSigningDateTime { get; set; }
        public bool Increment { get; set; }
    }

    public class Counter
    {
        public string Product { get; set; }
        public PeriodKind PeriodKind { get; set; }
        public int Value { get; set; }
        public CounterKind Kind { get; set; }
        public DateTimeOffset PeriodStart { get; set; }
        public bool IsClosed { get; set; }
    }

    public class CounterMessage
    {
        public string Product { get; set; }
        public PeriodKind PeriodKind { get; set; }
        public CounterKind Kind { get; set; }
        public int Value { get; set; }
        public DateTime PeriodStart { get; set; }
    }

    public class IndexMessage
    {
        public string Product { get; set; }
        public int Value { get; set; }
    }

    public class Index
    {
        public int Id { get; set; }
        [Required]
        public string Product { get; set; }
        public int Value { get; set; }
        [Required]
        public DateTimeOffset ActiveStart { get; set; }
        [Required]
        public IndexKind IndexKind { get; set; }
    }
}