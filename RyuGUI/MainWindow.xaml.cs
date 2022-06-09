using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModLoadOrder.Mods;
using RyuCLI;

namespace RyuGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ModInfo> ModList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetupModList(List<ModInfo> mods)
        {
            if (mods.Count > 0)
            {
                this.ModList = new ObservableCollection<ModInfo>(mods);
            }
            else
            {
                MessageBox.Show("No mods were found. Add some mods to the \"\\mods\\\" directory first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ModInfo m in this.ModListView.SelectedItems)
            {
                m.ToggleEnabled();
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            List<ModInfo> selection = new List<ModInfo>(this.ModListView.SelectedItems.Cast<ModInfo>());

            int limit = 0;
            foreach (int i in selection.Select(t => this.ModList.IndexOf(t)).OrderBy(x => x))
            {
                if (i > limit)
                {
                    ModInfo temp = this.ModList[i - 1];
                    this.ModList[i - 1] = this.ModList[i];
                    this.ModList[i] = temp;
                }
                else
                {
                    ++limit;
                }
            }

            // Restore selection
            foreach (ModInfo m in selection)
            {
                this.ModListView.SelectedItems.Add(m);
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            List<ModInfo> selection = new List<ModInfo>(this.ModListView.SelectedItems.Cast<ModInfo>());

            int limit = this.ModList.Count - 1;
            foreach (int i in selection.Select(t => this.ModList.IndexOf(t)).OrderByDescending(x => x))
            {
                if (i < limit)
                {
                    ModInfo temp = this.ModList[i + 1];
                    this.ModList[i + 1] = this.ModList[i];
                    this.ModList[i] = temp;
                }
                else
                {
                    --limit;
                }
            }

            // Restore selection
            foreach (ModInfo m in selection)
            {
                this.ModListView.SelectedItems.Add(m);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (RyuCLI.Program.WriteModListTxt(this.ModList.ToList()))
            {
                MessageBox.Show("Mod list was saved. Mods will be applied next time the game is run.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Mod list is empty and was not saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            Application.Current.Shutdown();
        }
    }
}
