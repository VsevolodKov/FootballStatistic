using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;

namespace WpfApp1.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private League league1 = null, league2 = null, league3 = null;
		public MainWindowViewModel()
		{
			var json = new WebClient().DownloadString("https://raw.githubusercontent.com/openfootball/football.json/master/2019-20/en.1.json");
			league1 = JsonConvert.DeserializeObject<Services.League>(json);
			json = new WebClient().DownloadString("https://raw.githubusercontent.com/openfootball/football.json/master/2019-20/en.2.json");
			league2 = JsonConvert.DeserializeObject<Services.League>(json);
			json = new WebClient().DownloadString("https://raw.githubusercontent.com/openfootball/football.json/master/2019-20/en.3.json");
			league3 = JsonConvert.DeserializeObject<Services.League>(json);
		}

		public ObservableCollection<string> results { get; private set; } =
			new ObservableCollection<string>();

		private DelegateCommand _commandLoad = null;
		public DelegateCommand CommandLoad =>
			_commandLoad ?? (_commandLoad = new DelegateCommand(CommandLoadExecute));

		private void CommandLoadExecute()
		{
			results.Clear();
			results.Add(GetInfoAboutLeague(league1));
			results.Add(GetInfoAboutLeague(league2));
			results.Add(GetInfoAboutLeague(league3));
			results.Add(GetBestDay(league1, league2, league3));
		}

		private string GetInfoAboutLeague(League league)
		{
			string resultString = league.name + "\n\n";

			Dictionary<string, int> teamsAndShootedGoals = new Dictionary<string, int>();
			resultString += "Best attacking team:\n" + GetBestAttackingTeam(league, teamsAndShootedGoals);

			Dictionary<string, int> teamsAndMissedGoals = new Dictionary<string, int>();
			resultString += "\n\nBest defending team:\n" + GetBestDefendingTeam(league, teamsAndMissedGoals);

			resultString += "\n\nBest attacking-defending team:\n" + GetBestAttackingDefendingTeam(league, teamsAndShootedGoals, teamsAndMissedGoals);

			return resultString;
		}

		private string GetBestAttackingTeam(League league, Dictionary<string, int> teamsAndShootedGoals)
		{
			foreach (var team in league.matches)
			{
				if (team.score != null)
				{
					if (!teamsAndShootedGoals.ContainsKey(team.team1))
					{
						teamsAndShootedGoals.Add(team.team1, team.score.ft[0]);
					}
					else
					{
						teamsAndShootedGoals[team.team1] += team.score.ft[0];
					}
					if (!teamsAndShootedGoals.ContainsKey(team.team2))
					{
						teamsAndShootedGoals.Add(team.team2, team.score.ft[1]);
					}
					else
					{
						teamsAndShootedGoals[team.team2] += team.score.ft[1];
					}
				}
			}
			var teamMostShootedGoals = teamsAndShootedGoals.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
			return teamMostShootedGoals;
		}

		private string GetBestDefendingTeam(League league, Dictionary<string, int> teamsAndMissedGoals)
		{
			foreach (var team in league.matches)
			{
				if (team.score != null)
				{
					if (!teamsAndMissedGoals.ContainsKey(team.team1))
					{
						teamsAndMissedGoals.Add(team.team1, team.score.ft[1]);
					}
					else
					{
						teamsAndMissedGoals[team.team1] += team.score.ft[1];
					}
					if (!teamsAndMissedGoals.ContainsKey(team.team2))
					{
						teamsAndMissedGoals.Add(team.team2, team.score.ft[0]);
					}
					else
					{
						teamsAndMissedGoals[team.team2] += team.score.ft[0];
					}
				}
			}
			var teamLeastMissedGoals = teamsAndMissedGoals.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
			return teamLeastMissedGoals;
		}

		private string GetBestAttackingDefendingTeam(League league, Dictionary<string, int> teamsAndShootedGoals, Dictionary<string, int> teamsAndMissedGoals)
		{
			Dictionary<string, int> teamsAndShootedMissedGoals = new Dictionary<string, int>(teamsAndShootedGoals);
			foreach (var team in teamsAndMissedGoals)
			{
				teamsAndShootedMissedGoals[team.Key] -= team.Value;
			}
			KeyValuePair<string, int> bestTeamShootedMissedGoals = new KeyValuePair<string, int>();
			foreach (var team in teamsAndShootedMissedGoals)
			{
				if (team.Value > bestTeamShootedMissedGoals.Value)
					bestTeamShootedMissedGoals = team;
				else if (team.Value == bestTeamShootedMissedGoals.Value)
				{
					if (teamsAndShootedGoals[team.Key] > teamsAndShootedGoals[bestTeamShootedMissedGoals.Key])
					{
						bestTeamShootedMissedGoals = team;
					}
				}
			}
			return bestTeamShootedMissedGoals.Key;
		}

		private string GetBestDay(League league1, League league2, League league3)
		{
			Dictionary<DateTime, int> leaguesDaysGoals = new Dictionary<DateTime, int>();
			JoiningLeaguesDays(leaguesDaysGoals, GetLeagueDaysGoals(league1));
			JoiningLeaguesDays(leaguesDaysGoals, GetLeagueDaysGoals(league2));
			JoiningLeaguesDays(leaguesDaysGoals, GetLeagueDaysGoals(league3));
			var dayWithMostGoals = leaguesDaysGoals.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
			return "\nMost efficient day " + dayWithMostGoals.ToString("MM-dd-yyyy");
		}

		private Dictionary<DateTime, int> GetLeagueDaysGoals(League league)
		{
			Dictionary<DateTime, int> daysAndGoals = new Dictionary<DateTime, int>();
			foreach (var day in league.matches)
			{
				if (day.score != null)
				{
					if (!daysAndGoals.ContainsKey(day.date))
					{
						daysAndGoals.Add(day.date, day.score.ft[0] + day.score.ft[1]);
					}
					else
					{
						daysAndGoals[day.date] += day.score.ft[0] + day.score.ft[1];
					}
				}
			}
			return daysAndGoals;
		}
		private void JoiningLeaguesDays(Dictionary<DateTime, int> resultLeague, Dictionary<DateTime, int> addingLeague)
		{
			foreach (var day in addingLeague)
			{
				if (!resultLeague.ContainsKey(day.Key))
				{
					resultLeague.Add(day.Key, day.Value);
				}
				else
				{
					resultLeague[day.Key] += day.Value;
				}
			}
		}
	}
}
