using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using AngleSharp.Dom;
using iV2EX.Util;
using iV2EX.Views;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace iV2EX.Controls
{
    public sealed partial class HtmlTextBlock
    {
        private readonly DependencyProperty _isTextSelectionProperty =
            DependencyProperty.Register("IsTextTextSelection", typeof(bool), typeof(HtmlTextBlock),
                new PropertyMetadata(true,
                    (d, e) =>
                    {
                        var element = d as HtmlTextBlock;
                        if (element == null) return;
                        element.RichText.IsTextSelectionEnabled = (bool) e.NewValue;
                    }));

        private readonly DependencyProperty _textProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(HtmlTextBlock), new PropertyMetadata("",
                async (d, e) =>
                {
                    if (d is HtmlTextBlock element)
                    {
                        element.RichText.Blocks.Clear();
                        var html = await new HandleHtml().HtmlToXaml(e.NewValue as string);
                        element.RichText.Blocks.Add(html);
                        element.RichText.TextWrapping = TextWrapping.Wrap;
                    }               
                }));

        public HtmlTextBlock()
        {
            InitializeComponent();
        }

        public bool IsTextSelection
        {
            get => (bool) GetValue(_isTextSelectionProperty);
            set => SetValue(_isTextSelectionProperty, value);
        }

        public string Text
        {
            get => (string) GetValue(_textProperty);
            set => SetValue(_textProperty, value);
        }
    }

    public class HandleHtml
    {
        private readonly Stack<INode> _elementList = new Stack<INode>();
        private readonly List<List<INode>> _renderList = new List<List<INode>>();

        public async Task<Paragraph> HtmlToXaml(string richText)
        {
            const string baseUrl = "https://www.v2ex.com";
            const string unsafeUrl = "http://www.v2ex.com";
            richText = new[] {"</p>", "</h1>", "</h2>", "</h2>", "</h3>", "</h4>", "</h5>", "</h6>"}
                .Aggregate(richText, (current, s) => current.Replace(s, $"\\r{s}"));
            var dom = await new HtmlParser().ParseDocumentAsync(richText);
            await Task.Run(() => GetElements(dom.Body));
            var paragraph = new Paragraph();
            foreach (var render in _renderList)
            {
                Run run = null;
                Inline inline = null;
                var isCode = false;
                foreach (var element in render)
                    switch (element.NodeName)
                    {
                        case "IMG":
                            var img = element as IHtmlImageElement;
                            //var source = img.Source.StartsWith("about:")
                            //    ? $"http:{img.Source.Replace("about:", "")}"
                            //    : img.Source;
                            var source = img.Source;
                            if (source.StartsWith("source:///"))
                            {
                                source = baseUrl + source.Replace("source://", "");
                            }
                            else if(source.StartsWith("source:"))
                            {
                                source = $"https{source.Replace("source:", "")}";
                            }
                            if (!Uri.IsWellFormedUriString(source, UriKind.Absolute)) break;
                            var bitmap = new Image
                            {
                                Source = new BitmapImage(new Uri(source)),
                                Stretch = Stretch.None
                            };
                            var viewBox = new Viewbox
                            {
                                Child = bitmap,
                                StretchDirection = StretchDirection.DownOnly
                            };
                            viewBox.Tapped += (v, s) =>
                            {
                                PageStack.Next("Right", "Right", typeof(ImageViewerPage), source);
                            };
                            paragraph.Inlines.Add(new InlineUIContainer {Child = viewBox});
                            goto Out;

                        case "BR":
                            paragraph.Inlines.Add(new LineBreak());
                            goto Out;

                        case "#text":
                            run = new Run {Text = element.TextContent};
                            inline = run;
                            break;
                        case "A":
                            var elementAnchor = element as IHtmlAnchorElement;
                            var href = elementAnchor.Href.StartsWith("about://")
                                ? $"{baseUrl}{elementAnchor.Href.Replace("about://", "")}"
                                : elementAnchor.Href;
                            try
                            {
                                var hrefUri = new Uri(href);
                                var baseUri = new Uri(baseUrl);
                                var unsafeUri = new Uri(unsafeUrl);
                                if (hrefUri.Host == baseUri.Host || hrefUri.Host == unsafeUri.Host)
                                {
                                    var segments = hrefUri.Segments;
                                    var hyperlink = new Hyperlink
                                    {
                                        UnderlineStyle = UnderlineStyle.None,
                                        Foreground = new SolidColorBrush(Colors.RoyalBlue)
                                    };
                                    hyperlink.Click += (sender, e) =>
                                    {
                                        if (segments[1] == "member/")
                                            PageStack.Next("Right", "Right", typeof(MemberView), segments[2]);
                                        else if (segments[1] == "t/")
                                            PageStack.Next("Right", "Right", typeof(RepliesAndTopicView),
                                                Convert.ToInt32(segments[2]));
                                    };
                                    hyperlink.Inlines.Add(inline);
                                    inline = hyperlink;
                                }
                                else
                                {
                                    var hyperlink = new Hyperlink
                                    {
                                        UnderlineStyle = UnderlineStyle.None,
                                        Foreground = new SolidColorBrush(Colors.RoyalBlue),
                                        NavigateUri = new Uri(href)
                                    };
                                    hyperlink.Inlines.Add(inline);
                                    inline = hyperlink;
                                }
                            }
                            catch
                            {
                                inline = new Run {Text = href};
                            }

                            break;

                        case "STRONG":
                            var bold = new Bold();
                            bold.Inlines.Add(inline);
                            inline = bold;
                            break;

                        case "H1":
                        case "H2":
                        case "H3":
                        case "H4":
                        case "H5":
                        case "H6":
                            var h = new Bold {FontSize = Convert.ToInt32(element.NodeName.Substring(1)) + 15};
                            h.Inlines.Add(inline);
                            inline = h;
                            break;

                        case "CODE":
                            break;

                        case "PRE":
                            var code = new Italic();
                            code.Inlines.Add(inline);
                            inline = code;
                            isCode = true;
                            break;

                        case "LI":
                            var span = new Span();
                            span.Inlines.Add(inline);
                            inline = span;
                            break;
                    }
                if (!isCode)
                    run.Text = run.Text.Replace("\n", "").Replace("\\r", "\n");
                paragraph.Inlines.Add(inline);
                Out:;
            }

            return paragraph;
        }

        public void GetElements(INode node)
        {
            foreach (var child in node.ChildNodes)
            {
                _elementList.Push(child);
                if (child.ChildNodes.Length != 0)
                {
                    GetElements(child);
                }
                else
                {
                    var label = new[] {"IMG", "#text", "BR"};
                    if (label.Any(s => child.NodeName == s))
                    {
                        _renderList.Add(new List<INode>(_elementList));
                        _elementList.Pop();
                    }

                    _elementList.Clear();
                }
            }
        }
    }
}