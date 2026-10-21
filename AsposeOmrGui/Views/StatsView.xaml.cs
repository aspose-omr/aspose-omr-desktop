using System.Windows;
using System.Windows.Controls;
using AsposeOmrGui.ViewModels;

namespace AsposeOmrGui.Views;

public partial class StatsView : UserControl
{
    public StatsView()
    {
        InitializeComponent();
    }

    private void SavePng_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement button)
            return;
        if (DataContext is not StatsViewModel vm)
            return;
        if (button.DataContext is not QuestionStatViewModel question)
            return;
        if (button.Tag is not FrameworkElement card)
            return;

        vm.SaveChartPng(question, card);
    }
}
