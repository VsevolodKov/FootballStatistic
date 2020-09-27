using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    public class Score
    {
        public List<int> ft { get; set; }
    }
    public class Match
    {
        public string round { get; set; }
        public DateTime date { get; set; }
        public string team1 { get; set; }
        public string team2 { get; set; }
        public Score score { get; set; }
    }
    public class League
    {
        public string name { get; set; }
        public List<Match> matches { get; set; }
    }
}
