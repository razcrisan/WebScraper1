using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace WebScraper1
{
  public class NameAndScore
  {
    public string Name
    {
      get; set;
    }
    public string Score
    {
      get; set;
    }
  }
  public partial class Form1 : Form
  {
    DataTable table;
    HtmlWeb web = new HtmlWeb();
    public Form1()
    {
      InitializeComponent();
      InitTable();
    }

    private void InitTable()
    {
      table = new DataTable("GameRankingsDataTable");
      table.Columns.Add("Name", typeof(string));
      table.Columns.Add("Score", typeof(string));
      gameRankingsDataView.DataSource = table;
    }

    private async Task<List<NameAndScore>> GameRankingsFromPage(int pageNum)
    {
      string url = "http://www.gamerankings.com/browse.html";
      if (pageNum != 0)
          url = "http://www.gamerankings.com/browse.html?page=" + pageNum.ToString();

      var doc = await Task.Factory.StartNew(() => web.Load(url));
      var nameNodes = doc.DocumentNode.SelectNodes("//*[@id=\"main_col\"]//div//div//table//tr//td//a");
      var scoreNodes = doc.DocumentNode.SelectNodes("//*[@id=\"main_col\"]//div//div//table//tr//td//span//b");
      //If these are null it means the name and score nodes couldn't be found on the html page
      if (nameNodes == null || scoreNodes == null)
        return new List<NameAndScore>();

      var names = nameNodes.Select(node => node.InnerText);
      var scores = scoreNodes.Select(node => node.InnerText);
      return names.Zip(scores, (name, score) => new NameAndScore() { Name = name, Score = score }).ToList();
    }


    private async void Form1_Load(object sender, EventArgs e)
    {
      int pageNum = 0;
      var rankings = await GameRankingsFromPage(0);
      while(rankings.Count > 0 )
      {
        foreach (var ranking in rankings)
        table.Rows.Add(ranking.Name, ranking.Score);
        pageNum++;
        rankings = await GameRankingsFromPage(pageNum);

      }
    }
  }
}
