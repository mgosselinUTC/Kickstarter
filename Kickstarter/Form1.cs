using Kickstarter.Api;
using Kickstarter.Api.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kickstarter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string state = textBox1.Text;

            var client = new KickstarterClient();

            var session = await client.StartSession("kickstarter@utc4me.org", "utc4medotorg");

            var tableTop = await session.Query(new FindCategory("Tabletop Games"));

            var query = new DiscoverProjects()
                          .InCategory(tableTop)
                          .InStatus("live")
                          .SortedBy("launch_date")
                          .Take(200);

            Console.WriteLine(from p in await session.Query(query) orderby p.LaunchedAt select p);

        }
    }
}
