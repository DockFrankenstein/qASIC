using qASIC.Core.Interfaces;
using System;
using System.Linq;

namespace qASIC.Options
{
    public class OptionsManager : ILoggable, IService
    {
        public OptionsManager(OptionsSerializer serializer = null) : this(new OptionTargetList().FindOptions(), serializer) { }

        public OptionsManager(OptionTargetList targetList, OptionsSerializer serializer = null, bool ensureListHasAllTargets = true)
        {
            TargetList = targetList;
            Serializer = serializer ?? new OptionsSerializer();

            OptionsList.OnValueSet += List_OnChanged;

            if (ensureListHasAllTargets)
                EnsureListHasAllTargets();

            Revert();
            Apply();
        }

        private void List_OnChanged(OptionsList.ListItem[] items)
        {
            foreach (var item in items)
                TargetList.Set(RegisteredObjects, item.Name, item.Value);
        }

        private qInstance _instance;
        public qInstance Instance 
        {
            get => _instance;
            set
            {
                RegisteredObjects.StopSyncingWithOther(_instance?.RegisteredObjects);
                _instance = value;
                RegisteredObjects.SyncWithOther(_instance?.RegisteredObjects);
            }
        }

        public LogManager Logs { get; set; } = new LogManager();
        public ILoggable[] Loggables => new ILoggable[0];

        public OptionsSerializer Serializer { get; set; }


        /// <summary>List of registered objects that will have members marked with the <see cref="OptionAttribute"/> invoked when an option gets set.</summary>
        public qRegisteredObjects RegisteredObjects { get; set; } = new qRegisteredObjects();
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

        /// <summary>Changes the value of a given option and applies it. 
        /// It's the same as calling <see cref="SetOption(string, object)"/> and <see cref="Apply"/>.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="value">Value to set.</param>
        public void SetOptionAndApply(string optionName, object value)
        {
            SetOption(optionName, value);
            Apply();
        }

        /// <summary>Changes values from a different <see cref="Options.OptionsList">.</summary>
        /// <param name="list">List containing options to set.</param>
        public void SetOptions(OptionsList list)
        {
            OptionsList.MergeList(list);
            Logs.Log($"Applied options: {string.Join("\n", list.Select(x => $"- {x}"))}", "settings_set_multiple");
        }

        /// <summary>Changes values from a different <see cref="Options.OptionsList"> and applies them 
        /// It's the same as calling <see cref="SetOptions"/> and <see cref="Apply"/>.</summary>
        /// <param name="list">List containing options to set.</param>
        public void SetOptionsAndApply(OptionsList list)
        {
            SetOptions(list);
            Apply();
        }

        /// <summary>Applies currently set options by saving them.</summary>
        public void Apply()
        {
            try
            {
                Serializer.Save(OptionsList);
            }
            catch (Exception e)
            {
                Logs.LogError($"An exception occured while saving options: {e}");
                return;
            }

            Logs.Log($"Successfully saved options at {Serializer.Path}", "settings_save_success");
        }

        /// <summary>Reverts options from the save file.</summary>
        public void Revert()
        {
            try
            {
                Serializer.Load(OptionsList);
            }
            catch (Exception e)
            {
                Logs.LogError($"An exception occured while loading options: {e}");
                return;
            }

            Logs.Log("Successfully loaded options.", "settings_load_success");
        }

        /// <summary>Ensures the list of options is properly generated using the list of targets that were found in <see cref="TargetList"/>.</summary>
        public void EnsureListHasAllTargets()
        {
            OptionsList.EnsureTargets(TargetList, RegisteredObjects);
            Logs.Log($"Created options from target list.", "settings_ensure_targets");
        }
    }
}