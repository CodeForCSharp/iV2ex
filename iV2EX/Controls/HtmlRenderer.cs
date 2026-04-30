using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using iV2EX.Util;
using iV2EX.Views;

namespace iV2EX.Controls
{
    internal static class HtmlRenderer
    {
        private const string BaseUrl = "https://www.v2ex.com";

        public static async Task<List<Paragraph>> Render(string html)
        {
            if (string.IsNullOrEmpty(html))
                return new List<Paragraph>();

            if (html.IndexOf('<') < 0 && html.IndexOf('&') < 0)
                return new List<Paragraph> { new Paragraph { Inlines = { new Run { Text = html } } } };

            var parser = new HtmlParser();
            var doc = await parser.ParseDocumentAsync(html);

            var paragraphs = new List<Paragraph>();
            var current = new Paragraph();
            var state = new RenderState();
            RenderChildren(doc.Body, ref current, paragraphs, state);

            if (current.Inlines.Count > 0)
                paragraphs.Add(current);

            return paragraphs;
        }

        private static void RenderChildren(INode parent, ref Paragraph current, List<Paragraph> paragraphs, RenderState state)
        {
            foreach (var child in parent.ChildNodes)
                RenderNode(child, ref current, paragraphs, state);
        }

        private static void RenderNode(INode node, ref Paragraph current, List<Paragraph> paragraphs, RenderState state)
        {
            switch (node.NodeName.ToUpperInvariant())
            {
                case "#TEXT":
                    var text = node.TextContent;
                    if (string.IsNullOrEmpty(text))
                        return;
                    text = text.Replace("\n", "").Replace("\r", "");
                    var wrapped = state.WrapInline(new Run { Text = text });
                    if (wrapped != null)
                        current.Inlines.Add(wrapped);
                    break;

                // --- Block-level elements ---
                case "H1":
                case "H2":
                case "H3":
                case "H4":
                case "H5":
                case "H6":
                    if (!state.SuppressBlockFlush)
                        FlushAndReset(ref current, paragraphs);
                    var level = node.NodeName[1] - '0';
                    state.PushBold();
                    state.PushFontSize(30 - level * 3);
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    state.Pop();
                    if (!state.SuppressBlockFlush)
                        FlushAndReset(ref current, paragraphs);
                    break;

                case "P":
                case "DIV":
                    if (!state.SuppressBlockFlush)
                        FlushAndReset(ref current, paragraphs);
                    RenderChildren(node, ref current, paragraphs, state);
                    if (!state.SuppressBlockFlush)
                        FlushAndReset(ref current, paragraphs);
                    break;

                case "BLOCKQUOTE":
                    if (!state.SuppressBlockFlush)
                        FlushAndReset(ref current, paragraphs);
                    if (state.SuppressBlockFlush)
                    {
                        RenderChildren(node, ref current, paragraphs, state);
                    }
                    else
                    {
                        current = new Paragraph
                        {
                            Margin = new Microsoft.UI.Xaml.Thickness(16, 4, 0, 4)
                        };
                        state.PushItalic();
                        state.PushInlineAction(i => i.Foreground = new SolidColorBrush(Colors.Gray));
                        RenderChildren(node, ref current, paragraphs, state);
                        state.Pop();
                        state.Pop();
                        FlushAndReset(ref current, paragraphs);
                    }
                    break;

                case "PRE":
                    if (!state.SuppressBlockFlush)
                        FlushAndReset(ref current, paragraphs);
                    if (state.SuppressBlockFlush)
                    {
                        RenderChildren(node, ref current, paragraphs, state);
                    }
                    else
                    {
                        current = new Paragraph
                        {
                            FontFamily = new FontFamily("Consolas"),
                            Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 4)
                        };
                        RenderChildren(node, ref current, paragraphs, state);
                        FlushAndReset(ref current, paragraphs);
                    }
                    break;

                case "UL":
                case "OL":
                    FlushAndReset(ref current, paragraphs);
                    var isOrdered = node.NodeName.Equals("OL", StringComparison.OrdinalIgnoreCase);
                    state.ListDepth++;
                    var indent = 20 * state.ListDepth;
                    var itemIndex = 0;
                    foreach (var child in node.ChildNodes)
                    {
                        if (!child.NodeName.Equals("LI", StringComparison.OrdinalIgnoreCase))
                            continue;
                        itemIndex++;
                        var liPara = new Paragraph
                        {
                            TextIndent = -16,
                            Margin = new Microsoft.UI.Xaml.Thickness(indent, 0, 0, 0)
                        };
                        var prefix = isOrdered ? $"{itemIndex}. " : "\u2022 ";
                        liPara.Inlines.Add(new Run { Text = prefix });
                        state.SuppressBlockFlush = true;
                        RenderChildren(child, ref liPara, paragraphs, state);
                        state.SuppressBlockFlush = false;
                        if (liPara.Inlines.Count > 0)
                            paragraphs.Add(liPara);
                    }
                    state.ListDepth--;
                    break;

                case "LI":
                    FlushAndReset(ref current, paragraphs);
                    current = new Paragraph();
                    state.SuppressBlockFlush = true;
                    RenderChildren(node, ref current, paragraphs, state);
                    state.SuppressBlockFlush = false;
                    FlushAndReset(ref current, paragraphs);
                    break;

                case "BR":
                    current.Inlines.Add(new LineBreak());
                    break;

                case "TABLE":
                    RenderTable(node, ref current, paragraphs);
                    break;

                case "HR":
                    FlushAndReset(ref current, paragraphs);
                    paragraphs.Add(new Paragraph
                    {
                        Inlines = { new Run { Text = new string('\u2500', 60) }, new LineBreak() }
                    });
                    break;

                // --- Inline elements ---
                case "STRONG":
                case "B":
                    state.PushBold();
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    break;

                case "EM":
                case "I":
                    state.PushItalic();
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    break;

                case "U":
                    state.PushUnderline();
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    break;

                case "S":
                case "DEL":
                    state.PushStrikethrough();
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    break;

                case "CODE":
                case "TT":
                    state.PushMonospace();
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    break;

                case "SUP":
                    state.PushInlineAction(i => i.FontSize -= 4);
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    break;

                case "SUB":
                    state.PushInlineAction(i => i.FontSize -= 4);
                    RenderChildren(node, ref current, paragraphs, state);
                    state.Pop();
                    break;

                case "A":
                    var anchor = node as IHtmlAnchorElement;
                    if (anchor == null) break;
                    var link = BuildHyperlink(anchor);
                    if (link != null)
                    {
                        state.PushHyperlink(link);
                        RenderChildren(node, ref current, paragraphs, state);
                        state.PopHyperlink();
                        current.Inlines.Add(link);
                    }
                    else
                    {
                        current.Inlines.Add(new Run { Text = anchor.Href ?? "" });
                    }
                    break;

                case "IMG":
                    RenderImage(node as IHtmlImageElement, current);
                    break;

                // --- Passthrough containers ---
                case "#document":
                case "HTML":
                case "HEAD":
                case "BODY":
                case "SPAN":
                case "FONT":
                    RenderChildren(node, ref current, paragraphs, state);
                    break;

                default:
                    if (IsBlockTag(node.NodeName))
                    {
                        if (!state.SuppressBlockFlush)
                            FlushAndReset(ref current, paragraphs);
                        RenderChildren(node, ref current, paragraphs, state);
                        if (!state.SuppressBlockFlush)
                            FlushAndReset(ref current, paragraphs);
                    }
                    else
                    {
                        RenderChildren(node, ref current, paragraphs, state);
                    }
                    break;
            }
        }

