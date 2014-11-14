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
        private bool busy = false;

        private void setBusy(bool b)
        {
            if (b)
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = 10;
            }
            else
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 1;
                progressBar1.Value = 0;
                progressBar1.Style = ProgressBarStyle.Continuous;
            }
            button1.Enabled = !b;
            busy = b;
            // busyLabel.Text = "Busy: " + b;
            if (!b) status.Text = "Ready!";
        }

        private async void login()
        {
            setBusy(true);
            var client = new KickstarterClient();
            session = await client.StartSession("kickstarter@utc4me.org", "utc4medotorg");
            setBusy(false);
        }

        //im going to assume this takes little to no time
        //sucks to be a slow HDD, don't it?
        private async void buildStates()
        {
            StreamReader reader = new StreamReader("states.txt");
            string line = "";
            states = new string[50];
            int counter = 0;
            while ((line = reader.ReadLine()) != null)
            {
                comboBox1.Items.Add(line);
                states[counter] = line;
                counter++;
            }
            Console.WriteLine("State list built!");
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

        private async Task<IEnumerable<Project>> getProjectsFrom(string state, int take, string sort)
        {
            status.Text = "Retrieving projects...";
            return await session.Query(new DiscoverProjects().Woe("" + getIDforState(state)).SortedBy(sort).Take(take));
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            go();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private async void writeProjects(string state, int take, string sort)
        {
            setBusy(true);

            IEnumerable<Project> projects = await getProjectsFrom(state, take, sort);

            status.Text = "Writing projects...";

            string path = Environment.GetEnvironmentVariable("userprofile") + "\\Desktop\\";
            
            path += "" + state + "-";
            path += DateTime.Now.ToString().Replace(",", "").Replace(":", "").Replace("/", "_") + ".csv";
            StreamWriter writer = new StreamWriter(path);
            writer.WriteLine("Project Name,Author,Location,,Status,Pledged,Goal,Backers,Launch Date,Deadline,Categorey,Url");

            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = projects.Count();
            progressBar1.Value = 0;
            progressBar1.Step = 1;

            foreach (Project project in projects)
            {

                string location = project.Location.DisplayableName.Replace(",", "");
                string place = location.Substring(0, location.Length - 3);
                string stateAbbr = location.Substring(location.Length - 2);
                location = place + "," + stateAbbr;

                writer.WriteLine(String.Join(",", new string[] {
                
                    "\"" + project.Name.Replace("\"", "\"\"") + "\"",
                    "\"" + project.Creator.Name.Replace("\"", "\"\"") + "\"",
                    location,
                    "\"" + project.State.Replace(",", "").Replace("\"", "\"\"") + "\"",
                    "" + project.Pledged,
                    "" + project.Goal,
                    "" + project.BackersCount,
                    "\"" + project.LaunchedAt.ToShortDateString() + "\"",
                    "\"" + project.Deadline.ToShortDateString() + "\"",
                    "\"" + project.Category.Name.Replace("\"", "\"\"") + "\"",
                    "\"" + createProjectURL(project) + "\"",

                }));
                progressBar1.PerformStep();
            }

            writer.Close();
            setBusy(false);
        }

        private string createProjectURL(Project p)
        {
            return "http://kickstarter.com/projects/" + p.Id + "/" + p.Slug;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
        }

        private async void go()
        {
            if (busy)
                return;

            writeProjects(comboBox1.Text, Int32.Parse(textBox2.Text), comboBox2.Text);
        }

        private void keyPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                go();
            }
        }
    }
}
