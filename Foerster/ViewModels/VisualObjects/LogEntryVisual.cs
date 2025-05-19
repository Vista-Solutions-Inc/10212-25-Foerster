using Foerster.Models.Managers;
using MessageBusLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Wpf.Ui.Controls;
using static VistaHelpers.Log4Net.NotifyAppender;

namespace Foerster.ViewModels.VisualObjects
{
    public partial class LogEntryVisual : ObservableObject
    {
        [ObservableProperty] private string _timestamp;
        [ObservableProperty] private string _logLevel;
        [ObservableProperty] private string _senderID;
        [ObservableProperty] private string _message;
        [ObservableProperty] private SolidColorBrush _levelBrush = new SolidColorBrush(Colors.Black);
        [ObservableProperty] private SymbolRegular _levelIcon = new SymbolRegular();
        public LogEntryVisual(LogEventArgs entry)
        {
            _logLevel = entry.LogLevel.ToString();
            _senderID = entry.SenderID;
            _message = entry.Message;
            _timestamp = entry.Timestamp.ToString("yyyy'-'MM'-'dd'-'hh':'mm':'ss'.'ff");
            switch (entry.LogLevel)
            {
                case DebugEventTypes.Error:
                    _levelBrush = new SolidColorBrush(Colors.Red);
                    _levelIcon =  SymbolRegular.ErrorCircle20;
                    break;
                case DebugEventTypes.Warning:
                    _levelBrush = new SolidColorBrush(Colors.Orange);
                    _levelIcon = SymbolRegular.Warning20;
                    break;
                case DebugEventTypes.Debug:
                    _levelBrush = new SolidColorBrush(Colors.Blue);
                    _levelIcon = SymbolRegular.Bug20;
                    break;
                default:
                    _levelBrush = new SolidColorBrush(Colors.DarkGray);
                    _levelIcon = SymbolRegular.Info20;
                    break;
            }
        }
    }
}
