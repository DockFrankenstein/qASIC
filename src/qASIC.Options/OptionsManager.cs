using qASIC.Core;
using System;
using System.Linq;

namespace qASIC.Options
{
    public class OptionsManager : IHasLogs, IService
    {
        public OptionsManager(OptionsSerializer serializer = null) :
            this(new OptionTargetList().FindOptions(), serializer: serializer)
        { }

        public OptionsManager(OptionTargetList targetList, OptionsSerializer serializer = null) :
            this(null, targetList, serializer)
        { }

        public OptionsManager(qInstance instance, OptionsSerializer serializer = null) :
            this(instance, new OptionTargetList().FindOptions(), serializer: serializer)
        { }

        public OptionsManager(qInstance instance, OptionTargetList targetList, OptionsSerializer serializer = null)
        {
            Instance = instance;

            TargetList = targetList;
            Serializer = serializer ?? new OptionsSerializer();

            OptionsList.OnValueSet += List_OnChanged;

            RegisteredObjects.OnObjectRegistered += RegisteredObjects_OnObjectRegistered;
        }

        private void List_OnChanged(OptionsList.ListItem[] items)
        {
            foreach (var item in items)
            {
                TargetList.Set(RegisteredObjects, item.Name, item.Value);
                OnOptionChanged?.Invoke(new ChangeOptionArgs()
                {
                    optionName = item.Name,
                    value = item.Value,
                });
            }
        }

        private void RegisteredObjects_OnObjectRegistered(object obj)
        {
            TargetList.LoadValuesForObject(obj, OptionsList);
        }

        /// <summary>Main static instance of <see cref="OptionsManager"/> that was set using <see cref="SetAsMain"/>.</summary>
        public static OptionsManager Main { get; private set; }
        /// <summary>Sets this instance as main to make it accessible from property <see cref="Main"/>.</summary>
        /// <returns>Returns itself.</returns>
        public OptionsManager SetAsMain()
        {
            Main = this;
            return this;
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

        public OptionsSerializer Serializer { get; set; }


        /// <summary>List of registered objects that will have members marked with the <see cref="OptionAttribute"/> invoked when an option gets set.</summary>
        public qRegisteredObjects RegisteredObjects { get; set; } = new qRegisteredObjects();
        /// <summary>List containing options and their values.</summary>
        public OptionsList OptionsList { get; private set; } = new OptionsList();

        /// <summary>List of found options and registered objects.</summary>
        public OptionTargetList TargetList { get; private set; }

        /// <summary>Initializes the options manager.</summary>
        /// <param name="log">If the change should be logged.</param>
        public void Initialize(bool log = true)
        {
            EnsureListHasAllTargets();
            Revert(log);
            Apply(log);

            if (log)
                Logs.Log("Settings initialized!", "settings_init");
        }

        /// <summary>Formats a <c>string</c> to be used as a key for an option.</summary>
        /// <param name="text">String to format.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatKeyString(string text) =>
            text?.ToLower();

        /// <summary>Gets the value of an option.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <returns>The value.</returns>
        public object GetOption(string optionName) =>
            OptionsList[optionName].Value;

        /// <summary>Gets the value of an option.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="defaultValue">Value to use if option doesn't exist on the list.</param>
        /// <returns>The value.</returns>
        public object GetOption(string optionName, object defaultValue) =>
            OptionsList.TryGetValue(optionName, out var val) ?
            val.Value :
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
        /// <param name="log">If the change should be logged.</param>
        public void SetOption(string optionName, object value, bool log = true)
        {
            OptionsList.Set(optionName, value);
            if (log)
                Logs.Log($"Changed option '{optionName}' to '{value}'.", "settings_set");
        }

        /// <summary>Changes the value of a given option and applies it. 
        /// It's the same as calling <see cref="SetOption(string, object)"/> and <see cref="Apply"/>.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="log">If the change should be logged.</param>
        public void SetOptionAndApply(string optionName, object value, bool log = true)
        {
            SetOption(optionName, value, log);
            Apply();
        }

        /// <summary>Changes values from a different <see cref="Options.OptionsList">.</summary>
        /// <param name="list">List containing options to set.</param>
        /// <param name="log">If the change should be logged.</param>
        public void SetOptions(OptionsList list, bool log = true)
        {
            OptionsList.MergeList(list);
            if (log)
                Logs.Log($"Applied options: {string.Join("\n", list.Select(x => $"- {x}"))}", "settings_set_multiple");
        }

        /// <summary>Changes values from a different <see cref="Options.OptionsList"> and applies them 
        /// It's the same as calling <see cref="SetOptions"/> and <see cref="Apply"/>.</summary>
        /// <param name="list">List containing options to set.</param>
        /// <param name="log">If the change should be logged.</param>
        public void SetOptionsAndApply(OptionsList list, bool log = true)
        {
            SetOptions(list, log);
            Apply();
        }

        /// <summary>Applies currently set options by saving them.</summary>
        /// <param name="log">If the change should be logged.</param>
        public void Apply(bool log = true)
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

            if (log)
                Logs.Log($"Successfully saved options at {Serializer.Path.Replace('\\', '/')}", "settings_save_success");
        }

        /// <summary>Reverts options from the save file.</summary>
        /// <param name="log">If the change should be logged.</param>
        public void Revert(bool log = true)
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

            if (log)
                Logs.Log("Successfully loaded options.", "settings_load_success");
        }

        /// <summary>Ensures the list of options is properly generated using the list of targets that were found in <see cref="TargetList"/>.</summary>
        /// <param name="log">If the change should be logged.</param>
        public void EnsureListHasAllTargets(bool log = true)
        {
            OptionsList.EnsureTargets(TargetList, RegisteredObjects);
            if (log)
                Logs.Log($"Created options from target list.", "settings_ensure_targets");
        }

        #region Callbacks
        /// <summary>Called whenever an option gets changed.</summary>
        public Action<ChangeOptionArgs> OnOptionChanged;

        /// <summary>Register on changed callback for an option.</summary>
        /// <param name="optionName">Name of the option.</param>
        /// <param name="onChanged">Action to register.</param>
        public void RegisterOnChangedCallback(string optionName, Action<ChangeOptionArgs> onChanged)
        {
            OnOptionChanged += (ChangeOptionArgs args) =>
            {
                if (FormatKeyString(optionName) == args.optionName)
                    onChanged?.Invoke(args);
            };
        }
        #endregion
    }
}