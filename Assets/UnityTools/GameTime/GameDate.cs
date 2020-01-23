using UnityEngine;
using System;
namespace UnityTools.Internal {
    [Serializable] public class GameDate  {
        public const float k_HoursPerDay = 24;   

        [Range(0, 1)] public float timeOfDay = .5f;
        [Range(1, 31)] public int day = 1;
        [Range(1, 12)] public int month = 1;
        public int year = 2000;
        [NonSerialized] int halfHours = 0;


        public GameDate (GameDate template) {
            timeOfDay = template.timeOfDay;
            day = template.day;
            month = template.month;
            year = template.year;
        }

        public int hour { get{ return (int)Mathf.Floor(timeOfDay); } }		
        public int minute { get{ return (int)Mathf.Floor((timeOfDay - hour) * 60); } }



        /*
            pre cache-ing string to avoid garbage creation....
        */
        static string PrefixWith0 (int i) {
            return (i < 10 ? "0" : "") + i;
        }

        // static string[] GetDigitsString() {
        //     string[] r = new string[60];
        //     for (int i = 0; i < 60; i++) 
        //         r[i] = (i < 10 ? "0" : "") + i;
        //     return r;
        // }
        // static readonly string[] digits = GetDigitsString();
        static readonly string[] monthNames = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        // public string dateString { get { return monthNames[month-1] + " " + digits[day] + ", " + year; } }
        public string dateString { get { return dates[day + (month - 1) * 31] + year; } }


        static string[] BuildDates () {
            string[] r = new string[12 * 31];
            for (int m = 0; m < 12; m++) {
                for (int d = 0; d < 31; d++) {
                    r[d + m * 31] = monthNames[m] + " " + PrefixWith0(d) + ", ";
                }    
            }
            return r;
        }
        static readonly string[] dates = BuildDates();

        static string[] BuildTimes () {
            string[] r = new string[24 * 60];
            for (int h = 0; h < 24; h++) {
                for (int m = 0; m < 60; m++) {
                    r[m + h * 60] = PrefixWith0(h) + ":" + PrefixWith0(m);
                }    
            }
            return r;
        }
        static readonly string[] times = BuildTimes();
        public string timeOfDayString { get { return times[minute + hour * 60]; } }// digits[hour] + ":" + digits[minute]; } }
            
        void BroadcastHalfHour (int hh, out bool halfHourly) {
            halfHourly = true;
            halfHours = hh;
        }

        public void ProgressTime(float secondsInGameDay, out bool halfHourly, out bool hourly, out bool daily, out bool monthly, out bool yearly)
        {
            halfHourly = hourly = daily = monthly = yearly = false;
            
            int hr = hour;
            timeOfDay += (Time.deltaTime / secondsInGameDay) * k_HoursPerDay;
            hourly = hour != hr;
            
            if (halfHours == 0 && minute < 30) 
                BroadcastHalfHour(30, out halfHourly);
            if (halfHours == 30 && minute >= 30) 
                BroadcastHalfHour(0, out halfHourly);
            
            if (timeOfDay >= k_HoursPerDay) {
                timeOfDay = 0;	
                day++;
                daily = true;
                if (day > DateTime.DaysInMonth(year, month)) {
                    day = 1;
                    month++;
                    monthly = true;
                    if (month > 12) {
                        month = 1;
                        year++;
                        yearly = true;
                    }
                }
            }
        }
    }
}
