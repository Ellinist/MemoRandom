using System;
using MemoRandom.Client.Views.UserControls;

namespace MemoRandom.Client.Models
{
    public class ComparedHumanProgressData
    {
        public ComparedBlockControl ComparedHumanBar { get; set; }
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
