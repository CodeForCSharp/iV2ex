using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace iV2EX.Controls
{
    public sealed partial class HtmlTextBlock
    {
        private static readonly DependencyProperty IsTextSelectionProperty =
            DependencyProperty.Register(nameof(IsTextSelection), typeof(bool), typeof(HtmlTextBlock),
                new PropertyMetadata(true,
                    (d, e) =>
                    {
                        if (d is HtmlTextBlock element)
                            element.RichText.IsTextSelectionEnabled = (bool)e.NewValue;
                    }));

        private static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HtmlTextBlock),
                new PropertyMetadata("",
                    async (d, e) =>
                    {
                        if (d is HtmlTextBlock element)
                        {
                            element.RichText.Blocks.Clear();
                            element.RichText.TextWrapping = TextWrapping.Wrap;
                            var paragraphs = await HtmlRenderer.Render(e.NewValue as string ?? "");
                            foreach (var p in paragraphs)
                                element.RichText.Blocks.Add(p);
                        }
                    }));

        public HtmlTextBlock()
        {
            InitializeComponent();
        }

        public bool IsTextSelection
        {
            get => (bool)GetValue(IsTextSelectionProperty);
            set => SetValue(IsTextSelectionProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
