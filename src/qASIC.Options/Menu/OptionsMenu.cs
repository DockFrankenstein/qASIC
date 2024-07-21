using System.Collections.Generic;
using System.Linq;

namespace qASIC.Options.Menu
{
    public class OptionsMenu
    {
        public OptionsMenu() { }

        public OptionsMenu(string header) : this(null, "") { }

        public OptionsMenu(OptionsMenu parent) : this(parent, "") { }

        public OptionsMenu(OptionsMenu parent, string header)
        {
            Parent = parent;
            Header = header;
        }

        public string Header { get; set; }
        public List<OptionsMenu> Sections { get; private set; } = new List<OptionsMenu>();
        public List<OptionsMenuItem> Options { get; private set; } = new List<OptionsMenuItem>();

        public OptionsMenu Parent { get; set; }

        public OptionsMenu StartNewSection(string header)
        {
            var section = new OptionsMenu(this);
            section.Header = header;
            Sections.Add(section);
            return section;
        }

        public OptionsMenu FinishSection() =>
            Parent;

        public OptionsMenu AddOption(OptionsMenuItem option)
        {
            Options.Add(option);
            return this;
        }

        public IEnumerable<OptionsMenuItem> GetMultipleOptions(string optionName) =>
            GetAllOptions()
            .Where(x => x?.name == optionName);

        public OptionsMenuItem GetOption(string optionName) =>
           GetMultipleOptions(optionName)
           .FirstOrDefault();

        public IEnumerable<OptionsMenuItem> GetAllOptions()
        {
            var options = Sections
                .SelectMany(x => x.GetAllOptions())
                .Concat(Options);

            return options;
        }

        public IEnumerable<string> GetListOfMissingOptions(OptionsList list)
        {
            var existingOptions = GetAllOptions()
                .Where(x => x != null)
                .Select(x => x.name);

            var allOptions = list
                .Select(x => x.Key);

            return allOptions.Except(existingOptions);
        }
    }
}
