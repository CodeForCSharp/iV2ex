using System.Linq;
using AngleSharp.Html.Parser;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace iV2EX.Views
{
    /// <summary>
    ///     可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MoneyDetailView
    {
        public MoneyDetailView()
        {
            Moneys.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetMoneyDetail(Moneys.CurrentPage);
                var dom = new HtmlParser().ParseDocument(html);
                var pages = int.Parse(dom.QuerySelector("input.page_input").GetAttribute("max"));
                var moneys = dom.QuerySelector("table.data").QuerySelectorAll("tr").Skip(1).Select(e =>
                {
                    var tds = e.QuerySelectorAll("td");
                    return new MoneyModel
                    {
                        Time = tds[0].TextContent,
                        Type = tds[1].TextContent,
                        Spend = tds[2].TextContent.Replace(".0", ""),
                        Desc = tds[4].TextContent
                    };
                });
                return new PagesBaseModel<MoneyModel> {Entity = moneys, Pages = pages};
            };
            InitializeComponent();
        }

        public IncrementalLoadingCollection<MoneyModel> Moneys { get; } =
            new IncrementalLoadingCollection<MoneyModel>();
    }
}