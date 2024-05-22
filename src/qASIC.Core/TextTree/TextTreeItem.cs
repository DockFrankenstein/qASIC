using System.Collections.Generic;

namespace qASIC
{
    public class TextTreeItem
    {
        public string Text { get; set; }
        public List<TextTreeItem> children = new List<TextTreeItem>();

        public TextTreeItem() { }

        public TextTreeItem(string text)
        {
            Text = text;
        }

        public void Add(TextTreeItem item) =>
            children?.Add(item);

        public void Add(string text) =>
            Add(new TextTreeItem(text));
    }
}
