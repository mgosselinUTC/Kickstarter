using Kickstarter.Api;
using Kickstarter.Api.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kickstarter.Api.Model;

namespace Kickstarter
{
    public partial class Form1 : Form
    {
        private IKickstarterSession session;
        private string[] states;

        private async void login()
        {
            var client = new KickstarterClient();
            session = await client.StartSession("kickstarter@utc4me.org", "utc4medotorg");
            Console.WriteLine("logged in!");
        }

        private async void buildStates()
        {
            StreamReader reader = new StreamReader("states.txt");
            string line = "";
            states = new string[50];
            int counter = 0;
            while ((line = reader.ReadLine()) != null)
            {
                states[counter] = line;
                counter++;
            }
        }

        public Form1()
        {
            InitializeComponent();
            login();
            buildStates();
        }

        private int getIDforState(string state)
        {
            return Array.IndexOf(states, state) + /*alabama... */ 2347560;
        }

        private async Task<IEnumerable<Project>> getProjectsFrom(string state, int take)
        {
            return await session.Query(new DiscoverProjects().Woe("" + getIDforState(state)).SortedBy("newest").Take(take));
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //int stateID = getIDforState(textBox1.Text);

            IEnumerable<Project> projects = await getProjectsFrom(textBox1.Text, 500);

            StreamWriter writer = new StreamWriter("C:\\Users\\mgosselin\\Desktop\\stuff.csv");
            writer.WriteLine("Project Name, Location,, Status, Pledged, Goal, Backers, Launch Date");

            foreach (Project project in projects)
            {
             
                string location = project.Location.DisplayableName.Replace(",", "");
                string place = location.Substring(0, location.Length - 3);
                string stateAbbr = location.Substring(location.Length - 2);
                location = place + "," + stateAbbr;

                writer.WriteLine(String.Join(", ", new string[] {
                
                    project.Name.Replace(",", ""),
                    location,
                    project.State.Replace(",", "").Replace("\"", "\\\""),
                    "" + project.Pledged,
                    "" + project.Goal,
                    "" + project.BackersCount,
                    project.LaunchedAt.Date.ToString().Replace(",", " "),

                }));
            }
            Close();
        }
    }
}