        private static void FlushAndReset(ref Paragraph current, List<Paragraph> paragraphs)
        {
            if (current.Inlines.Count > 0)
                paragraphs.Add(current);
            current = new Paragraph();
        }

        private static bool IsBlockTag(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case "P":
                case "DIV":
                case "H1":
                case "H2":
                case "H3":
                case "H4":
                case "H5":
                case "H6":
                case "BLOCKQUOTE":
                case "PRE":
                case "UL":
                case "OL":
                case "LI":
                case "HR":
                case "TABLE":
                case "TR":
                case "TD":
                case "TH":
                case "SECTION":
                case "ARTICLE":
                case "HEADER":
                case "FOOTER":
                case "NAV":
                case "ASIDE":
                case "FORM":
                case "FIELDSET":
                    return true;
                default:
                    return false;
            }
        }

        private static Hyperlink BuildHyperlink(IHtmlAnchorElement anchor)
        {
            if (anchor == null) return null;

            var href = anchor.Href ?? "";
            if (href.StartsWith("about://"))
                href = BaseUrl + href.Replace("about://", "");
            else if (href.StartsWith("about:"))
                href = "https:" + href.Replace("about:", "");

            var hyperlink = new Hyperlink
            {
                UnderlineStyle = UnderlineStyle.None,
                Foreground = new SolidColorBrush(Colors.RoyalBlue)
            };

            try
            {
                var hrefUri = new Uri(href);

                var v2exHosts = new[] { "www.v2ex.com", "v2ex.com" };
                if (Array.Exists(v2exHosts, h => hrefUri.Host == h))
                {
                    var segments = hrefUri.Segments;
                    if (segments.Length >= 3)
                    {
                        hyperlink.Click += (_, _) =>
                        {
                            if (segments[1] == "member/")
                                PageStack.Next("Right", "Right", typeof(MemberView), segments[2]);
                            else if (segments[1] == "t/")
                                PageStack.Next("Right", "Right", typeof(RepliesAndTopicView),
                                    Convert.ToInt32(segments[2]));
                        };
                    }
                }
                else
                {
                    hyperlink.NavigateUri = hrefUri;
                }
            }
            catch
            {
                return null;
            }

            return hyperlink;
        }

        private static void RenderTable(INode table, ref Paragraph current, List<Paragraph> paragraphs)
        {
            FlushAndReset(ref current, paragraphs);

            var rows = new List<INode>();
            CollectTableRows(table, rows);

            foreach (var row in rows)
            {
                var rowPara = new Paragraph
                {
                    FontFamily = new FontFamily("Consolas"),
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 2, 0, 2)
                };

                var cells = new List<INode>();
                foreach (var child in row.ChildNodes)
                {
                    var name = child.NodeName.ToUpperInvariant();
                    if (name == "TD" || name == "TH")
                        cells.Add(child);
                }

                for (var i = 0; i < cells.Count; i++)
                {
                    if (i > 0)
                        rowPara.Inlines.Add(new Run { Text = " | " });

                    var cellText = cells[i].TextContent.Trim().Replace("\n", " ").Replace("\r", "");
                    if (cells[i].NodeName.Equals("TH", StringComparison.OrdinalIgnoreCase))
                        rowPara.Inlines.Add(new Bold { Inlines = { new Run { Text = cellText } } });
                    else
                        rowPara.Inlines.Add(new Run { Text = cellText });
                }

                if (rowPara.Inlines.Count > 0)
                    paragraphs.Add(rowPara);
            }
        }

        private static void CollectTableRows(INode node, List<INode> rows)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeName.Equals("TR", StringComparison.OrdinalIgnoreCase))
                    rows.Add(child);
                else if (child.NodeName.Equals("THEAD", StringComparison.OrdinalIgnoreCase) ||
                         child.NodeName.Equals("TBODY", StringComparison.OrdinalIgnoreCase) ||
                         child.NodeName.Equals("TFOOT", StringComparison.OrdinalIgnoreCase))
                    CollectTableRows(child, rows);
            }
        }

        private static void RenderImage(IHtmlImageElement img, Paragraph current)
        {
            if (img == null)
                return;

            var source = img.Source ?? "";
            if (source.StartsWith("about:///"))
                source = BaseUrl + source.Replace("about://", "");
            else if (source.StartsWith("about:"))
                source = "https:" + source.Replace("about:", "");

            if (!Uri.IsWellFormedUriString(source, UriKind.Absolute))
                return;

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
            bitmap.Tapped += (_, s) =>
            {
                s.Handled = true;
                PageStack.Next("Right", "Right", typeof(ImageViewerPage), source);
            };

            current.Inlines.Add(new InlineUIContainer { Child = viewBox });
        }

        private sealed class RenderState
        {
            private readonly List<Func<Inline, Inline>> _formatters = new();
            private readonly Stack<Hyperlink> _hyperlinks = new();

            public bool SuppressBlockFlush { get; set; }
            public int ListDepth { get; set; }

            public Inline WrapInline(Inline leaf)
            {
                var result = leaf;
                for (var i = _formatters.Count - 1; i >= 0; i--)
                    result = _formatters[i](result);

                if (_hyperlinks.Count > 0)
                {
                    _hyperlinks.Peek().Inlines.Add(result);
                    return null;
                }

                return result;
            }

            private void Push(Func<Inline, Inline> formatter) => _formatters.Add(formatter);
            public void Pop() => _formatters.RemoveAt(_formatters.Count - 1);

            public void PushBold() => Push(i => new Bold { Inlines = { i } });
            public void PushItalic() => Push(i => new Italic { Inlines = { i } });
            public void PushUnderline() => Push(i => new Underline { Inlines = { i } });
            public void PushStrikethrough() => Push(i => new Span
            {
                TextDecorations = Windows.UI.Text.TextDecorations.Strikethrough,
                Inlines = { i }
            });
            public void PushMonospace() => Push(i => new Span
            {
                FontFamily = new FontFamily("Consolas"),
                Inlines = { i }
            });
            public void PushFontSize(double size) => Push(i =>
            {
                i.FontSize = size;
                return i;
            });
            public void PushInlineAction(Action<Inline> action) => Push(i =>
            {
                action(i);
                return i;
            });

            public void PushHyperlink(Hyperlink link) => _hyperlinks.Push(link);
            public void PopHyperlink()
            {
                if (_hyperlinks.Count > 0)
                    _hyperlinks.Pop();
            }
        }
    }
}
