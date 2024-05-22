namespace qASIC
{
    /// <summary>Class for generating text trees.</summary>
    public class TextTree
    {
        public TextTree(string middleItemBranch, string lastItemBranch, string space, string verticalBranch)
        {
            Middle = middleItemBranch;
            Last = lastItemBranch;
            Space = space;
            Vertical = verticalBranch;
        }

        /// <summary>Text Tree that uses basic characters that will work in every scenario.</summary>
        public static TextTree Basic =>
            new TextTree("+-", "+-", "   ", " │ ");

        /// <summary>Text Tree that uses more complex characters. This will only work with monospaced fonts (fonts with all characters being the same width).</summary>
        public static TextTree Fancy =>
            new TextTree(" ├─", " └─", "   ", " │ ");

        /// <summary>Prefix for items that are in the middle (everything except for the last item).</summary>
        public string Middle { get; set; }
        /// <summary>Prefix for the last item in a list.</summary>
        public string Last { get; set; }
        /// <summary>Text used for creating an empty space.</summary>
        public string Space { get; set; }
        /// <summary>Text used for creating a vertical line.</summary>
        public string Vertical { get; set; }

        public string GenerateTree(TextTreeItem list) =>
            GenerateItem(list, "", false, true);

        public string GenerateItem(TextTreeItem item, string indent, bool isLast, bool isFirst = false)
        {
            string text = indent;

            if (!isFirst)
            {
                text += isLast ? Last : Middle;
                indent += isLast ? Space : Vertical;
            }

            text += $"{item.Text}\n";

            int childCount = item.children.Count;
            for (int i = 0; i < childCount; i++)
                text += GenerateItem(item.children[i], indent, i == childCount - 1);

            return text;
        }
    }
}