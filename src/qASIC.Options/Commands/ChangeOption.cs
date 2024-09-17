using Microsoft.VisualBasic;
using qASIC.CommandPrompts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace qASIC.Options.Commands
{
    public class ChangeOption : OptionsCommand
    {
        public ChangeOption(OptionsManager manager) : base(manager) { }

        public override string CommandName => "changeoption";
        public override string[] Aliases => new string[] { "setoption", "changesetting", "setsetting" };
        public override string Description => "Changed the value of an option.";

        KeyPrompt navigationPrompt = new KeyPrompt();
        TextPrompt valuePrompt = new TextPrompt();

        qLog listLog;
        int index = 0;
        Options.OptionsList.ListItem targetOption;

        List<Options.OptionsList.ListItem> items;

        public override object Run(CommandArgs args)
        {
            if (args.prompt == navigationPrompt)
            {
                switch (navigationPrompt.Key)
                {
                    case KeyPrompt.NavigationKey.Cancel:
                        UpdateLog(args, true, true);
                        return null;
                    case KeyPrompt.NavigationKey.Up:
                        index = Math.Max(index - 1, 0);
                        break;
                    case KeyPrompt.NavigationKey.Down:
                        index = Math.Min(index + 1, items.Count - 1);
                        break;
                    case KeyPrompt.NavigationKey.Confirm:
                        UpdateLog(args, true);
                        targetOption = items[index];
                        return AskForValue(args);
                }

                UpdateLog(args);
                return navigationPrompt;
            }

            if (args.prompt == valuePrompt)
            {
                //Set

                var value = targetOption.Value;
                if (!(value is string))
                {
                    try
                    {
                        value = Convert.ChangeType(valuePrompt.Text, targetOption.Value?.GetType());
                    }
                    catch
                    {
                        throw new CommandParseException(targetOption.Value?.GetType(), valuePrompt.Text);
                    }
                }

                Manager.SetOption(targetOption.Name, value);

                return null;
            }

            args.CheckArgumentCount(0, 2);

            //No args
            if (args.Length == 1)
            {
                listLog = null;
                items = Manager.OptionsList.Select(x => x.Value)
                    .ToList();
                UpdateLog(args);
                return navigationPrompt;
            }

            //Option name
            if (args.Length == 2)
            {
                targetOption = GetOption(args[1].arg);
                return AskForValue(args);
            }

            //All args
            targetOption = GetOption(args[1].arg);
            var settType = targetOption.Value?.GetType();
            var val = settType == null ?
                args[2].parsedValues.First() :
                args[2].GetValue(settType);

            Manager.SetOption(targetOption.Name, val);
            return null;
        }

        object AskForValue(CommandArgs args)
        {
            args.logs.Log("Enter value...");
            return valuePrompt;
        }

        void UpdateLog(CommandArgs args, bool final = false, bool cancelled = false)
        {
            if (listLog == null)
                listLog = qLog.CreateNow("");

            StringBuilder txt = new StringBuilder(final ? (cancelled ? "Cancelled" : "Setting Selected") : "Select Setting");
            for (int i = 0; i < items.Count; i++)
            {
                txt.Append("\n");
                txt.Append(i == index ? (final ? "]" : ">") : " ");
                txt.Append($" {items[i].Name}: {items[i].Value} (default value:{items[i].DefaultValue})");
            }

            listLog.message = txt.ToString();
            args.logs.Log(listLog);
        }

        Options.OptionsList.ListItem GetOption(string settingName)
        {
            if (!Manager.OptionsList.ContainsKey(settingName))
                throw new CommandException($"Setting '{settingName}' does not exist!");

            return Manager.OptionsList[settingName];
        }
    }
}