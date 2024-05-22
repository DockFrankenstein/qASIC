using qASIC.Core.Interfaces;
using System.Linq;

namespace qASIC.Options
{
    public class OptionsManager : ILoggable
    {
        public OptionsManager() : this(new OptionTargetList().FindOptions()) { }

        public OptionsManager(OptionTargetList targetList, bool ensureListHasAllTargets = true)
        {
            TargetList = targetList;

            OptionsList.OnValueSet += List_OnChanged;

            if (ensureListHasAllTargets)
                EnsureListHasAllTargets();
        }

        private void List_OnChanged(OptionsList.ListItem[] items)
        {
            foreach (var item in items)
                TargetList.Set(item.Name, item.Value);
        }

        public LogManager Logs { get; set; } = new LogManager();
        public ILoggable[] Loggables => new ILoggable[0];

        /// <summary>List containing options and their values.</summary>
        public OptionsList OptionsList { get; private set; } = new OptionsList();

        /// <summary>List of found options and registered objects.</summary>
        public OptionTargetList TargetList { get; private set; }

        /// <summary>Formats a <c>string</c> to be used as a key for an option.</summary>
        /// <param name="text">String to format.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatKeyString(string text) =>
            text?.ToLower();

        /// <summary>Gets the value of an option.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <returns>The value.</returns>
        public object GetOption(string optionName) =>
            OptionsList[optionName];

        /// <summary>Gets the value of an option.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="defaultValue">Value to use if option doesn't exist on the list.</param>
        /// <returns>The value.</returns>
        public object GetOption(string optionName, object defaultValue) =>
            OptionsList.TryGetValue(optionName, out var val) ?
            val :
            defaultValue;

        /// <summary>Gets the value of an option.</summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="optionName">Name of the option.</param>
        /// <returns>The value.</returns>
        public T GetOption<T>(string optionName) =>
            (T)OptionsList[optionName].Value;

        /// <summary>Gets the value of an option.</summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="defaultValue">Value to use if option doesn't exist on the list.</param>
        /// <returns>The value.</returns>
        public T GetOption<T>(string optionName, T defaultValue) =>
            OptionsList.TryGetValue(optionName, out var val) ?
            (T)val.Value :
            defaultValue;

        /// <summary>Changes the value of a given option.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="value">Value to set.</param>
        public void SetOption(string optionName, object value)
        {
            OptionsList.Set(optionName, value);
            Logs.Log($"Changed option '{optionName}' to '{value}'.", "settings_set");
        }

        /// <summary>Changes values from a different <see cref="Options.OptionsList">.</summary>
        /// <param name="list">List containing options to set.</param>
        public void SetOptions(OptionsList list)
        {
            OptionsList.MergeList(list);
            Logs.Log($"Applied options: {string.Join("\n", list.Select(x => $"- {x}"))}", "settings_set_multiple");
        }

        public void Save()
        {

        }

        public void Load()
        {

        }

        /// <summary>Ensures the list of options is properly generated using the list of targets that were found in <see cref="TargetList"/>.</summary>
        public void EnsureListHasAllTargets()
        {
            OptionsList.EnsureTargets(TargetList);
            Logs.Log($"Created options from target list.", "settings_ensure_targets");
        }
    }
}