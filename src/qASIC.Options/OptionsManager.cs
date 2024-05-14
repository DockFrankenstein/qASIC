using Microsoft.VisualBasic;

namespace qASIC.Options
{
    public class OptionsManager
    {
        public OptionsManager() : this(new OptionTargetList().FindOptions()) { }

        public OptionsManager(OptionTargetList targetList, bool ensureListHasAllTargets = true)
        {
            TargetList = targetList;

            if (ensureListHasAllTargets)
                EnsureListHasAllTargets();
        }

        public string SavePath { get; set; }

        public OptionsList List = new OptionsList();
        public OptionTargetList TargetList;

        public static string FormatKeyString(string s) =>
            s?.ToLower();

        public object GetOption(string optionName) =>
            List[optionName];

        public object GetOption(string optionName, object defaultValue) =>
            List.TryGetValue(optionName, out var val) ?
            val :
            defaultValue;

        public T GetOption<T>(string optionName) =>
            (T)List[optionName].Value;

        public T GetOption<T>(string optionName, T defaultValue) =>
            List.TryGetValue(optionName, out var val) ?
            (T)val.Value :
            defaultValue;

        public void SetOption(string optionName, object obj)
        {
            List.Set(optionName, obj);
        }

        public void SetOption(OptionsList list)
        {
            foreach (var item in list)
                SetOption(item.Key, item.Value);
        }

        public void Save()
        {

        }

        public void Load()
        {

        }

        public void EnsureListHasAllTargets()
        {
            List.EnsureTargets(TargetList);
        }
    }
}