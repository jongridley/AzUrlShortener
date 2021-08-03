using System;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace Cloud5mins.domain
{
    public class ShortUrlEntity : TableEntity
    {
        public string Url { get; set; }
        private string _activeUrl { get; set; }

        public string ActiveUrl { 
            get{
                if(String.IsNullOrEmpty(_activeUrl) )
                    _activeUrl = GetActiveUrl();
                return _activeUrl;
            }
        }


        public string Title { get; set; }

        public string ShortUrl { get; set; }

        public int Clicks { get; set; }

        public bool? IsArchived { get; set; }

        public string SchedulesPropertyRaw { get; set; }

        [IgnoreProperty]
        public Schedule[] Schedules { 
            get{
                if(String.IsNullOrEmpty(SchedulesPropertyRaw))
                    return null;
                return JsonConvert.DeserializeObject<Schedule[]>(SchedulesPropertyRaw);
            } 
            set{
                SchedulesPropertyRaw = JsonConvert.SerializeObject(value); 
            } 
        }

        public string MetadataPropertyRaw { get; set; }

        [IgnoreProperty]
        public ShipmentMetadata ShipmentMetadata
        {
            get
            {
                if (String.IsNullOrEmpty(MetadataPropertyRaw))
                    return null;
                return JsonConvert.DeserializeObject<ShipmentMetadata>(MetadataPropertyRaw);
            }
            set
            {
                MetadataPropertyRaw = JsonConvert.SerializeObject(value);
            }
        }

        public ShortUrlEntity(){}

        public ShortUrlEntity(string longUrl, string endUrl)
        {
            Initialize(longUrl, endUrl, string.Empty, null);
        }

        public ShortUrlEntity(string longUrl, string endUrl, Schedule[] schedules)
        {
            Initialize(longUrl, endUrl, string.Empty, schedules);
        }

        public ShortUrlEntity(string longUrl, string endUrl, string title, Schedule[] schedules, ShipmentMetadata metadata = null)
        {
            Initialize(longUrl, endUrl, title, schedules, metadata);
        }

        private void Initialize(string longUrl, string endUrl, string title, Schedule[] schedules, ShipmentMetadata metadata = null)
        {
            PartitionKey = endUrl.First().ToString();
            RowKey = endUrl;
            Url = longUrl;
            Title = title;
            Clicks = 0;
            IsArchived = false;
            Schedules = schedules;
            ShipmentMetadata = metadata;
        }

        public static ShortUrlEntity GetEntity(string longUrl, string endUrl, string title, Schedule[] schedules){
            return new ShortUrlEntity
            {
                PartitionKey = endUrl.First().ToString(),
                RowKey = endUrl,
                Url = longUrl,
                Title = title,
                Schedules = schedules
            };
        }

        private string GetActiveUrl()
        {
            if(Schedules != null)
                    return GetActiveUrl(DateTime.UtcNow);
            return Url;
        }
        private string GetActiveUrl(DateTime pointInTime)
        {
            var link = Url;
            var active = Schedules.Where(s =>
                s.End > pointInTime && //hasn't ended
                s.Start < pointInTime //already started
                ).OrderBy(s => s.Start); //order by start to process first link

            foreach (var sched in active.ToArray())
            {
                if (sched.IsActive(pointInTime))
                {
                    link = sched.AlternativeUrl;
                    break;
                }
            }
            return link;
        }
    }


}
